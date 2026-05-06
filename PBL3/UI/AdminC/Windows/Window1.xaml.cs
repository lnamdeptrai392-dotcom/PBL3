using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PBL3a.services.BLL;

namespace PBL3a.UI.AdminC.Windows
{
    public partial class Window1 : UserControl
    {
        private AdminC_Service adminService = new AdminC_Service();
        private DataTable dtActiveClasses;

        public Window1()
        {
            InitializeComponent();
            Loaded += Window1_Load;
        }

        private void Window1_Load(object sender, RoutedEventArgs e)
        {
            dtActiveClasses = adminService.GetActiveClasses();

            cbbMH.ItemsSource = new List<string>
            {
                "Tất cả","Toán học","Vật lý","Hóa học",
                "Sinh học","Ngữ văn","Tiếng Anh"
            };

            cbbMH.SelectedIndex = 0;
            LoadData();
        }

        private void LoadData()
        {
            dgvDonXin.ItemsSource = adminService.GetPendingRegistrations().DefaultView;
        }

        private string GetKeyword(string monHoc)
        {
            if (monHoc == "Toán học") return "Toán";
            if (monHoc == "Vật lý") return "Lý";
            if (monHoc == "Hóa học") return "Hóa";
            if (monHoc == "Sinh học") return "Sinh";
            if (monHoc == "Ngữ văn") return "Văn";
            if (monHoc == "Tiếng Anh") return "Tiếng Anh";
            return monHoc;
        }

        private void dgvDonXin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvDonXin.SelectedItem is DataRowView row)
            {
                string maHS = row["Mã HS"]?.ToString();
                string maLop = row["Mã Lớp"]?.ToString();

                LoadThongTinHocSinh(maHS);
                LoadThongTinLopHoc(maLop);
                LoadLichSuHocTap(maHS);
            }
        }

        private void LoadThongTinHocSinh(string id)
        {
            var dt = adminService.GetStudentInfo(id);
            dgvTTHS.ItemsSource = dt.DefaultView;
        }

        private void LoadThongTinLopHoc(string id)
        {
            var dt = adminService.GetClassInfo(id);
            dgvTTLH.ItemsSource = dt.DefaultView;
        }

        private void LoadLichSuHocTap(string id)
        {
            dgvHS1.ItemsSource = adminService.GetStudentClassHistory(id).DefaultView;
        }


        private void btnDongY_Click(object sender, RoutedEventArgs e)
        {
            if (dgvDonXin.SelectedItem is DataRowView row)
            {
                try
                {
                    adminService.ApproveRegistration(
                        row["Mã HS"].ToString(),
                        row["Mã Lớp"].ToString()
                    );

                    MessageBox.Show("Đã duyệt thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi khi duyệt đơn", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnTuChoi_Click(object sender, RoutedEventArgs e)
        {
            if (dgvDonXin.SelectedItem is DataRowView row)
            {
                try
                {
                    adminService.RejectRegistration(
                        row["Mã HS"].ToString(),
                        row["Mã Lớp"].ToString()
                    );

                    MessageBox.Show("Đã từ chối đơn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData(); // Load lại bảng
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi khi từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            string monHoc = cbbMH.SelectedItem?.ToString() ?? "Tất cả";
            string khoi = cbbKhoi.SelectedItem?.ToString() ?? "Tất cả";
            string classId = cbbLop.SelectedValue?.ToString() ?? "Tất cả";

            dgvDonXin.ItemsSource = adminService
                .FilterRegistrations(monHoc, khoi, classId)
                .DefaultView;
        }

        private void cbbMH_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtActiveClasses == null) return;

            string monHoc = cbbMH.SelectedItem?.ToString() ?? "Tất cả";
            string keyword = GetKeyword(monHoc);

            var list = new HashSet<string>();

            foreach (DataRow row in dtActiveClasses.Rows)
            {
                string name = row["class_name"].ToString();

                if (monHoc != "Tất cả" && !name.Contains(keyword)) continue;

                foreach (var p in name.Split(' '))
                {
                    if (p == "10" || p == "11" || p == "12")
                        list.Add("Khối " + p);
                }
            }

            var listKhoi = list.ToList();
            listKhoi.Sort();
            listKhoi.Insert(0, "Tất cả");

            cbbKhoi.ItemsSource = listKhoi;
            cbbKhoi.SelectedIndex = 0;
        }

        private void cbbKhoi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtActiveClasses == null) return;

            string monHoc = cbbMH.SelectedItem?.ToString() ?? "Tất cả";
            string keywordMH = GetKeyword(monHoc);

            string khoi = cbbKhoi.SelectedItem?.ToString() ?? "Tất cả";
            string keywordKhoi = khoi.Replace("Khối ", "");

            var listLop = new List<dynamic>
            {
                new { classID = "Tất cả", class_name = "Tất cả" }
            };

            // Lọc danh sách Lớp
            foreach (DataRow row in dtActiveClasses.Rows)
            {
                string id = row["classID"].ToString();
                string name = row["class_name"].ToString();

                bool matchMH = (monHoc == "Tất cả") || name.Contains(keywordMH);
                bool matchKhoi = (khoi == "Tất cả") || name.Split(' ').Contains(keywordKhoi);

                if (matchMH && matchKhoi)
                {
                    listLop.Add(new { classID = id, class_name = name });
                }
            }

            cbbLop.ItemsSource = listLop;
            cbbLop.DisplayMemberPath = "class_name";
            cbbLop.SelectedValuePath = "classID";
            cbbLop.SelectedIndex = 0;
        }
    }
}