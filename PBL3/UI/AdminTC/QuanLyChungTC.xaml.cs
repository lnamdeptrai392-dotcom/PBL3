using PBL3.UI.AdminTC.ChildForm;
using PBL3a.UI.Login;
using PBL3a.UI.Teacher;
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
            OpenChild(new TongQuan());
        }
        private void OpenChild(UserControl child)
        {
            child.HorizontalAlignment = HorizontalAlignment.Stretch;
            child.VerticalAlignment = VerticalAlignment.Stretch;

            paDesktop.Content = child;
        }

        private void btnThuHP_Click(object sender, RoutedEventArgs e)
        {
            HocPhi hocPhi = new HocPhi();
            OpenChild(hocPhi);
            
        }

        private void btnLuong_Click(object sender, RoutedEventArgs e)
        {
            LuongGV luongGV = new LuongGV();
            OpenChild(luongGV);
        }

        private void btnKhac_Click(object sender, RoutedEventArgs e)
        {
            KhoanChi khoanChi = new KhoanChi();
            OpenChild(khoanChi);
        }

        private void btnProfit_Click(object sender, RoutedEventArgs e)
        {
            Lai lai = new Lai();
            OpenChild (lai);
        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        { 
            LoginWindow login = new LoginWindow();
            login.Show();
            Close();
        }

        private void btnDashborad_Click(object sender, RoutedEventArgs e)
        {
            //dashboard tong quan danh sach sinh vien cham hoc phi trong ki hoc hien tai, danh sach giao vien da chi tra luong
            TongQuan tq = new TongQuan();
            OpenChild(tq);
        }
        private void btnThongKe_Click(object sender, RoutedEventArgs e)
        {
            //mo form thong ke
            ThongKe tk = new ThongKe();
            OpenChild(tk);
        }
    }
}