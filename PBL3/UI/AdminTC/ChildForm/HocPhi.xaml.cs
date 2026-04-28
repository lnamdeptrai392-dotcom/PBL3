using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PBL3a.UI.AdminTC
{
    public partial class HocPhi : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private DataTable dtHocPhi = new DataTable();

        public HocPhi()
        {
            InitializeComponent();
            Loaded += HocPhi_Load;
        }

        private void HocPhi_Load(object sender, RoutedEventArgs e)
        {
            LoadDanhSachLop("");
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.CanUserAddRows = false;
            dataGridView1.SelectionMode = DataGridSelectionMode.Single;
            dataGridView1.SelectionUnit = DataGridSelectionUnit.FullRow;
        }

        private void LoadDanhSachLop(string text)
        {
            cbbML.Items.Clear();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT classID FROM Class WHERE classID LIKE @text ORDER BY classID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@text", "%" + text + "%");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cbbML.Items.Add(reader["classID"].ToString());
                        }
                    }
                }
            }

            if (cbbML.Items.Count > 0 && string.IsNullOrWhiteSpace(text))
            {
                cbbML.SelectedIndex = 0;
            }
        }

        private void cbbML_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbML.SelectedItem == null) return;

            string classID = cbbML.SelectedItem.ToString();

            LoadTenLop(classID);
            LoadHocPhiTheoLop(classID);
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

        private void LoadHocPhiTheoLop(string classID)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT 
                        hp.HocPhiID AS [ID],
                        hp.AccountID AS [Mã HS],
                        a.name AS [Tên học sinh],
                        hp.TuitionMonth AS [Tháng],
                        hp.TuitionYear AS [Năm],
                        hp.SoTien AS [Số tiền],
                        hp.TrangThai AS [Trạng thái],
                        hp.NgayDong AS [Ngày đóng],
                        hp.GhiChu AS [Ghi chú]
                    FROM HocPhi hp
                    INNER JOIN accountList a ON hp.AccountID = a.Id
                    WHERE hp.ClassID = @classID
                    ORDER BY hp.TuitionYear DESC, hp.TuitionMonth DESC, a.Id";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@classID", classID);

                    dtHocPhi = new DataTable();
                    adapter.Fill(dtHocPhi);

                    dataGridView1.ItemsSource = dtHocPhi.DefaultView;
                }
            }

            SetReadOnlyColumns();
        }

        private void SetReadOnlyColumns()
        {
            foreach (DataGridColumn column in dataGridView1.Columns)
            {
                if (column.Header == null) continue;

                string header = column.Header.ToString();

                if (header == "ID" ||
                    header == "Mã HS" ||
                    header == "Tên học sinh" ||
                    header == "Tháng" ||
                    header == "Năm")
                {
                    column.IsReadOnly = true;
                }
            }
        }

        private void btSetHP_Click(object sender, RoutedEventArgs e)
        {
            string malop = cbbML.Text;

            ThietLapHP thietLap = new ThietLapHP(malop);
            thietLap.ShowDialog();

            LoadHocPhiTheoLop(malop);
        }

        private void cbbML_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                string keyword = cbbML.Text;

                cbbML.SelectionChanged -= cbbML_SelectionChanged;
                LoadDanhSachLop(keyword);
                cbbML.SelectionChanged += cbbML_SelectionChanged;

                cbbML.Text = keyword;
                cbbML.IsDropDownOpen = !string.IsNullOrWhiteSpace(keyword) && cbbML.Items.Count > 0;
            }
            catch
            {
                cbbML.IsDropDownOpen = false;
            }
        }
    }
}