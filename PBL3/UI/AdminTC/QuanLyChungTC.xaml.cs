using PBL3.UI.AdminTC.ChildForm;
using PBL3a.UI.AdminTC.ChildForm;
using PBL3a.UI.Login;
using PBL3a.UI.Teacher;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminTC
{
    public partial class QuanLyChungTC : Window
    {
        public QuanLyChungTC()
        {
            InitializeComponent();
            OpenChild(new TongQuan(), btnDashboard);
        }

        private void OpenChild(UserControl child, Button selectedButton = null)
        {
            child.HorizontalAlignment = HorizontalAlignment.Stretch;
            child.VerticalAlignment = VerticalAlignment.Stretch;

            paDesktop.Content = child;

            if (selectedButton != null)
            {
                UpdateButtonState(selectedButton);
            }
        }

        private void UpdateButtonState(Button activeButton)
        {
            // ĐỒNG BỘ: Đã thêm btnThuKhac vào mảng để đồng bộ hiệu ứng đổi màu Active/Normal
            Button[] menuButtons = { btnDashboard, btnThuHP, btnLuong, btnKhac,btnThuKhac, btnProfit, btnThongKe, btnTheoDoiHP };

            // Lấy style của các nút từ Resource
            Style normalStyle = (Style)this.FindResource("MenuButtonStyle");
            Style activeStyle = (Style)this.FindResource("MenuButtonActiveStyle");

            foreach (Button btn in menuButtons)
            {
                if (btn == null) continue; // Phòng trường hợp trùng tên hoặc chưa khởi tạo bên XAML

                // gán cho nút được bấm
                btn.Style = (btn == activeButton) ? activeStyle : normalStyle;
            }
        }

        private void btnThuHP_Click(object sender, RoutedEventArgs e)
        {
            HocPhi hocPhi = new HocPhi();
            OpenChild(hocPhi, btnThuHP);
        }

        private void btnLuong_Click(object sender, RoutedEventArgs e)
        {
            LuongGV luongGV = new LuongGV();
            OpenChild(luongGV, btnLuong);
        }

        private void btnKhac_Click(object sender, RoutedEventArgs e)
        {
            KhoanChi khoanChi = new KhoanChi();
            OpenChild(khoanChi, btnKhac);
        }

        // ĐÃ THÊM: Sự kiện Click xử lý mở giao diện cho Khoản thu khác
        private void btnThuKhac_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnProfit_Click(object sender, RoutedEventArgs e)
        {
            Lai lai = new Lai();
            OpenChild(lai, btnProfit);
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
            OpenChild(tq, btnDashboard);
        }

        private void btnThongKe_Click(object sender, RoutedEventArgs e)
        {
            //mo form thong ke
            ThongKe tk = new ThongKe();
            OpenChild(tk, btnThongKe);
        }

        private void btnTheoDoiHP_Click(object sender, RoutedEventArgs e)
        {
            //mo form theo doi hoc phi
            TheoDoiHocPhi theoDoiHocPhi = new TheoDoiHocPhi();
            OpenChild(theoDoiHocPhi, btnTheoDoiHP);
        }

        private void btnThuKhac_Click_1(object sender, RoutedEventArgs e)
        {
            // Khởi tạo UserControl quản lý khoản thu (Bạn tạo file KhoanThu.xaml tương tự KhoanChi.xaml nhé)
            KhoanThu khoanThu = new KhoanThu();
            OpenChild(khoanThu, btnThuKhac);
        }
    }
}