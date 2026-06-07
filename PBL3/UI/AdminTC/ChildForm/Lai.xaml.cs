using Microsoft.Data.SqlClient;
using PBL3a.services;
using PBL3a.services.BLL;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminTC
{
    public partial class Lai : UserControl
    {
        private AdminTC_Service bll = new AdminTC_Service();

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

            try
            {
                DataTable dtBaoCao = bll.GetLoiNhuanData(nam, thang);

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
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo lợi nhuận: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}