using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;

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
                    r.RegistrationDate AS [Ngày Gửi],
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
                    dateOfBirth AS [Ngày Sinh], 
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

        // 3. Tải thông tin lớp học
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

        // 4. Xử lý duyệt đơn
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

        // 5. Lấy dữ liệu đơn đăng ký đã được LỌC
        public DataTable FilterRegistrations(string monHoc, string khoi, string classId)
        {
            string query = @"
        SELECT
            r.AccountID AS [Mã HS],
            a.name AS [Tên Học Sinh],
            r.ClassID AS [Mã Lớp],
            c.class_name AS [Tên Lớp],
            r.RegistrationDate AS [Ngày Gửi],
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

        // 6. Xử lý từ chối đơn
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

        // 7. Lấy danh sách Khối và Lớp đang có học sinh xin vào
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

        // 8. Lấy danh sách Khối đang có trong Class
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
    CASE 
        WHEN c.courseID LIKE 'NN01%' THEN SUBSTRING(c.courseID, 6, 2) 
        WHEN c.courseID LIKE 'NN02%' THEN SUBSTRING(c.courseID, 6, 2) 
        WHEN c.courseID LIKE 'NN03%' THEN REPLACE(SUBSTRING(c.courseID, 6, 2), 'H', 'HSK') 
        ELSE SUBSTRING(c.courseID, 2, 2) 
    END AS Khoi 
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


        // 9. Lọc danh sách Lớp
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
            c.start_date AS [Ngày Bắt Đầu], 
            c.end_date AS [Ngày Kết Thúc], 
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
                cmd.Parameters.AddWithValue("@status", status); // "Sắp mở", "Đang học" hoặc "Đã kết thúc"
                cmd.Parameters.AddWithValue("@khoi", khoi);
                cmd.Parameters.AddWithValue("@keyword", keyword);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // 10. Lấy thông tin GVCN của một lớp
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

        // 11. Lấy danh sách Học sinh của một lớp
        public DataTable GetStudentsByClass(string classId)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    a.Id AS [Mã HS], 
                    a.name AS [Tên Học Sinh], 
                    a.dateOfBirth AS [Ngày Sinh], 
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

        // 12. Lấy thông tin học sinh
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

        // 13. Search TT
        public DataTable SearchClasses(string searchType, string keyword)
        {
            DataTable dt = new DataTable();

            // Khung truy vấn cơ bản (giống hệt cách 1 để dgvData_CellClick vẫn chạy đúng)
            string query = @"
        SELECT DISTINCT
            c.classID AS [Mã Lớp], 
            c.class_name AS [Tên Lớp], 
            c.start_date AS [Ngày Bắt Đầu], 
            c.end_date AS [Ngày Kết Thúc], 
            c.capacity AS [Sức Chứa],
            CASE 
                WHEN c.start_date > GETDATE() THEN N'Sắp mở'
                WHEN c.start_date <= GETDATE() AND c.end_date >= GETDATE() THEN N'Đang học'
                ELSE N'Đã kết thúc'
            END AS [Tình Trạng]
        FROM Class c ";

            // Dựa vào tiêu chí để JOIN thêm bảng nếu cần thiết
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

            // Phân loại điều kiện tìm kiếm
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
            // Chuyển đổi tên môn học (nếu cần) để khớp với DB
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
            a.dateOfBirth AS [Ngày Sinh], 
            a.phone AS [SĐT],
            ISNULL(c.classID, '') AS [Mã Lớp],
            ISNULL(c.class_name, '') AS [Tên Lớp],
            c.start_date AS [Ngày Bắt Đầu],
            c.end_date AS [Ngày Kết Thúc],
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

        // Sinh tự động Mã Lớp (classID), Mã Khóa Học (courseID) và Tên Lớp (class_name)
        public void GenerateClassIdentifiers(string subject, string khoi, DateTime startDate, out string courseId, out string classId, out string className)
        {
            string subjectCode = "";
            string subjectName = "";

            // Mapping môn học sang mã ký tự (dựa trên STARTDB.sql)
            if (subject.Contains("Toán")) { subjectCode = "T"; subjectName = "Toán"; }
            else if (subject.Contains("Văn")) { subjectCode = "V"; subjectName = "Văn"; }
            else if (subject.Contains("Hóa")) { subjectCode = "H"; subjectName = "Hóa"; }
            else if (subject.Contains("Lý")) { subjectCode = "L"; subjectName = "Lý"; }
            else if (subject.Contains("Sinh")) { subjectCode = "S"; subjectName = "Sinh"; }
            else if (subject.Contains("Tiếng Anh")) { subjectCode = "NN01."; subjectName = "Tiếng Anh"; }

            // Lấy số khối (Ví dụ: "Khối 10" -> "10")
            string gradeNumber = khoi.Replace("Khối ", "").Trim();

            // 1. Tạo courseID (VD: T10, NN01.10)
            courseId = subjectCode + gradeNumber;

            // 2. Tạo classID (VD: T10.0426 - Tháng/Năm mở lớp)
            string monthYear = startDate.ToString("MMyy");
            classId = $"{courseId}.{monthYear}";

            // Xử lý nếu trùng classID (mở 2 lớp cùng 1 tháng)
            int suffix = 1;
            string originalClassId = classId;
            using (SqlConnection conn = dbHelper.GetConnection())
            {
                conn.Open();
                while (true)
                {
                    SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM Class WHERE classID = @classId", conn);
                    cmdCheck.Parameters.AddWithValue("@classId", classId);
                    int count = (int)cmdCheck.ExecuteScalar();
                    if (count == 0) break; // Không trùng

                    // Nếu trùng, thêm hậu tố .1, .2
                    classId = $"{originalClassId}.{suffix}";
                    suffix++;
                }

                // 3. Tạo Tên Lớp (VD: Lớp Toán 10 K009)
                // Tìm số K cao nhất hiện tại của courseID này
                string queryK = "SELECT TOP 1 class_name FROM Class WHERE courseID = @courseId ORDER BY class_name DESC";
                SqlCommand cmdK = new SqlCommand(queryK, conn);
                cmdK.Parameters.AddWithValue("@courseId", courseId);
                object result = cmdK.ExecuteScalar();

                int nextK = 1;
                if (result != null)
                {
                    string lastName = result.ToString();
                    // Cắt lấy số K (Giả sử định dạng chuẩn là ... K001)
                    int kIndex = lastName.LastIndexOf('K');
                    if (kIndex != -1 && int.TryParse(lastName.Substring(kIndex + 1), out int currentK))
                    {
                        nextK = currentK + 1;
                    }
                }
                className = $"Lớp {subjectName} {gradeNumber} K{nextK:D3}";
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
                        string insertClass = @"INSERT INTO Class (classID, class_name, courseID, teacherID, start_date, end_date, capacity) 
                                       VALUES (@classID, @class_name, @courseID, @teacherID, @start_date, @end_date, @capacity)";

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






    }
}
