using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PBL3a.UI.AdminC.Windows;
using PBL3a.UI.Login;

namespace PBL3a.UI.AdminC
{
    public partial class AdminC : Window
    {
        private readonly Brush defaultColor = new SolidColorBrush(Color.FromRgb(112, 146, 190));
        private readonly Brush activeColor = new SolidColorBrush(Color.FromRgb(70, 130, 180));

        public AdminC()
        {
            InitializeComponent();
            SetActiveMenu(btnDuyetDon);
            OpenChild(new Window1());
        }

        private void SetActiveMenu(Button clickedButton)
        {
            btnDuyetDon.Background = defaultColor;
            btnKhoaLop.Background = defaultColor;
            btnTaoTK.Background = defaultColor;
            btnTaoLH.Background = defaultColor;

            clickedButton.Background = activeColor;
        }

        private void OpenChild(UserControl child)
        {
            panelDesktop.Content = child;
        }

        private void btnDuyetDon_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(btnDuyetDon);
            OpenChild(new Window1());
        }

        private void btnKhoaLop_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(btnKhoaLop);
            OpenChild(new Window4());
        }

        private void btnTaoTK_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(btnTaoTK);
            OpenChild(new Window5());
        }

        private void btnTaoLH_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(btnTaoLH);
            OpenChild(new Window2());
        }

        private void butOut_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            Close();
        }
    }
}