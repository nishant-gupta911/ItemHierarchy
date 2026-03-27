# Deployment Guide

## Prerequisites

- .NET 10.0 SDK installed
- SQL Server 2019 or later
- IIS (For Windows deployment) or Linux web server
- Docker and Docker Compose (Optional, for containerized deployment)

## Local Development Deployment

### 1. Setup Database

```bash
# Connect to SQL Server and run setup script
sqlcmd -S localhost -U sa -P "YourPassword" -i setup.sql
```

### 2. Configure Connection String

Update `appsettings.json` with your SQL Server connection details:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ItemProcessingDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### 3. Run Application

```bash
dotnet run
```

Access the application at: `https://localhost:5001`

## Production Deployment

### Windows IIS Deployment

1. Publish the application: `dotnet publish -c Release`
2. Create new IIS site pointing to published folder
3. Configure Application Pool with .NET CLR version
4. Update connection string for production database
5. Ensure HTTPS is configured with valid certificate
6. Configure database backup strategies

### Docker Deployment

```bash
docker-compose up -d
```

## Security Checklist

- [ ] Update all password credentials
- [ ] Configure HTTPS with valid SSL certificates
- [ ] Enable SQL Server authentication
- [ ] Restrict database access to application user
- [ ] Configure firewall rules
- [ ] Enable logging for audit trail
- [ ] Set up regular backups
- [ ] Configure error logging service

## Monitoring

- Monitor application logs for errors
- Track database performance
- Monitor disk space and memory usage
- Setup alerts for critical issues
- Regular security audits
