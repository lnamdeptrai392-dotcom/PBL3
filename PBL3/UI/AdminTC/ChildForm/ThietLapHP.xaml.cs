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

        private decimal _soTienMoiHS = 0;

        private void btLuu_Click(object sender, RoutedEventArgs e)
        {
            // Bắt buộc phải đã nhập và tính tiền trước khi lưu
            if (_soTienMoiHS <= 0)
            {
                MessageBox.Show("Vui lòng nhập số tiền và nhấn 'Tính tổng tiền' trước khi lưu!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Commit ô đang edit (nếu user đang sửa trạng thái mà chưa click ra ngoài)
            dgHocSinhLop.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);

            DataView dv = (DataView)dgHocSinhLop.ItemsSource;
            DataTable dt = dv.ToTable();

            int thangHienTai = DateTime.Now.Month;
            int namHienTai = DateTime.Now.Year;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string trangThai = row["TrangThai"]?.ToString() ?? "Chưa đóng";

                        string query = @"
                    IF EXISTS (
                        SELECT 1 FROM HocPhi 
                        WHERE AccountID = @accID 
                          AND ClassID   = @classID 
                          AND TuitionMonth = @month 
                          AND TuitionYear  = @year
                    )
                    BEGIN
                        UPDATE HocPhi 
                        SET SoTien    = @tien,
                            TrangThai = @status,
                            NgayDong  = CASE WHEN @status = N'Đã đóng' THEN GETDATE() ELSE NULL END
                        WHERE AccountID = @accID 
                          AND ClassID   = @classID 
                          AND TuitionMonth = @month 
                          AND TuitionYear  = @year
                    END
                    ELSE
                    BEGIN
                        INSERT INTO HocPhi 
                            (AccountID, ClassID, TuitionMonth, TuitionYear, SoTien, TrangThai, NgayDong)
                        VALUES 
                            (@accID, @classID, @month, @year, @tien, @status,
                             CASE WHEN @status = N'Đã đóng' THEN GETDATE() ELSE NULL END)
                    END";

                        using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@accID", row["AccountID"]);
                            cmd.Parameters.AddWithValue("@classID", MaLop);
                            cmd.Parameters.AddWithValue("@tien", _soTienMoiHS);   // ← lấy từ biến đã tính
                            cmd.Parameters.AddWithValue("@status", trangThai);
                            cmd.Parameters.AddWithValue("@month", thangHienTai);   // ← tháng thực tế
                            cmd.Parameters.AddWithValue("@year", namHienTai);     // ← năm thực tế
                            cmd.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();
                    MessageBox.Show("Cập nhật học phí thành công!", "Thông báo",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void LoadChiTietHocPhi()
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                int thang = DateTime.Now.Month;
                int nam = DateTime.Now.Year;

                string query = @"
            SELECT 
                ROW_NUMBER() OVER (ORDER BY a.name) AS STT,
                a.Id   AS AccountID, 
                a.name AS HoTen, 
                ISNULL(hp.SoTien, @tienMacDinh)    AS SoTien, 
                ISNULL(hp.TrangThai, N'Chưa đóng') AS TrangThai
            FROM JoinClass jc
            INNER JOIN accountList a ON jc.AccountID = a.Id
            LEFT JOIN HocPhi hp 
                ON jc.AccountID  = hp.AccountID 
                AND jc.classID   = hp.ClassID
                AND hp.TuitionMonth = @thang        -- chỉ lấy bản ghi của tháng hiện tại
                AND hp.TuitionYear  = @nam
            WHERE jc.classID = @classID";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@classID", MaLop);
                    adapter.SelectCommand.Parameters.AddWithValue("@tienMacDinh", _soTienMoiHS);
                    adapter.SelectCommand.Parameters.AddWithValue("@thang", thang);
                    adapter.SelectCommand.Parameters.AddWithValue("@nam", nam);

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
            if (!decimal.TryParse(txtTienTrenNg.Text, out decimal hphi) || hphi < 0)
            {
                MessageBox.Show("Vui lòng nhập số tiền hợp lệ (≥ 0)!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _soTienMoiHS = hphi;                      // ← lưu vào biến class
            LoadChiTietHocPhi();                       // reload để cột SoTien hiện đúng
            decimal tongTien = SetHP(hphi);
            txtTongT.Text = tongTien.ToString("N0") + " VNĐ";
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }        
    }
}