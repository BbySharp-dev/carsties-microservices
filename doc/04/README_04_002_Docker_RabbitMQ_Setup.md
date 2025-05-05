# 🚀 Khởi tạo RabbitMQ với Docker Compose & Kết nối bằng MassTransit

## 🎯 Mục tiêu

Chúng ta sẽ:
- Khởi tạo dịch vụ **RabbitMQ** bằng Docker
- Truy cập giao diện quản trị RabbitMQ
- Hiểu cách **MassTransit tạo Exchange & Queue** theo Convention

---

## 🧱 Cài RabbitMQ bằng Docker Compose

Thêm vào `docker-compose.yml`:

```yaml
services:
  rabbitmq:
    container_name: rabbitmq
    image: rabbitmq:3-management-alpine
    ports:
      - "5672:5672"      # Port giao tiếp giữa các service
      - "15672:15672"    # Port quản trị giao diện Web
```

📌 Mặc định, RabbitMQ dùng user `guest`/`guest` để đăng nhập.

---

## 🚀 Khởi động dịch vụ

Chạy lệnh trong terminal:

```bash
docker compose up -d
```

- `-d`: chạy ở chế độ nền
- Sau khi chạy, truy cập trình duyệt tại: [http://localhost:15672](http://localhost:15672)
- Đăng nhập bằng:
  - **Username**: `guest`
  - **Password**: `guest`

---

## 📊 Giao diện quản lý RabbitMQ

Khi truy cập thành công, bạn sẽ thấy các tab sau:

| Tab         | Mô tả |
|-------------|-------|
| **Overview**   | Tổng quan message broker |
| **Connections** | Kết nối hiện có |
| **Channels**    | Kênh dữ liệu giữa producer – consumer |
| **Exchanges**   | Nơi nhận message từ producer |
| **Queues**      | Nơi lưu trữ chờ consumer lấy message |
| **Admin**       | Quản lý user, quyền hạn (ít dùng trong demo) |

---

## ⚙️ Tích hợp MassTransit

Chúng ta không tạo queue/exchange thủ công. Thay vào đó:

- Khi tạo 1 class (gọi là **Contract**), MassTransit sẽ tự tạo **Exchange**
- Khi tạo 1 **Consumer**, MassTransit sẽ tự tạo **Queue**
- Tất cả làm theo convention → **Đơn giản hoá cấu hình**

### 🌐 Quy trình minh họa:

1. Producer tạo 1 class `AuctionCreated`
2. MassTransit tạo exchange với tên `AuctionCreated`
3. Consumer đăng ký xử lý `AuctionCreated`
4. MassTransit tạo queue và kết nối queue → exchange

---

## 📌 Kết luận

- RabbitMQ được khởi chạy dễ dàng bằng Docker Compose
- Giao diện Web tại `http://localhost:15672`
- MassTransit giúp tự động hóa tạo exchange/queue
- Không cần cấu hình thủ công bên RabbitMQ

🔜 **Tiếp theo**: Tạo contract và cấu hình MassTransit trong code để publish/consume message.