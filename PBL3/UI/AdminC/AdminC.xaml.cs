using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PBL3a.UI.AdminC.Windows;
using PBL3a.UI.AdminDD;
using PBL3a.UI.Login;

namespace PBL3a.UI.AdminC
{
    public partial class AdminC : Window
    {
        public AdminC()
        {
            InitializeComponent();    
            OpenChild(new Window1());
        }

        private void OpenChild(UserControl child)
        {
            panelDesktop.Content = child;
        }

        private void btnDuyetDon_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new Window1());
            //Duyệt đơn
        }

        private void btnKhoaLop_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new Window4());
            //Xem khóa lớp
        }
        private void btnTaoTK_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new Window5());
            //Tạo tài khoản
        }

        private void btnTaoLH_Click(object sender, RoutedEventArgs e)
        {  
            OpenChild(new Window2());
            //Tạo lớp học
        }

        private void butOut_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            Close();
        }
        private void btnDiemDanh_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new DiemDanh());
        }

        private void btnStudentProfile_Click(object sender, RoutedEventArgs e)
        {
            //mo ra  ho so hoc sinh, xem danh sach hoc sinh
        }

        private void btnTeacherProfile_Click(object sender, RoutedEventArgs e)
        {
            //mo ra xem ho so giao vien
        }
    }
}