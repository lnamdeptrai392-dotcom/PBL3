using Microsoft.Data.SqlClient;
using PBL3a.services;
using PBL3a.services.BLL;
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
        private AdminTC_Service bll = new AdminTC_Service();

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

                bll.FetchThongKeTaiChinhData(dt, ref tongThu, ref tongChi, nam, thang);

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