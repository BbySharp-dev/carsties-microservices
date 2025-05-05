# 📦 RabbitMQ & MassTransit trong Microservices (.NET)

## 🧠 Khái niệm chính

Trong hệ thống microservices, **dịch vụ cần giao tiếp với nhau** mà không bị ràng buộc chặt chẽ (loosely coupled). Để làm điều đó, ta dùng **message broker** — và ở đây là **RabbitMQ**.

## 🕊 RabbitMQ là gì?

RabbitMQ là một **message broker** – nó **nhận và chuyển tiếp các message**.

- Message được gửi đến một **exchange**
- Sau đó được **chuyển tới một hoặc nhiều queue**
- Các **service (consumer)** sẽ đăng ký nhận message từ các queue này

### ✳️ Mô hình sử dụng:
- **Producer** gửi message
- **Consumer** nhận và xử lý message
- Gọi là mô hình **Producer/Consumer** hoặc **Publish/Subscribe**

## 🧾 Tính năng của RabbitMQ:

- **Lưu trữ message (buffer)** để đảm bảo không mất mát
- **Hỗ trợ lưu trữ bền vững (persistence)**: giúp khôi phục message khi server bị crash
- Dùng giao thức **AMQP (Advanced Message Queuing Protocol)**

## 🔀 Exchange trong RabbitMQ

Exchange định tuyến message đến queue theo kiểu:

| Loại Exchange | Mô tả |
|---------------|--------|
| `Direct`      | Gửi đến queue dựa vào routing key (chỉ 1 queue) |
| `Fanout`      | Gửi đến **tất cả các queue** đã đăng ký |
| `Topic`       | Dựa vào routing key có thể gửi tới nhiều queue |
| `Header`      | Dựa trên các cặp key-value trong message header |

🔶 Trong dự án này, ta sẽ **chỉ sử dụng `Fanout Exchange`**, vì nó phù hợp với yêu cầu gửi sự kiện cho nhiều service tiêu thụ.

### 💡 Minh hoạ Fanout Exchange

```text
AuctionService --> [Exchange] --> Queue A --> NotificationService
                               --> Queue B --> SearchService
```

- AuctionService gửi sự kiện (VD: `AuctionCreated`)
- Exchange đẩy sự kiện đến nhiều queue
- Các service khác nhau xử lý theo cách riêng

## 🚉 Kết nối RabbitMQ với .NET: dùng **MassTransit**

MassTransit là một thư viện .NET giúp:

- Kết nối đến RabbitMQ dễ dàng hơn
- Trừu tượng hóa transport (nếu sau này đổi RabbitMQ thành Kafka, code không cần thay đổi nhiều)
- Cung cấp tính năng nâng cao: retry, routing, exception handling, test harness, DI,...

```csharp
services.AddMassTransit(x =>
{
    x.AddConsumer<MyConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("rabbitmq://localhost");
        cfg.ReceiveEndpoint("my-queue", e =>
        {
            e.ConfigureConsumer<MyConsumer>(ctx);
        });
    });
});
```

## 🧪 Ưu điểm của MassTransit:

- 🧪 **Test dễ dàng** (test in-memory, không cần cấu hình hạ tầng phức tạp)
- ♻️ **Dễ bảo trì**, code rõ ràng
- 🧰 **Tích hợp tốt với .NET DI container**
- 📦 Có sẵn cấu hình cho RabbitMQ, Azure Service Bus, Kafka,...

---

## ✅ Kết luận

- RabbitMQ là **trung gian** nhận message và phân phối đến các service
- Fanout exchange sẽ giúp **gửi message đến nhiều queue cùng lúc**
- MassTransit giúp ta **quản lý message dễ dàng hơn**, test được, code rõ ràng và mở rộng tốt

📍 **Bước tiếp theo:** Cài đặt RabbitMQ + cấu hình MassTransit cho dịch vụ đầu tiên.