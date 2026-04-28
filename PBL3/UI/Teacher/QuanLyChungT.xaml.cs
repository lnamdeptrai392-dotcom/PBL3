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

            ActivateButton(btnInfo);
            OpenChildForm(new TTCN(currentTeacherID));
        }

        private void OpenChildForm(UserControl childForm)
        {
            panelMain.Content = childForm;
        }

        private void ActivateButton(Button btn)
        {
            if (currentButton != null)
            {
                currentButton.Background = System.Windows.Media.Brushes.Transparent;
            }

            currentButton = btn;
            currentButton.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(62, 106, 131)
            );
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            ActivateButton(btnInfo);
            OpenChildForm(new TTCN(currentTeacherID));
        }

        private void btnSchedule_Click(object sender, RoutedEventArgs e)
        {
            ActivateButton(btnSchedule);
            OpenChildForm(new TKB(currentTeacherID));
        }

        private void btnScore_Click(object sender, RoutedEventArgs e)
        {
            ActivateButton(btnScore);
            OpenChildForm(new NhapDiem(currentTeacherID));
        }

        private void btnSalary_Click(object sender, RoutedEventArgs e)
        {
            ActivateButton(btnSalary);
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