using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.Student
{
    public partial class StudentScore : UserControl
    {
        private readonly DatabaseHelper db = new DatabaseHelper();
        private string currentID = "";

        public StudentScore(string id)
        {
            InitializeComponent();
            currentID = id;

            Loaded += StudentScore_Load;
        }

        private void StudentScore_Load(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentID))
            {
                LoadScoreData();
            }
        }

        private void LoadScoreData()
        {
            using (SqlConnection con = db.GetConnection())
            {
                try
                {
                    con.Open();

                    string query = @"
                        SELECT 
                            d.ClassID AS [Mã môn], 
                            cl.class_name AS [Tên lớp học], 
                            d.Diem AS [Điểm học tập], 
                            d.NhanXet AS [Nhận xét]
                        FROM JoinClass jc
                        INNER JOIN Diem d 
                            ON d.ClassID = jc.classID 
                            AND d.AccountID = jc.AccountID
                        INNER JOIN Class cl 
                            ON cl.classID = jc.classID
                        WHERE jc.AccountID = @id";

                    using (SqlDataAdapter a = new SqlDataAdapter(query, con))
                    {
                        a.SelectCommand.Parameters.AddWithValue("@id", currentID);

                        DataTable dt = new DataTable();
                        a.Fill(dt);

                        dgvScore.ItemsSource = dt.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            // xuất file excel
        }

        private void btnShowData_Click(object sender, RoutedEventArgs e)
        {
            LoadScoreData();
        }

        private void cboSemester_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            // lọc theo học kỳ nếu sau này có cột học kỳ
        }
    }
}