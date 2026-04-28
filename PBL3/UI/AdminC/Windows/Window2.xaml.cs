using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using PBL3a.services.BLL;

namespace PBL3a.UI.AdminC.Windows
{
    public partial class Window2 : UserControl
    {
        private AdminC_Service adminService = new AdminC_Service();
        private DataTable dtTempClasses;
        private string selectedTeacherId = "";

        public Window2()
        {
            InitializeComponent();
            Loaded += Window2_Load;
        }

        private void Window2_Load(object sender, RoutedEventArgs e)
        {
            cbbMH.ItemsSource = new string[]
            {
                "Toán Học","Văn Học","Hóa Học","Vật Lý","Sinh Học","Tiếng Anh"
            };

            cbbKhoi.ItemsSource = new string[]
            {
                "Khối 8","Khối 9","Khối 10","Khối 11","Khối 12"
            };

            dtTempClasses = new DataTable();
            dtTempClasses.Columns.Add("Mã Lớp");
            dtTempClasses.Columns.Add("Tên Lớp");
            dtTempClasses.Columns.Add("Mã Khóa Học");
            dtTempClasses.Columns.Add("Mã GV");
            dtTempClasses.Columns.Add("Ngày Bắt Đầu", typeof(DateTime));
            dtTempClasses.Columns.Add("Ngày Kết Thúc", typeof(DateTime));
            dtTempClasses.Columns.Add("Sức Chứa", typeof(int));

            dgvLH.ItemsSource = dtTempClasses.DefaultView;
        }

        private void cbbMH_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbMH.SelectedItem != null)
            {
                dgvGV.ItemsSource =
                    adminService.GetTeachersBySubjectForm2(cbbMH.SelectedItem.ToString()).DefaultView;

                selectedTeacherId = "";
            }
        }

        private void dgvGV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvGV.SelectedItem is DataRowView row)
            {
                selectedTeacherId = row["Mã GV"].ToString();

                dgvTTGV.ItemsSource =
                    adminService.GetTeacherDetailsAndClasses(selectedTeacherId).DefaultView;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (cbbMH.SelectedItem == null)
            {
                MessageBox.Show("Chọn môn học");
                return;
            }

            if (cbbKhoi.SelectedItem == null)
            {
                MessageBox.Show("Chọn khối");
                return;
            }

            if (!int.TryParse(txtSL.Text, out int capacity) || capacity <= 0)
            {
                MessageBox.Show("Sức chứa không hợp lệ");
                return;
            }

            if (string.IsNullOrEmpty(selectedTeacherId))
            {
                MessageBox.Show("Chọn giáo viên");
                return;
            }

            DateTime startDate = dtpNgayMo.SelectedDate ?? DateTime.Now;
            DateTime endDate = startDate.AddMonths(5);

            adminService.GenerateClassIdentifiers(
                cbbMH.SelectedItem.ToString(),
                cbbKhoi.SelectedItem.ToString(),
                startDate,
                out string courseId,
                out string classId,
                out string className
            );

            dtTempClasses.Rows.Add(
                classId,
                className,
                courseId,
                selectedTeacherId,
                startDate,
                endDate,
                capacity
            );

            MessageBox.Show("Đã tạo lớp tạm");
        }

        private void btnDB_Click(object sender, RoutedEventArgs e)
        {
            if (dtTempClasses.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu");
                return;
            }

            try
            {
                adminService.SaveNewClasses(dtTempClasses);
                MessageBox.Show("Đã lưu DB");
                dtTempClasses.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}