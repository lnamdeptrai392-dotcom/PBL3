using PBL3a.UI.AdminTC;
using PBL3a.UI.Login;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.Teacher
{
    public partial class QuanLyChungT : Window
    {
        private readonly string currentTeacherID;
        private Button currentButton;

        public QuanLyChungT(string teacherId)
        {
            InitializeComponent();
            currentTeacherID = teacherId;
            OpenChildForm(new TTCN(currentTeacherID));
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