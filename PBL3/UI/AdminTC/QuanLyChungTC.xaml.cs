using PBL3a.UI.Login;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PBL3a.UI.AdminTC
{
    public partial class QuanLyChungTC : Window
    {
        private Button currentButton;

        public QuanLyChungTC()
        {
            InitializeComponent();
        }

        private void OpenChild(UserControl child, object sender)
        {
            
            paDesktop.Content = child;
        }

        

        private void btnFee_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new HocPhi(), sender);
        }

        private void btnSalary_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new LuongGV(), sender);
        }

        private void btnProfit_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new Lai(), sender);
        }

        private void btKT_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new KhoanThu(), sender);
        }

        private void btKC_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new KhoanChi(), sender);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow(); // WPF Window
            login.Show();
            this.Close();
        }
    }
}