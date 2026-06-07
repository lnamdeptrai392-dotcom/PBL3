using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;

namespace PBL3a.services.BLL
{
    public class AdminC_Service
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();

        // 1. Tải danh sách đơn chờ duyệt
        public DataTable GetPendingRegistrations()
        {
            string query = @"
                SELECT 
                    r.AccountID AS [Mã HS],
                    a.name AS [Tên Học Sinh],
                    r.ClassID AS [Mã Lớp],
                    c.class_name AS [Tên Lớp],
                    CONVERT(VARCHAR(10), r.RegistrationDate, 120) AS [Ngày Gửi],
                    r.Note AS [Ghi Chú]
                FROM Registration r
                INNER JOIN accountList a ON r.AccountID = a.Id
                INNER JOIN Class c ON r.ClassID = c.classID
                WHERE r.Status = N'Chờ duyệt'";

            DataTable dt = new DataTable();
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.Fill(dt);
            }
            return dt;
        }

        // 2. Tải thông tin học sinh
        public DataTable GetStudentInfo(string accountId)
        {
            string query = @"
                SELECT 
                    Id AS [Mã HS], 
                    name AS [Họ và Tên], 
                    CONVERT(VARCHAR(10), dateOfBirth, 120) AS [Ngày Sinh], 
                    sex AS [Giới Tính], 
                    phone AS [Số Điện Thoại]
                FROM accountList 
                WHERE Id = @Id";

            DataTable dt = new DataTable();
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", accountId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable GetClassesByDayOfWeek(int dayOfWeek)
        {
            string query = @"
                SELECT DISTINCT c.classID, c.class_name 
                FROM Class c
                JOIN ClassSchedule cs ON c.classID = cs.classID
                WHERE cs.dayOfWeek = @dayOfWeek AND c.status = N'Đang mở'";

            DataTable dt = new DataTable();
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@dayOfWeek", dayOfWeek);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // 2. Lấy danh sách học sinh để điểm danh
        public DataTable getAttendanceInfo(string classID, DateTime date)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    s.Id AS StudentID, 
                    s.name AS StudentName, 
                    ISNULL(a.Status, N'Có mặt') AS TrangThai, 
                    ISNULL(a.Note, '') AS Note
                FROM accountList s
                INNER JOIN JoinClass jc ON s.Id = jc.AccountID
                LEFT JOIN Attendance a ON s.Id = a.AccountID 
                    AND a.ClassID = jc.classID 
                    AND a.AttendanceDate = @date
                WHERE jc.classID = @classID AND s.Role = 'Student'";

            try
            {
                using (SqlConnection conn = dbHelper.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@classID", classID);
                    cmd.Parameters.AddWithValue("@date", date);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    if (dt.Columns.Contains("TrangThai")) dt.Columns["TrangThai"].ReadOnly = false;
                    if (dt.Columns.Contains("Note")) dt.Columns["Note"].ReadOnly = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load danh sách điểm danh: " + ex.Message);
            }

            return dt;
        }

        // 3. Lưu điểm danh
        public bool SaveAttendance(string classID, DateTime attendanceDate, DataTable dtAttendance)
        {
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (DataRow row in dtAttendance.Rows)
                        {
                            string studentID = row["StudentID"].ToString();
                            string status = row["TrangThai"]?.ToString() ?? "Có mặt";
                            string note = row["Note"]?.ToString() ?? "";

                            string checkQuery = @"
                                SELECT COUNT(*) 
                                FROM Attendance 
                                WHERE AccountID = @acc AND ClassID = @class AND AttendanceDate = @date";

                            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, trans))
                            {
                                checkCmd.Parameters.AddWithValue("@acc", studentID);
                                checkCmd.Parameters.AddWithValue("@class", classID);
                                checkCmd.Parameters.AddWithValue("@date", attendanceDate);

                                int count = (int)checkCmd.ExecuteScalar();

                                if (count > 0)
                                {
                                    string updateQuery = @"
                                        UPDATE Attendance 
                                        SET Status = @status, Note = @note
                                        WHERE AccountID = @acc AND ClassID = @class AND AttendanceDate = @date";

                                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn, trans))
                                    {
                                        updateCmd.Parameters.AddWithValue("@status", status);
                                        updateCmd.Parameters.AddWithValue("@note", note);
                                        updateCmd.Parameters.AddWithValue("@acc", studentID);
                                        updateCmd.Parameters.AddWithValue("@class", classID);
                                        updateCmd.Parameters.AddWithValue("@date", attendanceDate);
                                        updateCmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    string insertQuery = @"
                                        INSERT INTO Attendance (AccountID, ClassID, AttendanceDate, Status, Note)
                                        VALUES (@acc, @class, @date, @status, @note)";

                                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn, trans))
                                    {
                                        insertCmd.Parameters.AddWithValue("@acc", studentID);
                                        insertCmd.Parameters.AddWithValue("@class", classID);
                                        insertCmd.Parameters.AddWithValue("@date", attendanceDate);
                                        insertCmd.Parameters.AddWithValue("@status", status);
                                        insertCmd.Parameters.AddWithValue("@note", note);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        trans.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Lỗi lưu điểm danh: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        public DataTable GetActiveClassNow()
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    c.classID,
                    cs.dayOfWeek,
                    c.class_name, 
                    CASE cs.dayOfWeek
                        WHEN 1 THEN N'Thứ 2' WHEN 2 THEN N'Thứ 3'
                        WHEN 3 THEN N'Thứ 4' WHEN 4 THEN N'Thứ 5'
                        WHEN 5 THEN N'Thứ 6' WHEN 6 THEN N'Thứ 7'
                        WHEN 7 THEN N'Chủ nhật'
                    END AS [NgayHoc],
                    CONVERT(VARCHAR(5), cs.startTime, 108) AS [GioBatDau],
                    CONVERT(VARCHAR(5), cs.endTime, 108) AS [GioKetThuc]
                FROM ClassSchedule cs
                JOIN Class c ON cs.classID = c.classID
                WHERE c.status = N'Đang mở'";
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.Fill(dt);
            }
            return dt;
        }

        public DataRow GetClassDuration(string classID)
        {
            string query = @"SELECT 
                start_date AS [startDate], 
                end_date AS [endDate]
                FROM Class WHERE classID = @id";
            DataTable dt = new DataTable();
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", classID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        // Tải thông tin lớp học
        public DataTable GetClassInfo(string classId)
        {
            string query = @"
                SELECT 
                    c.classID AS [Mã Lớp],
                    c.class_name AS [Tên Lớp],
                    t.name AS [GV Chủ Nhiệm],
                    c.capacity AS [Sức Chứa],
                    (SELECT COUNT(*) FROM JoinClass jc WHERE jc.classID = c.classID) AS [Sĩ Số Hiện Tại]
                FROM Class c
                INNER JOIN accountList t ON c.teacherID = t.Id
                WHERE c.classID = @classID";

            DataTable dt = new DataTable();
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@classID", classId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Xử lý duyệt đơn
        public void ApproveRegistration(string accountId, string classId)
        {
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateQuery = "UPDATE Registration SET Status = N'Đã duyệt' WHERE AccountID = @accountID AND ClassID = @classID";
                        using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn, transaction))
                        {
                            cmdUpdate.Parameters.AddWithValue("@accountID", accountId);
                            cmdUpdate.Parameters.AddWithValue("@classID", classId);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        string insertQuery = "INSERT INTO JoinClass (AccountID, classID) VALUES (@accountID, @classID)";
                        using (SqlCommand cmdInsert = new SqlCommand(insertQuery, conn, transaction))
                        {
                            cmdInsert.Parameters.AddWithValue("@accountID", accountId);
                            cmdInsert.Parameters.AddWithValue("@classID", classId);
                            cmdInsert.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (SqlException sqlEx)
                    {
                        transaction.Rollback();
                        if (sqlEx.Number == 2627) // Lỗi trùng Primary Key
                        {
                            throw new Exception("Học sinh này đã tồn tại trong lớp rồi!");
                        }
                        throw new Exception("Lỗi CSDL: " + sqlEx.Message);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Đã xảy ra lỗi: " + ex.Message);
                    }
                }
            }
        }

        // Lấy dữ liệu đơn đăng ký đã được LỌC
        public DataTable FilterRegistrations(string monHoc, string khoi, string classId)
        {
            string query = @"
                SELECT
                    r.AccountID AS [Mã HS],
                    a.name AS [Tên Học Sinh],
                    r.ClassID AS [Mã Lớp],
                    c.class_name AS [Tên Lớp],
                    CONVERT(VARCHAR(10), r.RegistrationDate, 120) AS [Ngày Gửi],
                    r.Note AS [Ghi Chú]
                FROM Registration r
                INNER JOIN accountList a ON r.AccountID = a.Id
                INNER JOIN Class c ON r.ClassID = c.classID
                WHERE r.Status = N'Chờ duyệt'";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(monHoc) && monHoc != "Tất cả")
            {
                string keyword = monHoc;
                if (monHoc == "Toán học") keyword = "Toán";
                else if (monHoc == "Vật lý") keyword = "Lý";
                else if (monHoc == "Hóa học") keyword = "Hóa";
                else if (monHoc == "Sinh học") keyword = "Sinh";
                else if (monHoc == "Ngữ văn") keyword = "Văn";

                query += " AND c.class_name LIKE '%' + @monHoc + '%'";
                parameters.Add(new SqlParameter("@monHoc", keyword));
            }

            if (!string.IsNullOrEmpty(khoi) && khoi != "Tất cả")
            {
                string khoiKeyword = khoi.Replace("Khối ", "");
                query += " AND c.class_name LIKE '% ' + @khoi + ' %'";
                parameters.Add(new SqlParameter("@khoi", khoiKeyword));
            }

            if (!string.IsNullOrEmpty(classId) && classId != "Tất cả")
            {
                query += " AND c.classID = @classId";
                parameters.Add(new SqlParameter("@classId", classId));
            }

            DataTable dt = new DataTable();
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        // Xử lý từ chối đơn
        public void RejectRegistration(string accountId, string classId)
        {
            string updateQuery = "UPDATE Registration SET Status = N'Từ chối' WHERE AccountID = @accountID AND ClassID = @classID";
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@accountID", accountId);
                    cmd.Parameters.AddWithValue("@classID", classId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Lấy danh sách Khối và Lớp đang có học sinh xin vào
        public DataTable GetActiveClasses()
        {
            DataTable dtClasses = new DataTable();
            string query = @"
                SELECT DISTINCT c.classID, c.class_name
                FROM Registration r
                INNER JOIN Class c ON r.ClassID = c.classID
                WHERE r.Status = N'Chờ duyệt'";

            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.Fill(dtClasses);
            }
            return dtClasses;
        }

        public DataTable GetSubjects()
        {
            DataTable dt = new DataTable();
            string query = "SELECT DISTINCT subject FROM teacherInfo";
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.Fill(dt);
            }
            return dt;
        }

        // Lấy danh sách Khối đang có trong Class
        public DataTable GetBlocksBySubject(string subject)
        {
            DataTable dt = new DataTable();

            string keyword = subject;
            if (subject == "Toán học") keyword = "Toán";
            else if (subject == "Vật lý") keyword = "Lý";
            else if (subject == "Hóa học") keyword = "Hóa";
            else if (subject == "Sinh học") keyword = "Sinh";
            else if (subject == "Ngữ văn") keyword = "Văn";

            string query = @"
                SELECT DISTINCT 
                    CAST(c.grade AS NVARCHAR) AS Khoi 
                FROM Class c
                INNER JOIN teacherInfo ti ON c.teacherID = ti.Id
                WHERE ti.subject = @subject AND c.class_name LIKE '%' + @keyword + '%'";

            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@subject", subject);
                cmd.Parameters.AddWithValue("@keyword", keyword);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Lọc danh sách Lớp
        public DataTable GetClassesByFilter(string subject, string khoi, string status)
        {
            DataTable dt = new DataTable();

            string keyword = subject;
            if (subject == "Toán học") keyword = "Toán";
            else if (subject == "Vật lý") keyword = "Lý";
            else if (subject == "Hóa học") keyword = "Hóa";
            else if (subject == "Sinh học") keyword = "Sinh";
            else if (subject == "Ngữ văn") keyword = "Văn";

            string query = @"
                SELECT 
                    c.classID AS [Mã Lớp], 
                    c.class_name AS [Tên Lớp], 
                    CONVERT(VARCHAR(10), c.start_date, 120) AS [Ngày Bắt Đầu], 
                    CONVERT(VARCHAR(10), c.end_date, 120) AS [Ngày Kết Thúc], 
                    c.capacity AS [Sức Chứa],
                    CASE 
                        WHEN c.start_date > GETDATE() THEN N'Sắp mở'
                        WHEN c.start_date <= GETDATE() AND c.end_date >= GETDATE() THEN N'Đang học'
                        ELSE N'Đã kết thúc'
                    END AS [Tình Trạng]
                FROM Class c
                INNER JOIN teacherInfo ti ON c.teacherID = ti.Id
                WHERE ti.subject = @subject 
                    AND c.class_name LIKE '%' + @khoi + '%'
                    AND c.class_name LIKE '%' + @keyword + '%'
                    AND (
                        (@status = N'Sắp mở' AND c.start_date > GETDATE()) OR
                        (@status = N'Đang học' AND c.start_date <= GETDATE() AND c.end_date >= GETDATE()) OR
                        (@status = N'Đã kết thúc' AND c.end_date < GETDATE())
                    )";

            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@subject", subject);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@khoi", khoi);
                cmd.Parameters.AddWithValue("@keyword", keyword);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Lấy thông tin GVCN của một lớp
        public DataTable GetTeacherByClass(string classId)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    a.Id AS [Mã GV], 
                    a.name AS [Tên GV], 
                    a.phone AS [SĐT], 
                    a.sex AS [Giới Tính]
                FROM accountList a
                INNER JOIN Class c ON a.Id = c.teacherID
                WHERE c.classID = @classId";

            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@classId", classId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Lấy danh sách Học sinh của một lớp
        public DataTable GetStudentsByClass(string classId)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    a.Id AS [Mã HS], 
                    a.name AS [Tên Học Sinh], 
                    CONVERT(VARCHAR(10), a.dateOfBirth, 120) AS [Ngày Sinh], 
                    a.sex AS [Giới Tính], 
                    a.phone AS [SĐT]
                FROM accountList a
                INNER JOIN JoinClass jc ON a.Id = jc.AccountID
                WHERE jc.classID = @classId AND a.Role = 'Student'";

            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@classId", classId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Lấy thông tin học sinh
        public DataTable GetStudentClassHistory(string accountId)
        {
            string query = @"
                SELECT 
                    c.classID AS [Mã Lớp], 
                    c.class_name AS [Tên Lớp], 
                    CASE 
                        WHEN c.start_date > GETDATE() THEN N'Sắp mở'
                        WHEN c.start_date <= GETDATE() AND c.end_date >= GETDATE() THEN N'Đang học'
                        ELSE N'Đã kết thúc'
                    END AS [Trạng Thái]
                FROM Class c
                INNER JOIN JoinClass jc ON c.classID = jc.classID
                WHERE jc.accountID = @accountId";

            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@accountId", accountId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Search TT
        public DataTable SearchClasses(string searchType, string keyword)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT DISTINCT
                    c.classID AS [Mã Lớp], 
                    c.class_name AS [Tên Lớp], 
                    CONVERT(VARCHAR(10), c.start_date, 120) AS [Ngày Bắt Đầu], 
                    CONVERT(VARCHAR(10), c.end_date, 120) AS [Ngày Kết Thúc], 
                    c.capacity AS [Sức Chứa],
                    CASE 
                        WHEN c.start_date > GETDATE() THEN N'Sắp mở'
                        WHEN c.start_date <= GETDATE() AND c.end_date >= GETDATE() THEN N'Đang học'
                        ELSE N'Đã kết thúc'
                    END AS [Tình Trạng]
                FROM Class c ";

            // Dựa vào tiêu chí để JOIN thêm bảng
            if (searchType == "Mã Học Sinh" || searchType == "Tên Học Sinh")
            {
                query += " INNER JOIN JoinClass jc ON c.classID = jc.classID ";
                query += " INNER JOIN accountList a ON jc.AccountID = a.Id ";
            }
            else if (searchType == "Tên Giáo Viên")
            {
                query += " INNER JOIN accountList a ON c.teacherID = a.Id ";
            }

            query += " WHERE 1=1 ";

            // điều kiện tìm kiếm
            if (searchType == "Mã Lớp Học")
                query += " AND c.classID = @keyword";
            else if (searchType == "Tên Lớp Học")
                query += " AND c.class_name LIKE '%' + @keyword + '%'";
            else if (searchType == "Mã Giáo Viên")
                query += " AND c.teacherID = @keyword";
            else if (searchType == "Tên Giáo Viên")
                query += " AND a.name LIKE '%' + @keyword + '%' AND a.Role = 'Teacher'";
            else if (searchType == "Mã Học Sinh")
                query += " AND a.Id = @keyword AND a.Role = 'Student'";
            else if (searchType == "Tên Học Sinh")
                query += " AND a.name LIKE '%' + @keyword + '%' AND a.Role = 'Student'";

            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@keyword", keyword.Trim());
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Lấy danh sách giáo viên theo môn học
        public DataTable GetTeachersBySubjectForm2(string subject)
        {
            string dbSubject = subject;
            if (subject == "Toán Học") dbSubject = "Toán";
            else if (subject == "Văn Học") dbSubject = "Văn";

            string query = @"
                SELECT a.Id AS [Mã GV], a.name AS [Tên Giáo Viên], a.phone AS [SĐT]
                FROM accountList a
                INNER JOIN teacherInfo t ON a.Id = t.Id
                WHERE a.Role = 'Teacher' AND t.subject = @subject AND a.status = N'Hoạt động'";

            DataTable dt = new DataTable();
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@subject", dbSubject);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Lấy thông tin chi tiết giáo viên và các lớp đang/sắp dạy
        public DataTable GetTeacherDetailsAndClasses(string teacherId)
        {
            string query = @"
                SELECT 
                    a.Id AS [Mã GV], 
                    a.name AS [Họ Tên], 
                    CONVERT(VARCHAR(10), a.dateOfBirth, 120) AS [Ngày Sinh], 
                    a.phone AS [SĐT],
                    ISNULL(c.classID, '') AS [Mã Lớp],
                    ISNULL(c.class_name, '') AS [Tên Lớp],
                    CONVERT(VARCHAR(10), c.start_date, 120) AS [Ngày Bắt Đầu],
                    CONVERT(VARCHAR(10), c.end_date, 120) AS [Ngày Kết Thúc],
                    CASE 
                        WHEN c.start_date > GETDATE() THEN N'Sắp mở'
                        WHEN c.start_date <= GETDATE() AND c.end_date >= GETDATE() THEN N'Đang dạy'
                        ELSE N'Đã kết thúc'
                    END AS [Trạng Thái]
                FROM accountList a
                LEFT JOIN Class c ON a.Id = c.teacherID AND c.end_date >= GETDATE() -- Chỉ lấy lớp đang và sắp dạy
                WHERE a.Id = @teacherId";

            DataTable dt = new DataTable();
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@teacherId", teacherId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        private string GetSubjectCode(string subjectName)
        {
            if (subjectName.Contains("Toán")) return "MAT";
            if (subjectName.Contains("Văn")) return "LIT";
            if (subjectName.Contains("Hóa")) return "CHE";
            if (subjectName.Contains("Lý")) return "PHY";
            if (subjectName.Contains("Sinh")) return "BIO";
            if (subjectName.Contains("Anh")) return "ENG";
            return "SUB";
        }

        public void GenerateClassIdentifiers(string subject, string khoi, DateTime startDate, out string courseId, out string classId, out string className)
        {
            string subjectCode = GetSubjectCode(subject);
            string gradeNumber = khoi.Replace("Khối ", "").Trim();

            courseId = subjectCode;

            string prefix = $"{subjectCode}{gradeNumber}.";

            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Class WHERE classID LIKE @prefix + '%'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@prefix", prefix);
                int count = (int)cmd.ExecuteScalar();

                int nextIndex = count + 1;
                string suffix = nextIndex.ToString("D2"); // 01, 02...

                classId = $"{prefix}{suffix}"; // MAT10.01
                className = $"{subject.Replace(" Học", "")} {gradeNumber} - Nhóm {suffix}"; // Toán 10 - Nhóm 01
            }
        }

        // Lưu danh sách lớp vào CSDL
        public void SaveNewClasses(DataTable dtClasses)
        {
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string insertClass = @"INSERT INTO Class (classID, class_name, courseID, teacherID, start_date, end_date, capacity, grade, fee_default, status) 
                                       VALUES (@classID, @class_name, @courseID, @teacherID, @start_date, @end_date, @capacity, @grade, @fee, @status)";

                        foreach (DataRow row in dtClasses.Rows)
                        {
                            using (SqlCommand cmd = new SqlCommand(insertClass, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@classID", row["Mã Lớp"]);
                                cmd.Parameters.AddWithValue("@class_name", row["Tên Lớp"]);
                                cmd.Parameters.AddWithValue("@courseID", row["Mã Khóa Học"]);
                                cmd.Parameters.AddWithValue("@teacherID", row["Mã GV"]);
                                cmd.Parameters.AddWithValue("@start_date", row["Ngày Bắt Đầu"]);
                                cmd.Parameters.AddWithValue("@end_date", row["Ngày Kết Thúc"]);
                                cmd.Parameters.AddWithValue("@capacity", row["Sức Chứa"]);
                                cmd.Parameters.AddWithValue("@grade", row["Khối"]);
                                cmd.Parameters.AddWithValue("@fee", row["Học Phí"]);
                                cmd.Parameters.AddWithValue("@status", "Sắp mở");

                                cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new Exception("Lỗi khi lưu vào Database: " + ex.Message);
                    }
                }
            }
        }

        // Lấy danh sách học sinh theo độ tuổi trong cbb
        public DataTable GetStudentBirthYears()
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT DISTINCT YEAR(dateOfBirth) AS namSinh
                FROM accountList 
                WHERE Role = 'Student' AND dateOfBirth IS NOT NULL
                ORDER BY NamSinh DESC";

            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.Fill(dt);
            }
            return dt;
        }

        // Lấy học sinh theo Năm Sinh được truyền vào và CHƯA CÓ trong lớp
        public DataTable GetAvailableStudentsForClassByYear(string classId, int namSinh)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    a.Id AS [Mã Học Sinh], 
                    a.name AS [Tên Học Sinh], 
                    CONVERT(VARCHAR(10), a.dateOfBirth, 120) AS [Ngày Sinh], 
                    a.sex AS [Giới Tính], 
                    a.phone AS [SĐT]
                FROM accountList a
                WHERE a.Role = 'Student' 
                    AND YEAR(a.dateOfBirth) = @namSinh
                    AND NOT EXISTS (
                        SELECT 1 
                        FROM JoinClass jc
                        WHERE jc.AccountID = a.Id AND jc.classID = @classId
                    )";

            using (SqlConnection conn = dbHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@classId", classId);
                cmd.Parameters.AddWithValue("@namSinh", namSinh);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Thêm trực tiếp một học sinh vào lớp (Không qua form đăng ký)
        public void AddStudentToClass(string studentId, string classId)
        {
            string query = "INSERT INTO JoinClass (AccountID, classID) VALUES (@accountId, @classId)";

            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@accountId", studentId);
                        cmd.Parameters.AddWithValue("@classId", classId);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 2627)
                    {
                        throw new Exception($"Học sinh {studentId} đã tồn tại trong lớp {classId}!");
                    }
                    throw new Exception("Lỗi CSDL: " + sqlEx.Message);
                }
            }
        }
    }
}