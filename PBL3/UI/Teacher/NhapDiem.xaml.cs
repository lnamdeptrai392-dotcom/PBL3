using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PBL3a.UI.Teacher
{
    public partial class NhapDiem : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private List<string> dsLop = new List<string>();
        private bool isFiltering = false;
        private string currentTeacherID;

        public NhapDiem(string teacherId)
        {
            InitializeComponent();
            currentTeacherID = teacherId;

            Loaded += NhapDiem_Load;
        }

        private void NhapDiem_Load(object sender, RoutedEventArgs e)
        {
            LoadDanhSachLop();
        }

        private void LoadDanhSachLop()
        {
            dsLop.Clear();

            string query = @"SELECT classID 
                             FROM Class 
                             WHERE teacherID = @id";

            using (SqlConnection conn = db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", currentTeacherID);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dsLop.Add(reader["classID"].ToString());
                    }
                }
            }

            comboBox1.ItemsSource = dsLop;
        }

        private void comboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (isFiltering) return;

            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Escape)
                return;

            string keyword = comboBox1.Text.Trim();

            var filtered = string.IsNullOrWhiteSpace(keyword)
                ? dsLop
                : dsLop.Where(x => x.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            isFiltering = true;

            comboBox1.ItemsSource = null;
            comboBox1.ItemsSource = filtered;
            comboBox1.Text = keyword;
            comboBox1.IsDropDownOpen = filtered.Count > 0;

            isFiltering = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox1.SelectedItem == null) return;

            string classId = comboBox1.SelectedItem.ToString();
            LoadTenLop(classId);
            LoadHocSinhTheoLop(classId);
        }

        private void LoadTenLop(string classId)
        {
            string query = "SELECT class_name FROM Class WHERE classID = @id";

            using (SqlConnection conn = db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", classId);
                conn.Open();

                var result = cmd.ExecuteScalar();
                textBox1.Text = result?.ToString() ?? "";
            }
        }

        private void LoadHocSinhTheoLop(string classId)
        {
            string query = @"
                SELECT 
                    a.Id AS AccountID,
                    a.name AS HoTen,
                    d.Diem,
                    d.NhanXet
                FROM accountList a
                INNER JOIN JoinClass jc ON a.Id = jc.AccountID
                LEFT JOIN Diem d ON a.Id = d.AccountID AND jc.classID = d.ClassID
                WHERE jc.classID = @id";

            using (SqlConnection conn = db.GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                da.SelectCommand.Parameters.AddWithValue("@id", classId);

                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.ItemsSource = dt.DefaultView;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn lớp!");
                return;
            }

            string classId = comboBox1.SelectedItem.ToString();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                try
                {
                    foreach (DataRowView row in dataGridView1.ItemsSource)
                    {
                        string accountId = row["AccountID"]?.ToString();
                        string diemText = row["Diem"]?.ToString();
                        string nhanXet = row["NhanXet"]?.ToString();

                        object diemValue = DBNull.Value;

                        if (!string.IsNullOrWhiteSpace(diemText))
                        {
                            double diem = Convert.ToDouble(diemText);
                            if (diem < 0 || diem > 10)
                                throw new Exception("Điểm phải từ 0-10");

                            diemValue = diem;
                        }

                        string query = @"
                        IF EXISTS (SELECT 1 FROM Diem WHERE AccountID=@a AND ClassID=@c)
                        UPDATE Diem SET Diem=@d, NhanXet=@n WHERE AccountID=@a AND ClassID=@c
                        ELSE
                        INSERT INTO Diem VALUES(@a,@c,@d,@n)";

                        using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@a", accountId);
                            cmd.Parameters.AddWithValue("@c", classId);
                            cmd.Parameters.AddWithValue("@d", diemValue);
                            cmd.Parameters.AddWithValue("@n",
                                string.IsNullOrWhiteSpace(nhanXet) ? DBNull.Value : (object)nhanXet);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    MessageBox.Show("Lưu thành công!");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }
    }
}