using PBL3a.UI.Login;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using PBL3a.services;

namespace PBL3a.UI.Student
{
    public partial class StudentAll : Window
    {
        public string StudentID { get; set; } = "";
        private readonly DatabaseHelper db = new DatabaseHelper();
        public StudentAll(string id)
        {
            InitializeComponent();

            StudentID = id;
            LoadUserName(StudentID);
            OpenChild(new StudentINFO(StudentID));
        }
        private void LoadUserName(string id)
        {
            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT name FROM accountList WHERE Id = @id";
                    using (SqlCommand cmd = new SqlCommand(query,conn ))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        string name = cmd.ExecuteScalar()?.ToString() ?? "Học sinh";
                        tbUserName.Text = name;
                        tbAvatarChar.Text = name.Length > 0
                                            ? name[0].ToString().ToUpper() : "H";
                    }
                }
            }
            catch
            {
                tbUserName.Text = "Học sinh";
                tbAvatarChar.Text = "H";
            }
        }
        private void OpenChild(UserControl child)
        {
            panelChildBox.Content = child;
        }

        private void btn_info_Click_1(object sender, RoutedEventArgs e)
        {
            OpenChild(new StudentINFO(StudentID));
        }

        private void btn_score_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new StudentScore(StudentID));
        }

        private void btn_schedule_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new StudentSchedule(StudentID));
        }

        private void btn_fee_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new StudentFee(StudentID));
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất không?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                var login = new LoginWindow();
                login.Show();

                this.Close();
            }
        }
    }
}