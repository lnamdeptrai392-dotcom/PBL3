using Microsoft.Data.SqlClient;
using PBL3a.services;
using PBL3a.services.BLL;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminTC
{
    public partial class ThietLapHP : Window
    {
        private AdminTC_Service bll = new AdminTC_Service();
        private string MaLop;
        public ThietLapHP(string m)
        {
            InitializeComponent();
            MaLop = m;
            SetGUI();
            LoadChiTietHocPhi();
        }

        public void SetGUI()
        {
            txtMaLop.Text = MaLop;
            try
            {
                string className = bll.GetClassNameByID(MaLop);
                tbTL.Text = className;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin lớp: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private decimal _soTienMoiHS = 0;

        private void btLuu_Click(object sender, RoutedEventArgs e)
        {
            string chuoiTien = txtTienTrenNg.Text?.Trim() ?? "";
            bool coNhapTien = (chuoiTien != "");
            if (coNhapTien && _soTienMoiHS <= 0)
            {
                MessageBox.Show("Vui lòng nhấn nút 'Tính tổng tiền' để xác nhận số tiền trước khi lưu!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            dgHocSinhLop.CommitEdit(DataGridEditingUnit.Cell, exitEditingMode: true);
            dgHocSinhLop.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);

            DataView dv = (DataView)dgHocSinhLop.ItemsSource;

            int thangHienTai = DateTime.Now.Month;
            int namHienTai = DateTime.Now.Year;

            try
            {
                bll.SaveHocPhiSetup(dv.Table, MaLop, _soTienMoiHS, coNhapTien, thangHienTai, namHienTai);
                MessageBox.Show("Cập nhật dữ liệu học phí thành công!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadChiTietHocPhi()
        {
            try
            {
                int thang = DateTime.Now.Month;
                int nam = DateTime.Now.Year;

                DataTable dt = bll.GetChiTietHocPhi(MaLop, _soTienMoiHS, thang, nam);
                dgHocSinhLop.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết học phí: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public int capacity_cl(string idlop)
        {
            try
            {
                return bll.GetClassCapacity(idlop);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tính sĩ số: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }
        public decimal SetHP(decimal hphi)
        {
            int cap = capacity_cl(MaLop);
            txtSS.Text = cap.ToString();
            return cap * hphi;
        }
        private void butTT_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtTienTrenNg.Text, out decimal hphi) || hphi < 0)
            {
                MessageBox.Show("Vui lòng nhập số tiền hợp lệ (≥ 0)!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _soTienMoiHS = hphi;                      // ← lưu vào biến class
            LoadChiTietHocPhi();                       // reload để cột SoTien hiện đúng
            decimal tongTien = SetHP(hphi);
            txtTongT.Text = tongTien.ToString("N0") + " VNĐ";
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}