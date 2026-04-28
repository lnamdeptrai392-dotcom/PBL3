using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using PBL3a.services.BLL;

namespace PBL3a.UI.AdminC.Windows
{
    public partial class Window4 : UserControl
    {
        private AdminC_Service adminService = new AdminC_Service();

        public Window4()
        {
            InitializeComponent();
            Loaded += Window4_Load;
        }

        private void Window4_Load(object sender, RoutedEventArgs e)
        {
            cbbMH.ItemsSource = new string[]
            {
                "Toán học","Vật lý","Hóa học","Sinh học","Ngữ văn","Tiếng Anh","Tiếng Nhật","Tiếng Trung"
            };

            cbbTTL.ItemsSource = new string[]
            {
                "Đã kết thúc","Đang học","Sắp mở"
            };

            cbbSearch.ItemsSource = new string[]
            {
                "Mã Học Sinh","Tên Học Sinh",
                "Mã Giáo Viên","Tên Giáo Viên",
                "Mã Lớp Học","Tên Lớp Học"
            };
        }

        private string GetKeyword(string monHoc)
        {
            if (monHoc == "Toán học") return "Toán";
            if (monHoc == "Vật lý") return "Lý";
            if (monHoc == "Hóa học") return "Hóa";
            if (monHoc == "Sinh học") return "Sinh";
            if (monHoc == "Ngữ văn") return "Văn";
            return monHoc;
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (cbbMH.SelectedItem == null || cbbKhoi.SelectedItem == null || cbbTTL.SelectedItem == null)
            {
                MessageBox.Show("Chọn đủ thông tin");
                return;
            }

            string mon = GetKeyword(cbbMH.SelectedItem.ToString());
            string khoi = cbbKhoi.SelectedItem.ToString();
            string ttl = cbbTTL.SelectedItem.ToString();

            dgvData.ItemsSource =
                adminService.GetClassesByFilter(mon, khoi, ttl).DefaultView;

            dgvGV.ItemsSource = null;
            dgvHS.ItemsSource = null;
        }

        private void btnView2_Click(object sender, RoutedEventArgs e)
        {
            if (cbbSearch.SelectedItem == null || string.IsNullOrWhiteSpace(txtNhapDuLieu.Text))
            {
                MessageBox.Show("Nhập dữ liệu tìm kiếm");
                return;
            }

            dgvData.ItemsSource =
                adminService.SearchClasses(
                    cbbSearch.SelectedItem.ToString(),
                    txtNhapDuLieu.Text.Trim()
                ).DefaultView;

            dgvGV.ItemsSource = null;
            dgvHS.ItemsSource = null;
        }

        private void dgvData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvData.SelectedItem is DataRowView row)
            {
                string classId = row["Mã Lớp"]?.ToString();

                dgvGV.ItemsSource =
                    adminService.GetTeacherByClass(classId).DefaultView;

                dgvHS.ItemsSource =
                    adminService.GetStudentsByClass(classId).DefaultView;
            }
        }

        private void cbbMH_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbMH.SelectedItem == null) return;

            string mon = GetKeyword(cbbMH.SelectedItem.ToString());

            cbbKhoi.ItemsSource =
                adminService.GetBlocksBySubject(mon).DefaultView;
        }

        private void cbbTTL_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            cbbSearch.SelectedIndex = -1;
            txtNhapDuLieu.Clear();
        }

        private void cbbSearch_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            cbbMH.SelectedIndex = -1;
            cbbKhoi.ItemsSource = null;
            cbbTTL.SelectedIndex = -1;
        }

        private void txtNhapDuLieu_Enter(object sender, RoutedEventArgs e)
        {
            cbbMH.SelectedIndex = -1;
            cbbKhoi.ItemsSource = null;
            cbbTTL.SelectedIndex = -1;
        }
    }
}