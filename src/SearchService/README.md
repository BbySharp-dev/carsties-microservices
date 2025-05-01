# Search Service - Microservices Architecture

## 1. Tổng quan về Service

Search Service là một microservice được thiết kế để cung cấp chức năng tìm kiếm cho hệ thống. Service này sử dụng MongoDB để lưu trữ và tìm kiếm dữ liệu, đồng thời giao tiếp với Auction Service để đồng bộ dữ liệu.

## 2. Cấu hình và Cài đặt

### 2.1. Cấu hình Port
```json
// launchSettings.json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "applicationUrl": "http://localhost:7002"
    }
  }
}
```

### 2.2. Cấu hình MongoDB
```json
// appsettings.json
{
  "ConnectionStrings": {
    "MongoDbConnection": "mongodb://root:mongopw@localhost"
  }
}
```

### 2.3. Cấu hình Service Communication
```json
// appsettings.json
{
  "AuctionServiceUrl": "http://localhost:7001"
}
```

## 3. Kiến trúc và Thiết kế

### 3.1. Cấu trúc Thư mục
```
SearchService/
├── Controllers/
│   └── SearchController.cs
├── Models/
│   ├── Item.cs
│   └── DbInitializer.cs
├── Services/
│   └── AuctionSvcHttpClient.cs
├── Program.cs
└── appsettings.json
```

### 3.2. Các Thành phần Chính

#### 3.2.1. SearchController
```csharp
[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        // Implementation
    }
}
```

#### 3.2.2. Item Model
```csharp
public class Item : Entity
{
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
    public int Mileage { get; set; }
    public string ImageUrl { get; set; }
    // Other properties
}
```

#### 3.2.3. AuctionSvcHttpClient
```csharp
public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        // Implementation
    }
}
```

## 4. Giao tiếp giữa các Service

### 4.1. Cấu hình HTTP Client
```csharp
// Program.cs
builder.Services.AddHttpClient<AuctionSvcHttpClient>(client => 
{
    client.BaseAddress = new Uri(builder.Configuration["AuctionServiceUrl"]);
});
```

### 4.2. Retry Policy
```csharp
private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

### 4.3. Circuit Breaker
```csharp
private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
```

## 5. Chức năng Tìm kiếm

### 5.1. Full-text Search
```csharp
if(!string.IsNullOrEmpty(searchParams.SearchTerm)){
    query.Match(Search.Full, searchParams.SearchTerm)
         .Sort(x => x.Ascending(x => x.Make));
}
```

### 5.2. Pagination
```csharp
query.PageNumber(searchParams.PageNumber);
query.PageSize(searchParams.PageSize);
```

### 5.3. Sorting
```csharp
query = searchParams.OrderBy switch{
    "make" => query.Sort(x => x.Ascending(a => a.Make)),
    "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
    _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
};
```

## 6. Best Practices và Lưu ý

### 6.1. Error Handling
- Sử dụng try-catch để xử lý ngoại lệ
- Log đầy đủ thông tin lỗi
- Implement retry mechanism
- Sử dụng circuit breaker pattern

### 6.2. Performance
- Tối ưu MongoDB queries
- Sử dụng indexing
- Implement caching khi cần thiết
- Cấu hình timeout hợp lý

### 6.3. Security
- Sử dụng HTTPS trong production
- Implement authentication/authorization
- Validate input data
- Sanitize search queries

### 6.4. Monitoring
- Log đầy đủ thông tin
- Implement health checks
- Collect metrics
- Set up alerts

## 7. Testing

### 7.1. Unit Tests
```csharp
public class SearchControllerTests
{
    [Fact]
    public async Task SearchItems_ReturnsCorrectResults()
    {
        // Test implementation
    }
}
```

### 7.2. Integration Tests
```csharp
public class SearchServiceIntegrationTests
{
    [Fact]
    public async Task SearchService_CommunicatesWithAuctionService()
    {
        // Test implementation
    }
}
```

## 8. Deployment

### 8.1. Docker Configuration
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "SearchService.dll"]
```

### 8.2. Environment Variables
```bash
MongoDbConnection=mongodb://root:mongopw@mongodb
AuctionServiceUrl=http://auctionservice:7001
```

## 9. Câu hỏi Phỏng vấn Thường gặp

### 9.1. Kiến trúc
1. Tại sao chọn microservices cho dự án này?
2. Làm thế nào để xử lý service discovery?
3. Cách đảm bảo tính nhất quán dữ liệu giữa các service?

### 9.2. MongoDB
1. Tại sao chọn MongoDB cho search service?
2. Cách tối ưu queries trong MongoDB?
3. Cách implement full-text search?

### 9.3. API Design
1. Cách thiết kế RESTful API cho search?
2. Best practices cho pagination?
3. Cách xử lý filtering và sorting?

### 9.4. Performance
1. Cách tối ưu performance của search service?
2. Chiến lược caching?
3. Cách xử lý high load?

## 10. Tài liệu Tham khảo

- [MongoDB Documentation](https://docs.mongodb.com/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Microservices Patterns](https://microservices.io/patterns/)
- [REST API Best Practices](https://restfulapi.net/) 