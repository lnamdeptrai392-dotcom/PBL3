using System.Windows;

namespace PBL3a.UI.AdminDD
{
    public partial class AdminDDWindow : Window
    {
        public AdminDDWindow()
        {
            InitializeComponent();
            MainContent.Content = new DiemDanh();
        }
    }
}