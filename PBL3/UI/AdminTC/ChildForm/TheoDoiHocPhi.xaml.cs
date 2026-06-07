using Microsoft.Data.SqlClient;
using PBL3a.services;
using PBL3a.services.BLL;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PBL3a.UI.AdminTC.ChildForm
{
    /// <summary>
    /// Interaction logic for TheoDoiHocPhi.xaml
    /// </summary>
    public partial class TheoDoiHocPhi : UserControl
    {
        private AdminTC_Service bll = new AdminTC_Service();
        public TheoDoiHocPhi()
        {
            InitializeComponent();
            LoadDanhSachLop();
        }

        private void LoadDanhSachLop()
        {
            try
            {
                DataTable dt = bll.GetDanhSachLopForTheoDoi();
                DataRow rowAll = dt.NewRow();
                rowAll["classID"] = "ALL";         
                rowAll["class_name"] = "Tất cả"; 
                dt.Rows.InsertAt(rowAll, 0);

                cboLopHoc.ItemsSource = dt.DefaultView;
                cboLopHoc.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lớp: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cboLopHoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgvHocPhi.ItemsSource = null;
            paChiTiet.Visibility = Visibility.Collapsed;
            if (cboLopHoc.SelectedValue == null) return;
            string classId = cboLopHoc.SelectedValue.ToString();
            
            try
            {
                DataTable dtThongKe = bll.GetThongKeHocPhi(classId);
                if (dtThongKe.Rows.Count > 0)
                {
                    DataRow reader = dtThongKe.Rows[0];
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
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (cboLopHoc.SelectedValue == null) return;
            string classId = cboLopHoc.SelectedValue.ToString();

            try
            {
                DataTable dtDanhSachNo = bll.GetDanhSachHocVienNoHocPhi(classId);
                dgvHocPhi.ItemsSource = dtDanhSachNo.DefaultView;
                paChiTiet.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết danh sách nợ: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}