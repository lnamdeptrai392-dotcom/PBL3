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
            cbbThang.Text = DateTime.Now.Month.ToString();
            cbbNam.Text = DateTime.Now.Year.ToString();
            LoadKhoanChi();
        }

        private void SetupDataGridView()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.CanUserAddRows = false;            
            dataGridView1.SelectionMode = DataGridSelectionMode.Single;
            dataGridView1.SelectionUnit = DataGridSelectionUnit.Cell;
        }

        private void TinhTongKhoanChi()
        {
            decimal tong = 0;
            foreach (DataRow row in dtChi.Rows)
            {
                if (row.RowState != DataRowState.Deleted && row["SoTien"] != DBNull.Value)
                {
                    tong += Convert.ToDecimal(row["SoTien"]);
                }
            }
            tbKT.Text = tong.ToString("N0") + " VNĐ";
        }
        private void LoadKhoanChi()
        {
            if (cbbThang == null || cbbNam == null) return;
            string thang = (cbbThang.SelectedItem as ComboBoxItem)?.Content.ToString() ?? cbbThang.Text;
            string nam = (cbbNam.SelectedItem as ComboBoxItem)?.Content.ToString() ?? cbbNam.Text;
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"
                    Select ChiID, LoaiChi, NoiDung, SoTien, NgayChi,GhiChu
                    From KhoanChi
                    Where ChiMonth = @month AND ChiYear = @year";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@month", thang);
                    cmd.Parameters.AddWithValue("@year", nam);
                    try
                    {
                        conn.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        dtChi = new DataTable();
                        adapter.Fill(dtChi);
                        dataGridView1.ItemsSource = dtChi.DefaultView;
                        TinhTongKhoanChi();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Loi tai du lieu: " + ex.Message);
                    }
                }
            }
        }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadKhoanChi();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridView1.ItemsSource == null) return;
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    foreach (DataRow row in dtChi.Rows)
                    {
                        if (row.RowState == DataRowState.Deleted) continue;
                        string query = "";                        
                        if (row["ChiID"] == DBNull.Value || string.IsNullOrEmpty(row["ChiID"].ToString()))
                        {
                            query = @"INSERT INTO KhoanChi (LoaiChi, NoiDung, SoTien, NgayChi, ChiMonth, ChiYear, GhiChu)
                                      VALUES (@loai, @nd, @tien, @ngay, @month, @year, @gc)";
                        }
                        else
                        {
                            query = @"UPDATE KhoanChi 
                                      SET LoaiChi=@loai, NoiDung=@nd, SoTien=@tien, NgayChi=@ngay, 
                                          ChiMonth=@month, ChiYear=@year, GhiChu=@gc
                                      WHERE ChiID=@id";
                        }
                        using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                        {
                            DateTime ngayChi = row["NgayChi"] != DBNull.Value ? Convert.ToDateTime(row["NgayChi"]) : DateTime.Now;

                            cmd.Parameters.AddWithValue("@loai", row["LoaiChi"]?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@nd", row["NoiDung"]?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@tien", row["SoTien"] == DBNull.Value ? 0 : row["SoTien"]);
                            cmd.Parameters.AddWithValue("@ngay", ngayChi);
                            cmd.Parameters.AddWithValue("@month", ngayChi.Month);
                            cmd.Parameters.AddWithValue("@year", ngayChi.Year);
                            cmd.Parameters.AddWithValue("@gc", row["GhiChu"]?.ToString() ?? "");

                            if (row["ChiID"] != DBNull.Value && !string.IsNullOrEmpty(row["ChiID"].ToString()))
                            {
                                cmd.Parameters.AddWithValue("@id", row["ChiID"]);
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();
                    MessageBox.Show("Đã lưu tất cả thay đổi!", "Thông báo");
                    LoadKhoanChi(); // Load lại để cập nhật ID mới từ DB
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi khi lưu: " + ex.Message);
                }
            }
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataRow newRow = dtChi.NewRow();
            newRow["NgayChi"] = DateTime.Now;
            newRow["SoTien"] = 0;
            newRow["LoaiChi"] = "";
            newRow["NoiDung"] = "";
            dtChi.Rows.Add(newRow);
            dataGridView1.ScrollIntoView(newRow);
        }
    }
}