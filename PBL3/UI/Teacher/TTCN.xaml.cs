using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.Teacher
{
    public partial class TTCN : UserControl
    {
        private readonly DatabaseHelper db = new DatabaseHelper();
        private readonly string currentTeacherID;

        public TTCN(string teacherId)
        {
            InitializeComponent();
            currentTeacherID = teacherId;

            Loaded += TTCN_Load;
        }

        private void TTCN_Load(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(currentTeacherID))
            {
                MessageBox.Show("Không tìm thấy mã giảng viên!", "Lỗi");
                return;
            }

            LoadTeacherInfo();
        }

        private void LoadTeacherInfo()
        {
            string query = @"
                SELECT 
                    a.Id, a.name, a.sex, a.dateOfBirth, a.phone, t.subject
                FROM accountList a
                INNER JOIN teacherInfo t ON a.Id = t.Id
                WHERE a.Id = @TeacherID";

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TeacherID", currentTeacherID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tbHVT.Text = reader["name"] != DBNull.Value ? reader["name"].ToString() : "";
                                tbMGV.Text = reader["Id"] != DBNull.Value ? reader["Id"].ToString() : "";
                                tbMD.Text = reader["subject"] != DBNull.Value ? reader["subject"].ToString() : "";
                                tbSDT.Text = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : "";
                                string dbSex = reader["sex"] != DBNull.Value ? reader["sex"].ToString().Trim() : "";

                                if (dbSex.Equals("Male", StringComparison.OrdinalIgnoreCase))
                                {
                                    cbbGT.Text = "Nam";
                                }
                                else if (dbSex.Equals("Female", StringComparison.OrdinalIgnoreCase))
                                {
                                    cbbGT.Text = "Nữ";
                                }
                                else
                                {
                                    cbbGT.Text = dbSex;
                                }

                                if (reader["dateOfBirth"] != DBNull.Value)
                                {
                                    dtNS.SelectedDate = Convert.ToDateTime(reader["dateOfBirth"]);
                                }

                                tbE.Text = "Chưa có dữ liệu";
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy thông tin giảng viên trong hệ thống.", "Lỗi dữ liệu");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải thông tin cá nhân: " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection con = db.GetConnection())
            {
                try
                {
                    con.Open();

                    string query = @"
                        UPDATE accountList
                        SET name = @name, dateOfBirth = @dob, sex = @sex, phone = @phone
                        WHERE Id = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        string dbSex = "";
                        if (cbbGT.Text.Equals("Nữ", StringComparison.OrdinalIgnoreCase))
                        {
                            dbSex = "Female";
                        }
                        else if (cbbGT.Text.Equals("Nam", StringComparison.OrdinalIgnoreCase))
                        {
                            dbSex = "Male";
                        }
                        else
                        {
                            dbSex = cbbGT.Text;
                        }
                        cmd.Parameters.AddWithValue("@name", tbHVT.Text);
                        cmd.Parameters.AddWithValue("@dob", dtNS.SelectedDate ?? DateTime.Now);
                        cmd.Parameters.AddWithValue("@sex", dbSex);
                        cmd.Parameters.AddWithValue("@phone", tbSDT.Text);
                        cmd.Parameters.AddWithValue("@id", currentTeacherID);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Cập nhật thông tin thành công");
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