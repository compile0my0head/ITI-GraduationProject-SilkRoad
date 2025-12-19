# ?? Render Deployment - Quick Reference Card

## ? Quick Deploy Steps

### 1. **Push to GitHub** ? (Already Done)
```bash
git push origin dev
```

### 2. **Create Render Service**
1. Go to https://dashboard.render.com/
2. Click **"New +"** ? **"Blueprint"**
3. Connect GitHub: `compile0my0head/ITI-GraduationProject-SilkRoad`
4. Select branch: **dev**
5. Click **"Apply"**

### 3. **Set Environment Variables** (Required)
In Render dashboard ? Your service ? **"Environment"**:

```env
# Database (REQUIRED)
ConnectionStrings__ApplicationSqlConnection
  Server=YOUR_SERVER.database.windows.net,1433;Database=BusinessManager;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True

# JWT (REQUIRED)
JwtOptions__SecretKey
  YourSuperSecretKeyThatIsAtLeast32CharactersLong123456789

JwtOptions__Issuer
  https://your-app-name.onrender.com

JwtOptions__Audience
  https://your-app-name.onrender.com

# Facebook OAuth (REQUIRED if using social features)
Facebook__AppId
  your_facebook_app_id

Facebook__AppSecret
  your_facebook_app_secret

Facebook__RedirectUri
  https://your-app-name.onrender.com/api/social-platforms/facebook/callback
```

### 4. **Deploy & Monitor**
- Render auto-deploys from dev branch
- Monitor build logs in Render dashboard
- Health check: `https://your-app.onrender.com/api/diagnostics/health`

---

## ?? Common Error Solutions

### Error: "Application failed to respond"
**Fix**: Already implemented - PORT variable configured ?

### Error: "Cannot connect to database"
**Check**:
1. Connection string format (double underscores: `ConnectionStrings__ApplicationSqlConnection`)
2. Database firewall allows `0.0.0.0/0`
3. Database is online (Azure SQL might be paused)

### Error: "Health check failed"
**Fix**: Already implemented - `/api/diagnostics/health` endpoint added ?

---

## ?? Pre-Deployment Checklist

- [x] Code pushed to dev branch
- [x] Dockerfile configured for Render
- [x] PORT environment variable configured
- [x] Health check endpoint exists
- [x] render.yaml created
- [ ] Database created (Azure SQL / AWS RDS)
- [ ] Environment variables set in Render
- [ ] Database migrations run

---

## ??? Database Setup Options

### **Option A: Azure SQL Database** (Recommended)
```
Cost: $5/month (Basic tier)
Free Trial: $200 credit for 30 days
Link: https://portal.azure.com
```

**Steps**:
1. Create SQL Database
2. Set firewall: Allow 0.0.0.0 - 255.255.255.255
3. Copy connection string
4. Paste in Render environment variables
5. Run migrations (see below)

### **Option B: AWS RDS SQL Server**
```
Cost: ~$30/month (smallest instance)
Free Tier: 750 hours/month for 1 year
Link: https://aws.amazon.com/rds/
```

---

## ?? Run Migrations

### Local (Before Deployment)
```bash
# Set environment variable
$env:ConnectionStrings__ApplicationSqlConnection="Server=..."

# Run migrations
cd Infrastructure
dotnet ef database update --startup-project ../Presentation
```

### Remote (After Deployment)
Via Render Shell:
```bash
cd /app
dotnet ef database update --project Infrastructure --startup-project Presentation
```

---

## ?? Test Your Deployment

After deployment, test these endpoints:

```bash
# Health check
curl https://your-app.onrender.com/api/diagnostics/health

# Swagger UI
https://your-app.onrender.com/swagger

# Hangfire Dashboard
https://your-app.onrender.com/hangfire

# Register user
POST https://your-app.onrender.com/api/auth/register
{
  "fullName": "Test User",
  "email": "test@example.com",
  "password": "Test123!"
}
```

---

## ?? Important Notes

### Free Tier Limitations
- ?? **Spins down after 15 minutes** of inactivity
- ?? **Cold start**: ~30 seconds on first request
- ?? **750 hours/month** limit

### Solutions
1. Use **UptimeRobot** to ping every 14 minutes (keeps alive)
2. Upgrade to **Starter ($7/month)** for always-on
3. Use **Railway.app** or **Fly.io** as alternatives

### Security
- ? All secrets use Render's secret environment variables
- ? HTTPS enabled automatically
- ?? Update CORS in production (don't use `AllowAnyOrigin`)

---

## ?? Get Help

**Render Support**:
- Docs: https://render.com/docs
- Community: https://community.render.com
- Status: https://status.render.com

**Your Project**:
- Full Guide: `RENDER_DEPLOYMENT.md`
- Docker Guide: `DOCKER_README.md`
- GitHub: https://github.com/compile0my0head/ITI-GraduationProject-SilkRoad

---

## ?? Next Steps After Successful Deploy

1. ? Test all API endpoints via Swagger
2. ? Create first store
3. ? Connect Facebook page
4. ? Create test campaign
5. ? Monitor Hangfire jobs
6. ? Set up custom domain (optional)
7. ? Configure uptime monitoring

---

**Your backend is ready for Render deployment! ??**

If you encounter specific errors, share the error message and I'll help you resolve it.
