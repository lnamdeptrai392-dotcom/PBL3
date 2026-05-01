using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PBL3a.UI.AdminTC
{
    public partial class HocPhi : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private DataTable dtHocPhi = new DataTable();

        public HocPhi()
        {
            InitializeComponent();
            Loaded += HocPhi_Load;
        }

        private void HocPhi_Load(object sender, RoutedEventArgs e)
        {
            // Tắt tự động tạo cột để tránh bị chồng cột
            dataGridView1.AutoGenerateColumns = false;
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dataGridView1.CanUserAddRows = false;
            dataGridView1.SelectionMode = DataGridSelectionMode.Single;
            dataGridView1.SelectionUnit = DataGridSelectionUnit.FullRow;
        }

        // Hàm lọc Mã Lớp dựa trên Khối và Môn học
        private void FilterClass()
        {
            if (cbbGrade == null || cbbCourse == null || cbbML == null) return;

            string selectedGrade = (cbbGrade.SelectedItem as ComboBoxItem)?.Content.ToString();
            string selectedCourseTag = (cbbCourse.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            // Nếu chưa chọn cả 2 thì chưa lọc
            if (string.IsNullOrEmpty(selectedGrade) || string.IsNullOrEmpty(selectedCourseTag)) return;

            cbbML.Items.Clear();
            tbTL.Text = ""; // Xóa tên lớp cũ

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    // Truy vấn dựa trên cấu trúc database mới (grade và courseID)
                    string query = "SELECT classID FROM Class WHERE grade = @grade AND courseID = @courseID ORDER BY classID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@grade", selectedGrade);
                        cmd.Parameters.AddWithValue("@courseID", selectedCourseTag);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cbbML.Items.Add(reader["classID"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Lỗi kết nối dữ liệu: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Khi chọn Khối hoặc Môn học đều gọi chung hàm FilterClass
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterClass();
        }

        private void cbbML_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbML.SelectedItem == null) return;

            string classID = cbbML.SelectedItem.ToString();
            LoadTenLop(classID);
            LoadHocPhiTheoLop(classID);
        }

        private void LoadTenLop(string classID)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query = "SELECT class_name FROM Class WHERE classID = @classID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@classID", classID);
                    object result = cmd.ExecuteScalar();
                    tbTL.Text = result != null ? result.ToString() : "";
                }
            }
        }

        private void LoadHocPhiTheoLop(string classID)
        {
            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    // INNER JOIN với accountList để lấy tên học sinh
                    string query = @"
                    SELECT 
                    a.Id AS [AccountID], 
                    a.name AS [HoTen], 
                    
                    ISNULL(CAST(hp.TuitionMonth AS NVARCHAR), N'--') AS [TuitionMonth], 
                    
                    ISNULL(hp.SoTien, 0) AS [SoTien], 
                    
                    ISNULL(hp.TrangThai, N'Chưa thiết lập') AS [TrangThai]
                FROM JoinClass jc
                INNER JOIN accountList a ON jc.AccountID = a.Id
                LEFT JOIN HocPhi hp ON jc.AccountID = hp.AccountID AND jc.classID = hp.ClassID
                WHERE jc.classID = @classID
                ORDER BY hp.TrangThai DESC, a.name ASC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@classID", classID);
                        dtHocPhi = new DataTable();
                        adapter.Fill(dtHocPhi);

                        // Gán ItemsSource cho DataGrid đã định nghĩa sẵn cột trong XAML
                        dataGridView1.ItemsSource = dtHocPhi.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải danh sách học phí: " + ex.Message);
            }
        }

        private void btSetHP_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cbbML.Text))
            {
                MessageBox.Show("Vui lòng chọn lớp!");
                return;
            }
            string malop = cbbML.Text;
            ThietLapHP thietLap = new ThietLapHP(malop);
            thietLap.ShowDialog();
            LoadHocPhiTheoLop(malop);
        }

        // Các hàm phụ khác bạn giữ nguyên hoặc xóa nếu không dùng
    }
}