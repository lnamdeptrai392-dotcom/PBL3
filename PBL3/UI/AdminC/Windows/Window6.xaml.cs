using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PBL3.UI.AdminC.Windows
{
    /// <summary>
    /// Interaction logic for Window6.xaml
    /// </summary>
    public partial class HoSoGiaoVien : UserControl
    {
        // Giả sử có: private Teacher_Service _teacherService = new Teacher_Service();

        public HoSoGiaoVien()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData(string searchKeyword = "")
        {
            try
            {
                // Truyền searchKeyword (tên hoặc chuyên môn) xuống DTO
                // dgGiaoVien.ItemsSource = _teacherService.GetListGiaoVien(searchKeyword);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgGiaoVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgGiaoVien.SelectedItem != null)
            {
                dynamic row = dgGiaoVien.SelectedItem;

                txtMaGV.Text = row.MaGV;
                txtHoTen.Text = row.HoTen;
                dpNgaySinh.SelectedDate = row.NgaySinh;
                cmbGioiTinh.Text = row.GioiTinh;
                txtSDT.Text = row.SDT;
                txtEmail.Text = row.Email;
                cmbChuyenMon.Text = row.ChuyenMon;
                cmbTinhTrang.Text = row.TinhTrang;
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtMaGV.Text) || string.IsNullOrWhiteSpace(txtHoTen.Text) || string.IsNullOrWhiteSpace(cmbChuyenMon.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã giáo viên, Họ tên và Chuyên môn!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Định dạng Email không hợp lệ!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtSDT.Text) && !Regex.IsMatch(txtSDT.Text, @"^(0|\+84)\d{9}$"))
            {
                MessageBox.Show("Định dạng số điện thoại không hợp lệ!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadData(txtSearch.Text.Trim());
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtMaGV.Clear();
            txtHoTen.Clear();
            txtSDT.Clear();
            txtEmail.Clear();
            dpNgaySinh.SelectedDate = null;
            cmbGioiTinh.SelectedIndex = -1;
            cmbChuyenMon.SelectedIndex = -1;
            cmbTinhTrang.SelectedIndex = -1;
            txtSearch.Clear();
            LoadData();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;
            // Thực hiện Add thông qua service
            MessageBox.Show("Thêm giáo viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadData(); // Cập nhật lại grid
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            MessageBoxResult res = MessageBox.Show("Cập nhật lại thông tin giáo viên này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                // Thực hiện Update thông qua service
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaGV.Text))
            {
                MessageBox.Show("Hãy chọn một giáo viên bên cạnh để xóa!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult res = MessageBox.Show("Hành động xóa sẽ không khôi phục được. Xác nhận xóa?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                // Thực hiện Delete thông qua service
                MessageBox.Show("Xóa giáo viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnRefresh_Click(null, null);
            }
        }
    }
}
