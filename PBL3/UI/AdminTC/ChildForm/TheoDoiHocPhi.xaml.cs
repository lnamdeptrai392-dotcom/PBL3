using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PBL3a.UI.AdminTC.ChildForm
{
    /// <summary>
    /// Interaction logic for TheoDoiHocPhi.xaml
    /// </summary>
    public partial class TheoDoiHocPhi : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        public TheoDoiHocPhi()
        {
            InitializeComponent();
            LoadDanhSachLop();
        }

        private void LoadDanhSachLop()
        {
            string query = @"SELECT classID, class_name FROM Class ORDER BY class_name";
            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    DataRow drAll = dt.NewRow();
                    drAll["classID"] = "ALL";
                    drAll["class_name"] = "Tất cả";
                    dt.Rows.InsertAt(drAll, 0);
                    cboLopHoc.ItemsSource = dt.DefaultView;
                    cboLopHoc.SelectedIndex = 0;
                }
            }
        }

        private void cboLopHoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgvHocPhi.ItemsSource = null;
            paChiTiet.Visibility = Visibility.Collapsed;
            if (cboLopHoc.SelectedValue == null) return;
            string classId = cboLopHoc.SelectedValue.ToString();
            using (SqlConnection conn = db.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"
                    SELECT 
                        COUNT(DISTINCT jc.AccountID),
                        SUM(CASE WHEN hp.TrangThai = N'Đã đóng' THEN 1 ELSE 0 END)
                    FROM JoinClass jc
                    LEFT JOIN HocPhi hp ON jc.AccountID = hp.AccountID AND jc.classID = hp.ClassID";

                    if (classId != "ALL")
                    {
                        query += " WHERE jc.classID = @ClassID";
                    }
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (classId != "ALL")
                        {
                            cmd.Parameters.AddWithValue("@ClassID", classId);
                        }
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int tongHocVien = Convert.ToInt32(reader[0]);
                                if (tongHocVien == 0)
                                {
                                    txtTongHocVien.Text = "";
                                    txtDaNop.Text = "";
                                    txtChuaNop.Text = "Lớp trống"; 

                                    btnXemChiTiet.IsEnabled = false;
                                    dgvHocPhi.ItemsSource = null;
                                }
                                else
                                {
                                    int daNop = Convert.ToInt32(reader[1]);
                                    int chuaNop = tongHocVien - daNop;

                                    txtTongHocVien.Text = tongHocVien.ToString();
                                    txtDaNop.Text = daNop.ToString();
                                    txtChuaNop.Text = chuaNop.ToString();

                                    btnXemChiTiet.IsEnabled = (chuaNop > 0);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void btnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (cboLopHoc.SelectedValue == null) return;
            string classId = cboLopHoc.SelectedValue.ToString();

            using (SqlConnection conn = db.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query= @"
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
                            WHERE hp.TrangThai = N'Chưa đóng' OR hp.TrangThai IS NULL";

                    if (classId != "ALL")
                    {
                        query += " AND jc.classID = @ClassID";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (classId != "ALL") cmd.Parameters.AddWithValue("@ClassID", classId);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dtDanhSachNo = new DataTable();
                            da.Fill(dtDanhSachNo);

                            dgvHocPhi.ItemsSource = dtDanhSachNo.DefaultView;
                            paChiTiet.Visibility = Visibility.Visible;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải chi tiết danh sách nợ: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
