# 📦 Tạo Contract cho các Sự kiện (Event) trong Microservices

## 🎯 Mục tiêu

- Tạo các **contract** cho sự kiện (`AuctionCreated`, `AuctionUpdated`, `AuctionDeleted`)
- Sử dụng MassTransit để **gửi và nhận sự kiện** giữa các dịch vụ

---

## 🧑‍💻 Integration Events trong Microservices

- **Integration events** giúp các dịch vụ **giao tiếp bất đồng bộ** qua service bus (như RabbitMQ)
- Sự kiện không **đảm bảo thời gian nhận** và **không đảm bảo thứ tự nhận**

### 🌟 Ví dụ: **AuctionCreated Event**

- **Mục đích**: Khi một phiên đấu giá được tạo, **AuctionService** gửi một sự kiện `AuctionCreated` lên service bus.
- **Lợi ích**: **SearchService** nhận sự kiện và cập nhật cơ sở dữ liệu để giữ cho thông tin đồng nhất với **AuctionService**.

---

## 🛠 Tạo các Contract (Event) trong `Contracts` Folder

### 📝 Event: **AuctionCreated**

Tạo một class mới gọi là `AuctionCreated` và sao chép các thuộc tính từ `AuctionDto` vào:

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

> 🧠 Lý do tạo `AuctionCreated` riêng: Để đảm bảo **namespace** giữa các dịch vụ (lỗi nếu dùng `AuctionDto` vì khác namespace).

---

### 📝 Event: **AuctionUpdated**

Tạo một class mới gọi là `AuctionUpdated`, bao gồm các thuộc tính của phiên đấu giá đã được cập nhật:

```csharp
public class AuctionUpdated
{
    public string Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }
    public int Year { get; set; }
    public int Mileage { get; set; }
}
```

---

### 📝 Event: **AuctionDeleted**

Tạo một class mới gọi là `AuctionDeleted`, chỉ cần chứa `Id` của phiên đấu giá đã bị xóa:

```csharp
public class AuctionDeleted
{
    public string Id { get; set; }
}
```

---

## ✅ Kết luận

- Đã tạo các **contract** cho sự kiện như `AuctionCreated`, `AuctionUpdated`, và `AuctionDeleted`.
- Những sự kiện này sẽ giúp các dịch vụ giao tiếp và đồng bộ dữ liệu bất đồng bộ thông qua **MassTransit**.
- **Lưu ý**: Các contract phải có **namespace giống nhau** giữa các dịch vụ để MassTransit nhận diện được.

📌 **Tiếp theo**: Thêm **Consumer** để **tiêu thụ** sự kiện `AuctionCreated` và xử lý trong `SearchService`.