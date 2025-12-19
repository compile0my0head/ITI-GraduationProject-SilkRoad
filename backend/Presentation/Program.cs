using Application;
using Application.Common.Configuration;
using Application.Common.Interfaces;
using Domain.Entities;
using Hangfire;
using Hangfire.SqlServer;
using Infrastructure;
using Infrastructure.BackgroundJobs;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Presentation.Middleware;
using Presentation.Middlewares;
using Presentation.Common;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Add custom DateTime converters for readable AM/PM format
        options.JsonSerializerOptions.Converters.Add(new ReadableDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new ReadableNullableDateTimeConverter());
        
        // Optional: Preserve enum names instead of numbers
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configure CORS to allow all origins (for development)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register unified DbContext with single connection string
builder.Services.AddDbContext<SaasDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ApplicationSqlConnection"),
        b => b.MigrationsAssembly("Infrastructure"));
});

// Configure Identity to use unified SaasDbContext
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<SaasDbContext>()
  .AddDefaultTokenProviders();

// ⭐ ADD JWT AUTHENTICATION
var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // For development only
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Register Application layer services (MediatR, AutoMapper, etc.)
builder.Services.AddApplicationServices(builder.Configuration);

// Register Infrastructure layer services (Repositories, etc.)
builder.Services.AddInfrastructureServices();

// Add Swagger/OpenAPI with JWT Support + Store ID Header
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Business Manager API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: \"Bearer eyJhbGci...\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Add X-Store-ID header to Swagger UI
    c.OperationFilter<SwaggerStoreIdHeaderOperationFilter>();
});

builder.Services.AddScoped<IDataSeeding, DataSeeding>();

// register hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("ApplicationSqlConnection"), 
    new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.FromSeconds(15),
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer();



var app = builder.Build();

// Execute data seeding
using (var scope = app.Services.CreateScope())
{
    var objectOfDataSeeding = scope.ServiceProvider.GetRequiredService<IDataSeeding>();
    await objectOfDataSeeding.DataSeedAsync();
}
        
// ⭐ Register Exception Middleware FIRST - catches all exceptions
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); 
}

// ⭐ Enable static files (for facebook-callback.html)
app.UseStaticFiles();

// ⭐ IMPORTANT: CORS must come BEFORE Authentication/Authorization
app.UseCors("AllowAll");

// IMPORTANT: For development, allow HTTP (comment out HTTPS redirect)
// app.UseHttpsRedirection();

// ⭐ Store Context Middleware - Extract Store ID from header (BEFORE Authentication)
app.UseStoreContext();

// ⭐ Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// ⭐ Store Validation Middleware - Validate store exists and user has access (AFTER Authentication)
app.UseStoreValidation();

// Configure Hangfire Dashboard AFTER authentication middleware
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// ⚠️ CRITICAL: Clean up legacy jobs from Hangfire database (if they exist)
// This ensures no duplicate publishing even if old jobs are still in the database
RecurringJob.RemoveIfExists("campaign-scheduler-job");
RecurringJob.RemoveIfExists("campaign-scheduler");

// ✅ Register the ONLY job that should publish posts
// Platform Publisher Job - Publishes CampaignPostPlatform records to social media
// Campaign scheduling windows are enforced inside PlatformPublishingService
RecurringJob.AddOrUpdate<PlatformPublisherJob>(
    "platform-publisher",
    job => job.ExecuteAsync(),
    Cron.Minutely); // Run every minute for responsive publishing

app.MapControllers();

app.Run();


