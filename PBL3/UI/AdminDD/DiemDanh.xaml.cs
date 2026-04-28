using Microsoft.Data.SqlClient;
using PBL3a.services;
using PBL3a.UI.Login;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminDD
{
    public partial class DiemDanh : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private DataTable dtDiemDanh = new DataTable();

        public DiemDanh()
        {
            InitializeComponent();

            Loaded += DiemDanh_Load;
        }

        private void DiemDanh_Load(object sender, RoutedEventArgs e)
        {
            date.SelectedDate = DateTime.Now;
            SetupGrid();
            LoadClass();
        }

        private void LoadClass()
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                DateTime selectedDate = date.SelectedDate ?? DateTime.Now;

                int dayOfWeek = (int)selectedDate.DayOfWeek;
                if (dayOfWeek == 0)
                    dayOfWeek = 7;

                string query = @"
                    SELECT DISTINCT c.classID, c.class_name 
                    FROM Class c
                    JOIN ClassSchedule cs ON c.classID = cs.classID
                    WHERE cs.dayOfWeek = @dayOfWeek";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@dayOfWeek", dayOfWeek);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        cboLopHoc.ItemsSource = dt.DefaultView;
                        cboLopHoc.DisplayMemberPath = "class_name";
                        cboLopHoc.SelectedValuePath = "classID";
                        cboLopHoc.SelectedIndex = -1;
                    }
                }
            }
        }

        private void SetupGrid()
        {
            dtDiemDanh = new DataTable();
            dtDiemDanh.Columns.Add("AccountID");
            dtDiemDanh.Columns.Add("StudentName");
            dtDiemDanh.Columns.Add("Status");
            dtDiemDanh.Columns.Add("Note");

            dgvDiemDanh.ItemsSource = dtDiemDanh.DefaultView;
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            if (cboLopHoc.SelectedIndex == -1 || cboLopHoc.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn lớp");
                return;
            }

            int classID = Convert.ToInt32(cboLopHoc.SelectedValue);
            DateTime dateValue = (date.SelectedDate ?? DateTime.Now).Date;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT 
                        jc.AccountID,
                        al.name AS StudentName,
                        ISNULL(a.Status, N'Có mặt') AS Status,
                        ISNULL(a.Note, '') AS Note
                    FROM JoinClass jc
                    JOIN accountList al ON jc.AccountID = al.Id
                    LEFT JOIN Attendance a 
                        ON a.AccountID = jc.AccountID 
                        AND a.ClassID = jc.classID
                        AND a.AttendanceDate = @date
                    WHERE jc.classID = @classID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@classID", classID);
                    cmd.Parameters.AddWithValue("@date", dateValue);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dtDiemDanh.Rows.Clear();

                        while (reader.Read())
                        {
                            dtDiemDanh.Rows.Add(
                                reader["AccountID"].ToString(),
                                reader["StudentName"].ToString(),
                                reader["Status"].ToString(),
                                reader["Note"].ToString()
                            );
                        }
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cboLopHoc.SelectedIndex == -1 || cboLopHoc.SelectedValue == null)
            {
                MessageBox.Show("Chọn lớp trước");
                return;
            }

            int classID = Convert.ToInt32(cboLopHoc.SelectedValue);
            DateTime attendanceDate = (date.SelectedDate ?? DateTime.Now).Date;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                foreach (DataRow row in dtDiemDanh.Rows)
                {
                    string accountID = row["AccountID"].ToString();
                    string status = row["Status"]?.ToString() ?? "Có mặt";
                    string note = row["Note"]?.ToString() ?? "";

                    string check = @"
                        SELECT COUNT(*) 
                        FROM Attendance 
                        WHERE AccountID = @acc 
                        AND ClassID = @class 
                        AND AttendanceDate = @date";

                    using (SqlCommand checkCmd = new SqlCommand(check, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@acc", accountID);
                        checkCmd.Parameters.AddWithValue("@class", classID);
                        checkCmd.Parameters.AddWithValue("@date", attendanceDate);

                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            string update = @"
                                UPDATE Attendance 
                                SET Status = @status, Note = @note
                                WHERE AccountID = @acc 
                                AND ClassID = @class 
                                AND AttendanceDate = @date";

                            using (SqlCommand updateCmd = new SqlCommand(update, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@status", status);
                                updateCmd.Parameters.AddWithValue("@note", note);
                                updateCmd.Parameters.AddWithValue("@acc", accountID);
                                updateCmd.Parameters.AddWithValue("@class", classID);
                                updateCmd.Parameters.AddWithValue("@date", attendanceDate);

                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string insert = @"
                                INSERT INTO Attendance
                                (AccountID, ClassID, AttendanceDate, Status, Note)
                                VALUES
                                (@acc, @class, @date, @status, @note)";

                            using (SqlCommand insertCmd = new SqlCommand(insert, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@acc", accountID);
                                insertCmd.Parameters.AddWithValue("@class", classID);
                                insertCmd.Parameters.AddWithValue("@date", attendanceDate);
                                insertCmd.Parameters.AddWithValue("@status", status);
                                insertCmd.Parameters.AddWithValue("@note", note);

                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                MessageBox.Show("Lưu điểm danh thành công!");
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Window currentWindow = Window.GetWindow(this);

            LoginWindow login = new LoginWindow();
            login.Show();

            currentWindow?.Close();
        }

        private void date_ValueChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                LoadClass();
        }
    }
}