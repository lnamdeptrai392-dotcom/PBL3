using Microsoft.Data.SqlClient;
using PBL3a.services;
using PBL3a.services.BLL;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace PBL3a.UI.AdminTC
{
    public partial class LuongGV : UserControl
    {
        private AdminTC_Service bll = new AdminTC_Service();

        public LuongGV()
        {
            InitializeComponent();
            this.Loaded += LuongGV_Load;
        }

        private void LuongGV_Load(object sender, RoutedEventArgs e)
        {
            bool hasAllItem = false;
            foreach (ComboBoxItem item in cbbThang.Items)
            {
                if (item.Content.ToString() == "Tất cả")
                {
                    hasAllItem = true;
                    break;
                }
            }
            if (!hasAllItem)
            {
                ComboBoxItem allItem = new ComboBoxItem { Content = "Tất cả" };
                cbbThang.Items.Insert(0, allItem);
            }

            string currentMonth = DateTime.Now.Month.ToString();
            foreach (ComboBoxItem item in cbbThang.Items)
            {
                if (item.Content.ToString() == currentMonth)
                {
                    cbbThang.SelectedItem = item;
                    break;
                }
            }
            txtNam.Text = DateTime.Now.Year.ToString();

            LoadDanhSachGV("");
        }

        private void LoadDanhSachGV(string text)
        {
            try
            {
                cbbMGV.Items.Clear();
                List<string> ids = bll.GetTeacherIDs(text);
                foreach (string id in ids)
                {
                    cbbMGV.Items.Add(id);
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load danh sách: " + ex.Message); }
        }

        private void cbbMGV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbMGV.SelectedItem == null)
            {
                tbTL.Text = "";
                return;
            }

            string selectedID = cbbMGV.SelectedItem.ToString();
            LoadTenGV(selectedID);
            LoadLuong(selectedID);
        }

        private void LoadTenGV(string id)
        {
            try
            {
                string name = bll.GetTeacherName(id);
                tbTL.Text = !string.IsNullOrEmpty(name) ? name : "N/A";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải tên giáo viên: " + ex.Message); }
        }

        private void LoadLuong(string id)
        {
            if (string.IsNullOrEmpty(id) || cbbThang.SelectedItem == null || string.IsNullOrEmpty(txtNam.Text))
            {
                dataGridView1.ItemsSource = null;
                return;
            }

            try
            {
                string thang = ((ComboBoxItem)cbbThang.SelectedItem).Content.ToString();
                string nam = txtNam.Text.Trim();

                DataTable dtAll = bll.GetLuongByTeacherID(id);
                DataView dv = new DataView(dtAll);

                if (thang == "Tất cả")
                {
                    dv.RowFilter = $"SalaryYear = {nam}";
                }
                else
                {
                    dv.RowFilter = $"SalaryMonth = {thang} AND SalaryYear = {nam}";
                }

                dataGridView1.ItemsSource = dv;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải bảng lương: " + ex.Message); }
        }

        private void cbbThang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbMGV.SelectedItem != null)
            {
                LoadLuong(cbbMGV.SelectedItem.ToString());
            }
        }

        private void txtNam_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbbMGV.SelectedItem != null && txtNam.Text.Trim().Length == 4)
            {
                LoadLuong(cbbMGV.SelectedItem.ToString());
            }
        }

        private void btSetL_Click(object sender, RoutedEventArgs e)
        {
            if (cbbMGV.SelectedItem == null) return;

            string id = cbbMGV.SelectedItem.ToString();

            if (cbbThang.SelectedItem == null || string.IsNullOrEmpty(txtNam.Text))
            {
                MessageBox.Show("Vui lòng nhập đúng Tháng/Năm");
                return;
            }

            string selectedMonthStr = ((ComboBoxItem)cbbThang.SelectedItem).Content.ToString();

            if (selectedMonthStr == "Tất cả")
            {
                MessageBox.Show("Vui lòng chọn một Tháng cụ thể (1-12) để tính lương, không thể chọn 'Tất cả'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int month = int.Parse(((ComboBoxItem)cbbThang.SelectedItem).Content.ToString());
            int year = int.Parse(txtNam.Text);

            try
            {
                bool isExist = bll.CheckLuongExists(id, month, year);
                if (isExist)
                {
                    MessageBox.Show("Tháng này đã được tính lương!");
                    return;
                }

                bll.CalculateAndInsertLuong(id, month, year);
                MessageBox.Show("Tính lương thành công!");
                LoadLuong(id);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataView dv = dataGridView1.ItemsSource as DataView;
            if (dv == null) return;

            string teacherID = cbbMGV.SelectedItem != null ? cbbMGV.SelectedItem.ToString() : "";

            try
            {
                bll.SaveLuongChanges(dv.Table);
                MessageBox.Show("Cập nhật thành công!");
                if (!string.IsNullOrEmpty(teacherID)) LoadLuong(teacherID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message);
            }
        }

        private void cbbMGV_KeyUp(object sender, KeyEventArgs e)
        {
            string key = cbbMGV.Text;
            LoadDanhSachGV(key);
            cbbMGV.IsDropDownOpen = true;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string teacherID = cbbMGV.Text;
            if (string.IsNullOrEmpty(teacherID)) return;
            LoadLuong(teacherID);
        }
    }
}