using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

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
            // Tạo DataTable để lưu kết quả tổng hợp
            DataTable dt = new DataTable();
            dt.Columns.Add("Mã", typeof(string));
            dt.Columns.Add("Ngày", typeof(string));
            dt.Columns.Add("Loại", typeof(string));
            dt.Columns.Add("Nội dung", typeof(string));
            dt.Columns.Add("Số tiền", typeof(string));
            dt.Columns.Add("Ghi chú", typeof(string));

            // 1. Load dữ liệu từ KhoanThu
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

            // 2. Load dữ liệu từ KhoanChi
            string sqlChi = @"
                SELECT 
                    ChiID AS [Mã],
                    FORMAT(NgayChi, 'dd/MM/yyyy') AS [Ngày],
                    N'Chi' AS [Loại],
                    NoiDung AS [Nội dung],
                    SoTien AS [Số tiền gốc],
                    ISNULL(GhiChu, '') AS [Ghi chú]
                FROM KhoanChi
                WHERE CAST(ChiYear AS INT) = @Nam";

            if (thang.HasValue)
                sqlChi += " AND CAST(ChiMonth AS INT) = @Thang";

            decimal tongThu = 0;
            decimal tongChi = 0;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                // Lấy dữ liệu thu
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
                            row["Mã"] = reader["Mã"].ToString();
                            row["Ngày"] = reader["Ngày"].ToString();
                            row["Loại"] = reader["Loại"].ToString();
                            row["Nội dung"] = reader["Nội dung"].ToString();
                            decimal soTien = Convert.ToDecimal(reader["Số tiền gốc"]);
                            row["Số tiền"] = soTien.ToString("N0") + " đ";
                            row["Ghi chú"] = reader["Ghi chú"].ToString();
                            dt.Rows.Add(row);
                            tongThu += soTien;
                        }
                    }
                }

                // Lấy dữ liệu chi
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
                            row["Mã"] = reader["Mã"].ToString();
                            row["Ngày"] = reader["Ngày"].ToString();
                            row["Loại"] = reader["Loại"].ToString();
                            row["Nội dung"] = reader["Nội dung"].ToString();
                            decimal soTien = Convert.ToDecimal(reader["Số tiền gốc"]);
                            row["Số tiền"] = soTien.ToString("N0") + " đ";
                            row["Ghi chú"] = reader["Ghi chú"].ToString();
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

            // Thêm dòng tổng kết nếu muốn hiển thị lợi nhuận
            decimal loiNhuan = tongThu - tongChi;

            // Optional: Thay đổi màu sắc cho TextBox dựa trên lợi nhuận
            if (loiNhuan >= 0)
            {
                // Có thể thêm TextBlock để hiển thị lợi nhuận
                // Bạn có thể thêm một TextBox mới để hiển thị lợi nhuận
            }
        }

        private void cbbThang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Có thể để trống hoặc tự động refresh nếu muốn
        }

        private void cbbNam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Có thể để trống hoặc tự động refresh nếu muốn
        }
    }
}