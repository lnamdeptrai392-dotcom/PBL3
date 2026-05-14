using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PBL3.UI.AdminC.Windows;
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
            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất không?",
                "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

            // Chỉ thực hiện đăng xuất nếu người dùng chọn Yes
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
            OpenChild(new DiemDanh());
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