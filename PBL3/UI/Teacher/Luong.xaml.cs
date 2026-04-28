using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.Teacher
{
    public partial class Luong : UserControl
    {
        private readonly DatabaseHelper db = new DatabaseHelper();
        private readonly string currentTeacherID;

        public Luong(string teacherId)
        {
            InitializeComponent();
            currentTeacherID = teacherId;

            Loaded += Luong_Load;
        }

        private void Luong_Load(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(currentTeacherID))
            {
                MessageBox.Show("Không tìm thấy mã giảng viên!", "Lỗi");
                return;
            }

            LoadSalaryData();
        }

        private void LoadSalaryData()
        {
            string query = @"
                SELECT 
                    SalaryMonth AS [Tháng],
                    SalaryYear AS [Năm],
                    SoLopDay AS [Số lớp dạy],
                    SoBuoiDay AS [Số buổi dạy],
                    LuongCoBan AS [Lương cơ bản],
                    Thuong AS [Thưởng],
                    TongLuong AS [Tổng lương],
                    TrangThai AS [Trạng thái],
                    NgayThanhToan AS [Ngày thanh toán],
                    GhiChu AS [Ghi chú]
                FROM LuongGV
                WHERE TeacherID = @TeacherID
                ORDER BY SalaryYear DESC, SalaryMonth DESC";

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TeacherID", currentTeacherID);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count == 0)
                            {
                                dataGridView1.ItemsSource = null;
                                MessageBox.Show("Giảng viên này chưa có dữ liệu lương.", "Thông báo");
                                return;
                            }

                            dataGridView1.ItemsSource = dt.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu lương: " + ex.Message, "Lỗi");
            }
        }
    }
}