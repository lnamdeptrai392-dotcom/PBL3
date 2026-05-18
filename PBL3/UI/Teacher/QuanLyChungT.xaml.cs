using PBL3a.services;
using PBL3a.UI.AdminTC;
using PBL3a.UI.Login;
using System.Windows;
using Microsoft.Data.SqlClient;
using System.Windows.Controls;

namespace PBL3a.UI.Teacher
{
    public partial class QuanLyChungT : Window
    {
        private readonly string currentTeacherID;
        private readonly DatabaseHelper db = new DatabaseHelper();

        public QuanLyChungT(string teacherId)
        {
            InitializeComponent();
            currentTeacherID = teacherId;
            LoadUserName(currentTeacherID);
            OpenChildForm(new TTCN(currentTeacherID));
        }
        private void LoadUserName(string id)
        {
            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT name FROM accountList WHERE Id = @id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        string name = cmd.ExecuteScalar()?.ToString() ?? "Giáo viên";

                        tbUserName.Text = name;
                        tbAvatarChar.Text = name.Length > 0
                                            ? name[0].ToString().ToUpper() : "G";
                    }

                }
            }
            catch
            {
                tbUserName.Text = "Giáo viên";
                tbAvatarChar.Text = "G";
            }
        }
        private void OpenChildForm(UserControl childForm)
        {
            panelMain.Content = childForm;
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            
            OpenChildForm(new TTCN(currentTeacherID));
        }

        private void btnSchedule_Click(object sender, RoutedEventArgs e)
        {
            
            OpenChildForm(new TKB(currentTeacherID));
        }

        private void btnScore_Click(object sender, RoutedEventArgs e)
        {
            
            OpenChildForm(new NhapDiem(currentTeacherID));
        }

        private void btnSalary_Click(object sender, RoutedEventArgs e)
        {
            
            OpenChildForm(new Luong(currentTeacherID));
        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            this.Close();
        }
    }
}