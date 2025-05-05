# 📦 Cài đặt MassTransit & Tạo Thư viện Contract chia sẻ

## 🎯 Mục tiêu

- Cài đặt thư viện **MassTransit** vào các dịch vụ `.NET`
- Cấu hình kết nối RabbitMQ
- Tạo thư viện chia sẻ các **contract** (event/message objects) giữa các service

---

## 🧩 MassTransit là gì?

MassTransit giống như **Entity Framework cho message broker**.  
Nó giúp bạn **không cần làm việc trực tiếp với RabbitMQ API**.

✔️ **Tính năng nổi bật:**

- 🧭 Routing message
- 🛡 Exception handling
- 🧪 Test harness (test mà không cần RabbitMQ thật)
- 💉 Tích hợp với Dependency Injection
- 🔁 Dễ dàng chuyển sang các hệ thống khác như: Azure Service Bus, Amazon SQS, ActiveMQ

---

## 🛠 Cài đặt MassTransit

Cài vào từng service (VD: `AuctionService`, `SearchService`):

```bash
dotnet add package MassTransit.RabbitMQ
```

---

## ⚙️ Cấu hình MassTransit trong Program.cs

Thêm vào file `Program.cs` trong mỗi service:

```csharp
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});
```

✔️ Mặc định sẽ kết nối `localhost`  
✔️ Không cần user/pass nếu dùng cấu hình mặc định RabbitMQ

---

## 📁 Tạo thư viện `Contracts` (chia sẻ model giữa các service)

Mỗi service sẽ cần dùng chung một số object (VD: `AuctionCreated`)

### 🔄 2 cách chia sẻ:

| Cách | Ưu | Nhược |
|------|----|-------|
| Copy-paste mỗi service | Độc lập hoàn toàn | Tốn công, dễ sai lệch |
| 📦 Dùng class library chung | Dễ tái sử dụng | Cần tham chiếu (reference) |

### ✅ Chọn cách 2 – Tạo thư viện Contracts:

```bash
dotnet new classlib -o src/Contracts
dotnet sln add src/Contracts/Contracts.csproj
```

### ➕ Thêm reference trong mỗi service:

```bash
cd src/AuctionService
dotnet add reference ../Contracts/Contracts.csproj

cd ../SearchService
dotnet add reference ../Contracts/Contracts.csproj
```

> 🧠 Đây không biến hệ thống thành Monolith – chỉ là **cách chia sẻ model giữa các service** để tránh trùng lặp.

---

## 🧹 Tắt nullable warnings trong Contracts

Mở `Contracts.csproj`, thêm:

```xml
<Nullable>disable</Nullable>
```

---

## ✅ Kết luận

- MassTransit giúp publish/consume message đơn giản và sạch
- Cấu hình chỉ vài dòng
- Dùng thư viện Contracts để chia sẻ event object giữa các service

📌 **Tiếp theo**: Tạo các contract như `AuctionCreated` và tích hợp thực tế với MassTransit.