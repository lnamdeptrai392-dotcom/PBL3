using PBL3a.UI.Login;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PBL3a.UI.AdminTC
{
    public partial class QuanLyChungTC : Window
    {
        
        public QuanLyChungTC()
        {
            InitializeComponent();
        }

        private void OpenChild(UserControl child, object sender)
        {
            
            paDesktop.Content = child;
        }

        private void btnDshs_Click(object sender, RoutedEventArgs e)
        {
            //tao mot xaml show danh sach hoc sinh
        }

        private void btnDsgv_Click(object sender, RoutedEventArgs e)
        {
            //tao mot xaml show danh sach giao vien
        }

        private void btnThuHP_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new HocPhi(), sender);
        }

        private void btnLuong_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new LuongGV(), sender);
        }

        private void btnKhac_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new KhoanThu(), sender);
            //OpenChild(new KhoanChi(), sender);
        }

        private void btnLshd_Click(object sender, RoutedEventArgs e)
        {
            //OpenChild(new LSGD(),sender);
        }
        private void btnProfit_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new Lai(), sender);
        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow(); // WPF Window
            login.Show();
            this.Close();
        }
    }
}