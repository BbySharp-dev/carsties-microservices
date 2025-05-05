# 📦 Publish Sự kiện từ AuctionService và Tiêu thụ từ SearchService

## 🎯 Mục tiêu

- **Publish** sự kiện từ `AuctionService` lên RabbitMQ khi tạo phiên đấu giá mới
- **SearchService** sẽ tiêu thụ sự kiện và cập nhật cơ sở dữ liệu

---

## 🧑‍💻 Publishing Message trong AuctionService

### 💡 Các bước:

1. **Tạo Auction** trong controller
2. **Map** AuctionDTO thành `AuctionCreated` event
3. **Publish** sự kiện `AuctionCreated` lên RabbitMQ

### 📝 Code ví dụ trong `AuctionsController`:

```csharp
public class AuctionsController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;

    public AuctionsController(IPublishEndpoint publishEndpoint, IMapper mapper)
    {
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
    }

    public async Task<IActionResult> Create(AuctionDto auctionDto)
    {
        var newAuction = _mapper.Map<Auction>(auctionDto);
        // Save to DB first to get the ID
        await _auctionService.SaveAuctionAsync(newAuction);

        var auctionCreatedEvent = _mapper.Map<AuctionCreated>(newAuction);
        await _publishEndpoint.Publish(auctionCreatedEvent);

        return CreatedAtAction("Get", new { id = newAuction.Id }, newAuction);
    }
}
```

---

## ⚙️ Cấu hình AutoMapper trong `AuctionService`

### 🌱 Map từ `AuctionDTO` thành `AuctionCreated` event:

```csharp
public class AuctionProfile : Profile
{
    public AuctionProfile()
    {
        CreateMap<AuctionDto, AuctionCreated>();
    }
}
```

---

## 🛠 Kiểm tra hoạt động

1. **Sử dụng Postman** để gửi yêu cầu tạo phiên đấu giá
2. **Kiểm tra RabbitMQ**: 
    - Kiểm tra xem có message nào trong `search-auction-created` queue không
    - Kiểm tra hoạt động trong Exchange

---

## ✅ Kết luận

- **AuctionService** đã thành công trong việc **publish sự kiện** lên RabbitMQ khi tạo phiên đấu giá
- **SearchService** đã tiêu thụ sự kiện và cập nhật cơ sở dữ liệu
- Cấu hình **AutoMapper** giúp dễ dàng chuyển đổi dữ liệu giữa các lớp

📌 **Tiếp theo**: Xử lý **Unhappy Path** khi dịch vụ hoặc kết nối không thành công.