using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminTC
{
    public partial class ThietLapHP : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private string MaLop;

        public ThietLapHP(string m)
        {
            InitializeComponent();
            MaLop = m;
            SetGUI();
        }

        public void SetGUI()
        {
            cbbMaLop.Text = MaLop;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT class_name FROM Class WHERE classID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", MaLop);

                    object result = cmd.ExecuteScalar();
                    tbTL.Text = result != null ? result.ToString() : "";
                }
            }
        }

        private void btLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtTienTrenNg.Text, out decimal tienTrenNg) || tienTrenNg < 0)
            {
                MessageBox.Show("Số tiền không hợp lệ!");
                return;
            }

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                // Bắt đầu Transaction để bảo vệ dữ liệu
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    string query = @"
                UPDATE HocPhi 
                SET SoTien = @tienTrenNg 
                WHERE ClassID = @id 
                AND TrangThai = N'Chưa đóng'";

                    using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@tienTrenNg", tienTrenNg);
                        cmd.Parameters.AddWithValue("@id", MaLop);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            trans.Commit(); // Lưu vĩnh viễn nếu thành công
                            MessageBox.Show($"Đã cập nhật học phí cho {rowsAffected} học sinh!");
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy học sinh nào cần cập nhật học phí trong lớp này.");
                            trans.Rollback();
                        }
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback(); // Hoàn tác nếu có lỗi xảy ra
                    MessageBox.Show("Lỗi hệ thống: " + ex.Message);
                }
            }
        }

        private void LoadTenLop(string classID)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT class_name FROM Class WHERE classID = @classID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@classID", classID);

                    object result = cmd.ExecuteScalar();
                    tbTL.Text = result != null ? result.ToString() : "";
                }
            }
        }

        private void cbbMaLop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbMaLop.SelectedItem == null) return;

            MaLop = cbbMaLop.SelectedItem.ToString();
            LoadTenLop(MaLop);
            int siso = capacity_cl(MaLop);
            txtSS.Text = siso.ToString();
        }

        public int capacity_cl(string idlop)
        {
            int cap = 0;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT COUNT(AccountID) 
                    FROM HocPhi 
                    WHERE ClassID = @id 
                    AND TrangThai = N'Chưa đóng'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idlop);

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        cap = Convert.ToInt32(result);
                    }
                }
            }

            return cap;
        }

        public decimal SetHP(decimal hphi)
        {
            int cap = capacity_cl(MaLop);
            txtSS.Text = cap.ToString();

            return cap * hphi;
        }

        private void butTT_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtTienTrenNg.Text, out decimal hphi1))
            {
                if (hphi1 < 0)
                {
                    MessageBox.Show("Số tiền không hợp lệ!");
                }
                else
                {
                    decimal tongTien = SetHP(hphi1);
                    txtTongT.Text = tongTien.ToString("N0") + " VND";
                }
            }
            else
            {
                MessageBox.Show("Vui lòng nhập một con số hợp lệ cho học phí!");
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        
    }
}