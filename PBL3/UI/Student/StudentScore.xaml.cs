using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace PBL3a.UI.Student
{
    public partial class StudentScore : UserControl
    {
        private readonly DatabaseHelper db = new DatabaseHelper();
        private string currentID = "";

        public StudentScore(string id)
        {
            currentID = id;
            InitializeComponent();

            Loaded += StudentScore_Load;
        }

        private void StudentScore_Load(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentID))
            {
                LoadScoreData();
            }
        }

        private void LoadScoreData()
        {
            using (SqlConnection con = db.GetConnection())
            {
                try
                {
                    con.Open();

                    string query = @"
                        SELECT 
                        cl.classID AS ClassID, 
                        cl.class_name AS ClassName, 
                        ISNULL(CAST(d.Diem AS NVARCHAR), N'Chưa có điểm') AS DiemHocTap, 
                        ISNULL(d.NhanXet, N'Chưa có nhận xét') AS NhanXet
                    FROM JoinClass jc
                    INNER JOIN Class cl ON cl.classID = jc.classID
                    LEFT JOIN Diem d ON d.ClassID = jc.classID AND d.AccountID = jc.AccountID
                    WHERE jc.AccountID = @id";

                    var selectedItem = cboSemester.SelectedItem as ComboBoxItem;
                    string HocKi = selectedItem?.Content?.ToString() ?? "";
                    if (HocKi == "Học kì hiện tại")
                    {
                        query += " AND cl.status = N'Đang mở'";
                    }
                    else if (HocKi == "Tất cả")
                    {

                    }
                    else if (HocKi == "")
                    {
                        return;
                    }

                    using (SqlDataAdapter a = new SqlDataAdapter(query, con))
                    {
                        a.SelectCommand.Parameters.AddWithValue("@id", currentID);

                        DataTable dt = new DataTable();
                        a.Fill(dt);

                        dgvScore.ItemsSource = dt.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (dgvScore.ItemsSource == null)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"BangDiem_Student_{currentID}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    DataView dv = (DataView)dgvScore.ItemsSource;
                    DataTable dt = dv.ToTable();

                    dt.Columns["ClassID"].ColumnName = "Mã lớp học";
                    dt.Columns["ClassName"].ColumnName = "Tên lớp học";
                    dt.Columns["DiemHocTap"].ColumnName = "Điểm học tập";
                    dt.Columns["NhanXet"].ColumnName = "Nhận xét";

                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add(dt, "Bảng Điểm");

                        worksheet.FirstRow().Style.Font.Bold = true;
                        worksheet.FirstRow().Style.Fill.BackgroundColor = XLColor.LightGray;
                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Xuất dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnShowData_Click(object sender, RoutedEventArgs e)
        {
            LoadScoreData();
        }

        private void cboSemester_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                LoadScoreData();
            }
        }
    }
}