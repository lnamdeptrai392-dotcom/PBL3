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
            
        }
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbGrade == null || cbbCourse == null || cbbML == null) return;

            // Lấy giá trị từ ComboBox
            string selectedGrade = (cbbGrade.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string selectedCourseTag = (cbbCourse.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            if (string.IsNullOrEmpty(selectedGrade) || string.IsNullOrEmpty(selectedCourseTag)) return;

            cbbML.Items.Clear();
            dataGridView1.ItemsSource = null; // Xóa bảng khi đổi bộ lọc

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT classID FROM Class WHERE grade = @grade AND courseID = @courseID ORDER BY classID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@grade", selectedGrade);
                        cmd.Parameters.AddWithValue("@courseID", selectedCourseTag);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Thêm trực tiếp string vào Items
                                cbbML.Items.Add(reader["classID"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lớp: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HocPhi_Load(object sender, RoutedEventArgs e)
        {
            
            dataGridView1.AutoGenerateColumns = false;
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dataGridView1.CanUserAddRows = false;
            dataGridView1.SelectionMode = DataGridSelectionMode.Single;
            dataGridView1.SelectionUnit = DataGridSelectionUnit.FullRow;
        }

        private void LoadHocPhiTheoLop(string classID)
        {
            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = @"
                SELECT 
                    a.Id AS [AccountID], 
                    a.name AS [HoTen], 
                    ISNULL(hp.SoTien, 0) AS [SoTien], 
                    ISNULL(CONVERT(NVARCHAR, hp.NgayDong, 103), N'--') AS [NgayDong],
                    ISNULL(hp.TrangThai, N'Chưa thiết lập') AS [TrangThai]
                FROM JoinClass jc
                INNER JOIN accountList a ON jc.AccountID = a.Id
                LEFT JOIN HocPhi hp 
                    ON jc.AccountID = hp.AccountID 
                    AND jc.classID = hp.ClassID
                WHERE jc.classID = @classID
                ORDER BY hp.TrangThai DESC, a.name ASC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@classID", classID);
                        dtHocPhi = new DataTable();
                        adapter.Fill(dtHocPhi);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cbbML.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn mã lớp trước khi xem!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string classID = cbbML.SelectedItem.ToString();
            LoadHocPhiTheoLop(classID);
        }
    }
}