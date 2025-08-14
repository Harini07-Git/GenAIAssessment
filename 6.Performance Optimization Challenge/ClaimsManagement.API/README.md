Project: Claims Management API with Performance Optimization

This project demonstrates performance optimization techniques in a .NET 8 Web API application for claims management. Key features include:

1. In-Memory Caching
- Implementation of IMemoryCache for frequently accessed data
- Cache expiration policies (absolute and sliding)
- Manual cache invalidation endpoints

2. Pagination
- Server-side pagination for large datasets
- Configurable page size and number
- Metadata including total items and pages

3. Entity Framework Core Optimization
- Lazy loading for related entities
- Selective Include() statements
- Efficient query patterns

4. Performance Monitoring
- /metrics endpoint for performance metrics
- Integration with System.Diagnostics.Metrics
- Tracking of:
  - Average response times
  - Cache hit/miss counts
  - Request counts

5. Clean Architecture
- Separation of concerns
- Dedicated services for caching, metrics, and business logic
- Unit tests for core functionality

Getting Started:

1. Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code

2. Running the Application
```powershell
dotnet restore
dotnet build
dotnet run
```

3. Testing
```powershell
cd Tests
dotnet test
```

4. API Endpoints
- GET /api/claims - Get paginated claims list
- GET /api/claims/{id} - Get claim by ID
- POST /api/claims/invalidate-cache - Invalidate claims cache
- GET /api/claims/metrics - Get performance metrics

5. Configuration
The application uses default configuration values for:
- Cache expiration: 10 minutes absolute, 2 minutes sliding
- Default page size: 10 items
- Metrics collection interval: Real-time

For production deployment, consider:
- Adjusting cache durations based on data volatility
- Configuring appropriate page sizes
- Setting up proper monitoring and alerting
- Implementing authentication and authorization
