using Microsoft.Data.SqlClient;
using PBL3a.services;
using PBL3a.services.BLL;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminTC.ChildForm
{
    public partial class KhoanThu : UserControl
    {
        private DataTable dtThu = new DataTable();
        private AdminTC_Service bll = new AdminTC_Service();

        public KhoanThu()
        {
            InitializeComponent();
            Loaded += KhoanThu_Load;
        }

        private void KhoanThu_Load(object sender, RoutedEventArgs e)
        {
            SetupDataGridView();
            cbbThang.Text = DateTime.Now.Month.ToString();
            cbbNam.Text = DateTime.Now.Year.ToString();

            LoadKhoanThu();
        }

        private void SetupDataGridView()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.CanUserAddRows = false;
            dataGridView1.SelectionMode = DataGridSelectionMode.Single;
            dataGridView1.SelectionUnit = DataGridSelectionUnit.Cell;
        }
        private void TinhTongKhoanThu()
        {
            decimal tong = 0;
            foreach (DataRow row in dtThu.Rows)
            {
                if (row.RowState != DataRowState.Deleted && row["SoTien"] != DBNull.Value && !string.IsNullOrEmpty(row["SoTien"].ToString()))
                {
                    tong += Convert.ToDecimal(row["SoTien"]);
                }
            }
            tbKT.Text = tong.ToString("N0") + " VNĐ"; // Đồng bộ định dạng tiền tệ
        }

        private void LoadKhoanThu()
        {
            if (cbbThang == null || cbbNam == null) return;
            string thang = (cbbThang.SelectedItem as ComboBoxItem)?.Content.ToString() ?? cbbThang.Text;
            string nam = (cbbNam.SelectedItem as ComboBoxItem)?.Content.ToString() ?? cbbNam.Text;

            try
            {
                dtThu = bll.GetKhoanThuKhac(thang, nam);
                dataGridView1.ItemsSource = dtThu.DefaultView;

                TinhTongKhoanThu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu khoản thu: " + ex.Message);
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadKhoanThu();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadKhoanThu();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataRow newRow = dtThu.NewRow();
            newRow["NgayThu"] = DateTime.Today;
            newRow["SoTien"] = 0;
            newRow["LoaiThu"] = "";
            newRow["NoiDung"] = "";
            newRow["GhiChu"] = "";

            dtThu.Rows.Add(newRow);
            dataGridView1.ScrollIntoView(newRow);
            TinhTongKhoanThu();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridView1.ItemsSource == null) return;

            try
            {
                dataGridView1.CommitEdit(DataGridEditingUnit.Row, true);
                bll.SaveKhoanThuChanges(dtThu);

                MessageBox.Show("Đã lưu tất cả thay đổi khoản thu khác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadKhoanThu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu khoản thu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}