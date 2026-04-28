using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.Student
{
    public partial class StudentINFO : UserControl
    {
        private readonly DatabaseHelper db = new DatabaseHelper();
        private string currentID = "";

        public StudentINFO()
        {
            InitializeComponent();
        }

        public StudentINFO(string id)
        {
            InitializeComponent();
            currentID = id;

            Loaded += StudentINFO_Load;
        }

        private void StudentINFO_Load(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentID))
            {
                LoadThongTinHocSinh();
            }
        }

        private void LoadThongTinHocSinh()
        {
            using (SqlConnection con = db.GetConnection())
            {
                try
                {
                    con.Open();

                    string query = "SELECT name, dateOfBirth, sex, phone FROM accountList WHERE Id = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", currentID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtHoTen.Text = reader["name"].ToString();

                                dtpNgaySinh.SelectedDate =
                                    reader["dateOfBirth"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["dateOfBirth"])
                                    : DateTime.Now;

                                cboGioiTinh.Text = reader["sex"].ToString();
                                txtSDT.Text = reader["phone"].ToString();

                                txtEmail.Text = "Chưa cập nhật";
                                txtDiaChi.Text = "Chưa cập nhật";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error when loading information: " + ex.Message);
                }
            }
        }

        private void btn_update_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = @"
                        UPDATE accountList 
                        SET name = @name, dateOfBirth = @dob, sex = @sex, phone = @phone 
                        WHERE Id = @id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtHoTen.Text);
                        cmd.Parameters.AddWithValue("@dob", dtpNgaySinh.SelectedDate ?? DateTime.Now);
                        cmd.Parameters.AddWithValue("@sex", cboGioiTinh.Text);
                        cmd.Parameters.AddWithValue("@phone", txtSDT.Text);
                        cmd.Parameters.AddWithValue("@id", currentID);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Cập nhật thông tin thành công!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi cập nhật: " + ex.Message);
                }
            }
        }
    }
}