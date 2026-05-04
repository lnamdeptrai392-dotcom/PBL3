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
        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadLoiNhuan();
        }
        private void Lai_Load(object sender, RoutedEventArgs e)
        {
            SetupDataGridView();
            cbbThang.Text = DateTime.Now.Month.ToString();
            cbbNam.Text = DateTime.Now.Year.ToString();
            LoadLoiNhuan();
        }
        private void LoadLoiNhuan()
        {
            if (cbbThang == null || cbbNam == null) return;
            string thang = (cbbThang.SelectedItem as ComboBoxItem)?.Content.ToString() ?? cbbThang.Text;
            string nam = (cbbNam.SelectedItem as ComboBoxItem)?.Content.ToString() ?? cbbNam.Text;
            using (SqlConnection conn = db.GetConnection())
            {
                // Sử dụng câu query UNION ở trên
                string query = @"WITH ThuHocPhi AS (  
                                SELECT ISNULL(SUM(SoTien), 0) as TongThu    
                                FROM HocPhi    
                                WHERE TrangThai = N'Đã đóng' AND TuitionMonth = @month AND TuitionYear = @year),
                                ChiLuong AS (    
                                SELECT ISNULL(SUM(TongLuong), 0) as TongLuong 
                                FROM LuongGV     
                                WHERE TrangThai = N'Đã thanh toán' AND SalaryMonth = @month AND SalaryYear = @year),
                                ChiKhac AS (    
                                SELECT ISNULL(SUM(SoTien), 0) as TongChiKhac     
                                FROM KhoanChi    
                                WHERE ChiMonth = @month AND ChiYear = @year)
                                SELECT     
                                N'1. Thu học phí' AS DanhMuc, TongThu AS SoTien, N'Thu' AS Loai
                                FROM ThuHocPhi
                                UNION ALL
                                SELECT     
                                N'2. Chi trả lương' AS DanhMuc, TongLuong, N'Chi'
                                FROM ChiLuong
                                UNION ALL
                                SELECT   
                                N'3. Khoản chi khác' AS DanhMuc, TongChiKhac, N'Chi'
                                FROM ChiKhac;";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.SelectCommand.Parameters.AddWithValue("@month", cbbThang.Text);
                adapter.SelectCommand.Parameters.AddWithValue("@year", cbbNam.Text);

                DataTable dtBaoCao = new DataTable();
                adapter.Fill(dtBaoCao);

                decimal tongThu = 0;
                decimal tongChi = 0;

                foreach (DataRow row in dtBaoCao.Rows)
                {
                    decimal tien = Convert.ToDecimal(row["SoTien"]);
                    if (row["Loai"].ToString() == "Thu") tongThu += tien;
                    else tongChi += tien;
                }

                DataRow profitRow = dtBaoCao.NewRow();
                profitRow["DanhMuc"] = "LỢI NHUẬN THUẦN";
                profitRow["SoTien"] = tongThu - tongChi;
                profitRow["Loai"] = "KetQua";
                dtBaoCao.Rows.Add(profitRow);

                dataGridView1.ItemsSource = dtBaoCao.DefaultView;
                txtLoiNhuan.Text = (tongThu - tongChi).ToString("N0");
            }
        }
        private void SetupDataGridView()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.IsReadOnly = true;
            dataGridView1.CanUserAddRows = false;
            dataGridView1.SelectionMode = DataGridSelectionMode.Single;
            dataGridView1.SelectionUnit = DataGridSelectionUnit.FullRow;
        }
    }
}
