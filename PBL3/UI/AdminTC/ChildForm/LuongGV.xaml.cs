using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PBL3a.UI.AdminTC
{
    public partial class LuongGV : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private DataTable dtLuong = new DataTable();

        public LuongGV()
        {
            InitializeComponent();
            Loaded += LuongGV_Load;
        }

        private void LuongGV_Load(object sender, RoutedEventArgs e)
        {
            LoadDanhSachGV("");
        }

        // ===== LOAD GV =====
        private void LoadDanhSachGV(string text)
        {
            cbbMGV.Items.Clear();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query = "SELECT Id FROM accountList WHERE Role='Teacher' AND Id LIKE @text";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@text", "%" + text + "%");

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            cbbMGV.Items.Add(r["Id"].ToString());
                        }
                    }
                }
            }
        }

        private void cbbMGV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbMGV.SelectedItem == null) return;

            string id = cbbMGV.SelectedItem.ToString();

            LoadTenGV(id);
            LoadLuong(id);
            txtNam.Text = DateTime.Now.Year.ToString();
        }

        private void LoadTenGV(string id)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT name FROM accountList WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                tbTL.Text = cmd.ExecuteScalar()?.ToString();
            }
        }

        // ===== LOAD LƯƠNG =====
        private void LoadLuong(string id)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"SELECT * FROM LuongGV WHERE TeacherID=@id ORDER BY SalaryYear DESC, SalaryMonth DESC";

                SqlDataAdapter ad = new SqlDataAdapter(query, conn);
                ad.SelectCommand.Parameters.AddWithValue("@id", id);

                dtLuong = new DataTable();
                ad.Fill(dtLuong);

                dataGridView1.ItemsSource = dtLuong.DefaultView;
            }
        }

        // ===== TÍNH LƯƠNG =====
        private void btSetL_Click(object sender, RoutedEventArgs e)
        {
            if (cbbMGV.SelectedItem == null)
            {
                MessageBox.Show("Chọn giảng viên");
                return;
            }

            string id = cbbMGV.SelectedItem.ToString();

            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            if (cbbThang.SelectedItem != null && txtNam.Text != "")
            {
                month = int.Parse(((ComboBoxItem)cbbThang.SelectedItem).Content.ToString());
                year = int.Parse(txtNam.Text);
            }

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                // check tồn tại
                string check = "SELECT COUNT(*) FROM LuongGV WHERE TeacherID=@id AND SalaryMonth=@m AND SalaryYear=@y";
                SqlCommand cmdCheck = new SqlCommand(check, conn);
                cmdCheck.Parameters.AddWithValue("@id", id);
                cmdCheck.Parameters.AddWithValue("@m", month);
                cmdCheck.Parameters.AddWithValue("@y", year);

                if ((int)cmdCheck.ExecuteScalar() > 0)
                {
                    MessageBox.Show("Đã tồn tại");
                    return;
                }

                // tính
                int soLop = (int)new SqlCommand(
                    @"SELECT COUNT(*) FROM Class WHERE teacherID=@id",
                    conn)
                { Parameters = { new SqlParameter("@id", id) } }.ExecuteScalar();

                int soBuoi = soLop * 8;
                decimal luong = soBuoi * 400000;

                string insert = @"INSERT INTO LuongGV
                (TeacherID,SalaryMonth,SalaryYear,SoLopDay,SoBuoiDay,LuongCoBan,Thuong,Phat,TongLuong)
                VALUES(@id,@m,@y,@lop,@buoi,@luong,0,0,@luong)";

                SqlCommand cmd = new SqlCommand(insert, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@m", month);
                cmd.Parameters.AddWithValue("@y", year);
                cmd.Parameters.AddWithValue("@lop", soLop);
                cmd.Parameters.AddWithValue("@buoi", soBuoi);
                cmd.Parameters.AddWithValue("@luong", luong);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Tính lương xong");
            LoadLuong(id);
        }

        // ===== SEARCH =====
        private void cbbMGV_KeyUp(object sender, KeyEventArgs e)
        {
            string key = cbbMGV.Text;
            LoadDanhSachGV(key);
            cbbMGV.IsDropDownOpen = true;
        }

        // ===== UPDATE GRID =====
        private void dataGridView1_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // giữ đơn giản (nếu cần mình viết full update như WinForms cho bạn)
        }
    }
}