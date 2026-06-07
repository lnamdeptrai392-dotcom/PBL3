using Microsoft.Data.SqlClient;
using PBL3a.services;
using PBL3a.services.BLL;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminTC
{
    public partial class KhoanChi : UserControl
    {
        private DataTable dtChi = new DataTable();
        private AdminTC_Service bll = new AdminTC_Service();

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

            try
            {
                dtChi = bll.GetKhoanChiByTime(thang, nam);
                dataGridView1.ItemsSource = dtChi.DefaultView;
                TinhTongKhoanChi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loi tai du lieu: " + ex.Message);
            }
        }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadKhoanChi();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridView1.ItemsSource == null) return;

            try
            {
                bll.SaveKhoanChiChanges(dtChi);
                MessageBox.Show("Đã lưu tất cả thay đổi!", "Thông báo");
                LoadKhoanChi(); // Load lại để cập nhật ID mới từ DB
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message);
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