using PBL3a.services.BLL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminC.Windows
{
    public partial class Window4 : UserControl
    {
        private AdminC_Service adminService = new AdminC_Service();
        private object _originalDgvDataSource;
        private string _currentAddingClassId = "";

        public Window4()
        {
            InitializeComponent();
            Loaded += Window4_Load;
        }

        private void Window4_Load(object sender, RoutedEventArgs e)
        {
            cbbMH.ItemsSource = new string[]
            {
                "Toán học","Vật lý","Hóa học","Sinh học","Ngữ văn","Tiếng Anh"
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
            string ttl = cbbTTL.SelectedItem.ToString();
            string khoi = cbbKhoi.SelectedValue.ToString();

            dgvData.ItemsSource = adminService.GetClassesByFilter(mon, khoi, ttl).DefaultView;

            dgvGV.ItemsSource = null;
            dgvHS.ItemsSource = null;
            btnPrepareAddStudent.IsEnabled = false;
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
            btnPrepareAddStudent.IsEnabled = false;
        }

        private void dgvData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvData.SelectedItem is DataRowView row)
            {
                string classId = row["Mã Lớp"]?.ToString();

                dgvGV.ItemsSource = adminService.GetTeacherByClass(classId).DefaultView;

                dgvHS.ItemsSource = adminService.GetStudentsByClass(classId).DefaultView;

                btnPrepareAddStudent.IsEnabled = true;
            }
            else
            {
                if (btnPrepareAddStudent != null)
                    btnPrepareAddStudent.IsEnabled = false;
            }
        }

        private void btnPrepareAddStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgvData.SelectedItem is DataRowView selectedClass)
                {
                    _currentAddingClassId = selectedClass["Mã Lớp"]?.ToString();

                    _originalDgvDataSource = dgvData.ItemsSource;
                    DataTable dtSingleClass = selectedClass.DataView.Table.Clone();
                    dtSingleClass.ImportRow(selectedClass.Row);
                    dgvData.ItemsSource = dtSingleClass.DefaultView;

                    pnlAvailableStudents.Visibility = Visibility.Visible;
                    rowDgvData.Height = new GridLength(1, GridUnitType.Star);
                    rowDgvAvailableStudents.Height = new GridLength(2, GridUnitType.Star);

                    DataTable dtYears = adminService.GetStudentBirthYears();
                    List<string> yearList = new List<string>();
                    foreach (DataRow row in dtYears.Rows)
                    {
                        if (row[0] != DBNull.Value) yearList.Add(row[0].ToString());
                    }
                    cbbNamSinh.ItemsSource = yearList;

                    if (cbbNamSinh.Items.Count > 0) cbbNamSinh.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnCancelAdd_Click(object sender, RoutedEventArgs e)
        {
            RestoreUI();
        }

        private void btnConfirmAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgvAvailableStudents.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một học sinh để thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!string.IsNullOrEmpty(_currentAddingClassId))
                {
                    foreach (var item in dgvAvailableStudents.SelectedItems)
                    {
                        if (item is DataRowView studentRow)
                        {
                            string studentId = studentRow["Mã Học Sinh"]?.ToString();
                            adminService.AddStudentToClass(studentId, _currentAddingClassId);
                        }
                    }

                    MessageBox.Show("Đã thêm học sinh thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    dgvHS.ItemsSource = adminService.GetStudentsByClass(_currentAddingClassId).DefaultView;

                    RestoreUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu vào DataBase:\n" + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreUI()
        {
            _currentAddingClassId = "";

            dgvData.ItemsSource = _originalDgvDataSource as System.Collections.IEnumerable;

            pnlAvailableStudents.Visibility = Visibility.Collapsed;

            if (rowDgvAvailableStudents != null && rowDgvData != null)
            {
                rowDgvAvailableStudents.Height = new GridLength(0);
                rowDgvData.Height = new GridLength(1, GridUnitType.Star);
            }

            dgvData.IsEnabled = true;
            btnPrepareAddStudent.IsEnabled = true;
        }

        private void cbbMH_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbMH.SelectedItem == null) return;

            string mon = GetKeyword(cbbMH.SelectedItem.ToString());

            cbbKhoi.ItemsSource = adminService.GetBlocksBySubject(mon).DefaultView;

            cbbKhoi.DisplayMemberPath = "Khoi";
            cbbKhoi.SelectedValuePath = "Khoi";

            if (cbbKhoi.Items.Count > 0)
            {
                cbbKhoi.SelectedIndex = 0;
            }
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

        private void cbbNamSinh_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbNamSinh.SelectedItem != null && !string.IsNullOrEmpty(_currentAddingClassId))
            {
                if (int.TryParse(cbbNamSinh.SelectedItem.ToString(), out int namSinh))
                {
                    DataTable dtHS = adminService.GetAvailableStudentsForClassByYear(_currentAddingClassId, namSinh);

                    dgvAvailableStudents.ItemsSource = dtHS.DefaultView;
                }
            }
        }
    }
}