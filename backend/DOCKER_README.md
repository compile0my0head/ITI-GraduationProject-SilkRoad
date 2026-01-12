# Business Manager Backend - Docker Deployment

## ?? Docker Setup

This backend is containerized using Docker and can be deployed with Docker Compose.

### Prerequisites
- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose v2.0+

### Quick Start

1. **Clone the repository**:
   ```bash
   git clone https://github.com/compile0my0head/ITI-GraduationProject-SilkRoad.git
   cd backend
   ```

2. **Create environment file**:
   ```bash
   cp .env.example .env
   # Edit .env and fill in your configuration
   ```

3. **Build and run**:
   ```bash
   docker-compose up -d
   ```

4. **Access the application**:
   - API: http://localhost:5000
   - Swagger: http://localhost:5000/swagger
   - Hangfire Dashboard: http://localhost:5000/hangfire

### Environment Variables

Create a `.env` file based on `.env.example`:

```env
# Database
DB_CONNECTION_STRING=Server=sqlserver,1433;Database=BusinessManager;User Id=sa;Password=YourPassword;...
DB_SA_PASSWORD=YourStrong@Password123

# JWT
JWT_SECRET_KEY=Your32CharacterOrLongerSecretKey
JWT_ISSUER=https://your-domain.com
JWT_AUDIENCE=https://your-domain.com
JWT_DURATION_DAYS=1

# Facebook OAuth
FACEBOOK_APP_ID=your_app_id
FACEBOOK_APP_SECRET=your_app_secret
FACEBOOK_REDIRECT_URI=https://your-domain.com/api/social-platforms/facebook/callback
```

### Docker Commands

**Build and start services**:
```bash
docker-compose up -d
```

**View logs**:
```bash
docker-compose logs -f backend
```

**Stop services**:
```bash
docker-compose down
```

**Rebuild after code changes**:
```bash
docker-compose up -d --build
```

**Run migrations** (if needed):
```bash
docker exec -it businessmanager-api dotnet ef database update --project Infrastructure --startup-project Presentation
```

### Architecture

```
???????????????????????????????????????????????
?  Docker Compose Network                     ?
?                                             ?
?  ????????????????????  ??????????????????? ?
?  ?  Backend API     ?  ?  SQL Server     ? ?
?  ?  (Port 5000)     ????  (Port 1433)    ? ?
?  ?  .NET 8          ?  ?  MSSQL 2022     ? ?
?  ????????????????????  ??????????????????? ?
?         ?                      ?            ?
?         ?                      ?            ?
?         ????????????????????????            ?
?           Persistent Volume                 ?
???????????????????????????????????????????????
```

### Services

1. **backend** - ASP.NET Core 8 API
   - Ports: 5000 (HTTP), 5001 (HTTPS)
   - Health check: `/api/diagnostics/health`
   - Auto-restart enabled

2. **sqlserver** - Microsoft SQL Server 2022
   - Port: 1433
   - Persistent volume: `sqlserver-data`
   - Auto-restart enabled

### Health Checks

Both services include health checks:
- Backend: HTTP check on `/api/diagnostics/health`
- Database: SQL connection test

View health status:
```bash
docker ps
docker-compose ps
```

### Production Deployment

For production deployment:

1. Update `.env` with production values
2. Use proper SSL certificates (not self-signed)
3. Set `ASPNETCORE_ENVIRONMENT=Production`
4. Use strong database passwords
5. Rotate JWT secret keys regularly
6. Configure firewall rules
7. Set up reverse proxy (nginx/Caddy) for HTTPS
8. Enable monitoring and logging

### Troubleshooting

**Container won't start**:
```bash
docker-compose logs backend
```

**Database connection failed**:
- Check `DB_CONNECTION_STRING` in `.env`
- Ensure SQL Server container is running: `docker ps`
- Verify SQL Server logs: `docker-compose logs sqlserver`

**Port already in use**:
```bash
# Change ports in docker-compose.yml
ports:
  - "8080:80"  # Change 5000 to 8080
```

**Clear volumes and restart**:
```bash
docker-compose down -v
docker-compose up -d
```

### Development vs Production

**Development** (local):
```bash
docker-compose up -d
```

**Production** (server):
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

---

## ?? Manual Deployment (No Docker)

If you prefer to run without Docker:

1. Install .NET 8 SDK
2. Install SQL Server
3. Update `appsettings.json` connection string
4. Run migrations:
   ```bash
   dotnet ef database update --project Infrastructure --startup-project Presentation
   ```
5. Run the application:
   ```bash
   cd Presentation
   dotnet run
   ```

---

## ?? CI/CD Integration

This setup is compatible with:
- GitHub Actions
- Azure DevOps
- GitLab CI/CD
- Jenkins

Example GitHub Actions workflow available in `.github/workflows/` (if added).

---

**For questions or issues, check the main documentation or create an issue on GitHub.**
