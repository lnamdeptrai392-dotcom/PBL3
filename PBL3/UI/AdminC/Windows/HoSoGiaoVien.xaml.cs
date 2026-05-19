using PBL3a.services.BLL;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace PBL3.UI.AdminC.Windows
{
    public partial class HoSoGiaoVien : UserControl
    {
        private Teacher_Service _teacherService = new Teacher_Service();

        public HoSoGiaoVien()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData(string searchKeyword = "")
        {
            try
            {
                DataView dv = _teacherService.GetListGiaoVien(searchKeyword).DefaultView;
                dgvGiaoVien.ItemsSource = dv;
                FilterBySubject();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cbbChuyenMon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterBySubject();
        }

        private void FilterBySubject()
        {
            if (dgvGiaoVien != null && dgvGiaoVien.ItemsSource is DataView dv)
            {
                if (cbbChuyenMon.SelectedItem is ComboBoxItem selectedItem)
                {
                    string subject = selectedItem.Content.ToString();

                    if (subject == "Tất cả")
                    {
                        dv.RowFilter = "";
                    }
                    else
                    {
                        dv.RowFilter = $"ChuyenMon = '{subject}'";
                    }
                }
            }
        }

        private void dgvGiaoVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvGiaoVien.SelectedItem is DataRowView row)
            {
                txtMaGV.Text = row["MaGV"].ToString();
                txtHoTen.Text = row["HoTen"].ToString();
                txtSDT.Text = row["SDT"].ToString();

                if (DateTime.TryParse(row["NgaySinh"].ToString(), out DateTime ngaySinh))
                    dpNgaySinh.SelectedDate = ngaySinh;
                else
                    dpNgaySinh.SelectedDate = null;

                string gioiTinh = row["GioiTinh"].ToString().Trim();
                foreach (ComboBoxItem item in cmbGioiTinh.Items)
                {
                    if (item.Content.ToString().Equals(gioiTinh, StringComparison.OrdinalIgnoreCase))
                    {
                        cmbGioiTinh.SelectedItem = item;
                        break;
                    }
                }

                txtChuyenMon.Text = row["ChuyenMon"].ToString().Trim();
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtMaGV.Text) || string.IsNullOrWhiteSpace(txtHoTen.Text) || string.IsNullOrWhiteSpace(txtChuyenMon.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã giáo viên, Họ tên và Chuyên môn!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            dpNgaySinh.SelectedDate = null;
            cmbGioiTinh.SelectedItem = null;
            txtChuyenMon.Clear();
            txtSearch.Clear();

            cbbChuyenMon.SelectedIndex = 0;

            LoadData();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            MessageBoxResult res = MessageBox.Show("Cập nhật lại thông tin giáo viên này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                string gt = cmbGioiTinh.SelectedItem != null ? ((ComboBoxItem)cmbGioiTinh.SelectedItem).Content.ToString() : "Male";
                string chuyenMon = txtChuyenMon.Text.Trim();

                bool success = _teacherService.UpdateGiaoVien(txtMaGV.Text.Trim(), txtHoTen.Text.Trim(), dpNgaySinh.SelectedDate, gt, txtSDT.Text.Trim(), chuyenMon, "Hoạt động");

                if (success)
                {
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

       
    }
}