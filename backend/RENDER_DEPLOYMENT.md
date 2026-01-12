# ?? Render Deployment Guide - Business Manager Backend

## Overview

This guide walks you through deploying the Business Manager .NET 8 backend to Render.com using Docker.

---

## Prerequisites

1. **Render Account**: Sign up at https://render.com
2. **GitHub Repository**: Your code pushed to GitHub
3. **External Database**: SQL Server database (Azure SQL, AWS RDS, or similar)
   - Render's free tier doesn't include SQL Server
   - You'll need an external database connection string

---

## Quick Start

### Option 1: Deploy Using render.yaml (Recommended)

1. **Push `render.yaml` to your repository**:
   ```bash
   git add render.yaml
   git commit -m "Add Render configuration"
   git push origin dev
   ```

2. **Create New Web Service on Render**:
   - Go to https://dashboard.render.com/
   - Click **"New +"** ? **"Blueprint"**
   - Connect your GitHub repository
   - Render will auto-detect `render.yaml`
   - Click **"Apply"**

3. **Set Environment Variables** (in Render dashboard):
   - `ConnectionStrings__ApplicationSqlConnection` - Your SQL Server connection string
   - `JwtOptions__SecretKey` - Your JWT secret key (32+ characters)
   - `JwtOptions__Issuer` - Your domain (e.g., `https://your-app.onrender.com`)
   - `JwtOptions__Audience` - Same as Issuer
   - `Facebook__AppId` - Your Facebook App ID
   - `Facebook__AppSecret` - Your Facebook App Secret
   - `Facebook__RedirectUri` - `https://your-app.onrender.com/api/social-platforms/facebook/callback`

4. **Deploy**: Render will automatically build and deploy

---

### Option 2: Manual Deployment

1. **Go to Render Dashboard**: https://dashboard.render.com/

2. **Create New Web Service**:
   - Click **"New +"** ? **"Web Service"**
   - Select **"Build and deploy from a Git repository"**
   - Connect your GitHub account
   - Select your repository
   - Select branch: **dev**

3. **Configure Service**:
   - **Name**: `businessmanager-api`
   - **Region**: Choose closest to your users
   - **Branch**: `dev`
   - **Runtime**: **Docker**
   - **Dockerfile Path**: `./Dockerfile`
   - **Docker Context**: `.` (root directory)

4. **Set Environment Variables**:
   Click **"Advanced"** ? **"Add Environment Variable"**:

   | Key | Value | Secret |
   |-----|-------|--------|
   | `ASPNETCORE_ENVIRONMENT` | `Production` | No |
   | `ASPNETCORE_URLS` | `http://0.0.0.0:$PORT` | No |
   | `ConnectionStrings__ApplicationSqlConnection` | Your SQL Server connection string | Yes |
   | `JwtOptions__SecretKey` | Your 32+ character secret key | Yes |
   | `JwtOptions__Issuer` | `https://your-app.onrender.com` | No |
   | `JwtOptions__Audience` | `https://your-app.onrender.com` | No |
   | `JwtOptions__DurationInDays` | `1` | No |
   | `Facebook__AppId` | Your Facebook App ID | Yes |
   | `Facebook__AppSecret` | Your Facebook App Secret | Yes |
   | `Facebook__RedirectUri` | `https://your-app.onrender.com/api/social-platforms/facebook/callback` | No |

5. **Health Check Path**: `/api/diagnostics/health`

6. **Click "Create Web Service"**

---

## Database Setup

### Option 1: Azure SQL Database (Recommended)

1. **Create Azure SQL Database**:
   - Go to https://portal.azure.com
   - Create a new SQL Database
   - Choose **Basic** tier ($5/month) or **Serverless** (pay-per-use)

2. **Configure Firewall**:
   - Add **0.0.0.0 - 255.255.255.255** (allow all IPs)
   - Or use Azure's "Allow Azure Services" option

3. **Get Connection String**:
   ```
   Server=your-server.database.windows.net,1433;Database=BusinessManager;User Id=your-username;Password=your-password;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True
   ```

4. **Run Migrations**:
   - Locally with connection string, OR
   - Via Render shell (see below)

### Option 2: Render PostgreSQL + EF Core Provider Change

If you want to use Render's free PostgreSQL:

1. **Create PostgreSQL Database on Render**:
   - Click **"New +"** ? **"PostgreSQL"**
   - Choose **Free** plan
   - Copy the **Internal Connection String**

2. **Update Your Code**:
   - Add `Npgsql.EntityFrameworkCore.PostgreSQL` package
   - Change `UseSqlServer` to `UseNpgsql` in Program.cs
   - Regenerate migrations for PostgreSQL

---

## Running Migrations

### Option 1: From Local Machine

```bash
# Set environment variable
$env:ConnectionStrings__ApplicationSqlConnection="Server=your-server.database.windows.net,1433;..."

# Run migrations
cd Infrastructure
dotnet ef database update --startup-project ../Presentation
```

### Option 2: From Render Shell

1. Go to your service in Render dashboard
2. Click **"Shell"** tab
3. Run:
   ```bash
   cd /app
   dotnet ef database update --project Infrastructure --startup-project Presentation
   ```

   **Note**: This requires EF Core tools in the Docker image. Add to Dockerfile if needed:
   ```dockerfile
   RUN dotnet tool install --global dotnet-ef
   ENV PATH="${PATH}:/root/.dotnet/tools"
   ```

---

## Common Render Deployment Errors & Solutions

### 1. **Error: "Application failed to respond to HTTP request"**

**Cause**: App not listening on the PORT environment variable

**Solution**: Ensure Program.cs uses `$PORT`:
```csharp
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(port));
});
```

---

### 2. **Error: "Could not find a part of the path '/app/appsettings.json'"**

**Cause**: appsettings.json not copied to output

**Solution**: Ensure .csproj has:
```xml
<ItemGroup>
  <Content Include="appsettings.json" CopyToOutputDirectory="Always" />
  <Content Include="appsettings.Production.json" CopyToOutputDirectory="Always" />
</ItemGroup>
```

---

### 3. **Error: "A network-related or instance-specific error occurred"**

**Cause**: Cannot connect to SQL Server

**Solutions**:
1. **Check connection string format** (use environment variable format with double underscores)
2. **Verify SQL Server firewall allows Render IPs** (0.0.0.0/0 for testing)
3. **Use TrustServerCertificate=True** if using self-signed certificate
4. **Check database is online** (Azure SQL pauses after inactivity)

---

### 4. **Error: "No such file or directory: 'Presentation.dll'"**

**Cause**: Build or publish failed

**Solutions**:
1. Check Render build logs for errors
2. Verify Dockerfile WORKDIR matches publish output
3. Ensure `dotnet publish` runs in Dockerfile

---

### 5. **Error: "Health check failed"**

**Cause**: Health check endpoint not responding

**Solutions**:
1. **Create DiagnosticsController** (if missing):
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class DiagnosticsController : ControllerBase
   {
       [HttpGet("health")]
       public IActionResult Health()
       {
           return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
       }
   }
   ```
2. Or disable health check in Render dashboard (not recommended)

---

### 6. **Error: "The instance of entity type 'X' cannot be tracked"**

**Cause**: EF Core tracking issue with multi-tenancy

**Solution**: Already handled in your code with proper `IStoreContext` usage

---

## Monitoring & Logs

### View Logs
- Go to Render dashboard ? Your service ? **"Logs"** tab
- Real-time streaming logs
- Search and filter capabilities

### Metrics
- **CPU/Memory Usage**: Dashboard ? **"Metrics"** tab
- **Request Count**: Integrated with logs
- **Response Times**: Available in paid plans

### Alerts
- Set up email alerts for:
  - Deploy failures
  - Health check failures
  - High resource usage

---

## Scaling & Performance

### Free Tier Limitations
- ?? **Spins down after 15 minutes of inactivity**
- ?? **Cold start time**: ~30 seconds
- ?? **750 hours/month** (shared across all free services)

### Solutions for Free Tier
1. **Keep-Alive Service**: Use UptimeRobot or similar to ping every 14 minutes
2. **Cache frequently accessed data**
3. **Optimize Docker image size**

### Upgrade to Paid Plan
- **Starter Plan** ($7/month): Always on, no cold starts
- **Standard Plan** ($25/month): More resources, auto-scaling

---

## Custom Domain

1. **Go to Render Dashboard** ? Your service ? **"Settings"**
2. **Scroll to "Custom Domain"**
3. **Add your domain**: `api.yourdomain.com`
4. **Update DNS Records** (at your domain registrar):
   - **CNAME**: `api` ? `your-app.onrender.com`
5. **Update Environment Variables**:
   - `JwtOptions__Issuer` ? `https://api.yourdomain.com`
   - `Facebook__RedirectUri` ? `https://api.yourdomain.com/api/social-platforms/facebook/callback`

---

## CI/CD with Render

Render auto-deploys on every push to your branch:

1. **Auto-Deploy**: Enabled by default
2. **Pull Request Previews**: Available in paid plans
3. **Manual Deploy**: Click **"Manual Deploy"** in dashboard

### Disable Auto-Deploy
- Go to **"Settings"** ? Disable **"Auto-Deploy"**

---

## Environment-Specific Configuration

### appsettings.Production.json

Create `Presentation/appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Hangfire": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

**Do not include secrets** (use environment variables instead)

---

## Security Best Practices

1. ? **Use Render's Secret Environment Variables**
2. ? **Enable HTTPS** (automatic with Render)
3. ? **Rotate JWT secrets regularly**
4. ? **Restrict CORS to specific origins** (not `AllowAnyOrigin()`)
5. ? **Use strong SQL Server passwords**
6. ? **Enable SQL Server firewall rules**
7. ? **Keep dependencies updated**

---

## Troubleshooting Checklist

- [ ] Dockerfile builds successfully locally (`docker build -t test .`)
- [ ] PORT environment variable is used in Program.cs
- [ ] All required environment variables are set in Render
- [ ] Database connection string is correct
- [ ] Database firewall allows Render IPs
- [ ] Migrations have been run
- [ ] Health check endpoint exists and responds
- [ ] appsettings.json is copied to output directory
- [ ] CORS is configured properly

---

## Support & Resources

- **Render Documentation**: https://render.com/docs
- **Render Community**: https://community.render.com
- **Render Status**: https://status.render.com
- **GitHub Discussions**: Use your repository's discussions

---

## Example: Complete Render Dashboard Configuration

**Service Configuration**:
```
Name: businessmanager-api
Environment: Docker
Region: Oregon (or closest to you)
Branch: dev
Dockerfile Path: ./Dockerfile
Docker Build Context: .
```

**Environment Variables**:
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
ConnectionStrings__ApplicationSqlConnection=<your-sql-connection-string>
JwtOptions__SecretKey=<32-char-secret>
JwtOptions__Issuer=https://businessmanager-api.onrender.com
JwtOptions__Audience=https://businessmanager-api.onrender.com
JwtOptions__DurationInDays=1
Facebook__AppId=<your-facebook-app-id>
Facebook__AppSecret=<your-facebook-app-secret>
Facebook__RedirectUri=https://businessmanager-api.onrender.com/api/social-platforms/facebook/callback
```

**Health Check Path**: `/api/diagnostics/health`

---

## Next Steps After Deployment

1. ? Test Swagger UI: `https://your-app.onrender.com/swagger`
2. ? Test Hangfire Dashboard: `https://your-app.onrender.com/hangfire`
3. ? Test authentication: `POST /api/auth/register`
4. ? Create first store: `POST /api/stores`
5. ? Connect Facebook page: `POST /api/social-platforms/facebook/connect`
6. ? Monitor logs for any errors
7. ? Set up uptime monitoring (UptimeRobot, Pingdom)

---

**Your backend is now live on Render! ??**

For specific error messages, check the **Logs** tab in your Render dashboard.
