using PBL3a.services;
using PBL3a.services.BLL;
using PBL3a.UI.AdminDD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PBL3.UI.AdminC.Windows
{
    
    public partial class Attendance : UserControl
    {
        
        public AdminC_Service adminC_Service = new AdminC_Service();
        private DataTable dtActive;
        public Attendance()
        {
            InitializeComponent();
            Loaded += Attendance_Load;
        }
        private void Attendance_Load(object sender, EventArgs e)
        {
            try
            {
                dtActive = adminC_Service.GetActiveClassNow();
                if (dtActive != null && dtActive.Rows.Count > 0)
                {
                    cbbClass.ItemsSource = dtActive.DefaultView;
                    cbbClass.DisplayMemberPath = "class_name";
                    cbbClass.SelectedValuePath = "classID";
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void cbbClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (cbbClass.SelectedValue == null) return;

    try
    {
        string classID = cbbClass.SelectedValue.ToString();

        if (cbbClass.SelectedItem is DataRowView selectedRow)
        {
            // --- PHẦN 1: TÍNH TOÁN DANH SÁCH NGÀY HỌC ---
            int targetDayOfWeek = Convert.ToInt32(selectedRow["dayOfWeek"]);
            DayOfWeek csharpDay = (targetDayOfWeek == 7) ? DayOfWeek.Sunday : (DayOfWeek)targetDayOfWeek;

            DataRow duration = adminC_Service.GetClassDuration(classID);
            if (duration != null)
            {
                DateTime start = Convert.ToDateTime(duration["startDate"]);
                DateTime end = Convert.ToDateTime(duration["endDate"]);
                List<string> validDates = new List<string>();

                for (DateTime date = start; date <= end; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == csharpDay)
                    {
                        validDates.Add(date.ToString("dd/MM/yyyy"));
                    }
                }

                // Gán nguồn dữ liệu cho ComboBox Ngày
                cbbDate.ItemsSource = validDates;

                // Chọn ngày: Ưu tiên ngày hôm nay, nếu không thì chọn ngày đầu tiên
                string todayStr = DateTime.Now.ToString("dd/MM/yyyy");
                if (validDates.Contains(todayStr))
                    cbbDate.SelectedItem = todayStr;
                else if (validDates.Count > 0)
                    cbbDate.SelectedIndex = 0;
            }

            // Hiển thị thông tin thời gian lên TextBox
            txtDayOfWeek.Text = selectedRow["NgayHoc"]?.ToString();
            txtStartTime.Text = selectedRow["GioBatDau"]?.ToString();
            txtEndTime.Text = selectedRow["GioKetThuc"]?.ToString();
        }

        // --- PHẦN 2: LOAD THÔNG TIN GIÁO VIÊN ---
        DataTable dtInfo = adminC_Service.GetClassInfo(classID);
        if (dtInfo != null && dtInfo.Rows.Count > 0)
        {
            txtTeacher.Text = dtInfo.Rows[0]["GV Chủ Nhiệm"].ToString();
            txtStatus.Text = "Đang mở"; 
        }

        // --- PHẦN 3: LOAD DANH SÁCH HỌC SINH ---
        // Lấy ngày đang được chọn từ cbbDate để load điểm danh
        if (cbbDate.SelectedItem != null)
        {
            string selectedDate = cbbDate.SelectedItem.ToString();
            LoadDataGridAttendance(classID, selectedDate);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message);
    }
}
        private void cbbDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbClass.SelectedValue != null && cbbDate.SelectedItem != null)
            {
                string classID = cbbClass.SelectedValue.ToString();
                string selectedDate = cbbDate.SelectedItem.ToString();
                LoadDataGridAttendance(classID, selectedDate);
            }
        }
        private void LoadDataGridAttendance(string classID, string date)
        {
            DataTable dtAttendance = adminC_Service.getAttendanceInfo(classID, date);
            if (dtAttendance != null)
            {
                dgvAttendance.ItemsSource = dtAttendance.DefaultView;
            }
        }
    }
}
