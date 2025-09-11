-- 1. Bảng QuyenHan (Roles)
-- Lưu trữ các quyền hạn của người dùng (Người đọc, Tác giả, Quản trị)
CREATE TABLE QuyenHan (
    IDQuyenHan INT PRIMARY KEY IDENTITY(1,1),
    TenQuyenHan NVARCHAR(50) NOT NULL UNIQUE,
    MoTa NVARCHAR(255)
);
GO

-- 2. Bảng NguoiDung (Users)
-- Lưu trữ thông tin tài khoản người dùng
CREATE TABLE NguoiDung (
    IDNguoiDung INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    SoDienThoai VARCHAR(20) UNIQUE,
    MatKhauHash VARCHAR(255) NOT NULL, -- Luôn mã hóa mật khẩu trước khi lưu
    URLAnhDaiDien VARCHAR(255),
    NgayDangKy DATETIME DEFAULT GETDATE(),
    LanDangNhapCuoi DATETIME,
    DaKichHoat BIT DEFAULT 1, -- Để khóa/mở tài khoản
    IDQuyenHan INT NOT NULL,
    
    CONSTRAINT FK_NguoiDung_QuyenHan FOREIGN KEY (IDQuyenHan) REFERENCES QuyenHan(IDQuyenHan)
);
GO

-- 3. Bảng ChuDe (Topics)
-- Quản lý các chủ đề/danh mục tin tức
CREATE TABLE ChuDe (
    IDChuDe INT PRIMARY KEY IDENTITY(1,1),
    TenChuDe NVARCHAR(100) NOT NULL UNIQUE,
    Slug VARCHAR(120) NOT NULL UNIQUE, -- Dùng cho URL thân thiện, ví dụ: "the-gioi"
    ThuTuHienThi INT DEFAULT 0, -- Để sắp xếp thứ tự hiển thị trên header/footer
    DaKichHoat BIT DEFAULT 1
);
GO

-- 4. Bảng TrangThaiBaiViet (ArticleStatus)
-- Quản lý các trạng thái trong quy trình đăng bài
CREATE TABLE TrangThaiBaiViet (
    IDTrangThai INT PRIMARY KEY IDENTITY(1,1),
    TenTrangThai NVARCHAR(50) NOT NULL UNIQUE, -- Ví dụ: Bản nháp, Chờ duyệt, Đã xuất bản, Bị từ chối
    MoTa NVARCHAR(255)
);
GO

-- 5. Bảng BaiViet (Articles)
-- Bảng trung tâm, lưu trữ nội dung các bài viết
CREATE TABLE BaiViet (
    IDBaiViet INT PRIMARY KEY IDENTITY(1,1),
    TieuDe NVARCHAR(255) NOT NULL,
    Slug VARCHAR(280) NOT NULL UNIQUE, -- URL thân thiện cho bài viết
    TomTat NVARCHAR(500), -- Đoạn tóm tắt (200-300 từ) hiển thị ở trang chủ/danh sách
    NoiDung NVARCHAR(MAX) NOT NULL, -- Nội dung chi tiết của bài viết (hỗ trợ HTML)
    URLAnhBia VARCHAR(255), -- Ảnh đại diện bài viết
    
    IDTacGia INT NOT NULL,
    IDChuDe INT NOT NULL,
    IDTrangThai INT NOT NULL,
    
    NgayTao DATETIME DEFAULT GETDATE(),
    NgayXuatBan DATETIME, -- Ngày được duyệt và xuất bản
    NgayChinhSuaCuoi DATETIME,
    
    LaTinNong BIT DEFAULT 0, -- Gán nhãn HOT
    LuotXem INT DEFAULT 0,

    IDNguoiDuyet INT, -- ID của quản trị viên đã duyệt bài
    GhiChuDuyet NVARCHAR(500), -- Ghi chú khi từ chối bài viết
    
    CONSTRAINT FK_BaiViet_NguoiDung_TacGia FOREIGN KEY (IDTacGia) REFERENCES NguoiDung(IDNguoiDung),
    CONSTRAINT FK_BaiViet_ChuDe FOREIGN KEY (IDChuDe) REFERENCES ChuDe(IDChuDe),
    CONSTRAINT FK_BaiViet_TrangThaiBaiViet FOREIGN KEY (IDTrangThai) REFERENCES TrangThaiBaiViet(IDTrangThai),
    CONSTRAINT FK_BaiViet_NguoiDung_NguoiDuyet FOREIGN KEY (IDNguoiDuyet) REFERENCES NguoiDung(IDNguoiDung)
);
GO

-- 6. Bảng BinhLuan (Comments)
-- Lưu trữ các bình luận của người đọc về bài viết
CREATE TABLE BinhLuan (
    IDBinhLuan INT PRIMARY KEY IDENTITY(1,1),
    IDBaiViet INT NOT NULL,
    IDNguoiDung INT NOT NULL,
    IDBinhLuanCha INT, -- Để làm bình luận trả lời (nested comments)
    NoiDung NVARCHAR(1000) NOT NULL,
    NgayBinhLuan DATETIME DEFAULT GETDATE(),
    DaDuyet BIT DEFAULT 1, -- Có thể thêm chức năng duyệt bình luận
    
    CONSTRAINT FK_BinhLuan_BaiViet FOREIGN KEY (IDBaiViet) REFERENCES BaiViet(IDBaiViet) ON DELETE CASCADE, -- Xóa bài viết thì xóa luôn bình luận
    CONSTRAINT FK_BinhLuan_NguoiDung FOREIGN KEY (IDNguoiDung) REFERENCES NguoiDung(IDNguoiDung),
    CONSTRAINT FK_BinhLuan_BinhLuanCha FOREIGN KEY (IDBinhLuanCha) REFERENCES BinhLuan(IDBinhLuan)
);
GO


-- Script để thêm dữ liệu mẫu cho WebTinTuc
-- Chạy script này sau khi đã tạo database

-- 1. Thêm dữ liệu mẫu cho bảng QuyenHan
INSERT INTO QuyenHan (TenQuyenHan, MoTa) VALUES
(N'Người đọc', N'Người dùng có thể đọc tin tức và bình luận'),
(N'Tác giả', N'Người dùng có thể viết bài và quản lý bài viết của mình'),
(N'Quản trị', N'Người dùng có quyền quản lý toàn bộ hệ thống');

-- 2. Thêm dữ liệu mẫu cho bảng ChuDe
INSERT INTO ChuDe (TenChuDe, Slug, ThuTuHienThi, DaKichHoat) VALUES
(N'Thời sự', 'thoi-su', 1, 1),
(N'Thế giới', 'the-gioi', 2, 1),
(N'Kinh doanh', 'kinh-doanh', 3, 1),
(N'Giải trí', 'giai-tri', 4, 1),
(N'Thể thao', 'the-thao', 5, 1),
(N'Sức khỏe', 'suc-khoe', 6, 1),
(N'Công nghệ', 'cong-nghe', 7, 1),
(N'Giáo dục', 'giao-duc', 8, 1);

-- 3. Thêm dữ liệu mẫu cho bảng TrangThaiBaiViet
INSERT INTO TrangThaiBaiViet (TenTrangThai, MoTa) VALUES
(N'Bản nháp', N'Bài viết đang được soạn thảo'),
(N'Chờ duyệt', N'Bài viết đã gửi và đang chờ quản trị viên duyệt'),
(N'Đã xuất bản', N'Bài viết đã được duyệt và xuất bản'),
(N'Bị từ chối', N'Bài viết bị từ chối và cần chỉnh sửa');

-- 4. Thêm dữ liệu mẫu cho bảng NguoiDung
-- Mật khẩu mặc định cho tất cả user là: 123456
INSERT INTO NguoiDung (HoTen, Email, SoDienThoai, MatKhauHash, NgayDangKy, DaKichHoat, IDQuyenHan) VALUES
(N'Nguyễn Văn Admin', 'admin@webtintuc.com', '0123456789', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', '2025-01-01 00:00:00', 1, 3),
(N'Trần Thị Tác Giả', 'tacgia@webtintuc.com', '0987654321', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', '2025-01-01 00:00:00', 1, 2),
(N'Lê Văn Đọc Giả', 'docgia@webtintuc.com', '0369852147', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', '2025-01-01 00:00:00', 1, 1);

-- 5. Thêm dữ liệu mẫu cho bảng BaiViet
INSERT INTO BaiViet (TieuDe, Slug, TomTat, NoiDung, IDTacGia, IDChuDe, IDTrangThai, NgayTao, NgayXuatBan, LaTinNong, LuotXem) VALUES
(N'Tin tức công nghệ mới nhất năm 2025', 'tin-tuc-cong-nghe-moi-nhat-nam-2025', N'Những xu hướng công nghệ đáng chú ý trong năm 2025 sẽ thay đổi cách chúng ta sống và làm việc.', N'<h2>Xu hướng công nghệ 2025</h2><p>Năm 2025 hứa hẹn mang đến nhiều đột phá công nghệ...</p><p>Trí tuệ nhân tạo sẽ tiếp tục phát triển mạnh mẽ...</p>', 2, 7, 3, '2025-01-15 10:00:00', '2025-01-15 10:30:00', 1, 1250),
(N'Kinh tế Việt Nam tăng trưởng tích cực', 'kinh-te-viet-nam-tang-truong-tich-cuc', N'GDP Việt Nam trong quý 1/2025 tăng trưởng 7.2%, vượt kỳ vọng của các chuyên gia.', N'<h2>Tăng trưởng kinh tế quý 1/2025</h2><p>Bộ Kế hoạch và Đầu tư công bố...</p><p>Các ngành kinh tế chủ lực đều có tăng trưởng tích cực...</p>', 2, 3, 3, '2025-01-14 14:00:00', '2025-01-14 14:15:00', 0, 890),
(N'Giải trí: Phim Việt Nam thu hút khán giả', 'giai-tri-phim-viet-nam-thu-hut-khan-gia', N'Những bộ phim Việt Nam mới ra mắt đang thu hút sự quan tâm lớn từ khán giả.', N'<h2>Điện ảnh Việt Nam 2025</h2><p>Năm 2025 đánh dấu sự phát triển mạnh mẽ...</p><p>Các tác phẩm điện ảnh Việt Nam ngày càng chất lượng...</p>', 2, 4, 3, '2025-01-13 16:00:00', '2025-01-13 16:20:00', 0, 567),
(N'Thể thao: Đội tuyển Việt Nam chuẩn bị cho giải đấu', 'the-thao-doi-tuyen-viet-nam-chuan-bi-cho-giai-dau', N'Đội tuyển bóng đá Việt Nam đang tích cực chuẩn bị cho giải đấu sắp tới.', N'<h2>Chuẩn bị cho giải đấu</h2><p>HLV Park Hang-seo đã công bố danh sách...</p><p>Các cầu thủ đang trong giai đoạn tập luyện cường độ cao...</p>', 2, 5, 3, '2025-01-12 09:00:00', '2025-01-12 09:30:00', 0, 445),
(N'Sức khỏe: Cách phòng chống bệnh mùa đông', 'suc-khoe-cach-phong-chong-benh-mua-dong', N'Những lời khuyên hữu ích để bảo vệ sức khỏe trong mùa đông lạnh giá.', N'<h2>Bảo vệ sức khỏe mùa đông</h2><p>Mùa đông là thời điểm dễ mắc các bệnh...</p><p>Dinh dưỡng hợp lý và tập thể dục đều đặn...</p>', 2, 6, 3, '2025-01-11 11:00:00', '2025-01-11 11:15:00', 0, 334);

-- 6. Thêm dữ liệu mẫu cho bảng BinhLuan
INSERT INTO BinhLuan (IDBaiViet, IDNguoiDung, NoiDung, NgayBinhLuan, DaDuyet) VALUES
(1, 3, N'Bài viết rất hay và bổ ích!', '2025-01-15 11:00:00', 1),
(1, 1, N'Cảm ơn tác giả đã chia sẻ thông tin hữu ích.', '2025-01-15 12:00:00', 1),
(2, 3, N'Tin tức kinh tế rất tích cực!', '2025-01-14 15:00:00', 1),
(3, 3, N'Phim Việt Nam ngày càng hay!', '2025-01-13 17:00:00', 1);