# ⚙️ Sử dụng Outbox Pattern trong Microservices

## 🎯 Mục tiêu

- Hiểu và triển khai **Outbox Pattern** để đảm bảo tính nhất quán dữ liệu giữa các dịch vụ khi dịch vụ phụ trợ không khả dụng.
- Sử dụng **MassTransit** kết hợp với **Entity Framework** để xử lý các message bị chậm khi **Service Bus** gặp sự cố.

---

## 🧑‍💻 Cấu trúc hiện tại

### 1. **Hiện tại**:
- **Dữ liệu inconsistency** giữa **Postgres DB** (auction) và **MongoDB** (search service) trước khi sử dụng **Service Bus**.
- Khi **RabbitMQ** bị down, hệ thống vẫn có thể lưu phiên đấu giá vào Postgres, nhưng không gửi được message tới search service.

---

## 🔄 Giới thiệu về Outbox Pattern

### **Outbox Pattern**:
- Giải pháp **Persistence** cho message nếu **Service Bus** không khả dụng (thông qua **Outbox table** trong cơ sở dữ liệu).
- Các message sẽ được giữ trong **Outbox** cho đến khi **Service Bus** phục hồi, và hệ thống sẽ thử lại gửi các message bị trì hoãn.

---

## ⚙️ Thiết lập Outbox trong MassTransit

### 1. Cài đặt **Entity Framework Core** Outbox:
- **Cài đặt package** `MassTransit.EntityFrameworkCore` trong dự án của bạn.
- Thêm cấu hình vào `Program.cs` trong **Auction Service**.

```csharp
builder.Services.AddMassTransit(cfg =>
{
    cfg.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseEntityFrameworkOutbox<YourDbContext>();
        // Cấu hình retry với khoảng thời gian delay
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));
    });
});
```

### 2. **Tạo Migration cho Outbox**:
- **DbContext** cần có các bảng Outbox để lưu trữ message trong trường hợp Service Bus không khả dụng.
  
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.AddOutboxStateEntity();
    modelBuilder.AddOutboxMessageEntity();
}
```

---

## ✅ Cách Hoạt Động

### 1. **Message thất bại khi Service Bus Down**:
- Khi **RabbitMQ** down, message được lưu vào **Outbox** trong cơ sở dữ liệu.
- Sau khi **RabbitMQ** phục hồi, hệ thống sẽ tự động gửi lại các message từ **Outbox** tới bus.

### 2. **Xử lý khi Service Bus Down**:
- Nếu Service Bus down, các message sẽ được ghi vào bảng **Outbox**.
- **Retry** sẽ thử lại sau mỗi vài giây cho đến khi message được gửi thành công.

---

## ✅ Kết luận

- **Outbox Pattern** giúp đảm bảo tính nhất quán của dữ liệu giữa các dịch vụ khi xảy ra sự cố.
- **MassTransit** kết hợp với **Entity Framework** giúp triển khai Outbox một cách dễ dàng, giúp đảm bảo tính ổn định của hệ thống.

📌 **Tiếp theo**: Xử lý khi có lỗi trong message delivery và retry các message gây ra exception.