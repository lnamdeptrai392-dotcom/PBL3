using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminTC
{
    public partial class KhoanChi : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private DataTable dtChi = new DataTable();

        public KhoanChi()
        {
            InitializeComponent();
            Loaded += KhoanChi_Load;
        }

        private void KhoanChi_Load(object sender, RoutedEventArgs e)
        {
            SetupDataGridView();
            cbbC.SelectedIndex = 0;
            ngay.SelectedDate = DateTime.Now;
        }

        private void SetupDataGridView()
        {
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.CanUserAddRows = false;
            dataGridView1.IsReadOnly = false;
            dataGridView1.SelectionMode = DataGridSelectionMode.Single;
            dataGridView1.SelectionUnit = DataGridSelectionUnit.Cell;
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            if (cbbC.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn tùy chọn thời gian để xem");
                return;
            }

            DateTime selectedDate = ngay.SelectedDate ?? DateTime.Now;
            DateTime today = selectedDate.Date;
            int month = selectedDate.Month;
            int year = selectedDate.Year;

            string option = ((ComboBoxItem)cbbC.SelectedItem).Content.ToString();
            string query = "";

            using (SqlConnection conn = db.GetConnection())
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                if (option == "ngày")
                {
                    query = @"
                        SELECT ChiID AS [Mã], 
                               LoaiChi AS [Loại chi], 
                               NoiDung AS [Nội dung], 
                               SoTien AS [Số tiền], 
                               NgayChi AS [Ngày chi], 
                               GhiChu AS [Ghi chú]
                        FROM KhoanChi 
                        WHERE NgayChi = @date";

                    cmd.Parameters.Add("@date", SqlDbType.Date).Value = today;
                }
                else if (option == "tháng")
                {
                    query = @"
                        SELECT ChiID AS [Mã], 
                               LoaiChi AS [Loại chi], 
                               NoiDung AS [Nội dung], 
                               SoTien AS [Số tiền], 
                               NgayChi AS [Ngày chi], 
                               GhiChu AS [Ghi chú]
                        FROM KhoanChi 
                        WHERE ChiMonth = @month AND ChiYear = @year";

                    cmd.Parameters.AddWithValue("@month", month);
                    cmd.Parameters.AddWithValue("@year", year);
                }
                else if (option == "năm")
                {
                    query = @"
                        SELECT ChiID AS [Mã], 
                               LoaiChi AS [Loại chi], 
                               NoiDung AS [Nội dung], 
                               SoTien AS [Số tiền], 
                               NgayChi AS [Ngày chi], 
                               GhiChu AS [Ghi chú]
                        FROM KhoanChi 
                        WHERE ChiYear = @year";

                    cmd.Parameters.AddWithValue("@year", year);
                }

                if (string.IsNullOrEmpty(query)) return;

                cmd.CommandText = query;

                try
                {
                    conn.Open();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        dtChi = new DataTable();
                        adapter.Fill(dtChi);

                        dataGridView1.ItemsSource = dtChi.DefaultView;
                        dataGridView1.CanUserAddRows = true;
                    }

                    TinhTongKhoanChi();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối: " + ex.Message);
                }
            }
        }

        private void TinhTongKhoanChi()
        {
            decimal tong = 0;

            foreach (DataRow row in dtChi.Rows)
            {
                if (row["Số tiền"] != DBNull.Value)
                {
                    tong += Convert.ToDecimal(row["Số tiền"]);
                }
            }

            tbKT.Text = tong.ToString("N0") + " VNĐ";
        }

        private void CapNhatKhoanChiXuongDB(int chiID, string loaiChi, string noiDung, decimal soTien, DateTime ngayChi, string ghiChu)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"
                    UPDATE KhoanChi 
                    SET LoaiChi = @loai, 
                        NoiDung = @nd, 
                        SoTien = @tien, 
                        NgayChi = @ngay,
                        ChiMonth = @month,
                        ChiYear = @year,
                        GhiChu = @gc
                    WHERE ChiID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@loai", loaiChi);
                    cmd.Parameters.AddWithValue("@nd", noiDung);
                    cmd.Parameters.AddWithValue("@tien", soTien);
                    cmd.Parameters.Add("@ngay", SqlDbType.Date).Value = ngayChi;
                    cmd.Parameters.AddWithValue("@month", ngayChi.Month);
                    cmd.Parameters.AddWithValue("@year", ngayChi.Year);
                    cmd.Parameters.AddWithValue("@gc", ghiChu);
                    cmd.Parameters.AddWithValue("@id", chiID);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private int ThemMoiKhoanChiXuongDB(string loaiChi, string noiDung, decimal soTien, DateTime ngayChi, string ghiChu)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"
                    INSERT INTO KhoanChi 
                    (LoaiChi, NoiDung, SoTien, NgayChi, ChiMonth, ChiYear, GhiChu)
                    OUTPUT INSERTED.ChiID 
                    VALUES 
                    (@loai, @nd, @tien, @ngay, @month, @year, @gc)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@loai", loaiChi);
                    cmd.Parameters.AddWithValue("@nd", noiDung);
                    cmd.Parameters.AddWithValue("@tien", soTien);
                    cmd.Parameters.Add("@ngay", SqlDbType.Date).Value = ngayChi;
                    cmd.Parameters.AddWithValue("@month", ngayChi.Month);
                    cmd.Parameters.AddWithValue("@year", ngayChi.Year);
                    cmd.Parameters.AddWithValue("@gc", ghiChu);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private void dataGridView1_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Header == null) return;

            if (e.Column.Header.ToString() == "Mã")
            {
                DataRowView row = e.Row.Item as DataRowView;

                if (row != null && row["Mã"] != DBNull.Value && row["Mã"].ToString() != "")
                {
                    e.Cancel = true;
                }
            }
        }

        private void dataGridView1_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SaveCurrentRow(e.Row.Item as DataRowView);
            }));
        }

        private void SaveCurrentRow(DataRowView row)
        {
            if (row == null) return;

            try
            {
                string idStr = row["Mã"]?.ToString() ?? "";
                string loaiChi = row["Loại chi"]?.ToString() ?? "";
                string noiDung = row["Nội dung"]?.ToString() ?? "";
                string soTienStr = row["Số tiền"]?.ToString() ?? "";
                string ghiChu = row["Ghi chú"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(idStr) &&
                    (string.IsNullOrWhiteSpace(loaiChi) || string.IsNullOrWhiteSpace(soTienStr)))
                {
                    return;
                }

                decimal.TryParse(soTienStr, out decimal soTien);

                DateTime ngayChi = ngay.SelectedDate ?? DateTime.Now;

                if (row["Ngày chi"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["Ngày chi"].ToString()))
                {
                    DateTime.TryParse(row["Ngày chi"].ToString(), out ngayChi);
                }

                if (string.IsNullOrEmpty(idStr))
                {
                    int newID = ThemMoiKhoanChiXuongDB(loaiChi, noiDung, soTien, ngayChi, ghiChu);

                    if (newID > 0)
                    {
                        row["Mã"] = newID;
                        MessageBox.Show("Thêm thành công!");
                    }
                }
                else
                {
                    int chiID = int.Parse(idStr);
                    CapNhatKhoanChiXuongDB(chiID, loaiChi, noiDung, soTien, ngayChi, ghiChu);
                }

                TinhTongKhoanChi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message);
            }
        }
    }
}