using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminTC
{
    public partial class KhoanThu : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private DataTable dtThu = new DataTable();

        public KhoanThu()
        {
            InitializeComponent();
            Loaded += KhoanThu_Load;
        }

        private void KhoanThu_Load(object sender, RoutedEventArgs e)
        {
            cbbT.SelectedIndex = 0;
            ngay.SelectedDate = DateTime.Now;
        }

        private void TinhTongKhoanThu()
        {
            decimal tong = 0;

            foreach (DataRow row in dtThu.Rows)
            {
                if (row["Số tiền"] != DBNull.Value)
                {
                    tong += Convert.ToDecimal(row["Số tiền"]);
                }
            }

            tbKT.Text = tong.ToString("N0") + " VNĐ";
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            DateTime date = ngay.SelectedDate ?? DateTime.Now;

            string option = ((ComboBoxItem)cbbT.SelectedItem).Content.ToString();

            string query = "";

            using (SqlConnection conn = db.GetConnection())
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                if (option == "ngày")
                {
                    query = "SELECT * FROM KhoanThu WHERE NgayThu=@d";
                    cmd.Parameters.Add("@d", SqlDbType.Date).Value = date.Date;
                }
                else if (option == "tháng")
                {
                    query = "SELECT * FROM KhoanThu WHERE ThuMonth=@m AND ThuYear=@y";
                    cmd.Parameters.AddWithValue("@m", date.Month);
                    cmd.Parameters.AddWithValue("@y", date.Year);
                }
                else
                {
                    query = "SELECT * FROM KhoanThu WHERE ThuYear=@y";
                    cmd.Parameters.AddWithValue("@y", date.Year);
                }

                cmd.CommandText = query;

                conn.Open();

                SqlDataAdapter ad = new SqlDataAdapter(cmd);
                dtThu = new DataTable();
                ad.Fill(dtThu);

                dataGridView1.ItemsSource = dtThu.DefaultView;
            }

            TinhTongKhoanThu();
        }

        private void dataGridView1_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Header?.ToString() == "Mã")
            {
                var row = e.Row.Item as DataRowView;
                if (row != null && row["Mã"] != DBNull.Value)
                {
                    e.Cancel = true;
                }
            }
        }

        private void dataGridView1_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SaveRow(e.Row.Item as DataRowView);
            }));
        }

        private void SaveRow(DataRowView row)
        {
            if (row == null) return;

            string id = row["Mã"]?.ToString() ?? "";
            string loai = row["Loại thu"]?.ToString() ?? "";
            string nd = row["Nội dung"]?.ToString() ?? "";
            string tienStr = row["Số tiền"]?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(loai) || string.IsNullOrWhiteSpace(tienStr))
                return;

            decimal.TryParse(tienStr, out decimal tien);

            DateTime ngayThu = ngay.SelectedDate ?? DateTime.Now;

            if (string.IsNullOrEmpty(id))
            {
                ThemMoi(loai, nd, tien, ngayThu);
            }
            else
            {
                CapNhat(int.Parse(id), loai, nd, tien, ngayThu);
            }

            TinhTongKhoanThu();
        }

        private void CapNhat(int id, string loai, string nd, decimal tien, DateTime ngay)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(
                    "UPDATE KhoanThu SET LoaiThu=@l,NoiDung=@n,SoTien=@t,NgayThu=@d WHERE ThuID=@id",
                    conn);

                cmd.Parameters.AddWithValue("@l", loai);
                cmd.Parameters.AddWithValue("@n", nd);
                cmd.Parameters.AddWithValue("@t", tien);
                cmd.Parameters.Add("@d", SqlDbType.Date).Value = ngay;
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }
        }

        private void ThemMoi(string loai, string nd, decimal tien, DateTime ngay)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO KhoanThu(LoaiThu,NoiDung,SoTien,NgayThu,ThuMonth,ThuYear)
                      VALUES(@l,@n,@t,@d,@m,@y)", conn);

                cmd.Parameters.AddWithValue("@l", loai);
                cmd.Parameters.AddWithValue("@n", nd);
                cmd.Parameters.AddWithValue("@t", tien);
                cmd.Parameters.Add("@d", SqlDbType.Date).Value = ngay;
                cmd.Parameters.AddWithValue("@m", ngay.Month);
                cmd.Parameters.AddWithValue("@y", ngay.Year);

                cmd.ExecuteNonQuery();
            }
        }
    }
}