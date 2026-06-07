using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PBL3.UI.AdminC.Windows;
using PBL3a.UI.AdminC.Windows;
using PBL3a.UI.Login;

namespace PBL3a.UI.AdminC
{
    public partial class AdminC : Window
    {
        
        public AdminC()
        {
            InitializeComponent();    
            OpenChild(new DuyetDon());
        }

        private void OpenChild(UserControl child)
        {
            panelDesktop.Content = child;
        }

        private void btnDuyetDon_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new DuyetDon());            
        }

        private void btnKhoaLop_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new XemKhoaLop());
            
        }
        private void btnTaoTK_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new TaoTaiKhoan());
            
        }

        private void btnTaoLH_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new TaoLopHoc());
            
        }

        private void butOut_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất không?",
                "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginWindow login = new LoginWindow();
                login.Show();

                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    parentWindow.Close();
                }
            }
        }
        private void btnDiemDanh_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new Attendance());            
        }

        private void btnStudentProfile_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new HoSoHocSinh());            
        }

        private void btnTeacherProfile_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new HoSoGiaoVien());            
        }
    }
}