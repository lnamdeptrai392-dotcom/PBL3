using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminTC
{
    public partial class Lai : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private DataTable thuchi = new DataTable();

        public Lai()
        {
            InitializeComponent();
            Loaded += Lai_Load;
        }

        private void Lai_Load(object sender, RoutedEventArgs e)
        {
            SetupDataGridView();
            cbbLN.SelectedIndex = 0;
            date.SelectedDate = DateTime.Now;
        }

        private void SetupDataGridView()
        {
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.IsReadOnly = true;
            dataGridView1.CanUserAddRows = false;
            dataGridView1.SelectionMode = DataGridSelectionMode.Single;
            dataGridView1.SelectionUnit = DataGridSelectionUnit.FullRow;
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            if (cbbLN.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn tùy chọn thời gian để xem");
                return;
            }

            DateTime selectedDate = date.SelectedDate ?? DateTime.Now;
            DateTime today = selectedDate.Date;
            int month = selectedDate.Month;
            int year = selectedDate.Year;

            string option = ((ComboBoxItem)cbbLN.SelectedItem).Content.ToString();
            string query = "";

            using (SqlConnection conn = db.GetConnection())
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                if (option == "ngày")
                {
                    query = @"
                        SELECT ThuID AS [Mã], LoaiThu AS [Loại], NoiDung, SoTien AS [Số tiền],
                               NgayThu AS [Ngày], GhiChu, 'Thu' AS [Loại Giao Dịch]
                        FROM KhoanThu WHERE NgayThu = @date
                        UNION ALL
                        SELECT ChiID, LoaiChi, NoiDung, SoTien,
                               NgayChi, GhiChu, 'Chi'
                        FROM KhoanChi WHERE NgayChi = @date";

                    cmd.Parameters.Add("@date", SqlDbType.Date).Value = today;
                }
                else if (option == "tháng")
                {
                    query = @"
                        SELECT ThuID AS [Mã], LoaiThu AS [Loại], NoiDung, SoTien AS [Số tiền],
                               NgayThu AS [Ngày], GhiChu, 'Thu' AS [Loại Giao Dịch]
                        FROM KhoanThu WHERE ThuMonth = @month AND ThuYear = @year
                        UNION ALL
                        SELECT ChiID, LoaiChi, NoiDung, SoTien,
                               NgayChi, GhiChu, 'Chi'
                        FROM KhoanChi WHERE ChiMonth = @month AND ChiYear = @year";

                    cmd.Parameters.AddWithValue("@month", month);
                    cmd.Parameters.AddWithValue("@year", year);
                }
                else if (option == "năm")
                {
                    query = @"
                        SELECT ThuID AS [Mã], LoaiThu AS [Loại], NoiDung, SoTien AS [Số tiền],
                               NgayThu AS [Ngày], GhiChu, 'Thu' AS [Loại Giao Dịch]
                        FROM KhoanThu WHERE ThuYear = @year
                        UNION ALL
                        SELECT ChiID, LoaiChi, NoiDung, SoTien,
                               NgayChi, GhiChu, 'Chi'
                        FROM KhoanChi WHERE ChiYear = @year";

                    cmd.Parameters.AddWithValue("@year", year);
                }

                if (string.IsNullOrEmpty(query)) return;

                cmd.CommandText = query;

                try
                {
                    conn.Open();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        thuchi = new DataTable();
                        adapter.Fill(thuchi);

                        dataGridView1.ItemsSource = thuchi.DefaultView;
                    }

                    TinhTongKhoan();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối: " + ex.Message);
                }
            }
        }

        private void TinhTongKhoan()
        {
            decimal tong = 0;

            foreach (DataRow row in thuchi.Rows)
            {
                if (row["Số tiền"] != DBNull.Value)
                {
                    string loai = row["Loại Giao Dịch"].ToString();

                    if (loai == "Thu")
                        tong += Convert.ToDecimal(row["Số tiền"]);
                    else if (loai == "Chi")
                        tong -= Convert.ToDecimal(row["Số tiền"]);
                }
            }

            tbT.Text = tong.ToString("N0") + " VNĐ";
        }
    }
}