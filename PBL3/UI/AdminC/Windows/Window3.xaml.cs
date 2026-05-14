using PBL3a.services.BLL;
using System;
using System.Collections.Generic;
using System.Data;
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
    public partial class HoSoHocSinh : UserControl
    {
        private Student_Service _studentService = new Student_Service();
        public HoSoHocSinh()
        {
            InitializeComponent();
            LoadData();
            LoadComboBoxLop();
        }

        private void LoadData(string searchKeyword = "")
        {
            try
            {
                // Thêm .DefaultView khi gán DataTable vào ItemsSource của WPF DataGrid
                dgvHocSinh.ItemsSource = _studentService.GetListHocSinh(searchKeyword).DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadComboBoxLop()
        {
            try
            {
                cmbLop.ItemsSource = _studentService.GetDanhSachLop().DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lớp: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgHocSinh_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvHocSinh.SelectedItem != null)
            {
                // Ép kiểu dòng được chọn về DataRowView (vì dữ liệu trả lên là DataTable)
                DataRowView selectedRow = (DataRowView)dgvHocSinh.SelectedItem;

                txtMaHS.Text = selectedRow["MaHS"].ToString();
                txtHoTen.Text = selectedRow["HoTen"].ToString();
                dpNgaySinh.SelectedDate = selectedRow["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(selectedRow["NgaySinh"]) : (DateTime?)null;
                cmbGioiTinh.Text = selectedRow["GioiTinh"].ToString();
                txtSDT.Text = selectedRow["SDTPhuHuynh"].ToString();
                cmbLop.SelectedValue = selectedRow["MaLop"].ToString();
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtMaHS.Text) || string.IsNullOrWhiteSpace(txtHoTen.Text) || cmbLop.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng điền đầy đủ Mã học sinh, Họ tên và chọn Lớp!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate SĐT (bắt đầu bằng 0 hoặc +84 và đủ 10 số)
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
            txtMaHS.Clear();
            txtHoTen.Clear();
            txtSDT.Clear();
            dpNgaySinh.SelectedDate = null;
            cmbGioiTinh.SelectedIndex = -1;
            cmbLop.SelectedIndex = -1;
            txtSearch.Clear();

            LoadData(); // Load lại DataGrid
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                // Gọi hàm Add truyền đầy đủ tham số
                bool result = _studentService.AddHocSinh(
                    txtMaHS.Text.Trim(),
                    txtHoTen.Text.Trim(),
                    dpNgaySinh.SelectedDate,
                    cmbGioiTinh.Text,
                    txtSDT.Text.Trim(),
                    cmbLop.SelectedValue.ToString()
                );

                if (result)
                {
                    MessageBox.Show("Thêm học sinh thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    BtnRefresh_Click(null, null); // Làm mới Form và Grid
                }
                else
                {
                    MessageBox.Show("Thêm thất bại. Có thể trùng Mã học sinh!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thực thi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            MessageBoxResult confirm = MessageBox.Show("Bạn có chắc chắn muốn cập nhật hồ sơ này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    // Gọi hàm Edit truyền đầy đủ tham số
                    bool result = _studentService.EditHocSinh(
                        txtMaHS.Text.Trim(),
                        txtHoTen.Text.Trim(),
                        dpNgaySinh.SelectedDate,
                        cmbGioiTinh.Text,
                        txtSDT.Text.Trim(),
                        cmbLop.SelectedValue.ToString()
                    );

                    if (result)
                    {
                        MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi thực thi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaHS.Text))
            {
                MessageBox.Show("Vui lòng chọn một học sinh để xóa!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult confirm = MessageBox.Show("Hành động này không thể hoàn tác. Bạn có chắc chắn muốn xóa học sinh này không?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    bool result = _studentService.DeleteHocSinh(txtMaHS.Text.Trim());
                    if (result)
                    {
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        BtnRefresh_Click(null, null); // Clear form và Refresh lưới
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi thực thi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


    }
}
