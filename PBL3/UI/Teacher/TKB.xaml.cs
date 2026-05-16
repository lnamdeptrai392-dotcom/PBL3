using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.Teacher
{
    public partial class TKB : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private string currentTeacherID;

        public TKB(string teacherId)
        {
            currentTeacherID = teacherId;
            InitializeComponent();

            Loaded += TKB_Load;
        }

        private void TKB_Load(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(currentTeacherID))
            {
                MessageBox.Show("Không tìm thấy mã giảng viên!");
                return;
            }

            LoadClassList();
        }

        private void LoadClassList()
        {
            string query = @"
                SELECT classID, class_name
                FROM Class
                WHERE teacherID = @TeacherID AND status = N'Đang mở'
                ORDER BY classID";

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
                                comboBox1.ItemsSource = null;
                                textBox1.Text = "";
                                dataGridView1.ItemsSource = null;

                                MessageBox.Show("Giảng viên chưa có lớp học nào đang mở!");
                                return;
                            }

                            comboBox1.ItemsSource = dt.DefaultView;
                            comboBox1.DisplayMemberPath = "class_name";
                            comboBox1.SelectedValuePath = "classID";

                            if (comboBox1.SelectedItem != null)
                            {
                                DataRowView row = (DataRowView)comboBox1.SelectedItem;
                                textBox1.Text = row["class_name"].ToString();
                                LoadScheduleByClass(row["classID"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load lớp: " + ex.Message);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox1.SelectedItem is DataRowView row)
            {
                textBox1.Text = row["class_name"].ToString();

                if (comboBox1.SelectedValue != null)
                {
                    LoadScheduleByClass(comboBox1.SelectedValue.ToString());
                }
            }
        }

        private void LoadScheduleByClass(string classID)
        {
            // Tạo DataTable với cấu trúc khớp với DataGrid (4 cột)
            DataTable dt = new DataTable();
            dt.Columns.Add("Thu", typeof(string));
            dt.Columns.Add("Ca", typeof(string));
            dt.Columns.Add("ThoiGian", typeof(string));
            dt.Columns.Add("TrangThai", typeof(string));

            string query = @"
                SELECT 
                    cs.dayOfWeek,
                    cs.startTime,
                    cs.endTime
                FROM ClassSchedule cs
                JOIN Class c ON cs.classID = c.classID
                WHERE c.classID = @ClassID AND c.status = N'Đang mở'
                ORDER BY cs.dayOfWeek, cs.startTime";

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClassID", classID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DataRow row = dt.NewRow();

                                // Xác định thứ
                                int dayOfWeek = reader.GetInt32(0);
                                switch (dayOfWeek)
                                {
                                    case 1: row["Thu"] = "Thứ 2"; break;
                                    case 2: row["Thu"] = "Thứ 3"; break;
                                    case 3: row["Thu"] = "Thứ 4"; break;
                                    case 4: row["Thu"] = "Thứ 5"; break;
                                    case 5: row["Thu"] = "Thứ 6"; break;
                                    case 6: row["Thu"] = "Thứ 7"; break;
                                    case 7: row["Thu"] = "Chủ nhật"; break;
                                    default: row["Thu"] = "Không xác định"; break;
                                }

                                // Xác định ca dạy dựa trên giờ
                                TimeSpan startTime = reader.GetTimeSpan(1);
                                if (startTime.Hours >= 7 && startTime.Hours < 12)
                                    row["Ca"] = "Sáng";
                                else if (startTime.Hours >= 13 && startTime.Hours < 17)
                                    row["Ca"] = "Chiều";
                                else if (startTime.Hours >= 17 && startTime.Hours < 21)
                                    row["Ca"] = "Tối";
                                else
                                    row["Ca"] = "Khác";

                                // Thời gian
                                string start = reader.GetTimeSpan(1).ToString(@"hh\:mm");
                                string end = reader.GetTimeSpan(2).ToString(@"hh\:mm");
                                row["ThoiGian"] = $"{start} - {end}";

                                // Trạng thái
                                row["TrangThai"] = GetTrangThai(startTime, reader.GetTimeSpan(2), dayOfWeek);

                                dt.Rows.Add(row);
                            }
                        }
                    }
                }

                dataGridView1.ItemsSource = dt.DefaultView;

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Lớp học này chưa có lịch trình!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load thời khóa biểu: " + ex.Message);
            }
        }

        // Hàm xác định trạng thái buổi học
        private string GetTrangThai(TimeSpan startTime, TimeSpan endTime, int dayOfWeek)
        {
            DateTime now = DateTime.Now;
            DateTime today = now.Date;

            // Tính ngày của buổi học trong tuần này
            int currentDayOfWeek = (int)now.DayOfWeek;
            if (currentDayOfWeek == 0) currentDayOfWeek = 7; // Chủ nhật = 7

            int daysUntil = dayOfWeek - currentDayOfWeek;
            DateTime classDate = today.AddDays(daysUntil);

            TimeSpan classStart = new TimeSpan(startTime.Hours, startTime.Minutes, 0);
            TimeSpan classEnd = new TimeSpan(endTime.Hours, endTime.Minutes, 0);

            if (classDate < today)
                return "Đã kết thúc";
            else if (classDate > today)
                return "Sắp diễn ra";
            else // Hôm nay
            {
                TimeSpan nowTime = now.TimeOfDay;
                if (nowTime < classStart)
                    return "Sắp diễn ra";
                else if (nowTime >= classStart && nowTime <= classEnd)
                    return "Đang diễn ra";
                else
                    return "Đã kết thúc";
            }
        }
    }
}