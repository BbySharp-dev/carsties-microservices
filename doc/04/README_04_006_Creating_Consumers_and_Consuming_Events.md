# 📦 Tạo Consumer để Tiêu thụ Sự kiện từ RabbitMQ

## 🎯 Mục tiêu

- Tạo **Consumer** trong `SearchService` để tiêu thụ sự kiện `AuctionCreated` từ RabbitMQ
- Cấu hình MassTransit để nhận và xử lý sự kiện

---

## 🧑‍💻 Consumer trong Microservices

**Consumer** sẽ nhận sự kiện từ RabbitMQ và thực hiện hành động cần thiết. Trong ví dụ này, **SearchService** sẽ tiêu thụ sự kiện **`AuctionCreated`** và lưu thông tin vào cơ sở dữ liệu.

### 🧩 Các bước:

1. Tạo folder **`Consumers`** trong `SearchService`
2. Tạo class **`AuctionCreatedConsumer`** để xử lý sự kiện

```csharp
public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;

    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine($"Consuming auction created: {context.Message.Id}");

        var item = _mapper.Map<Item>(context.Message);
        await item.SaveAsync();
    }
}
```

---

## ⚙️ Cấu hình MassTransit để Đăng ký Consumer

### 🔧 Cấu hình trong `Program.cs`:

1. Thêm MassTransit vào `SearchService`
2. Thêm Consumer `AuctionCreatedConsumer` vào cấu hình MassTransit

```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AuctionCreatedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});
```

---

## 📁 Tạo thư viện **`Contracts`** để chia sẻ các đối tượng

- **Contract** cần thiết cho sự kiện: `AuctionCreated`, `AuctionUpdated`, `AuctionDeleted`
- Đảm bảo namespace của các contract giống nhau trên tất cả các service

```csharp
public class AuctionCreated
{
    public string Id { get; set; }
    public string Seller { get; set; }
    public int ReservePrice { get; set; }
    public DateTime AuctionEnd { get; set; }
    public bool Finished { get; set; }
}
```

---

## 🛠 Cấu hình Định dạng Tên Queue và Exchange

- **MassTransit** tự động tạo Exchange và Queue theo convention
- **Kebab case** cho tên queue và exchange
- Ví dụ: `search-auction-created`

```csharp
cfg.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
```

---

## ✅ Kết luận

- **Consumer** giúp nhận sự kiện và xử lý trong `SearchService`
- **MassTransit** giúp kết nối và xử lý sự kiện bất đồng bộ dễ dàng
- **Tạo thư viện Contracts** giúp chia sẻ các model giữa các service mà không cần sao chép mã

📌 **Tiếp theo**: Tạo và publish message từ `AuctionService` vào RabbitMQ để `SearchService` tiêu thụ.