using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using ClothingStoreManager.Services;

namespace ClothingStoreManager.Forms
{
    public partial class ActivityLogForm : Form
    {
        private ListView listView;
        private DateTimePicker dateFrom, dateTo;
        private ComboBox cmbUser;
        private TextBox txtAction;
        private Button btnFilter, btnClear, btnRefresh, btnClose;
        private Label lblDateFrom, lblDateTo, lblUser, lblAction;

        public ActivityLogForm()
        {
            InitializeComponent();
            LoadUsers();
            LoadActivityLog();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = "سجل النشاط";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // فلاتر البحث
            Panel filterPanel = new Panel();
            filterPanel.Size = new Size(950, 80);
            filterPanel.Location = new Point(20, 20);
            filterPanel.BorderStyle = BorderStyle.FixedSingle;
            filterPanel.BackColor = Color.LightGray;

            // من تاريخ
            lblDateFrom = new Label();
            lblDateFrom.Text = "من تاريخ:";
            lblDateFrom.Location = new Point(850, 15);
            lblDateFrom.Size = new Size(60, 23);
            lblDateFrom.Font = new Font("Arial", 9);

            dateFrom = new DateTimePicker();
            dateFrom.Location = new Point(700, 12);
            dateFrom.Size = new Size(140, 25);
            dateFrom.Format = DateTimePickerFormat.Short;
            dateFrom.Value = DateTime.Today.AddDays(-30);

            // إلى تاريخ
            lblDateTo = new Label();
            lblDateTo.Text = "إلى تاريخ:";
            lblDateTo.Location = new Point(630, 15);
            lblDateTo.Size = new Size(60, 23);
            lblDateTo.Font = new Font("Arial", 9);

            dateTo = new DateTimePicker();
            dateTo.Location = new Point(480, 12);
            dateTo.Size = new Size(140, 25);
            dateTo.Format = DateTimePickerFormat.Short;
            dateTo.Value = DateTime.Today;

            // المستخدم
            lblUser = new Label();
            lblUser.Text = "المستخدم:";
            lblUser.Location = new Point(410, 15);
            lblUser.Size = new Size(60, 23);
            lblUser.Font = new Font("Arial", 9);

            cmbUser = new ComboBox();
            cmbUser.Location = new Point(260, 12);
            cmbUser.Size = new Size(140, 25);
            cmbUser.DropDownStyle = ComboBoxStyle.DropDownList;

            // العملية
            lblAction = new Label();
            lblAction.Text = "العملية:";
            lblAction.Location = new Point(190, 15);
            lblAction.Size = new Size(60, 23);
            lblAction.Font = new Font("Arial", 9);

            txtAction = new TextBox();
            txtAction.Location = new Point(40, 12);
            txtAction.Size = new Size(140, 25);
            txtAction.Font = new Font("Arial", 9);

            // أزرار الفلتر
            btnFilter = new Button();
            btnFilter.Text = "فلتر";
            btnFilter.Location = new Point(850, 45);
            btnFilter.Size = new Size(80, 25);
            btnFilter.BackColor = Color.Blue;
            btnFilter.ForeColor = Color.White;
            btnFilter.Font = new Font("Arial", 8, FontStyle.Bold);
            btnFilter.FlatStyle = FlatStyle.Flat;
            btnFilter.Click += BtnFilter_Click;

            btnClear = new Button();
            btnClear.Text = "مسح";
            btnClear.Location = new Point(760, 45);
            btnClear.Size = new Size(80, 25);
            btnClear.BackColor = Color.Gray;
            btnClear.ForeColor = Color.White;
            btnClear.Font = new Font("Arial", 8, FontStyle.Bold);
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.Click += BtnClear_Click;

            btnRefresh = new Button();
            btnRefresh.Text = "تحديث";
            btnRefresh.Location = new Point(670, 45);
            btnRefresh.Size = new Size(80, 25);
            btnRefresh.BackColor = Color.Green;
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Font = new Font("Arial", 8, FontStyle.Bold);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Click += BtnRefresh_Click;

            // إضافة عناصر الفلتر للوحة
            filterPanel.Controls.Add(lblDateFrom);
            filterPanel.Controls.Add(dateFrom);
            filterPanel.Controls.Add(lblDateTo);
            filterPanel.Controls.Add(dateTo);
            filterPanel.Controls.Add(lblUser);
            filterPanel.Controls.Add(cmbUser);
            filterPanel.Controls.Add(lblAction);
            filterPanel.Controls.Add(txtAction);
            filterPanel.Controls.Add(btnFilter);
            filterPanel.Controls.Add(btnClear);
            filterPanel.Controls.Add(btnRefresh);

            // قائمة سجل النشاط
            listView = new ListView();
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.Location = new Point(20, 120);
            listView.Size = new Size(850, 450);
            listView.Font = new Font("Arial", 9);

            listView.Columns.Add("الرقم", 50);
            listView.Columns.Add("المستخدم", 120);
            listView.Columns.Add("العملية", 150);
            listView.Columns.Add("الجدول", 100);
            listView.Columns.Add("رقم السجل", 80);
            listView.Columns.Add("التاريخ والوقت", 150);
            listView.Columns.Add("القيم القديمة", 200);
            listView.Columns.Add("القيم الجديدة", 200);

            // زر الإغلاق
            btnClose = new Button();
            btnClose.Text = "إغلاق";
            btnClose.Location = new Point(890, 580);
            btnClose.Size = new Size(80, 35);
            btnClose.BackColor = Color.DarkRed;
            btnClose.ForeColor = Color.White;
            btnClose.Font = new Font("Arial", 9, FontStyle.Bold);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Click += BtnClose_Click;

            // إضافة العناصر للنافذة
            this.Controls.Add(filterPanel);
            this.Controls.Add(listView);
            this.Controls.Add(btnClose);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadUsers()
        {
            try
            {
                cmbUser.Items.Clear();
                cmbUser.Items.Add("جميع المستخدمين");

                var userService = new UserService();
                var users = userService.GetAllUsers();

                foreach (var user in users)
                {
                    cmbUser.Items.Add($"{user.FullName} ({user.Username})");
                }

                cmbUser.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المستخدمين: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadActivityLog(string userFilter = null, string actionFilter = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                listView.Items.Clear();

                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT 
                            al.Id,
                            COALESCE(u.FullName, 'نظام') as UserName,
                            al.Action,
                            al.TableName,
                            al.RecordId,
                            al.Timestamp,
                            al.OldValues,
                            al.NewValues
                        FROM ActivityLog al
                        LEFT JOIN Users u ON al.UserId = u.Id
                        WHERE 1=1";

                    var parameters = new List<SQLiteParameter>();

                    if (!string.IsNullOrWhiteSpace(userFilter) && userFilter != "جميع المستخدمين")
                    {
                        query += " AND (u.FullName LIKE @userFilter OR u.Username LIKE @userFilter)";
                        parameters.Add(new SQLiteParameter("@userFilter", $"%{userFilter}%"));
                    }

                    if (!string.IsNullOrWhiteSpace(actionFilter))
                    {
                        query += " AND al.Action LIKE @actionFilter";
                        parameters.Add(new SQLiteParameter("@actionFilter", $"%{actionFilter}%"));
                    }

                    if (fromDate.HasValue)
                    {
                        query += " AND DATE(al.Timestamp) >= DATE(@fromDate)";
                        parameters.Add(new SQLiteParameter("@fromDate", fromDate.Value.ToString("yyyy-MM-dd")));
                    }

                    if (toDate.HasValue)
                    {
                        query += " AND DATE(al.Timestamp) <= DATE(@toDate)";
                        parameters.Add(new SQLiteParameter("@toDate", toDate.Value.ToString("yyyy-MM-dd")));
                    }

                    query += " ORDER BY al.Timestamp DESC LIMIT 1000";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param);
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new ListViewItem(reader.GetInt32("Id").ToString());
                                item.SubItems.Add(reader.IsDBNull("UserName") ? "نظام" : reader.GetString("UserName"));
                                item.SubItems.Add(reader.GetString("Action"));
                                item.SubItems.Add(reader.IsDBNull("TableName") ? "" : reader.GetString("TableName"));
                                item.SubItems.Add(reader.IsDBNull("RecordId") ? "" : reader.GetInt32("RecordId").ToString());
                                item.SubItems.Add(reader.GetDateTime("Timestamp").ToString("yyyy/MM/dd HH:mm:ss"));
                                item.SubItems.Add(reader.IsDBNull("OldValues") ? "" : reader.GetString("OldValues"));
                                item.SubItems.Add(reader.IsDBNull("NewValues") ? "" : reader.GetString("NewValues"));

                                listView.Items.Add(item);
                            }
                        }
                    }
                }

                // عرض عدد السجلات
                this.Text = $"سجل النشاط - عدد السجلات: {listView.Items.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل سجل النشاط: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            string userFilter = cmbUser.SelectedIndex > 0 ? 
                cmbUser.SelectedItem.ToString().Split('(')[0].Trim() : null;
            string actionFilter = txtAction.Text.Trim();
            DateTime? fromDate = dateFrom.Value.Date;
            DateTime? toDate = dateTo.Value.Date.AddDays(1).AddSeconds(-1);

            LoadActivityLog(userFilter, actionFilter, fromDate, toDate);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            cmbUser.SelectedIndex = 0;
            txtAction.Clear();
            dateFrom.Value = DateTime.Today.AddDays(-30);
            dateTo.Value = DateTime.Today;
            LoadActivityLog();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadActivityLog();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}