using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using PBL3a.services;
using PBL3a.services.BLL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PBL3.UI.AdminTC.ChildForm
{
    /// <summary>
    /// Interaction logic for TongQuan.xaml
    /// </summary>
    public partial class TongQuan : UserControl
    {
        private AdminTC_Service bll = new AdminTC_Service();

        public TongQuan()
        {
            InitializeComponent();
            LoadHocSinhChamHocPhi();
        }

        private void LoadHocSinhChamHocPhi()
        {
            dgvDanhSach.ItemsSource = null;
            try
            {
                DataTable dt = bll.GetHocSinhChamHocPhi();
                dgvDanhSach.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách học sinh chậm học phí: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadGiaoVienChuaTraLuong()
        {
            dgvDanhSach.ItemsSource = null;
            try
            {
                DataTable dt = bll.GetGiaoVienChuaTraLuong();
                dgvDanhSach.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách giáo viên chưa trả lương: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cbbList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbList.SelectedIndex == 0)
                LoadHocSinhChamHocPhi();
            else if (cbbList.SelectedIndex == 1)
                LoadGiaoVienChuaTraLuong();
        }

        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            if (cbbList.SelectedIndex == 0)
                LoadHocSinhChamHocPhi();
            else
                LoadGiaoVienChuaTraLuong();
        }
    }
}