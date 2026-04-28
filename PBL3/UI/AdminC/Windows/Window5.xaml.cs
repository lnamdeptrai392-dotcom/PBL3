using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminC.Windows
{
    public partial class Window5 : UserControl
    {
        DatabaseHelper dbHelper = new DatabaseHelper();
        DataTable dtPreview = new DataTable();

        public Window5()
        {
            InitializeComponent();
            Loaded += Window5_Load;
        }

        private void Window5_Load(object sender, RoutedEventArgs e)
        {
            cbbRole.Items.Add("Học sinh");
            cbbRole.Items.Add("Giáo viên");
            cbbRole.SelectedIndex = 0;

            dtPreview.Columns.Add("Id");
            dtPreview.Columns.Add("username");
            dtPreview.Columns.Add("Password");
            dtPreview.Columns.Add("name");
            dtPreview.Columns.Add("phone");
            dtPreview.Columns.Add("dateOfBirth");
            dtPreview.Columns.Add("sex");
            dtPreview.Columns.Add("Role");

            dgvData.ItemsSource = dtPreview.DefaultView;
        }

        private (string id, string user) GenerateId(string role, bool isNu, DateTime dob)
        {
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();

                if (role == "Học sinh")
                {
                    string prefix = (isNu ? "101" : "102") + dob.ToString("yy");

                    string query = "SELECT TOP 1 Id FROM accountList WHERE Id LIKE @p + '%' ORDER BY Id DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@p", prefix);
                        var result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            int stt = int.Parse(result.ToString().Substring(5, 3)) + 1;
                            string id = prefix + stt.ToString("D3");
                            return (id, id);
                        }
                        return (prefix + "001", prefix + "001");
                    }
                }
                else
                {
                    string query = "SELECT TOP 1 Id FROM accountList WHERE Role='Teacher' ORDER BY Id DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        var result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            int stt = int.Parse(result.ToString().Substring(1)) + 1;
                            return ("T" + stt.ToString("D3"), "teacher" + stt);
                        }
                        return ("T001", "teacher1");
                    }
                }
            }
        }

        private void btnTao_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Nhập đủ thông tin");
                return;
            }

            if (!rdoNam.IsChecked.Value && !rdoNu.IsChecked.Value)
            {
                MessageBox.Show("Chọn giới tính");
                return;
            }

            var (id, user) = GenerateId(
                cbbRole.SelectedItem.ToString(),
                rdoNu.IsChecked.Value,
                dtpDOB.SelectedDate ?? DateTime.Now
            );

            dtPreview.Rows.Clear();

            dtPreview.Rows.Add(
                id,
                user,
                "123456",
                txtFullName.Text,
                txtPhone.Text,
                (dtpDOB.SelectedDate ?? DateTime.Now).ToString("yyyy-MM-dd"),
                rdoNam.IsChecked.Value ? "Male" : "Female",
                cbbRole.SelectedItem.ToString() == "Học sinh" ? "Student" : "Teacher"
            );
        }

        private void btnThemDB_Click(object sender, RoutedEventArgs e)
        {
            if (dtPreview.Rows.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu");
                return;
            }

            DataRow row = dtPreview.Rows[0];

            string query = @"INSERT INTO accountList 
                            VALUES (@id,@user,@pass,@name,@phone,@dob,@sex,@role)";

            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", row["Id"]);
                    cmd.Parameters.AddWithValue("@user", row["username"]);
                    cmd.Parameters.AddWithValue("@pass", row["Password"]);
                    cmd.Parameters.AddWithValue("@name", row["name"]);
                    cmd.Parameters.AddWithValue("@phone", row["phone"]);
                    cmd.Parameters.AddWithValue("@dob", row["dateOfBirth"]);
                    cmd.Parameters.AddWithValue("@sex", row["sex"]);
                    cmd.Parameters.AddWithValue("@role", row["Role"]);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Đã thêm DB");
                dtPreview.Clear();
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtFullName.Clear();
            txtPhone.Clear();
            dtpDOB.SelectedDate = DateTime.Now;
            rdoNam.IsChecked = false;
            rdoNu.IsChecked = false;
            cbbRole.SelectedIndex = 0;
            dtPreview.Clear();
        }
    }
}