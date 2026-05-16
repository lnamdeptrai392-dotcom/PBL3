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
        

        public Lai()
        {
            InitializeComponent();
            NapComboBox();
        }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadLoiNhuan();
        }
        private void NapComboBox()
        {
            int currentYear = DateTime.Now.Year;
            cbbNam.Items.Clear();
            for (int y = currentYear; y >= 2020; y--)
                cbbNam.Items.Add(y);

            if (cbbNam.Items.Count > 0)
                cbbNam.SelectedItem = currentYear;

            // Nạp tháng: thêm "Tất cả" và 1-12
            cbbThang.Items.Clear();
            cbbThang.Items.Add("Tất cả");
            for (int m = 1; m <= 12; m++)
                cbbThang.Items.Add(m);

            cbbThang.SelectedItem = DateTime.Now.Month;
        }
        private void LoadLoiNhuan()
        {
            if (cbbNam.SelectedItem == null) return;

            int nam = (int)cbbNam.SelectedItem;
            int? thang = null;

            // Kiểm tra nếu chọn "Tất cả" thì thang = null
            if (cbbThang.SelectedItem != null && cbbThang.SelectedItem.ToString() != "Tất cả")
                thang = (int)cbbThang.SelectedItem;

            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"
            WITH ThuHocPhi AS (  
                SELECT ISNULL(SUM(SoTien), 0) as TongThu    
                FROM HocPhi    
                WHERE TrangThai = N'Đã đóng' 
                    AND TuitionYear = @year
                    " + (thang.HasValue ? "AND TuitionMonth = @month" : "") + @"
            ),
            ThuKhac AS (
                SELECT ISNULL(SUM(SoTien), 0) as TongThuKhac
                FROM KhoanThu
                WHERE ThuYear = @year
                    " + (thang.HasValue ? "AND ThuMonth = @month" : "") + @"
            ),
            ChiLuong AS (    
                SELECT ISNULL(SUM(TongLuong), 0) as TongLuong 
                FROM LuongGV     
                WHERE TrangThai = N'Đã thanh toán' 
                    AND SalaryYear = @year
                    " + (thang.HasValue ? "AND SalaryMonth = @month" : "") + @"
            ),
            ChiKhac AS (    
                SELECT ISNULL(SUM(SoTien), 0) as TongChiKhac     
                FROM KhoanChi    
                WHERE ChiYear = @year
                    " + (thang.HasValue ? "AND ChiMonth = @month" : "") + @"
            )
            SELECT     
                N'1. Thu học phí' AS DanhMuc, TongThu AS SoTien, N'Thu' AS Loai
            FROM ThuHocPhi
            WHERE TongThu > 0
            UNION ALL
            SELECT     
                N'2. Thu khác' AS DanhMuc, TongThuKhac, N'Thu'
            FROM ThuKhac
            WHERE TongThuKhac > 0
            UNION ALL
            SELECT     
                N'3. Chi trả lương' AS DanhMuc, TongLuong, N'Chi'
            FROM ChiLuong
            WHERE TongLuong > 0
            UNION ALL
            SELECT   
                N'4. Chi khác' AS DanhMuc, TongChiKhac, N'Chi'
            FROM ChiKhac
            WHERE TongChiKhac > 0;";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@year", nam);
                if (thang.HasValue)
                    cmd.Parameters.AddWithValue("@month", thang.Value);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dtBaoCao = new DataTable();
                adapter.Fill(dtBaoCao);

                decimal tongThu = 0;
                decimal tongChi = 0;

                foreach (DataRow row in dtBaoCao.Rows)
                {
                    decimal tien = Convert.ToDecimal(row["SoTien"]);
                    if (row["Loai"].ToString() == "Thu")
                        tongThu += tien;
                    else
                        tongChi += tien;
                }

                

                dataGridView1.ItemsSource = dtBaoCao.DefaultView;
                txtLoiNhuan.Text = (tongThu - tongChi).ToString("N0") + " VNĐ";
            
        }
        }
        
    }
}
