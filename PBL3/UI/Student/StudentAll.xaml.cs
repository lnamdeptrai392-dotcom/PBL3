using PBL3a.UI.Login;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.Student
{
    public partial class StudentAll : Window
    {
        public string StudentID { get; set; } = "";

        public StudentAll(string id)
        {
            InitializeComponent();
            StudentID = id;
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