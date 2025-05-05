# ⚠️ Xử lý Lỗi và Retry trong Microservices với MassTransit

## 🎯 Mục tiêu

- Hiểu cách cấu hình **Retry Policies** cho các endpoint trong **MassTransit**.
- Giải quyết các lỗi tạm thời (transient errors) khi **MongoDB** hoặc các dịch vụ khác không khả dụng.
- Sử dụng **Fault Queues** trong **RabbitMQ** để xử lý và theo dõi lỗi.

---

## 🔄 Xử lý Retry khi dịch vụ không khả dụng

### **1. Retry Policies**:
Khi các dịch vụ như **MongoDB** không khả dụng, chúng ta có thể cấu hình **retry** để thử lại hành động nhiều lần trước khi báo lỗi.
```csharp
cfg.ReceiveEndpoint("search-auction-created", e =>
{
    e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));
    e.ConfigureConsumer<AuctionCreatedConsumer>(context);
});
```

- Cấu hình này cho phép **retry 5 lần** với **5 giây** giữa các lần thử.
- Nếu **MongoDB** bị down, message sẽ không bị mất và sẽ được thử lại cho đến khi dịch vụ phục hồi.

### **2. Cấu hình Retry cho Consumer**:
Lỗi có thể xảy ra khi một dịch vụ phụ trợ (như MongoDB) không khả dụng.
Với cấu hình retry, chúng ta đảm bảo rằng các thông điệp bị lỗi sẽ được thử lại nhiều lần trước khi xảy ra lỗi cuối cùng.

---

## 🚨 **Fault Queues**: 

Khi có lỗi không thể xử lý ngay lập tức, thông điệp sẽ được gửi đến **Fault Queues** trong RabbitMQ. Điều này giúp chúng ta có thể **theo dõi** và **xử lý** sau.

### **Cấu hình Fault Queues**:
Khi message gặp lỗi và không thể được xử lý, **Fault Queues** sẽ lưu trữ các lỗi này và cho phép chúng ta **tiếp tục theo dõi**.

```csharp
cfg.ReceiveEndpoint("search-auction-created", e =>
{
    e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));
    e.ConfigureConsumer<AuctionCreatedConsumer>(context);
    e.UseInMemoryOutbox();
});
```

---

## ✅ Kết luận

- **Retry Policies** giúp tránh mất dữ liệu trong các tình huống dịch vụ không khả dụng và đảm bảo tính ổn định của hệ thống.
- **Fault Queues** hỗ trợ theo dõi và xử lý lỗi, giúp hệ thống không bị gián đoạn hoàn toàn khi có sự cố.
- Việc **retry** nhiều lần có thể giúp các lỗi tạm thời được giải quyết mà không cần phải can thiệp thủ công.

📌 **Tiếp theo**: Tìm hiểu cách xử lý **exception** chi tiết và cấu hình **Fault Handlers** trong MassTransit.