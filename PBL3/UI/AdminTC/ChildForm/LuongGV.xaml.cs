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

        public LuongGV()
        {
            InitializeComponent();
            this.Loaded += LuongGV_Load;
        }

        private void LuongGV_Load(object sender, RoutedEventArgs e)
        {
            LoadDanhSachGV("");
        }

        private void LoadDanhSachGV(string text)
        {
            try
            {
                cbbMGV.Items.Clear();
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT Id FROM accountList WHERE Role='Teacher' AND Id LIKE @text";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@text", "%" + text + "%");
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cbbMGV.Items.Add(reader["Id"].ToString());
                            }
                        }
                    }
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
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT name FROM accountList WHERE Id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    object result = cmd.ExecuteScalar();
                    tbTL.Text = result != null ? result.ToString() : "N/A";
                }
            }
        }

        // Hàm LoadLuong để sửa lỗi CS0103
        private void LoadLuong(string id)
        {
            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT LuongID, SalaryMonth, SoLopDay, SoBuoiDay, LuongCoBan, Thuong, Phat, TongLuong, TrangThai, NgayThanhToan
                                    FROM LuongGV 
                                    WHERE TeacherID=@id 
                                    ORDER BY SalaryYear DESC, SalaryMonth DESC";

                    using (SqlDataAdapter ad = new SqlDataAdapter(query, conn))
                    {
                        ad.SelectCommand.Parameters.AddWithValue("@id", id);
                        DataTable dt = new DataTable();
                        ad.Fill(dt);
                        dataGridView1.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải bảng lương: " + ex.Message); }
        }

        private void btSetL_Click(object sender, RoutedEventArgs e)
        {
            if (cbbMGV.SelectedItem == null) return;

            string id = cbbMGV.SelectedItem.ToString();

            // Sửa lỗi ép kiểu và kiểm tra null cho C# 7.3
            if (cbbThang.SelectedItem == null || string.IsNullOrEmpty(txtNam.Text))
            {
                MessageBox.Show("Vui lòng nhập đúng Tháng/Năm");
                return;
            }

            int month = int.Parse(((ComboBoxItem)cbbThang.SelectedItem).Content.ToString());
            int year = int.Parse(txtNam.Text);

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();

                    // Kiểm tra trùng
                    using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(1) FROM LuongGV WHERE TeacherID=@id AND SalaryMonth=@m AND SalaryYear=@y", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@id", id);
                        checkCmd.Parameters.AddWithValue("@m", month);
                        checkCmd.Parameters.AddWithValue("@y", year);
                        if ((int)checkCmd.ExecuteScalar() > 0)
                        {
                            MessageBox.Show("Tháng này đã được tính lương!");
                            return;
                        }
                    }

                    int soLop = (int)new SqlCommand("SELECT COUNT(*) FROM Class WHERE teacherID=@id", conn)
                    { Parameters = { new SqlParameter("@id", id) } }.ExecuteScalar();

                    int soBuoi = soLop * 8;
                    decimal mucLuongMoiBuoi = 400000;
                    decimal tongLuong = soBuoi * mucLuongMoiBuoi;

                    string insert = @"INSERT INTO LuongGV (TeacherID, SalaryMonth, SalaryYear, SoLopDay, SoBuoiDay, LuongCoBan, Thuong, Phat, TongLuong, TrangThai)
                                    VALUES (@id, @m, @y, @lop, @buoi, @muc, 0, 0, @tong, N'Chưa thanh toán')";

                    using (SqlCommand cmd = new SqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@m", month);
                        cmd.Parameters.AddWithValue("@y", year);
                        cmd.Parameters.AddWithValue("@lop", soLop);
                        cmd.Parameters.AddWithValue("@buoi", soBuoi);
                        cmd.Parameters.AddWithValue("@muc", mucLuongMoiBuoi);
                        cmd.Parameters.AddWithValue("@tong", tongLuong);
                        cmd.ExecuteNonQuery();
                    }
                }
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

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (DataRow row in dv.Table.Rows)
                        {
                            // C# 7.3 không hỗ trợ RowState check trực tiếp trong foreach dễ dàng, 
                            // nhưng lệnh UPDATE này vẫn an toàn.
                            string query = @"UPDATE LuongGV SET 
                                           TrangThai = @status, 
                                           NgayThanhToan = (CASE WHEN @status = N'Đã thanh toán' THEN GETDATE() ELSE NULL END),
                                           Thuong = @thuong, Phat = @phat,
                                           TongLuong = (LuongCoBan * SoBuoiDay + @thuong - @phat)
                                           WHERE LuongID = @luongID";

                            using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@luongID", row["LuongID"]);
                                cmd.Parameters.AddWithValue("@status", row["TrangThai"] != null ? row["TrangThai"].ToString() : "Chưa thanh toán");
                                cmd.Parameters.AddWithValue("@thuong", row["Thuong"] == DBNull.Value ? 0 : row["Thuong"]);
                                cmd.Parameters.AddWithValue("@phat", row["Phat"] == DBNull.Value ? 0 : row["Phat"]);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                        MessageBox.Show("Cập nhật thành công!");
                        if (!string.IsNullOrEmpty(teacherID)) LoadLuong(teacherID);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message);
                    }
                }
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