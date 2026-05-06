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
            currentID = id;
            InitializeComponent();

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
                            ClassID AS ClassID,
                            TuitionYear AS TuitionYear, 
                            TuitionMonth AS TuitionMonth, 
                            SoTien AS SoTien, 
                            NgayDong AS NgayDong, 
                            TrangThai AS TrangThai
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