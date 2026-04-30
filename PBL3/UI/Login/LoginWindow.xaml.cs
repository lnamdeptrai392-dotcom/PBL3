using System;
using System.Windows;
using Microsoft.Data.SqlClient;
using PBL3a.services;

namespace PBL3a.UI.Login
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseHelper db = new DatabaseHelper();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();

            string password = chkShowPassword.IsChecked == true
                ? txtPasswordVisible.Text.Trim()
                : txtPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu");
                return;
            }

            using (SqlConnection conn = db.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT Id, Role 
                        FROM accountList 
                        WHERE Username = @u AND Password = @p";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@p", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("Sai tài khoản hoặc mật khẩu");
                                return;
                            }

                            string role = reader["Role"] == DBNull.Value ? "" : reader["Role"].ToString();
                            string userId = reader["Id"] == DBNull.Value ? "" : reader["Id"].ToString();

                            OpenWindowByRole(role, userId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi SQL: " + ex.Message);
                }
            }
        }

        private void OpenWindowByRole(string role, string userId)
        {
            Window next = null;

            switch (role)
            {
                case "AdminC":
                    next = new PBL3a.UI.AdminC.AdminC();
                    break;

                //case "AdminDD":
                //    next = new PBL3a.UI.AdminDD.AdminDDWindow();
                //    break;

                case "AdminTC":
                    next = new PBL3a.UI.AdminTC.QuanLyChungTC();
                    break;

                case "Student":
                    next = new PBL3a.UI.Student.StudentAll(userId);
                    break;

                case "Teacher":
                    next = new PBL3a.UI.Teacher.QuanLyChungT(userId);
                    break;

                default:
                    MessageBox.Show("Role chưa hỗ trợ: " + role);
                    return;
            }

            next.Show();
            Close();
        }

        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordVisible.Visibility = Visibility.Visible;
        }

        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
            txtPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
        }
    }
}