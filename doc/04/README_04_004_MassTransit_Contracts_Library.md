# 📦 Cài đặt MassTransit và Tạo Thư viện Contracts cho Microservices

## 🎯 Mục tiêu

- Cài đặt **MassTransit** để kết nối RabbitMQ
- Tạo **class library** cho các contract (event/message objects) dùng chung giữa các service

---

## 🧑‍💻 MassTransit - Trừu tượng hóa kết nối RabbitMQ

**MassTransit** là thư viện tương tự **Entity Framework**, nhưng dành cho message broker:

- **Trừu tượng hóa** việc kết nối với RabbitMQ
- **Message routing** và **Exception handling**
- **Test harness** để test mà không cần RabbitMQ thực tế
- **Dependency Injection** giúp việc publish/consume message dễ dàng

### ✔️ Tính năng tuyệt vời:
- Hỗ trợ nhiều service bus khác nhau (Azure, Amazon, ActiveMQ)
- **Dễ dàng chuyển sang các bus khác** mà không cần thay đổi quá nhiều code

---

## 🛠 Cài đặt MassTransit

Cài đặt **MassTransit.RabbitMQ** vào cả `AuctionService` và `SearchService`:

```bash
dotnet add package MassTransit.RabbitMQ
```

### 🚀 Cấu hình MassTransit trong `Program.cs`

```csharp
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});
```

> Lưu ý: Kết nối đến RabbitMQ qua `localhost` và không cần authentication.

---

## 📁 Tạo thư viện `Contracts` (Event Objects dùng chung giữa các service)

**Tạo thư viện contracts** để chứa các đối tượng chung (event/message objects) giữa các service:

### 🌱 Tạo class library:

```bash
dotnet new classlib -o src/Contracts
dotnet sln add src/Contracts/Contracts.csproj
```

### ➕ Thêm reference vào `AuctionService` và `SearchService`:

```bash
cd src/AuctionService
dotnet add reference ../Contracts/Contracts.csproj

cd ../SearchService
dotnet add reference ../Contracts/Contracts.csproj
```

> 🧠 Đây là cách **chia sẻ contract** giữa các service mà không phải sao chép code nhiều lần.

---

## ⚠️ Tắt cảnh báo nullable trong `Contracts`

Mở `Contracts.csproj` và thêm:

```xml
<Nullable>disable</Nullable>
```

---

## ✅ Kết luận

- **MassTransit** giúp bạn **quản lý message dễ dàng** và không cần phải trực tiếp làm việc với RabbitMQ.
- **Contracts** giúp các service dùng chung các **event object** mà không bị trùng lặp code.
- Cấu hình và cài đặt đơn giản, giúp mở rộng dễ dàng.

📌 **Tiếp theo**: Tạo các contract như `AuctionCreated` và tích hợp MassTransit vào các dịch vụ.