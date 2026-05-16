using System;
using System.Data;
using Microsoft.Data.SqlClient;
using PBL3a.services;

namespace PBL3a.services.BLL
{
    public class Teacher_Service
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();

        // 1. Lấy danh sách giáo viên
        public DataTable GetListGiaoVien(string keyword = "")
        {
            string query = @"
                SELECT 
                    a.Id AS MaGV, 
                    a.name AS HoTen, 
                    a.dateOfBirth AS NgaySinh, 
                    a.sex AS GioiTinh, 
                    a.phone AS SDT,
                    t.subject AS ChuyenMon,
                    a.status AS TinhTrang
                FROM accountList a
                LEFT JOIN teacherInfo t ON a.Id = t.Id
                WHERE a.Role = 'Teacher' 
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

        // 2. Thêm mới giáo viên
        public bool AddGiaoVien(string ma, string ten, DateTime? ns, string gt, string sdt, string chuyenMon, string tinhTrang)
        {
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string queryAcc = "INSERT INTO accountList (Id, username, Password, name, dateOfBirth, sex, phone, Role, status) " +
                                          "VALUES (@ma, @ma, '123456', @ten, @ns, @gt, @sdt, 'Teacher', @tinhTrang)";
                        using (SqlCommand cmd = new SqlCommand(queryAcc, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@ma", ma);
                            cmd.Parameters.AddWithValue("@ten", ten);
                            cmd.Parameters.AddWithValue("@ns", (object)ns ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@gt", gt);
                            cmd.Parameters.AddWithValue("@sdt", string.IsNullOrEmpty(sdt) ? DBNull.Value : (object)sdt);
                            cmd.Parameters.AddWithValue("@tinhTrang", string.IsNullOrEmpty(tinhTrang) ? "Hoạt động" : tinhTrang);
                            cmd.ExecuteNonQuery();
                        }

                        string queryTeacher = "INSERT INTO teacherInfo (Id, subject) VALUES (@ma, @chuyenMon)";
                        using (SqlCommand cmd = new SqlCommand(queryTeacher, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@ma", ma);
                            cmd.Parameters.AddWithValue("@chuyenMon", chuyenMon);
                            cmd.ExecuteNonQuery();
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

        // 3. Cập nhật giáo viên
        public bool UpdateGiaoVien(string ma, string ten, DateTime? ns, string gt, string sdt, string chuyenMon, string tinhTrang)
        {
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string queryAcc = "UPDATE accountList SET name=@ten, dateOfBirth=@ns, sex=@gt, phone=@sdt, status=@tinhTrang " +
                                          "WHERE Id=@ma AND Role='Teacher'";
                        using (SqlCommand cmd = new SqlCommand(queryAcc, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@ma", ma);
                            cmd.Parameters.AddWithValue("@ten", ten);
                            cmd.Parameters.AddWithValue("@ns", (object)ns ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@gt", gt);
                            cmd.Parameters.AddWithValue("@sdt", string.IsNullOrEmpty(sdt) ? DBNull.Value : (object)sdt);
                            cmd.Parameters.AddWithValue("@tinhTrang", tinhTrang);
                            cmd.ExecuteNonQuery();
                        }

                        string queryTeacher = "UPDATE teacherInfo SET subject=@chuyenMon WHERE Id=@ma";
                        using (SqlCommand cmd = new SqlCommand(queryTeacher, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@ma", ma);
                            cmd.Parameters.AddWithValue("@chuyenMon", chuyenMon);
                            cmd.ExecuteNonQuery();
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
    }
}