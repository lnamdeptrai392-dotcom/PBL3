using System;
using System.Data;
using Microsoft.Data.SqlClient;
using PBL3a.services;

namespace PBL3a.services.BLL
{
    public class Student_Service
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();

        // 1. Tải danh sách học sinh (Từ accountList kết hợp với JoinClass và Class)
        public DataTable GetListHocSinh(string keyword = "")
        {
            string query = @"
                SELECT 
                    a.Id AS MaHS, 
                    a.name AS HoTen, 
                    a.dateOfBirth AS NgaySinh, 
                    a.sex AS GioiTinh, 
                    a.phone AS SDTPhuHuynh,
                    c.classID AS MaLop,
                    c.class_name AS Lop
                FROM accountList a
                LEFT JOIN JoinClass jc ON a.Id = jc.AccountID
                LEFT JOIN Class c ON jc.classID = c.classID
                WHERE a.Role = 'Student' 
                  AND (a.Id LIKE @key OR a.name LIKE @key)";

            DataTable dt = new DataTable();
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@key", "%" + keyword + "%");
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // 2. Tải danh sách lớp học cho ComboBox
        public DataTable GetDanhSachLop()
        {
            string query = "SELECT classID AS MaLop, class_name AS TenLop FROM Class";
            DataTable dt = new DataTable();
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.Fill(dt);
            }
            return dt;
        }

        // 3. Thêm mới học sinh (Thêm vào accountList và JoinClass)
        public bool AddHocSinh(string ma, string ten, DateTime? ns, string gt, string sdt, string maLop)
        {
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert vào bảng accountList
                        string queryAcc = @"
                            INSERT INTO accountList (Id, name, dateOfBirth, sex, phone, Role, password, status) 
                            VALUES (@ma, @ten, @ns, @gt, @sdt, 'Student', '123456', N'Hoạt động')";

                        using (SqlCommand cmdAcc = new SqlCommand(queryAcc, conn, trans))
                        {
                            cmdAcc.Parameters.AddWithValue("@ma", ma);
                            cmdAcc.Parameters.AddWithValue("@ten", ten);
                            cmdAcc.Parameters.AddWithValue("@ns", (object)ns ?? DBNull.Value);
                            cmdAcc.Parameters.AddWithValue("@gt", gt);
                            cmdAcc.Parameters.AddWithValue("@sdt", sdt);
                            cmdAcc.ExecuteNonQuery();
                        }

                        // Insert vào bảng JoinClass (để biết HS này học lớp nào)
                        if (!string.IsNullOrEmpty(maLop))
                        {
                            string queryJoin = "INSERT INTO JoinClass (AccountID, classID) VALUES (@ma, @maLop)";
                            using (SqlCommand cmdJoin = new SqlCommand(queryJoin, conn, trans))
                            {
                                cmdJoin.Parameters.AddWithValue("@ma", ma);
                                cmdJoin.Parameters.AddWithValue("@maLop", maLop);
                                cmdJoin.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }

        // 4. Cập nhật học sinh
        public bool EditHocSinh(string ma, string ten, DateTime? ns, string gt, string sdt)
        {
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Chỉ cập nhật thông tin cá nhân trong accountList
                        string queryAcc = "UPDATE accountList SET name=@ten, dateOfBirth=@ns, sex=@gt, phone=@sdt WHERE Id=@ma AND Role='Student'";
                        using (SqlCommand cmdAcc = new SqlCommand(queryAcc, conn, trans))
                        {
                            cmdAcc.Parameters.AddWithValue("@ma", ma);
                            cmdAcc.Parameters.AddWithValue("@ten", ten);
                            cmdAcc.Parameters.AddWithValue("@ns", (object)ns ?? DBNull.Value);
                            cmdAcc.Parameters.AddWithValue("@gt", gt);
                            cmdAcc.Parameters.AddWithValue("@sdt", sdt);
                            cmdAcc.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }

        public DataTable GetLichSuHocTap(string maHS)
        {
            string query = @"
                 SELECT 
                     c.classID AS MaLop, 
                     c.class_name AS TenLop, 
                     c.start_date AS NgayMoLop 
                 FROM JoinClass jc
                 JOIN Class c ON jc.classID = c.classID
                 WHERE jc.AccountID = @maHS";

            DataTable dt = new DataTable();
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@maHS", maHS);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }


    }
}