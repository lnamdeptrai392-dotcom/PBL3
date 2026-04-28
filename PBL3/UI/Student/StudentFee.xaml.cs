using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.Student
{
    public partial class StudentFee : UserControl
    {
        private readonly DatabaseHelper db = new DatabaseHelper();
        private readonly string currentID = "";

        public StudentFee(string id)
        {
            InitializeComponent();
            currentID = id;

            Loaded += StudentFee_Load;
        }

        private void StudentFee_Load(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentID))
            {
                LoadHocPhiHocSinh();
            }
        }

        private void LoadHocPhiHocSinh()
        {
            using (SqlConnection con = db.GetConnection())
            {
                try
                {
                    con.Open();

                    string query = @"
                        SELECT 
                            ClassID AS [Mã lớp học],
                            TuitionMonth AS [Tháng học], 
                            TuitionYear AS [Năm học], 
                            SoTien AS [Số tiền], 
                            TrangThai AS [Trạng thái], 
                            NgayDong AS [Ngày đóng], 
                            GhiChu AS [Ghi chú]
                        FROM HocPhi 
                        WHERE AccountID = @id";

                    using (SqlDataAdapter a = new SqlDataAdapter(query, con))
                    {
                        a.SelectCommand.Parameters.AddWithValue("@id", currentID);

                        DataTable dt = new DataTable();
                        a.Fill(dt);

                        dataGridView1.ItemsSource = dt.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}