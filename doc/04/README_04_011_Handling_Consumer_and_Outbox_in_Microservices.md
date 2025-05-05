
# README.md: Xử Lý Consumer và Outbox cho Auction Service

## Tổng Quan

Trong bài học này, chúng ta sẽ xử lý các consumer cho các sự kiện **Auction Updated** và **Auction Deleted**. Các consumer này sẽ giúp chúng ta cập nhật và xóa đấu giá trong cơ sở dữ liệu MongoDB của dịch vụ **search** sau khi các thay đổi được thực hiện trong cơ sở dữ liệu Postgres của dịch vụ **auction**. Bên cạnh đó, chúng ta sẽ sử dụng **Outbox** để đảm bảo tính nhất quán của dữ liệu giữa các dịch vụ, đồng thời xử lý lỗi và các trường hợp ngoại lệ.

## Mục Tiêu

- Cấu hình consumer cho sự kiện **Auction Updated** và **Auction Deleted**.
- Sử dụng **Outbox** để lưu trữ tin nhắn khi bus dịch vụ không khả dụng.
- Đảm bảo dữ liệu giữa các dịch vụ **auction** và **search** luôn nhất quán.
- Cấu hình retry cho các consumer và xử lý lỗi khi có sự cố.

## Các Tính Năng Cần Xử Lý

### 1. **Auction Updated Consumer** (Consumer cho Cập Nhật Đấu Giá)

Consumer này sẽ xử lý sự kiện khi một đấu giá bị cập nhật trong dịch vụ **auction**. Sau khi cập nhật, tin nhắn sẽ được gửi lên service bus và dịch vụ **search** sẽ nhận và cập nhật dữ liệu.

**Consumer Mẫu:**
```csharp
public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;
    private readonly IAuctionRepository _auctionRepository;

    public AuctionUpdatedConsumer(IMapper mapper, IAuctionRepository auctionRepository)
    {
        _mapper = mapper;
        _auctionRepository = auctionRepository;
    }

    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        var updatedAuction = context.Message;
        var auctionEntity = await _auctionRepository.GetByIdAsync(updatedAuction.Id);

        if (auctionEntity != null)
        {
            // Cập nhật thông tin đấu giá
            _mapper.Map(updatedAuction, auctionEntity);
            await _auctionRepository.UpdateAsync(auctionEntity);
        }
    }
}
```

### 2. **Auction Deleted Consumer** (Consumer cho Xóa Đấu Giá)

Consumer này sẽ xử lý sự kiện khi một đấu giá bị xóa. Sau khi xóa, tin nhắn sẽ được gửi lên bus dịch vụ và dịch vụ **search** sẽ xóa dữ liệu tương ứng trong MongoDB.

**Consumer Mẫu:**
```csharp
public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    private readonly IAuctionRepository _auctionRepository;

    public AuctionDeletedConsumer(IAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        var auctionId = context.Message.Id;
        await _auctionRepository.DeleteAsync(auctionId);
    }
}
```

### 3. **Cấu Hình Consumer trong Auction Service**

Đăng ký các consumer trong chương trình `auction` trong `Program.cs`.

**Cấu Hình Đăng Ký Consumer:**
```csharp
services.AddMassTransit(x =>
{
    x.AddConsumer<AuctionUpdatedConsumer>();
    x.AddConsumer<AuctionDeletedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint("auction-updated", e =>
        {
            e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("auction-deleted", e =>
        {
            e.ConfigureConsumer<AuctionDeletedConsumer>(context);
        });
    });
});
```

### 4. **Sử Dụng Outbox để Đảm Bảo Tính Nhất Quán Dữ Liệu**

Để đảm bảo tính nhất quán khi bus dịch vụ không khả dụng, chúng ta sẽ sử dụng **Outbox** để lưu trữ các tin nhắn chưa gửi được và gửi lại chúng khi bus dịch vụ có sẵn.

**Cấu Hình Outbox trong Auction Service:**
```csharp
services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.AddEntityFrameworkOutbox(options =>
        {
            options.UsePostgres(context.GetService<DbContext>());
            options.QueryDelay = TimeSpan.FromSeconds(10); // Cấu hình khoảng thời gian kiểm tra outbox
        });
    });
});
```

### 5. **Kiểm Tra Các Consumer**

Sau khi cấu hình các consumer, bạn có thể kiểm tra chức năng của chúng bằng cách sử dụng **Postman** hoặc công cụ kiểm tra API khác.

- **Tạo Đấu Giá Mới**: Gửi yêu cầu tạo đấu giá mới và kiểm tra xem dữ liệu đã được cập nhật trong cơ sở dữ liệu **search**.
- **Cập Nhật Đấu Giá**: Gửi yêu cầu cập nhật đấu giá và xác minh rằng cơ sở dữ liệu **search** đã được cập nhật.
- **Xóa Đấu Giá**: Gửi yêu cầu xóa đấu giá và kiểm tra cơ sở dữ liệu **search** để chắc chắn rằng đấu giá đã bị xóa.

**Test Mẫu trong Postman:**
- Tạo Đấu Giá: `POST /api/auctions`
- Cập Nhật Đấu Giá: `PUT /api/auctions/{id}`
- Xóa Đấu Giá: `DELETE /api/auctions/{id}`

### 6. **Lỗi và Xử Lý**

Khi có lỗi xảy ra trong quá trình tiêu thụ tin nhắn, tin nhắn sẽ được đưa vào **fault queue** (hàng đợi lỗi). Bạn có thể sử dụng các consumer để xử lý lỗi này và thực hiện hành động khắc phục.

### 7. **Đảm Bảo Tính Nhất Quán Dữ Liệu**

Sau khi cấu hình các consumer và outbox, bạn cần đảm bảo rằng các thao tác như tạo, cập nhật và xóa đấu giá đều được thực hiện đúng và dữ liệu giữa các dịch vụ là nhất quán.

### 8. **Lệnh Tạo Migration Cho Outbox**
Sau khi thêm cấu hình outbox vào `DbContext`, hãy tạo migration để thêm các bảng `outbox` vào cơ sở dữ liệu:
```bash
dotnet ef migrations add AddOutboxTables
dotnet ef database update
```

## Kết Luận

Trong phần này, chúng ta đã cấu hình các consumer cho các sự kiện **Auction Updated** và **Auction Deleted**, giúp dữ liệu trong dịch vụ `auction` và `search` luôn nhất quán. Chúng ta cũng đã sử dụng outbox để đảm bảo rằng nếu bus dịch vụ bị gián đoạn, các tin nhắn vẫn được lưu trữ và gửi lại khi có thể. Các chính sách thử lại và xử lý lỗi cũng được cấu hình để đảm bảo tính ổn định của hệ thống.

Nếu gặp khó khăn trong quá trình triển khai, đừng ngần ngại tham khảo tài liệu hoặc sử dụng các công cụ debug để kiểm tra hành vi của các consumer và các tin nhắn.

---

Hy vọng tài liệu này sẽ giúp bạn hoàn thành thách thức và đạt được mục tiêu trong quá trình học!
