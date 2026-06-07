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
    public partial class HocPhi : UserControl
    {
        private DataTable dtHocPhi = new DataTable();
        private AdminTC_Service bll = new AdminTC_Service();

        public HocPhi()
        {
            InitializeComponent();
            
        }
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbGrade == null || cbbCourse == null || cbbML == null) return;

            // Lấy giá trị từ ComboBox
            string selectedGrade = (cbbGrade.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string selectedCourseTag = (cbbCourse.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            if (string.IsNullOrEmpty(selectedGrade) || string.IsNullOrEmpty(selectedCourseTag)) return;

            cbbML.Items.Clear();
            dataGridView1.ItemsSource = null; // Xóa bảng khi đổi bộ lọc

            try
            {
                List<string> classIDs = bll.GetClassIDsByFilters(selectedGrade, selectedCourseTag);
                foreach (string classID in classIDs)
                {
                    cbbML.Items.Add(classID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lớp: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HocPhi_Load(object sender, RoutedEventArgs e)
        {
            
            dataGridView1.AutoGenerateColumns = false;
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dataGridView1.CanUserAddRows = false;
            dataGridView1.SelectionMode = DataGridSelectionMode.Single;
            dataGridView1.SelectionUnit = DataGridSelectionUnit.FullRow;
        }

        private void LoadHocPhiTheoLop(string classID)
        {
            try
            {
                dtHocPhi = bll.GetHocPhiByClassID(classID);

                dataGridView1.ItemsSource = dtHocPhi.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải danh sách học phí: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btSetHP_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cbbML.Text))
            {
                MessageBox.Show("Vui lòng chọn lớp!");
                return;
            }
            string malop = cbbML.Text;
            ThietLapHP thietLap = new ThietLapHP(malop);
            thietLap.ShowDialog();
            LoadHocPhiTheoLop(malop);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cbbML.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn mã lớp trước khi xem!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string classID = cbbML.SelectedItem.ToString();
            LoadHocPhiTheoLop(classID);
        }
    }
}