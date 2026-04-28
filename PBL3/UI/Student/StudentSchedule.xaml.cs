using Microsoft.Data.SqlClient;
using PBL3a.services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PBL3a.UI.Student
{
    public partial class StudentSchedule : UserControl
    {
        private readonly DatabaseHelper db = new DatabaseHelper();
        private readonly string currentID = "";

        private DataTable calendarTable = new DataTable();

        public StudentSchedule(string id)
        {
            InitializeComponent();
            currentID = id;

            Loaded += StudentSchedule_Load;
        }

        private void StudentSchedule_Load(object sender, RoutedEventArgs e)
        {
            LoadComboBoxMonth();

            if (cbbWeekPick.SelectedItem is MonthItem selected)
            {
                DisplayMonthCalendar(selected.Month, selected.Year);
            }
        }

        private void LoadComboBoxMonth()
        {
            cbbWeekPick.Items.Clear();

            for (int i = 1; i <= 12; i++)
            {
                cbbWeekPick.Items.Add(new MonthItem
                {
                    Text = $"Tháng {i}",
                    Month = i,
                    Year = 2026
                });
            }

            cbbWeekPick.SelectedIndex = 2;
        }

        private void DisplayMonthCalendar(int month, int year)
        {
            calendarTable = new DataTable();

            string[] days =
            {
                "Thứ 2",
                "Thứ 3",
                "Thứ 4",
                "Thứ 5",
                "Thứ 6",
                "Thứ 7",
                "Chủ Nhật"
            };

            foreach (string day in days)
            {
                calendarTable.Columns.Add(day);
            }

            for (int i = 0; i < 6; i++)
            {
                calendarTable.Rows.Add(calendarTable.NewRow());
            }

            DateTime firstDay = new DateTime(year, month, 1);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int startCol = ((int)firstDay.DayOfWeek + 6) % 7;

            int dayCounter = 1;

            for (int row = 0; row < 6; row++)
            {
                for (int col = row == 0 ? startCol : 0; col < 7; col++)
                {
                    if (dayCounter <= daysInMonth)
                    {
                        calendarTable.Rows[row][col] = dayCounter.ToString();
                        dayCounter++;
                    }
                }
            }

            FillScheduleToCalendar(month, year);
            BindCalendarToGrid();
        }

        private void BindCalendarToGrid()
        {
            dgvSchedule.Columns.Clear();

            foreach (DataColumn col in calendarTable.Columns)
            {
                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = col.ColumnName,
                    Binding = new Binding($"[{col.ColumnName}]"),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                };

                Style style = new Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Top));
                style.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(6)));

                column.ElementStyle = style;

                dgvSchedule.Columns.Add(column);
            }

            dgvSchedule.ItemsSource = calendarTable.DefaultView;

            foreach (var item in dgvSchedule.Items)
            {
                DataGridRow row = (DataGridRow)dgvSchedule.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    row.Height = 100;
                }
            }

            dgvSchedule.RowHeight = 100;
        }

        private void FillScheduleToCalendar(int month, int year)
        {
            string query = @"
                SELECT 
                    c.class_name, 
                    cs.dayOfWeek, 
                    CAST(cs.startTime AS VARCHAR(5)) AS startTime,
                    c.start_date, 
                    c.end_date
                FROM JoinClass jc
                JOIN Class c ON jc.classID = c.classID
                JOIN ClassSchedule cs ON c.classID = cs.classID
                WHERE jc.AccountID = @id";

            try
            {
                using (SqlConnection con = db.GetConnection())
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", currentID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string clsName = reader["class_name"].ToString();
                                int dayOfW = Convert.ToInt32(reader["dayOfWeek"]);
                                string time = reader["startTime"].ToString();
                                DateTime start = Convert.ToDateTime(reader["start_date"]);
                                DateTime end = Convert.ToDateTime(reader["end_date"]);

                                UpdateCalendarCells(clsName, dayOfW, time, start, end, month, year);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch: " + ex.Message);
            }
        }

        private void UpdateCalendarCells(
            string name,
            int dayOfWeek,
            string time,
            DateTime start,
            DateTime end,
            int month,
            int year)
        {
            for (int row = 0; row < calendarTable.Rows.Count; row++)
            {
                for (int col = 0; col < calendarTable.Columns.Count; col++)
                {
                    string value = calendarTable.Rows[row][col].ToString();

                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    string firstLine = value.Split('\n')[0];

                    if (!int.TryParse(firstLine, out int day))
                        continue;

                    DateTime cellDate = new DateTime(year, month, day);

                    int cellDotW = ((int)cellDate.DayOfWeek + 6) % 7 + 2;

                    if (cellDotW == dayOfWeek && cellDate >= start && cellDate <= end)
                    {
                        calendarTable.Rows[row][col] = value + $"\n● {name} ({time})";
                    }
                }
            }
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (cbbWeekPick.SelectedItem is MonthItem selected)
            {
                DisplayMonthCalendar(selected.Month, selected.Year);
            }
        }

        public class MonthItem
        {
            public string Text { get; set; } = "";
            public int Month { get; set; }
            public int Year { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}