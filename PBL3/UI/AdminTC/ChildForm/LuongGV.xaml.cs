using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PBL3a.UI.AdminTC
{
    public partial class LuongGV : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private DataTable dtLuong = new DataTable();

        public LuongGV()
        {
            InitializeComponent();
            Loaded += LuongGV_Load;
        }

        private void LuongGV_Load(object sender, RoutedEventArgs e)
        {
            LoadDanhSachGV("");
        }

        private void LoadDanhSachGV(string text)
        {
            cbbMGV.Items.Clear();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query = "SELECT Id FROM accountList WHERE Role='Teacher' AND Id LIKE @text";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@text", "%" + text + "%");
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            cbbMGV.Items.Add(r["Id"].ToString());
                        }
                    }
                }
            }
        }

        private void cbbMGV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbMGV.SelectedItem == null) return;
            string id = cbbMGV.SelectedItem.ToString();
            LoadTenGV(id);
            LoadLuong(id);
            txtNam.Text = DateTime.Now.Year.ToString();
        }

        private void LoadTenGV(string id)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT name FROM accountList WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                tbTL.Text = cmd.ExecuteScalar()?.ToString();
            }
        }

        private void LoadLuong(string id)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"SELECT LuongID, SalaryMonth, SoLopDay, SoBuoiDay, LuongCoBan, Thuong, Phat, TongLuong, TrangThai, NgayThanhToan
                                FROM LuongGV 
                                WHERE TeacherID=@id 
                                ORDER BY SalaryYear DESC, SalaryMonth DESC";

                SqlDataAdapter ad = new SqlDataAdapter(query, conn);
                ad.SelectCommand.Parameters.AddWithValue("@id", id);
                dtLuong = new DataTable();
                ad.Fill(dtLuong);
                dataGridView1.ItemsSource = dtLuong.DefaultView;
            }
        }

        private void btSetL_Click(object sender, RoutedEventArgs e)
        {
            if (cbbMGV.SelectedItem == null)
            {
                MessageBox.Show("Chọn giảng viên");
                return;
            }

            string id = cbbMGV.SelectedItem.ToString();

            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            if (cbbThang.SelectedItem != null && txtNam.Text != "")
            {
                month = int.Parse(((ComboBoxItem)cbbThang.SelectedItem).Content.ToString());
                year = int.Parse(txtNam.Text);
            }

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                // 1. Lấy số lớp dạy
                int soLop = (int)new SqlCommand("SELECT COUNT(*) FROM Class WHERE teacherID=@id", conn)
                { Parameters = { new SqlParameter("@id", id) } }.ExecuteScalar();

                // 2. Logic mới:
                int soBuoi = soLop * 8; // Giả định mỗi lớp 8 buổi/tháng
                decimal mucLuongMoiBuoi = 400000; // Đây là LuongCoBan (định mức)
                decimal thanhTien = soBuoi * mucLuongMoiBuoi;

                string insert = @"INSERT INTO LuongGV
            (TeacherID, SalaryMonth, SalaryYear, SoLopDay, SoBuoiDay, LuongCoBan, Thuong, Phat, TongLuong, TrangThai)
            VALUES (@id, @m, @y, @lop, @buoi, @mucLuong, 0, 0, @tong, N'Chưa thanh toán')";

                SqlCommand cmd = new SqlCommand(insert, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@m", month);
                cmd.Parameters.AddWithValue("@y", year);
                cmd.Parameters.AddWithValue("@lop", soLop);
                cmd.Parameters.AddWithValue("@buoi", soBuoi);
                cmd.Parameters.AddWithValue("@mucLuong", mucLuongMoiBuoi); // Lưu định mức
                cmd.Parameters.AddWithValue("@tong", thanhTien); // Lưu thành tiền

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Tính lương xong");
            LoadLuong(id);
        }

        private void cbbMGV_KeyUp(object sender, KeyEventArgs e)
        {
            string key = cbbMGV.Text;
            LoadDanhSachGV(key);
            cbbMGV.IsDropDownOpen = true;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //luu du lieu cho cac thay doi tren datagrid
            DataView dv = (DataView)dataGridView1.ItemsSource;
            DataTable dt = dv.ToTable();
            string id = cbbMGV.SelectedItem.ToString();
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        // lenh sql kiem tra: neu thay doi thi update, neu co thi insert
                        string query = @"
                    UPDATE LuongGV
                    SET TrangThai = @status, 
                        NgayThanhToan = (CASE WHEN @status = N'Đã thanh toán' THEN GETDATE() ELSE NULL END),                        
                        Thuong = @thuong,
                        Phat = @phat,
                        TongLuong = (LuongCoBan * 8 + @thuong - @phat)
                    WHERE LuongID = @luongID";

                        using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@luongID", row["LuongID"]);
                            cmd.Parameters.AddWithValue("@status", row["TrangThai"]?.ToString() ?? "Chưa thanh toán");
                            cmd.Parameters.AddWithValue("@thuong", row["Thuong"] == DBNull.Value ? 0 : row["Thuong"]);
                            cmd.Parameters.AddWithValue("@phat", row["Phat"] == DBNull.Value ? 0 : row["Phat"]);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();
                    LoadLuong(id);
                    MessageBox.Show("Cập nhật thành công!", "Thông báo");
                    
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }
        }
    }
}