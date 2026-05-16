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
        private DatabaseHelper db = new DatabaseHelper();
        
        public TongQuan()
        {
            InitializeComponent();
            LoadHocSinhChamHocPhi();
        }

        private void LoadHocSinhChamHocPhi()
        {
            string query = @"
                SELECT 
                    a.Id         AS [Mã HS],
                    a.name       AS [Họ tên],
                    a.phone      AS [Số điện thoại],
                    c.class_name AS [Lớp],
                    hp.TuitionMonth AS [Tháng],
                    hp.TuitionYear  AS [Năm],
                    FORMAT(hp.SoTien, 'N0') + N' đ' AS [Số tiền],
                    hp.TrangThai AS [Trạng thái]
                FROM HocPhi hp
                JOIN accountList a ON hp.AccountID = a.Id
                JOIN Class c       ON hp.ClassID   = c.classID
                WHERE hp.TrangThai = N'Chưa đóng'
                ORDER BY hp.TuitionYear DESC, hp.TuitionMonth DESC, a.name";

            DataTable dt = new DataTable();
            using (SqlConnection conn = db.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            dgvDanhSach.ItemsSource = dt.DefaultView;

        }
        private void LoadGiaoVienChuaTraLuong()
        {
            string query = @"
                SELECT 
                    a.Id       AS [Mã GV],
                    a.name     AS [Họ tên],
                    a.phone    AS [Số điện thoại],
                    ti.subject AS [Môn dạy],
                    lg.SalaryMonth AS [Tháng],
                    lg.SalaryYear  AS [Năm],
                    FORMAT(lg.TongLuong, 'N0') + N' đ' AS [Tổng lương],
                    lg.TrangThai AS [Trạng thái]
                FROM LuongGV lg
                JOIN accountList a  ON lg.TeacherID = a.Id
                JOIN teacherInfo ti ON lg.TeacherID = ti.Id
                WHERE lg.TrangThai = N'Chưa thanh toán'
                ORDER BY lg.SalaryYear DESC, lg.SalaryMonth DESC, a.name";

            DataTable dt = new DataTable();
            using (SqlConnection conn = db.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            dgvDanhSach.ItemsSource = dt.DefaultView;
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
