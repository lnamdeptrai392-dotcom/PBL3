using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PBL3.UI.AdminTC.ChildForm
{
    /// <summary>
    /// Interaction logic for ThongKe.xaml
    /// </summary>
    public partial class ThongKe : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();

        public ThongKe()
        {
            InitializeComponent();
            NapComboBox();
        }

        private void NapComboBox()
        {
            try
            {
                // Nạp năm: từ 2024 đến năm hiện tại
                int currentYear = DateTime.Now.Year;
                cbbNam.Items.Clear();
                for (int y = currentYear; y >= 2024; y--)
                    cbbNam.Items.Add(y);

                if (cbbNam.Items.Count > 0)
                    cbbNam.SelectedIndex = 0;

                // Nạp tháng: 1-12 + "Cả năm"
                cbbThang.Items.Clear();
                cbbThang.Items.Add("Cả năm");
                for (int m = 1; m <= 12; m++)
                    cbbThang.Items.Add(m);

                cbbThang.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nạp dữ liệu combo box: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            if (cbbNam.SelectedItem == null) return;

            int nam = (int)cbbNam.SelectedItem;
            int? thang = null;

            // Nếu chọn "Cả năm" thì thang = null, ngược lại lấy số tháng
            if (cbbThang.SelectedIndex > 0)
                thang = (int)cbbThang.SelectedItem;

            LoadThongKeTaiChinh(nam, thang);
        }

        private void LoadThongKeTaiChinh(int nam, int? thang)
        {
            try
            {
                // Tạo DataTable để lưu kết quả tổng hợp
                DataTable dt = new DataTable();
                dt.Columns.Add("Mã", typeof(string));
                dt.Columns.Add("Ngày", typeof(string));
                dt.Columns.Add("Loại", typeof(string));
                dt.Columns.Add("Nội dung", typeof(string));
                dt.Columns.Add("Số tiền", typeof(string));
                dt.Columns.Add("Ghi chú", typeof(string));
                dt.Columns.Add("SoTienGoc", typeof(decimal)); // Cột ẩn để tính tổng

                decimal tongThu = 0;
                decimal tongChi = 0;

                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();

                    // ==================== 1. THU TỪ KHOANTHU ====================
                    string sqlThu = @"
                        SELECT 
                            ThuID AS [Mã],
                            FORMAT(NgayThu, 'dd/MM/yyyy') AS [Ngày],
                            N'Thu' AS [Loại],
                            NoiDung AS [Nội dung],
                            SoTien AS [Số tiền gốc],
                            ISNULL(GhiChu, '') AS [Ghi chú]
                        FROM KhoanThu
                        WHERE ThuYear = @Nam";

                    if (thang.HasValue)
                        sqlThu += " AND ThuMonth = @Thang";

                    using (SqlCommand cmd = new SqlCommand(sqlThu, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nam", nam);
                        if (thang.HasValue)
                            cmd.Parameters.AddWithValue("@Thang", thang.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DataRow row = dt.NewRow();
                                row["Mã"] = "KT_" + reader["Mã"].ToString();
                                row["Ngày"] = reader["Ngày"].ToString();
                                row["Loại"] = reader["Loại"].ToString();
                                row["Nội dung"] = reader["Nội dung"].ToString();
                                decimal soTien = Convert.ToDecimal(reader["Số tiền gốc"]);
                                row["Số tiền"] = soTien.ToString("N0") + " đ";
                                row["Ghi chú"] = reader["Ghi chú"].ToString();
                                row["SoTienGoc"] = soTien;
                                dt.Rows.Add(row);
                                tongThu += soTien;
                            }
                        }
                    }

                    // ==================== 2. THU TỪ HỌC PHÍ (HocPhi) ====================
                    string sqlHocPhi = @"
                        SELECT 
                            hp.HocPhiID AS [Mã],
                            FORMAT(hp.NgayDong, 'dd/MM/yyyy') AS [Ngày],
                            N'Thu học phí' AS [Loại],
                            a.name + N' - ' + c.class_name + N' (T' + CAST(hp.TuitionMonth AS VARCHAR) + N'/' + CAST(hp.TuitionYear AS VARCHAR) + N')' AS [Nội dung],
                            hp.SoTien AS [Số tiền gốc],
                            ISNULL(hp.GhiChu, '') AS [Ghi chú]
                        FROM HocPhi hp
                        JOIN accountList a ON hp.AccountID = a.Id
                        JOIN Class c ON hp.ClassID = c.classID
                        WHERE hp.TrangThai = N'Đã đóng'
                            AND hp.NgayDong IS NOT NULL
                            AND YEAR(hp.NgayDong) = @Nam";

                    if (thang.HasValue)
                        sqlHocPhi += " AND MONTH(hp.NgayDong) = @Thang";

                    using (SqlCommand cmd = new SqlCommand(sqlHocPhi, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nam", nam);
                        if (thang.HasValue)
                            cmd.Parameters.AddWithValue("@Thang", thang.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DataRow row = dt.NewRow();
                                row["Mã"] = "HP_" + reader["Mã"].ToString();
                                row["Ngày"] = reader["Ngày"].ToString();
                                row["Loại"] = reader["Loại"].ToString();
                                row["Nội dung"] = reader["Nội dung"].ToString();
                                decimal soTien = Convert.ToDecimal(reader["Số tiền gốc"]);
                                row["Số tiền"] = soTien.ToString("N0") + " đ";
                                row["Ghi chú"] = reader["Ghi chú"].ToString();
                                row["SoTienGoc"] = soTien;
                                dt.Rows.Add(row);
                                tongThu += soTien;
                            }
                        }
                    }

                    // ==================== 3. CHI TỪ KHOANCHI ====================
                    string sqlChi = @"
                        SELECT 
                            ChiID AS [Mã],
                            FORMAT(NgayChi, 'dd/MM/yyyy') AS [Ngày],
                            N'Chi' AS [Loại],
                            NoiDung AS [Nội dung],
                            SoTien AS [Số tiền gốc],
                            ISNULL(GhiChu, '') AS [Ghi chú]
                        FROM KhoanChi
                        WHERE ChiYear = @Nam";

                    if (thang.HasValue)
                        sqlChi += " AND ChiMonth = @Thang";

                    using (SqlCommand cmd = new SqlCommand(sqlChi, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nam", nam);
                        if (thang.HasValue)
                            cmd.Parameters.AddWithValue("@Thang", thang.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DataRow row = dt.NewRow();
                                row["Mã"] = "KC_" + reader["Mã"].ToString();
                                row["Ngày"] = reader["Ngày"].ToString();
                                row["Loại"] = reader["Loại"].ToString();
                                row["Nội dung"] = reader["Nội dung"].ToString();
                                decimal soTien = Convert.ToDecimal(reader["Số tiền gốc"]);
                                row["Số tiền"] = soTien.ToString("N0") + " đ";
                                row["Ghi chú"] = reader["Ghi chú"].ToString();
                                row["SoTienGoc"] = soTien;
                                dt.Rows.Add(row);
                                tongChi += soTien;
                            }
                        }
                    }

                    // ==================== 4. CHI LƯƠNG GIÁO VIÊN (LuongGV) ====================
                    string sqlLuongGV = @"
                        SELECT 
                            lg.LuongID AS [Mã],
                            FORMAT(lg.NgayThanhToan, 'dd/MM/yyyy') AS [Ngày],
                            N'Chi lương' AS [Loại],
                            a.name + N' - ' + ti.subject + N' (T' + CAST(lg.SalaryMonth AS VARCHAR) + N'/' + CAST(lg.SalaryYear AS VARCHAR) + N')' AS [Nội dung],
                            lg.TongLuong AS [Số tiền gốc],
                            ISNULL(lg.GhiChu, '') AS [Ghi chú]
                        FROM LuongGV lg
                        JOIN accountList a ON lg.TeacherID = a.Id
                        JOIN teacherInfo ti ON lg.TeacherID = ti.Id
                        WHERE lg.TrangThai = N'Đã thanh toán'
                            AND lg.NgayThanhToan IS NOT NULL
                            AND YEAR(lg.NgayThanhToan) = @Nam";

                    if (thang.HasValue)
                        sqlLuongGV += " AND MONTH(lg.NgayThanhToan) = @Thang";

                    using (SqlCommand cmd = new SqlCommand(sqlLuongGV, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nam", nam);
                        if (thang.HasValue)
                            cmd.Parameters.AddWithValue("@Thang", thang.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DataRow row = dt.NewRow();
                                row["Mã"] = "LG_" + reader["Mã"].ToString();
                                row["Ngày"] = reader["Ngày"].ToString();
                                row["Loại"] = reader["Loại"].ToString();
                                row["Nội dung"] = reader["Nội dung"].ToString();
                                decimal soTien = Convert.ToDecimal(reader["Số tiền gốc"]);
                                row["Số tiền"] = soTien.ToString("N0") + " đ";
                                row["Ghi chú"] = reader["Ghi chú"].ToString();
                                row["SoTienGoc"] = soTien;
                                dt.Rows.Add(row);
                                tongChi += soTien;
                            }
                        }
                    }
                }

                // Sắp xếp theo ngày giảm dần
                dt.DefaultView.Sort = "Ngày DESC";

                // Hiển thị lên DataGrid
                dataGridView1.ItemsSource = dt.DefaultView;

                // Hiển thị tổng thu, tổng chi
                txtTongThu.Text = tongThu.ToString("N0") + " đ";
                txtTongChi.Text = tongChi.ToString("N0") + " đ";


                // Hiển thị thông báo nếu không có dữ liệu
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show($"Không có dữ liệu tài chính cho {(thang.HasValue ? $"tháng {thang.Value}/{nam}" : $"năm {nam}")}",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu thống kê:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cbbThang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Có thể tự động refresh nếu muốn
            // btnShow_Click(sender, e);
        }

        private void cbbNam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Có thể tự động refresh nếu muốn
            // btnShow_Click(sender, e);
        }
    }
}