# ⚠️ Xử lý Unhappy Path trong Microservices với MassTransit

## 🎯 Mục tiêu

- Hiểu và xử lý các tình huống **Unhappy Path** trong Microservices khi các dịch vụ gặp sự cố
- Tìm cách đảm bảo **data consistency** khi các dịch vụ bị lỗi hoặc không có kết nối

---

## 🧑‍💻 So sánh Microservices và Monolith

### 1. Monolith:
- **ACID Transactions** giúp đảm bảo tính nhất quán dữ liệu: **Atomicity, Consistency, Isolation, Durability**.
- **Rollback** khi có lỗi xảy ra: Dữ liệu không bị **inconsistency** trong hệ thống.

### 2. Microservices:
- Không có **ACID Transactions** giữa các service, dẫn đến việc khó quản lý tính nhất quán dữ liệu giữa các dịch vụ.
- Mỗi service có thể thất bại và có thể gây **data inconsistency**.
- Cần có chiến lược để xử lý lỗi và đảm bảo tính nhất quán như **retry**, **outbox**, và **message persistence**.

---

## 🔍 Unhappy Path trong Microservices

### Các tình huống có thể xảy ra:

1. **Auction Service** tạo phiên đấu giá và lưu vào **Postgres**.
2. **Service Bus (RabbitMQ)** và **Search Service** có thể gặp sự cố.

#### Các tình huống cụ thể:
- **Postgres DB bị down**: Không thể lưu phiên đấu giá, nhưng không có dữ liệu **inconsistency** vì không có thay đổi nào.
- **Search Service bị down**: **Search Service** sẽ tiếp nhận message từ **queue** khi phục hồi, đảm bảo tính nhất quán dữ liệu sau đó.
- **MongoDB bị down**: Dữ liệu **inconsistency** xảy ra vì không thể lưu vào MongoDB.
- **RabbitMQ bị down**: Không thể **publish message**, **Auction Service** sẽ báo lỗi, không có message nào được gửi đi, gây ra **data inconsistency**.

---

## ⚙️ Cách xử lý Unhappy Path với MassTransit

### 🧩 **Outbox Pattern**:
- **Outbox** cho phép lưu trữ các message khi dịch vụ không khả dụng.
- Khi dịch vụ phục hồi, các message trong **Outbox** sẽ được gửi lại và xử lý, tránh mất dữ liệu.

### 🔄 **Retry Mechanism**:
- **Retry** cho phép thử lại các message khi có lỗi, đảm bảo message sẽ được xử lý sau khi dịch vụ phục hồi.
  
```csharp
cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
```

---

## ✅ Kết luận

- **Microservices** yêu cầu phải quản lý **data consistency** khi các dịch vụ gặp sự cố.
- **Outbox pattern** và **retry mechanism** là các chiến lược hiệu quả giúp xử lý **unhappy path**.
- **MassTransit** hỗ trợ các tính năng này để đảm bảo tính ổn định của hệ thống.

📌 **Tiếp theo**: Cấu hình **retry** và **outbox** cho message trong MassTransit.