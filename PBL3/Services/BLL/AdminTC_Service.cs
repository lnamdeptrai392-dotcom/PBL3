using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PBL3a.services.BLL
{
    public class AdminTC_Service
    {
        private DatabaseHelper db = new DatabaseHelper();

        // Nghiệp vụ HocPhi

        public List<string> GetClassIDsByFilters(string grade, string courseID)
        {
            List<string> classIDs = new List<string>();
            string query = "SELECT classID FROM Class WHERE grade = @grade AND courseID = @courseID ORDER BY classID";

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@grade", grade);
                    cmd.Parameters.AddWithValue("@courseID", courseID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["classID"] != DBNull.Value)
                            {
                                classIDs.Add(reader["classID"].ToString());
                            }
                        }
                    }
                }
            }
            return classIDs;
        }

        public DataTable GetHocPhiByClassID(string classID)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    a.Id AS [AccountID], 
                    a.name AS [HoTen], 
                    ISNULL(hp.SoTien, 0) AS [SoTien], 
                    ISNULL(CONVERT(NVARCHAR, hp.NgayDong, 103), N'--') AS [NgayDong],
                    ISNULL(hp.TrangThai, N'Chưa thiết lập') AS [TrangThai]
                FROM JoinClass jc
                INNER JOIN accountList a ON jc.AccountID = a.Id
                LEFT JOIN HocPhi hp 
                    ON jc.AccountID = hp.AccountID 
                    AND jc.classID = hp.ClassID
                WHERE jc.classID = @classID
                ORDER BY hp.TrangThai DESC, a.name ASC";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@classID", classID);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }


        // Nghiệp vụ khoản chi
        public DataTable GetKhoanChiByTime(string month, string year)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT ChiID, LoaiChi, NoiDung, SoTien, NgayChi, GhiChu
                FROM KhoanChi
                WHERE ChiMonth = @month AND ChiYear = @year";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@month", month);
                    cmd.Parameters.AddWithValue("@year", year);

                    conn.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public void SaveKhoanChiChanges(DataTable dtChi)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (DataRow row in dtChi.Rows)
                        {
                            if (row.RowState == DataRowState.Deleted) continue;

                            string query = "";
                            if (row["ChiID"] == DBNull.Value || string.IsNullOrEmpty(row["ChiID"].ToString()))
                            {
                                query = @"INSERT INTO KhoanChi (LoaiChi, NoiDung, SoTien, NgayChi, ChiMonth, ChiYear, GhiChu)
                                          VALUES (@loai, @nd, @tien, @ngay, @month, @year, @gc)";
                            }
                            else
                            {
                                query = @"UPDATE KhoanChi 
                                          SET LoaiChi=@loai, NoiDung=@nd, SoTien=@tien, NgayChi=@ngay, 
                                              ChiMonth=@month, ChiYear=@year, GhiChu=@gc
                                          WHERE ChiID=@id";
                            }

                            using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                            {
                                DateTime ngayChi = row["NgayChi"] != DBNull.Value ? Convert.ToDateTime(row["NgayChi"]) : DateTime.Now;

                                cmd.Parameters.AddWithValue("@loai", row["LoaiChi"]?.ToString() ?? "");
                                cmd.Parameters.AddWithValue("@nd", row["NoiDung"]?.ToString() ?? "");
                                cmd.Parameters.AddWithValue("@tien", row["SoTien"] == DBNull.Value ? 0 : row["SoTien"]);
                                cmd.Parameters.AddWithValue("@ngay", ngayChi);
                                cmd.Parameters.AddWithValue("@month", ngayChi.Month);
                                cmd.Parameters.AddWithValue("@year", ngayChi.Year);
                                cmd.Parameters.AddWithValue("@gc", row["GhiChu"]?.ToString() ?? "");

                                if (row["ChiID"] != DBNull.Value && !string.IsNullOrEmpty(row["ChiID"].ToString()))
                                {
                                    cmd.Parameters.AddWithValue("@id", row["ChiID"]);
                                }
                                cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        // Nghiệp vụ khoản thu
        public DataTable GetKhoanThuKhac(string month, string year)
        {
            DataTable dt = new DataTable();
            string query = @"
                            SELECT ThuID, LoaiThu, NoiDung, SoTien, NgayThu, GhiChu
                            FROM KhoanThu
                            WHERE ThuMonth = @month AND ThuYear = @year";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@month", month);
                    cmd.Parameters.AddWithValue("@year", year);

                    conn.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public void SaveKhoanThuChanges(DataTable dtThu)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (DataRow row in dtThu.Rows)
                        {
                            if (row.RowState == DataRowState.Deleted) continue;

                            string query = "";
                            if (row["ThuID"] == DBNull.Value || string.IsNullOrEmpty(row["ThuID"].ToString()))
                            {
                                query = @"INSERT INTO KhoanThu (LoaiThu, NoiDung, SoTien, NgayThu, ThuMonth, ThuYear, GhiChu)
                                  VALUES (@loai, @nd, @tien, @ngay, @month, @year, @gc)";
                            }
                            else
                            {
                                query = @"UPDATE KhoanThu 
                                  SET LoaiThu=@loai, NoiDung=@nd, SoTien=@tien, NgayThu=@ngay, 
                                      ThuMonth=@month, ThuYear=@year, GhiChu=@gc
                                  WHERE ThuID=@id";
                            }

                            using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                            {
                                DateTime ngayThu = row["NgayThu"] != DBNull.Value ? Convert.ToDateTime(row["NgayThu"]) : DateTime.Now;

                                cmd.Parameters.AddWithValue("@loai", row["LoaiThu"]?.ToString() ?? "");
                                cmd.Parameters.AddWithValue("@nd", row["NoiDung"]?.ToString() ?? "");
                                cmd.Parameters.AddWithValue("@tien", row["SoTien"] == DBNull.Value ? 0 : row["SoTien"]);
                                cmd.Parameters.AddWithValue("@ngay", ngayThu);

                                cmd.Parameters.AddWithValue("@month", ngayThu.Month);
                                cmd.Parameters.AddWithValue("@year", ngayThu.Year);
                                cmd.Parameters.AddWithValue("@gc", row["GhiChu"]?.ToString() ?? "");

                                if (row["ThuID"] != DBNull.Value && !string.IsNullOrEmpty(row["ThuID"].ToString()))
                                {
                                    cmd.Parameters.AddWithValue("@id", row["ThuID"]);
                                }

                                cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        // Nghiệp vụ lợi nhuận, lãi

        public DataTable GetLoiNhuanData(int nam, int? thang)
        {
            DataTable dtBaoCao = new DataTable();
            string query = @"
                            WITH ThuHocPhi AS (  
                                SELECT ISNULL(SUM(SoTien), 0) as TongThu    
                                FROM HocPhi    
                                WHERE TrangThai = N'Đã đóng' 
                                    AND TuitionYear = @year
                                    " + (thang.HasValue ? "AND TuitionMonth = @month" : "") + @"
                            ),
                            ThuKhac AS (
                                SELECT ISNULL(SUM(SoTien), 0) as TongThuKhac
                                FROM KhoanThu
                                WHERE ThuYear = @year
                                    " + (thang.HasValue ? "AND ThuMonth = @month" : "") + @"
                            ),
                            ChiLuong AS (    
                                SELECT ISNULL(SUM(TongLuong), 0) as TongLuong 
                                FROM LuongGV     
                                WHERE TrangThai = N'Đã thanh toán' 
                                    AND SalaryYear = @year
                                    " + (thang.HasValue ? "AND SalaryMonth = @month" : "") + @"
                            ),
                            ChiKhac AS (    
                                SELECT ISNULL(SUM(SoTien), 0) as TongChiKhac     
                                FROM KhoanChi    
                                WHERE ChiYear = @year
                                    " + (thang.HasValue ? "AND ChiMonth = @month" : "") + @"
                            )
                            SELECT     
                                N'1. Thu học phí' AS DanhMuc, TongThu AS SoTien, N'Thu' AS Loai
                            FROM ThuHocPhi
                            WHERE TongThu > 0
                            UNION ALL
                            SELECT     
                                N'2. Thu khác' AS DanhMuc, TongThuKhac, N'Thu'
                            FROM ThuKhac
                            WHERE TongThuKhac > 0
                            UNION ALL
                            SELECT     
                                N'3. Chi trả lương' AS DanhMuc, TongLuong, N'Chi'
                            FROM ChiLuong
                            WHERE TongLuong > 0
                            UNION ALL
                            SELECT   
                                N'4. Chi khác' AS DanhMuc, TongChiKhac, N'Chi'
                            FROM ChiKhac
                            WHERE TongChiKhac > 0;";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@year", nam);
                    if (thang.HasValue)
                        cmd.Parameters.AddWithValue("@month", thang.Value);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dtBaoCao);
                    }
                }
            }
            return dtBaoCao;
        }


        // Nghiệp vụ lương giáo viên

        public List<string> GetTeacherIDs(string text)
        {
            List<string> ids = new List<string>();
            string query = "SELECT Id FROM accountList WHERE Role='Teacher' AND Id LIKE @text";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@text", "%" + text + "%");
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(reader["Id"].ToString());
                        }
                    }
                }
            }
            return ids;
        }

        public string GetTeacherName(string id)
        {
            string query = "SELECT name FROM accountList WHERE Id=@id";
            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : null;
                }
            }
        }

        public DataTable GetLuongByTeacherID(string id)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT LuongID, SalaryMonth, SalaryYear, SoLopDay, SoBuoiDay, LuongCoBan, Thuong, Phat, TongLuong, TrangThai, NgayThanhToan
                            FROM LuongGV 
                            WHERE TeacherID=@id 
                            ORDER BY SalaryYear DESC, SalaryMonth DESC";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlDataAdapter ad = new SqlDataAdapter(query, conn))
                {
                    ad.SelectCommand.Parameters.AddWithValue("@id", id);
                    ad.Fill(dt);
                }
            }
            return dt;
        }

        public bool CheckLuongExists(string id, int month, int year)
        {
            string query = "SELECT COUNT(1) FROM LuongGV WHERE TeacherID=@id AND SalaryMonth=@m AND SalaryYear=@y";
            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@m", month);
                    cmd.Parameters.AddWithValue("@y", year);
                    conn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public int GetClassCountByTeacher(string id, int month, int year)
        {
            DateTime startOfMonth = new DateTime(year, month, 1);

            DateTime startOfNextMonth = startOfMonth.AddMonths(1);
            string query = @"SELECT COUNT(*) FROM Class 
                     WHERE teacherID = @id 
                       AND start_date < @startOfNextMonth
                       AND end_date >= @startOfMonth";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@startOfNextMonth", startOfNextMonth);
                    cmd.Parameters.AddWithValue("@startOfMonth", startOfMonth);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public void CalculateAndInsertLuong(string id, int month, int year)
        {
            int soLop = GetClassCountByTeacher(id, month, year);
            int soBuoi = soLop * 8;
            decimal mucLuongMoiBuoi = 400000;
            decimal tongLuong = soBuoi * mucLuongMoiBuoi;

            InsertLuong(id, month, year, soLop, soBuoi, mucLuongMoiBuoi, tongLuong);
        }

        public void InsertLuong(string id, int month, int year, int soLop, int soBuoi, decimal mucLuongMoiBuoi, decimal tongLuong)
        {
            string query = @"INSERT INTO LuongGV (TeacherID, SalaryMonth, SalaryYear, SoLopDay, SoBuoiDay, LuongCoBan, Thuong, Phat, TongLuong, TrangThai)
                            VALUES (@id, @m, @y, @lop, @buoi, @muc, 0, 0, @tong, N'Chưa thanh toán')";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@m", month);
                    cmd.Parameters.AddWithValue("@y", year);
                    cmd.Parameters.AddWithValue("@lop", soLop);
                    cmd.Parameters.AddWithValue("@buoi", soBuoi);
                    cmd.Parameters.AddWithValue("@muc", mucLuongMoiBuoi);
                    cmd.Parameters.AddWithValue("@tong", tongLuong);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SaveLuongChanges(DataTable dt)
        {
            string query = @"UPDATE LuongGV SET 
                           TrangThai = @status, 
                           NgayThanhToan = (CASE WHEN @status = N'Đã thanh toán' THEN GETDATE() ELSE NULL END),
                           Thuong = @thuong, Phat = @phat,
                           TongLuong = (LuongCoBan * SoBuoiDay + @thuong - @phat)
                           WHERE LuongID = @luongID";

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@luongID", row["LuongID"]);
                                cmd.Parameters.AddWithValue("@status", row["TrangThai"] != null ? row["TrangThai"].ToString() : "Chưa thanh toán");
                                cmd.Parameters.AddWithValue("@thuong", row["Thuong"] == DBNull.Value ? 0 : row["Thuong"]);
                                cmd.Parameters.AddWithValue("@phat", row["Phat"] == DBNull.Value ? 0 : row["Phat"]);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception) { trans.Rollback(); throw; }
                }
            }
        }


        // Nghiệp vụ theo dõi học phí

        public DataTable GetDanhSachLopForTheoDoi()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT classID, class_name FROM Class ORDER BY class_name";
            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        public DataTable GetThongKeHocPhi(string classId)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    COUNT(DISTINCT jc.AccountID),
                    ISNULL(SUM(CASE WHEN hp.TrangThai = N'Đã đóng' THEN 1 ELSE 0 END), 0) AS DaNop
                FROM JoinClass jc
                LEFT JOIN HocPhi hp ON jc.AccountID = hp.AccountID AND jc.classID = hp.ClassID";

            if (classId != "ALL")
            {
                query += " WHERE jc.classID = @ClassID";
            }

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (classId != "ALL")
                    {
                        cmd.Parameters.AddWithValue("@ClassID", classId);
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public DataTable GetDanhSachHocVienNoHocPhi(string classId)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    jc.AccountID,
                    a.name AS HoTen, 
                    a.phone AS SoDienThoai, 
                    a.sex, 
                    c.class_name AS TenLop,
                    c.fee_default AS MucHocPhi
                FROM JoinClass jc
                INNER JOIN accountList a ON jc.AccountID = a.Id
                INNER JOIN Class c ON jc.classID = c.classID
                LEFT JOIN HocPhi hp ON jc.AccountID = hp.AccountID AND jc.classID = hp.ClassID
                WHERE (hp.TrangThai = N'Chưa đóng' OR hp.TrangThai IS NULL)";

            if (classId != "ALL")
            {
                query += " AND jc.classID = @ClassID";
            }

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (classId != "ALL")
                    {
                        cmd.Parameters.AddWithValue("@ClassID", classId);
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }


        // nghiệp vụ thiết lập học phí

        public string GetClassNameByID(string classID)
        {
            string query = "SELECT class_name FROM Class WHERE classID = @id";
            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", classID);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "";
                }
            }
        }

        public DataTable GetChiTietHocPhi(string MaLop, decimal _soTienMoiHS, int thang, int nam)
        {
            DataTable dt = new DataTable();
            string query = @"
            SELECT 
                ROW_NUMBER() OVER (ORDER BY a.name) AS STT,
                a.Id   AS AccountID, 
                a.name AS HoTen, 
                ISNULL(hp.SoTien, @tienMacDinh)    AS SoTien, 
                ISNULL(hp.TrangThai, N'Chưa đóng') AS TrangThai
            FROM JoinClass jc
            INNER JOIN accountList a ON jc.AccountID = a.Id
            LEFT JOIN HocPhi hp 
                ON jc.AccountID  = hp.AccountID 
                AND jc.classID   = hp.ClassID
                AND hp.TuitionMonth = @thang        
                AND hp.TuitionYear  = @nam
            WHERE jc.classID = @classID";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@classID", MaLop);
                    adapter.SelectCommand.Parameters.AddWithValue("@tienMacDinh", _soTienMoiHS);
                    adapter.SelectCommand.Parameters.AddWithValue("@thang", thang);
                    adapter.SelectCommand.Parameters.AddWithValue("@nam", nam);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        public int GetClassCapacity(string classID)
        {
            string query = "SELECT COUNT(*) FROM JoinClass WHERE classID = @id";
            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", classID);
                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public void SaveHocPhiSetup(DataTable dt, string classID, decimal soTienMoiHS, bool coNhapTien, int month, int year)
        {
            string query = @"
            IF EXISTS (SELECT 1 FROM HocPhi WHERE AccountID = @accID AND ClassID = @classID AND TuitionMonth = @month AND TuitionYear = @year)
            BEGIN
                UPDATE HocPhi SET SoTien = CASE WHEN @coNhapTien = 1 THEN @tien ELSE SoTien END, TrangThai = @status, NgayDong = CASE WHEN @status = N'Đã đóng' THEN GETDATE() ELSE NULL END
                WHERE AccountID = @accID AND ClassID = @classID AND TuitionMonth = @month AND TuitionYear = @year
            END
            ELSE
            BEGIN
                INSERT INTO HocPhi (AccountID, ClassID, TuitionMonth, TuitionYear, SoTien, TrangThai, NgayDong)
                VALUES (@accID, @classID, @month, @year, CASE WHEN @coNhapTien = 1 THEN @tien ELSE 0 END, @status, CASE WHEN @status = N'Đã đóng' THEN GETDATE() ELSE NULL END)
            END";

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row.RowState == DataRowState.Deleted) continue;
                            string trangThai = row["TrangThai"]?.ToString() ?? "Chưa đóng";
                            using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@accID", row["AccountID"]);
                                cmd.Parameters.AddWithValue("@classID", classID);
                                cmd.Parameters.AddWithValue("@tien", soTienMoiHS);
                                cmd.Parameters.AddWithValue("@coNhapTien", coNhapTien ? 1 : 0);
                                cmd.Parameters.AddWithValue("@status", trangThai);
                                cmd.Parameters.AddWithValue("@month", month);
                                cmd.Parameters.AddWithValue("@year", year);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception) { trans.Rollback(); throw; }
                }
            }
        }


        // Nghiệp vụ thống kê

        public void FetchThongKeTaiChinhData(DataTable dt, ref decimal tongThu, ref decimal tongChi, int nam, int? thang)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string sqlThu = @"
                    SELECT ThuID AS [Mã], FORMAT(NgayThu, 'dd/MM/yyyy') AS [Ngày], N'Thu' AS [Loại], NoiDung AS [Nội dung], SoTien AS [Số tiền gốc], ISNULL(GhiChu, '') AS [Ghi chú]
                    FROM KhoanThu WHERE ThuYear = @Nam" + (thang.HasValue ? " AND ThuMonth = @Thang" : "");

                using (SqlCommand cmd = new SqlCommand(sqlThu, conn))
                {
                    cmd.Parameters.AddWithValue("@Nam", nam);
                    if (thang.HasValue) cmd.Parameters.AddWithValue("@Thang", thang.Value);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            DataRow row = dt.NewRow();
                            row["Mã"] = "KT_" + r["Mã"].ToString(); row["Ngày"] = r["Ngày"].ToString(); row["Loại"] = r["Loại"].ToString(); row["Nội dung"] = r["Nội dung"].ToString();
                            decimal tien = Convert.ToDecimal(r["Số tiền gốc"]); row["Số tiền"] = tien.ToString("N0") + " đ"; row["Ghi chú"] = r["Ghi chú"].ToString(); row["SoTienGoc"] = tien;
                            dt.Rows.Add(row);
                            tongThu += tien;
                        }
                    }
                }

                string sqlHocPhi = @"
                    SELECT hp.HocPhiID AS [Mã], FORMAT(hp.NgayDong, 'dd/MM/yyyy') AS [Ngày], N'Thu học phí' AS [Loại], a.name + N' - ' + c.class_name + N' (T' + CAST(hp.TuitionMonth AS VARCHAR) + N'/' + CAST(hp.TuitionYear AS VARCHAR) + N')' AS [Nội dung], hp.SoTien AS [Số tiền gốc], ISNULL(hp.GhiChu, '') AS [Ghi chú]
                    FROM HocPhi hp JOIN accountList a ON hp.AccountID = a.Id JOIN Class c ON hp.ClassID = c.classID
                    WHERE hp.TrangThai = N'Đã đóng' AND hp.NgayDong IS NOT NULL AND YEAR(hp.NgayDong) = @Nam" + (thang.HasValue ? " AND MONTH(hp.NgayDong) = @Thang" : "");

                using (SqlCommand cmd = new SqlCommand(sqlHocPhi, conn))
                {
                    cmd.Parameters.AddWithValue("@Nam", nam);
                    if (thang.HasValue) cmd.Parameters.AddWithValue("@Thang", thang.Value);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            DataRow row = dt.NewRow();
                            row["Mã"] = "HP_" + r["Mã"].ToString(); row["Ngày"] = r["Ngày"].ToString(); row["Loại"] = r["Loại"].ToString(); row["Nội dung"] = r["Nội dung"].ToString();
                            decimal tien = Convert.ToDecimal(r["Số tiền gốc"]); row["Số tiền"] = tien.ToString("N0") + " đ"; row["Ghi chú"] = r["Ghi chú"].ToString(); row["SoTienGoc"] = tien;
                            dt.Rows.Add(row);
                            tongThu += tien;
                        }
                    }
                }

                string sqlChi = @"
                    SELECT ChiID AS [Mã], FORMAT(NgayChi, 'dd/MM/yyyy') AS [Ngày], N'Chi' AS [Loại], NoiDung AS [Nội dung], SoTien AS [Số tiền gốc], ISNULL(GhiChu, '') AS [Ghi chú]
                    FROM KhoanChi WHERE ChiYear = @Nam" + (thang.HasValue ? " AND ChiMonth = @Thang" : "");

                using (SqlCommand cmd = new SqlCommand(sqlChi, conn))
                {
                    cmd.Parameters.AddWithValue("@Nam", nam);
                    if (thang.HasValue) cmd.Parameters.AddWithValue("@Thang", thang.Value);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            DataRow row = dt.NewRow();
                            row["Mã"] = "KC_" + r["Mã"].ToString(); row["Ngày"] = r["Ngày"].ToString(); row["Loại"] = r["Loại"].ToString(); row["Nội dung"] = r["Nội dung"].ToString();
                            decimal tien = Convert.ToDecimal(r["Số tiền gốc"]); row["Số tiền"] = tien.ToString("N0") + " đ"; row["Ghi chú"] = r["Ghi chú"].ToString(); row["SoTienGoc"] = tien;
                            dt.Rows.Add(row);
                            tongChi += tien;
                        }
                    }
                }

                string sqlLuongGV = @"
                    SELECT lg.LuongID AS [Mã], FORMAT(lg.NgayThanhToan, 'dd/MM/yyyy') AS [Ngày], N'Chi lương' AS [Loại], a.name + N' - ' + ti.subject + N' (T' + CAST(lg.SalaryMonth AS VARCHAR) + N'/' + CAST(lg.SalaryYear AS VARCHAR) + N')' AS [Nội dung], lg.TongLuong AS [Số tiền gốc], ISNULL(lg.GhiChu, '') AS [Ghi chú]
                    FROM LuongGV lg JOIN accountList a ON lg.TeacherID = a.Id JOIN teacherInfo ti ON lg.TeacherID = ti.Id
                    WHERE lg.TrangThai = N'Đã thanh toán' AND lg.NgayThanhToan IS NOT NULL AND YEAR(lg.NgayThanhToan) = @Nam" + (thang.HasValue ? " AND MONTH(lg.NgayThanhToan) = @Thang" : "");

                using (SqlCommand cmd = new SqlCommand(sqlLuongGV, conn))
                {
                    cmd.Parameters.AddWithValue("@Nam", nam);
                    if (thang.HasValue) cmd.Parameters.AddWithValue("@Thang", thang.Value);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            DataRow row = dt.NewRow();
                            row["Mã"] = "LG_" + r["Mã"].ToString(); row["Ngày"] = r["Ngày"].ToString(); row["Loại"] = r["Loại"].ToString(); row["Nội dung"] = r["Nội dung"].ToString();
                            decimal tien = Convert.ToDecimal(r["Số tiền gốc"]); row["Số tiền"] = tien.ToString("N0") + " đ"; row["Ghi chú"] = r["Ghi chú"].ToString(); row["SoTienGoc"] = tien;
                            dt.Rows.Add(row);
                            tongChi += tien;
                        }
                    }
                }
            }
        }


        // Nghiệp vụ tổng quan

        public DataTable GetHocSinhChamHocPhi()
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT a.Id AS [Mã HS], a.name AS [Họ tên], a.phone AS [Số điện thoại], c.class_name AS [Lớp], hp.TuitionMonth AS [Tháng], hp.TuitionYear AS [Năm], FORMAT(hp.SoTien, 'N0') + N' đ' AS [Số tiền], hp.TrangThai AS [Trạng thái]
                FROM HocPhi hp JOIN accountList a ON hp.AccountID = a.Id JOIN Class c ON hp.ClassID = c.classID
                WHERE hp.TrangThai = N'Chưa đóng' ORDER BY hp.TuitionYear DESC, hp.TuitionMonth DESC, a.name";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd)) { da.Fill(dt); }
                }
            }
            return dt;
        }

        public DataTable GetGiaoVienChuaTraLuong()
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT a.Id AS [Mã GV], a.name AS [Họ tên], a.phone AS [Số điện thoại], ti.subject AS [Môn dạy], lg.SalaryMonth AS [Tháng], lg.SalaryYear AS [Năm], FORMAT(lg.TongLuong, 'N0') + N' đ' AS [Tổng lương], lg.TrangThai AS [Trạng thái]
                FROM LuongGV lg JOIN accountList a ON lg.TeacherID = a.Id JOIN teacherInfo ti ON lg.TeacherID = ti.Id
                WHERE lg.TrangThai = N'Chưa thanh toán' ORDER BY lg.SalaryYear DESC, lg.SalaryMonth DESC, a.name";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd)) { da.Fill(dt); }
                }
            }
            return dt;
        }
    }
}