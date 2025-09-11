-- Script để thêm dữ liệu mẫu cho WebTinTuc
-- Chạy script này sau khi đã tạo database

-- 1. Thêm dữ liệu mẫu cho bảng QuyenHan
INSERT INTO QuyenHan (TenQuyenHan, MoTa) VALUES
('Người đọc', 'Người dùng có thể đọc tin tức và bình luận'),
('Tác giả', 'Người dùng có thể viết bài và quản lý bài viết của mình'),
('Quản trị', 'Người dùng có quyền quản lý toàn bộ hệ thống');

-- 2. Thêm dữ liệu mẫu cho bảng ChuDe
INSERT INTO ChuDe (TenChuDe, Slug, ThuTuHienThi, DaKichHoat) VALUES
('Thời sự', 'thoi-su', 1, 1),
('Thế giới', 'the-gioi', 2, 1),
('Kinh doanh', 'kinh-doanh', 3, 1),
('Giải trí', 'giai-tri', 4, 1),
('Thể thao', 'the-thao', 5, 1),
('Sức khỏe', 'suc-khoe', 6, 1),
('Công nghệ', 'cong-nghe', 7, 1),
('Giáo dục', 'giao-duc', 8, 1);

-- 3. Thêm dữ liệu mẫu cho bảng TrangThaiBaiViet
INSERT INTO TrangThaiBaiViet (TenTrangThai, MoTa) VALUES
('Bản nháp', 'Bài viết đang được soạn thảo'),
('Chờ duyệt', 'Bài viết đã gửi và đang chờ quản trị viên duyệt'),
('Đã xuất bản', 'Bài viết đã được duyệt và xuất bản'),
('Bị từ chối', 'Bài viết bị từ chối và cần chỉnh sửa');

-- 4. Thêm dữ liệu mẫu cho bảng NguoiDung
-- Mật khẩu mặc định cho tất cả user là: 123456
INSERT INTO NguoiDung (HoTen, Email, SoDienThoai, MatKhauHash, NgayDangKy, DaKichHoat, IDQuyenHan) VALUES
('Nguyễn Văn Admin', 'admin@webtintuc.com', '0123456789', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMzjrcBiJI=', '2025-01-01 00:00:00', 1, 3),
('Trần Thị Tác Giả', 'tacgia@webtintuc.com', '0987654321', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMzjrcBiJI=', '2025-01-01 00:00:00', 1, 2),
('Lê Văn Đọc Giả', 'docgia@webtintuc.com', '0369852147', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMzjrcBiJI=', '2025-01-01 00:00:00', 1, 1);

-- 5. Thêm dữ liệu mẫu cho bảng BaiViet
INSERT INTO BaiViet (TieuDe, Slug, TomTat, NoiDung, IDTacGia, IDChuDe, IDTrangThai, NgayTao, NgayXuatBan, LaTinNong, LuotXem) VALUES
('Tin tức công nghệ mới nhất năm 2025', 'tin-tuc-cong-nghe-moi-nhat-nam-2025', 'Những xu hướng công nghệ đáng chú ý trong năm 2025 sẽ thay đổi cách chúng ta sống và làm việc.', '<h2>Xu hướng công nghệ 2025</h2><p>Năm 2025 hứa hẹn mang đến nhiều đột phá công nghệ...</p><p>Trí tuệ nhân tạo sẽ tiếp tục phát triển mạnh mẽ...</p>', 2, 7, 3, '2025-01-15 10:00:00', '2025-01-15 10:30:00', 1, 1250),
('Kinh tế Việt Nam tăng trưởng tích cực', 'kinh-te-viet-nam-tang-truong-tich-cuc', 'GDP Việt Nam trong quý 1/2025 tăng trưởng 7.2%, vượt kỳ vọng của các chuyên gia.', '<h2>Tăng trưởng kinh tế quý 1/2025</h2><p>Bộ Kế hoạch và Đầu tư công bố...</p><p>Các ngành kinh tế chủ lực đều có tăng trưởng tích cực...</p>', 2, 3, 3, '2025-01-14 14:00:00', '2025-01-14 14:15:00', 0, 890),
('Giải trí: Phim Việt Nam thu hút khán giả', 'giai-tri-phim-viet-nam-thu-hut-khan-gia', 'Những bộ phim Việt Nam mới ra mắt đang thu hút sự quan tâm lớn từ khán giả.', '<h2>Điện ảnh Việt Nam 2025</h2><p>Năm 2025 đánh dấu sự phát triển mạnh mẽ...</p><p>Các tác phẩm điện ảnh Việt Nam ngày càng chất lượng...</p>', 2, 4, 3, '2025-01-13 16:00:00', '2025-01-13 16:20:00', 0, 567),
('Thể thao: Đội tuyển Việt Nam chuẩn bị cho giải đấu', 'the-thao-doi-tuyen-viet-nam-chuan-bi-cho-giai-dau', 'Đội tuyển bóng đá Việt Nam đang tích cực chuẩn bị cho giải đấu sắp tới.', '<h2>Chuẩn bị cho giải đấu</h2><p>HLV Park Hang-seo đã công bố danh sách...</p><p>Các cầu thủ đang trong giai đoạn tập luyện cường độ cao...</p>', 2, 5, 3, '2025-01-12 09:00:00', '2025-01-12 09:30:00', 0, 445),
('Sức khỏe: Cách phòng chống bệnh mùa đông', 'suc-khoe-cach-phong-chong-benh-mua-dong', 'Những lời khuyên hữu ích để bảo vệ sức khỏe trong mùa đông lạnh giá.', '<h2>Bảo vệ sức khỏe mùa đông</h2><p>Mùa đông là thời điểm dễ mắc các bệnh...</p><p>Dinh dưỡng hợp lý và tập thể dục đều đặn...</p>', 2, 6, 3, '2025-01-11 11:00:00', '2025-01-11 11:15:00', 0, 334);

-- 6. Thêm dữ liệu mẫu cho bảng BinhLuan
INSERT INTO BinhLuan (IDBaiViet, IDNguoiDung, NoiDung, NgayBinhLuan, DaDuyet) VALUES
(1, 3, 'Bài viết rất hay và bổ ích!', '2025-01-15 11:00:00', 1),
(1, 1, 'Cảm ơn tác giả đã chia sẻ thông tin hữu ích.', '2025-01-15 12:00:00', 1),
(2, 3, 'Tin tức kinh tế rất tích cực!', '2025-01-14 15:00:00', 1),
(3, 3, 'Phim Việt Nam ngày càng hay!', '2025-01-13 17:00:00', 1);

-- Kiểm tra dữ liệu đã được thêm
SELECT 'QuyenHan' as TableName, COUNT(*) as RecordCount FROM QuyenHan
UNION ALL
SELECT 'ChuDe', COUNT(*) FROM ChuDe
UNION ALL
SELECT 'TrangThaiBaiViet', COUNT(*) FROM TrangThaiBaiViet
UNION ALL
SELECT 'NguoiDung', COUNT(*) FROM NguoiDung
UNION ALL
SELECT 'BaiViet', COUNT(*) FROM BaiViet
UNION ALL
SELECT 'BinhLuan', COUNT(*) FROM BinhLuan;
