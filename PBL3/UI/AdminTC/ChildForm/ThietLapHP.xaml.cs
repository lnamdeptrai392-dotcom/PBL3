using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
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
            LoadChiTietHocPhi();
        }

        public void SetGUI()
        {
            txtMaLop.Text = MaLop;
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
            DataView dv = (DataView)dgHocSinhLop.ItemsSource;
            DataTable dt = dv.ToTable();
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
                    IF EXISTS (SELECT 1 FROM HocPhi WHERE AccountID = @accID AND ClassID = @classID AND TuitionMonth = @month AND TuitionYear = @year)
                    BEGIN
                        UPDATE HocPhi 
                        SET SoTien = @tien, TrangThai = @status, 
                            NgayDong = (CASE WHEN @status = N'Đã đóng' THEN GETDATE() ELSE NULL END)
                        WHERE AccountID = @accID AND ClassID = @classID AND TuitionMonth = @month AND TuitionYear = @year
                    END
                    ELSE
                    BEGIN
                        INSERT INTO HocPhi (AccountID, ClassID, TuitionMonth, TuitionYear, SoTien, TrangThai, NgayDong)
                        VALUES (@accID, @classID, @month, @year, @tien, @status, 
                               (CASE WHEN @status = N'Đã đóng' THEN GETDATE() ELSE NULL END))
                    END";

                        using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@accID", row["AccountID"]);
                            cmd.Parameters.AddWithValue("@classID", MaLop);
                            cmd.Parameters.AddWithValue("@tien", row["SoTien"]);
                            cmd.Parameters.AddWithValue("@status", row["TrangThai"]);
                            cmd.Parameters.AddWithValue("@month", 5); 
                            cmd.Parameters.AddWithValue("@year", 2026);                          
                            cmd.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();
                    MessageBox.Show("Cập nhật thiết lập học phí thành công!", "Thông báo");
                    this.Close();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }
        }
        private void LoadChiTietHocPhi()
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                // Lấy danh sách từ JoinClass làm gốc, nối với HocPhi để lấy thông tin (nếu có)
                string query = @"
            SELECT 
                ROW_NUMBER() OVER (ORDER BY a.name) AS STT,
                a.Id AS AccountID, 
                a.name AS HoTen, 
                ISNULL(hp.SoTien, @tienMacDinh) AS SoTien, 
                ISNULL(hp.TrangThai, N'Chưa đóng') AS TrangThai
            FROM JoinClass jc
            INNER JOIN accountList a ON jc.AccountID = a.Id
            LEFT JOIN HocPhi hp ON jc.AccountID = hp.AccountID AND jc.classID = hp.ClassID
            WHERE jc.classID = @classID";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@classID", MaLop);
                    adapter.SelectCommand.Parameters.AddWithValue("@tienMacDinh", 0); // Hoặc lấy fee_default từ bảng Class

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgHocSinhLop.ItemsSource = dt.DefaultView;
                }
            }
        }
        public int capacity_cl(string idlop)
        {
            int cap = 0;
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM JoinClass WHERE classID = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idlop);
                    cap = (int)cmd.ExecuteScalar();
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
                    LoadChiTietHocPhi();
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