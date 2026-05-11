<p align="center">
  <img height="150" alt="Teaching Center Logo" src="https://github.com/user-attachments/assets/3f722f89-1613-4f6d-8f70-bf888f2b1039" />
</p>

<h1 align="center">PBL3: Hệ thống Quản lý Trung tâm Dạy học (TCMS)</h1>

<p align="center">
  <strong>✨ Số hóa quy trình - Nâng tầm giáo dục ✨</strong>
</p>

<p align="center">
Quản lý toàn diện quy trình dạy học, xếp lớp, điểm danh và tài chính giữa Ban quản lý, Giáo viên và Học viên tại trung tâm.
</p>

<p align="center">
  <img alt="C#" src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" />
  <img alt=".NET WPF" src="https://img.shields.io/badge/.NET_WPF-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img alt="SQL Server" src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" />
  <img alt="Docker" src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" />
</p>

## 📸 Minh họa hệ thống (Một số chức năng nổi bật của hệ thống)
</br>
<img width="1920" height="1200" alt="z7805474846574_e340b13e874729b8143943ed78eb40c9" src="https://github.com/user-attachments/assets/a379cec0-26ad-4b85-9c56-10a94d25a7a7" />
</br>
<img width="1920" height="1200" alt="z7805474846569_111f61837a02b6b13e810acadb3d01ce" src="https://github.com/user-attachments/assets/3e0bd133-57b0-4392-89fe-93853aa18d27" />
</br>
<img width="2586" height="1480" alt="image" src="https://github.com/user-attachments/assets/dbb11618-581c-4d81-870b-6f813b56c1ca" />
</br>
<img width="1524" height="1067" alt="image" src="https://github.com/user-attachments/assets/1b3319c6-761d-44e2-affc-356eb83c9966" />
---

## 🚀 Các tính năng nổi bật (Features)

Hệ thống được thiết kế theo mô hình phân quyền chặt chẽ (Role-Based Access Control - RBAC) với các module chuyên biệt:

### 👑 Module Quản trị & Điều hành (Admin / General Panel)
* **Quản lý cốt lõi (CRUD Operations):** Cung cấp giao diện trực quan để quản lý toàn diện danh mục Tài khoản, Khóa học, Lớp học và Nhân sự.
* **Theo dõi Tài chính (Finance Management):** Số hóa quy trình thu học phí, lương giáo viên và thống kê thu nhập. Quản lý trạng thái đóng tiền của học viên và lương giáo viên theo từng tháng/năm một cách logic và minh bạch 
* **Quản lý Điểm danh (Attendance Tracking):** Giám sát tình trạng đi học của học viên theo từng ngày. Dữ liệu điểm danh được liên kết chặt chẽ với lịch học để đảm bảo tính đồng bộ dữ liệu.

### 👨‍🏫 Module Giáo Viên (Teacher Panel)
* **Quản lý Lịch dạy (Dynamic Schedule):** Tự động hiển thị các lớp đang phụ trách dựa trên tính toán thời gian thực (`GETDATE()`), loại bỏ hoàn toàn việc lưu trữ trạng thái tĩnh dư thừa.
* **Hệ thống Nhập điểm an toàn (Robust Grading System):** * Áp dụng **SQL Transaction** (`BeginTransaction`, `Commit`, `Rollback`) để đảm bảo tính toàn vẹn dữ liệu khi chấm điểm hàng loạt.
  * Tích hợp xử lý đa ngôn ngữ (`CultureInfo.InvariantCulture`), loại bỏ lỗi Crash ứng dụng khi nhập số thập phân (dấu chấm/phẩy) trên các môi trường OS khác nhau.
  * Xử lý triệt để vòng đời giao diện (WPF Lifecycle) với lệnh ép commit chỉnh sửa (`CommitEdit`) trên DataGrid.

### 👩‍🎓 Module Học Viên (Student Panel)
* **Lịch học trực quan (Visual Calendar UI):** Chuyển đổi dữ liệu bảng khô khan thành giao diện tờ lịch tháng thông minh. Xử lý thành công bài toán tịnh tiến trục thời gian (Offset mapping) giữa hệ tọa độ của SQL và C#.
* **Quản lý Học tập & Tra cứu:** * Sử dụng kỹ thuật `LEFT JOIN` và `ISNULL` để hiển thị minh bạch toàn bộ môn học (bao gồm các môn chưa có điểm).
  * **Xuất Excel tốc độ cao:** Tích hợp thư viện **ClosedXML**, cho phép xuất báo cáo điểm số từ `DataView` ra định dạng `.xlsx` cực nhanh mà không yêu cầu cài đặt Microsoft Office.

---

## 🛠️ Công nghệ sử dụng (Tech Stack)
* **Ngôn ngữ lập trình:** C#
* **Framework giao diện:** .NET WPF (Windows Presentation Foundation)
* **Cơ sở dữ liệu:** Microsoft SQL Server (T-SQL)
* **Thư viện bên thứ 3:** `ClosedXML` (Thao tác file Excel chuẩn OpenXML).
* **Kiến trúc:** Event-Driven, Data-Driven Design & 3NF Database Normalization.

---

## ⚙️ Hướng dẫn cài đặt (Installation & Setup)

**Bước 1: Clone dự án**
```bash
git clone https://github.com/lnamdeptrai392-dotcom/PBL3.git
```

**Bước 2: Thiết lập Cơ sở dữ liệu (Database Setup)**
* Mở **SQL Server Management Studio (SSMS)**.
* Chạy file script SQL đính kèm trong thư mục dự án để tạo cấu trúc bảng (Schema) và nạp dữ liệu mẫu.
* *Lưu ý: Hệ thống sử dụng logic thời gian thực, hãy đảm bảo ngày tháng trong dữ liệu mẫu phù hợp với thời điểm test.*

**Bước 3: Cấu hình chuỗi kết nối (Connection String)**
* Mở Solution bằng **Visual Studio 2022**.
* Tìm đến file `DatabaseHelper.cs`, thay đổi thuộc tính `Data Source` thành tên Server SQL trên máy của bạn.

**Bước 4: Cài đặt thư viện**
* Chuột phải vào Solution -> Chọn **Restore NuGet Packages**.
* Đảm bảo gói `ClosedXML` đã được cài đặt thành công.

**Bước 5: Khởi chạy (Run)**
* Nhấn `F5` hoặc chọn `Start` để chạy ứng dụng.

---

## 🔑 Tài khoản Test

| Vai trò | ID (Tên đăng nhập) | Mật khẩu |
| :--- | :--- | :--- |
| **Admin chung** | `adminc` | `123456` |
| **Admin tài chính** | `admintc` | `123456` |
| **Giáo viên** | `T01` | `123456` |
| **Học viên** | `10110001` | `123456` |

---

## Tác giả (Contributors)

* **Sinh viên thực hiện:**
    * Lê Xuân Nam
    * Phạm Thị Ngọc Khuê
    * Nguyễn Phương Uyên
    * Nguyễn Quang Trường Duy
* **Học phần:** Đồ án PBL3
* **Khoa:** Công nghệ Thông tin - Trường Đại học Bách khoa - Đại học Đà Nẵng (DUT)
