using PBL3a.services.BLL;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3.UI.AdminC.Windows
{
    public partial class HoSoHocSinh : UserControl
    {
        private Student_Service _studentService = new Student_Service();

        public HoSoHocSinh()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData(string searchKeyword = "")
        {
            try
            {
                DataView dv = _studentService.GetListHocSinh(searchKeyword).DefaultView;
                dgvHocSinh.ItemsSource = _studentService.GetListHocSinh(searchKeyword).DefaultView;
                FilterByYear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLichSuHocTap(string maHS)
        {
            try
            {
                dgvLichSuHocTap.ItemsSource = _studentService.GetLichSuHocTap(maHS).DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch sử học tập: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterByYear()
        {
            if (dgvHocSinh != null && dgvHocSinh.ItemsSource is DataView dv)
            {
                if (cbbChooseClass.SelectedItem is ComboBoxItem selectedItem)
                {
                    string year = selectedItem.Content.ToString();
                    if (year == "Tất cả")
                    {
                        dv.RowFilter = "";
                    }
                    else
                    {
                        dv.RowFilter = $"NgaySinh >= '{year}-01-01' AND NgaySinh <= '{year}-12-31'";
                    }
                }
            }
        }

        private void dgvHocSinh_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvHocSinh.SelectedItem is DataRowView row)
            {
                txtMaHS.Text = row["MaHS"].ToString();
                txtHoTen.Text = row["HoTen"].ToString();

                if (DateTime.TryParse(row["NgaySinh"].ToString(), out DateTime ngaySinh))
                {
                    dpNgaySinh.SelectedDate = ngaySinh;
                }
                else
                {
                    dpNgaySinh.SelectedDate = null;
                }

                string gioiTinh = row["GioiTinh"].ToString().Trim();

                if (gioiTinh == "Male")
                {
                    cmbGioiTinh.SelectedIndex = 0;
                }
                else if (gioiTinh == "Female")
                {
                    cmbGioiTinh.SelectedIndex = 1;
                }
                else
                {
                    cmbGioiTinh.SelectedIndex = -1;
                }

                if (row.DataView.Table.Columns.Contains("SDTPhuHuynh"))
                {
                    txtSDT.Text = row["SDTPhuHuynh"].ToString();
                }

                LoadLichSuHocTap(txtMaHS.Text);
            }
        }

        private void cbbChooseClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterByYear();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadData(txtSearch.Text.Trim());
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtMaHS.Clear();
            txtHoTen.Clear();
            dpNgaySinh.SelectedDate = null;
            cmbGioiTinh.SelectedItem = null;
            txtSDT.Clear();
            txtSearch.Clear();
            cbbChooseClass.SelectedIndex = 0;
            dgvLichSuHocTap.ItemsSource = null;

            LoadData();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaHS.Text))
            {
                MessageBox.Show("Vui lòng chọn một học sinh để cập nhật!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string maHS = txtMaHS.Text.Trim();
                string hoTen = txtHoTen.Text.Trim();
                DateTime? ngaySinh = dpNgaySinh.SelectedDate;
                string gioiTinh = (cmbGioiTinh.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                string sdt = txtSDT.Text.Trim();

                bool result = _studentService.EditHocSinh(maHS, hoTen, ngaySinh, gioiTinh, sdt);
                if (result)
                {
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    BtnRefresh_Click(null, null);
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
}