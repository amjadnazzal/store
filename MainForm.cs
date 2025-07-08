using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClothingStoreManager.Forms;
using ClothingStoreManager.Services;
using ClothingStoreManager.Models;

namespace ClothingStoreManager
{
    public partial class MainForm : Form
    {
        private UserService userService;
        private ReportService reportService;
        private Panel dashboardPanel;

        public MainForm()
        {
            userService = new UserService();
            reportService = new ReportService();
            InitializeComponent();
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            UpdateUserInterface();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // إعدادات النافذة الرئيسية
            this.Text = "نظام إدارة متجر الملابس";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // إنشاء شريط القوائم
            CreateMenuStrip();

            // إنشاء شريط الأدوات
            CreateToolStrip();

            // إنشاء شريط الحالة
            CreateStatusStrip();

            // إنشاء لوحة التحكم الرئيسية
            CreateDashboard();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CreateMenuStrip()
        {
            MenuStrip menuStrip = new MenuStrip();
            menuStrip.RightToLeft = RightToLeft.Yes;

            // قائمة الملف
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("ملف");
            if (userService.HasPermission("BackupRestore"))
            {
                fileMenu.DropDownItems.Add("نسخة احتياطية", null, BackupDatabase_Click);
                fileMenu.DropDownItems.Add("استعادة", null, RestoreDatabase_Click);
                fileMenu.DropDownItems.Add(new ToolStripSeparator());
            }
            if (userService.HasPermission("ChangeSettings"))
            {
                fileMenu.DropDownItems.Add("إعدادات", null, Settings_Click);
                fileMenu.DropDownItems.Add(new ToolStripSeparator());
            }
            fileMenu.DropDownItems.Add("تغيير كلمة المرور", null, ChangePassword_Click);
            fileMenu.DropDownItems.Add("تسجيل خروج", null, Logout_Click);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("خروج", null, Exit_Click);

            // قائمة المخزون
            ToolStripMenuItem inventoryMenu = new ToolStripMenuItem("المخزون");
            if (userService.HasPermission("ManageProducts"))
            {
                inventoryMenu.DropDownItems.Add("إدارة المنتجات", null, Products_Click);
            }
            if (userService.HasPermission("ManageSuppliers"))
            {
                inventoryMenu.DropDownItems.Add("إدارة الموردين", null, Suppliers_Click);
            }

            // قائمة المبيعات
            ToolStripMenuItem salesMenu = new ToolStripMenuItem("المبيعات");
            if (userService.HasPermission("MakeSales"))
            {
                salesMenu.DropDownItems.Add("نقطة البيع", null, POS_Click);
            }
            if (userService.HasPermission("ManageCustomers"))
            {
                salesMenu.DropDownItems.Add("إدارة العملاء", null, Customers_Click);
            }
            if (userService.HasPermission("ProcessReturns"))
            {
                salesMenu.DropDownItems.Add("المرتجعات", null, Returns_Click);
            }

            // قائمة التقارير
            ToolStripMenuItem reportsMenu = new ToolStripMenuItem("التقارير");
            if (userService.HasPermission("ViewReports"))
            {
                reportsMenu.DropDownItems.Add("لوحة التحكم", null, Dashboard_Click);
                reportsMenu.DropDownItems.Add(new ToolStripSeparator());
                reportsMenu.DropDownItems.Add("تقرير المبيعات", null, SalesReport_Click);
                reportsMenu.DropDownItems.Add("تقرير الأرباح", null, ProfitReport_Click);
                reportsMenu.DropDownItems.Add("تقرير المخزون", null, InventoryReport_Click);
                reportsMenu.DropDownItems.Add("منتجات منخفضة المخزون", null, LowStockReport_Click);
            }

            // قائمة المستخدمين (للمديرين فقط)
            ToolStripMenuItem usersMenu = new ToolStripMenuItem("المستخدمين");
            if (userService.HasPermission("ManageUsers"))
            {
                usersMenu.DropDownItems.Add("إدارة المستخدمين", null, Users_Click);
                usersMenu.DropDownItems.Add("سجل النشاط", null, ActivityLog_Click);
            }

            menuStrip.Items.Add(fileMenu);
            if (inventoryMenu.DropDownItems.Count > 0)
                menuStrip.Items.Add(inventoryMenu);
            if (salesMenu.DropDownItems.Count > 0)
                menuStrip.Items.Add(salesMenu);
            if (reportsMenu.DropDownItems.Count > 0)
                menuStrip.Items.Add(reportsMenu);
            if (usersMenu.DropDownItems.Count > 0)
                menuStrip.Items.Add(usersMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void CreateToolStrip()
        {
            ToolStrip toolStrip = new ToolStrip();
            toolStrip.RightToLeft = RightToLeft.Yes;

            // أزرار سريعة
            ToolStripButton posButton = new ToolStripButton("نقطة البيع");
            posButton.Image = SystemIcons.Application.ToBitmap();
            posButton.Click += POS_Click;

            ToolStripButton productsButton = new ToolStripButton("المنتجات");
            productsButton.Image = SystemIcons.Information.ToBitmap();
            productsButton.Click += Products_Click;

            ToolStripButton customersButton = new ToolStripButton("العملاء");
            customersButton.Image = SystemIcons.Question.ToBitmap();
            customersButton.Click += Customers_Click;

            toolStrip.Items.Add(posButton);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(productsButton);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(customersButton);

            this.Controls.Add(toolStrip);
        }

        private void CreateStatusStrip()
        {
            StatusStrip statusStrip = new StatusStrip();
            statusStrip.RightToLeft = RightToLeft.Yes;

            ToolStripStatusLabel userLabel = new ToolStripStatusLabel();
            var currentUser = UserService.CurrentUser;
            userLabel.Text = $"المستخدم: {currentUser?.FullName ?? "غير محدد"} ({GetRoleName(currentUser?.Role ?? UserRole.Employee)})";

            ToolStripStatusLabel dateLabel = new ToolStripStatusLabel();
            dateLabel.Text = $"التاريخ: {DateTime.Now:yyyy/MM/dd}";

            ToolStripStatusLabel timeLabel = new ToolStripStatusLabel();
            timeLabel.Text = $"الوقت: {DateTime.Now:HH:mm}";

            statusStrip.Items.Add(userLabel);
            statusStrip.Items.Add(new ToolStripStatusLabel() { Spring = true });
            statusStrip.Items.Add(dateLabel);
            statusStrip.Items.Add(timeLabel);

            this.Controls.Add(statusStrip);

            // تحديث الوقت كل ثانية
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) => timeLabel.Text = $"الوقت: {DateTime.Now:HH:mm:ss}";
            timer.Start();
        }

        private void CreateDashboard()
        {
            dashboardPanel = new Panel();
            dashboardPanel.Dock = DockStyle.Fill;
            dashboardPanel.BackColor = Color.FromArgb(245, 245, 245);
            dashboardPanel.AutoScroll = true;

            RefreshDashboard();
            this.Controls.Add(dashboardPanel);
        }

        private void RefreshDashboard()
        {
            dashboardPanel.Controls.Clear();
            
            var currentUser = UserService.CurrentUser;
            
            // رسالة ترحيب
            Label welcomeLabel = new Label();
            welcomeLabel.Text = $"مرحباً {currentUser?.FullName ?? "غير محدد"}";
            welcomeLabel.Font = new Font("Arial", 18, FontStyle.Bold);
            welcomeLabel.ForeColor = Color.DarkBlue;
            welcomeLabel.AutoSize = true;
            welcomeLabel.Location = new Point(50, 30);
            dashboardPanel.Controls.Add(welcomeLabel);

            if (!userService.HasPermission("ViewReports"))
            {
                Label noPermissionLabel = new Label();
                noPermissionLabel.Text = "ليس لديك صلاحية لعرض لوحة التحكم";
                noPermissionLabel.Font = new Font("Arial", 12);
                noPermissionLabel.ForeColor = Color.Red;
                noPermissionLabel.Location = new Point(50, 80);
                noPermissionLabel.AutoSize = true;
                dashboardPanel.Controls.Add(noPermissionLabel);
                return;
            }

            try
            {
                var dashboardData = reportService.GetDashboardData();
                
                // إنشاء بطاقات الإحصائيات
                CreateStatCard("مبيعات اليوم", dashboardData.TodaySales.ToString("C2"), Color.Green, 50, 80);
                CreateStatCard("عدد الفواتير اليوم", dashboardData.TodayInvoices.ToString(), Color.Blue, 300, 80);
                CreateStatCard("مبيعات الشهر", dashboardData.MonthSales.ToString("C2"), Color.Orange, 550, 80);
                CreateStatCard("منتجات منخفضة المخزون", dashboardData.LowStockCount.ToString(), Color.Red, 800, 80);

                CreateStatCard("إجمالي المنتجات", dashboardData.TotalProducts.ToString(), Color.Purple, 50, 200);
                CreateStatCard("إجمالي العملاء", dashboardData.TotalCustomers.ToString(), Color.Teal, 300, 200);
                CreateStatCard("مرتجعات اليوم", dashboardData.TotalReturns.ToString("C2"), Color.Brown, 550, 200);

                // أفضل المنتجات مبيعاً
                CreateTopProductsList(dashboardData.TopProducts, 50, 320);
                
                // مبيعات الأسبوع
                CreateWeeklySalesChart(dashboardData.WeeklySales, 400, 320);
            }
            catch (Exception ex)
            {
                Label errorLabel = new Label();
                errorLabel.Text = $"خطأ في تحميل البيانات: {ex.Message}";
                errorLabel.Font = new Font("Arial", 10);
                errorLabel.ForeColor = Color.Red;
                errorLabel.Location = new Point(50, 80);
                errorLabel.AutoSize = true;
                dashboardPanel.Controls.Add(errorLabel);
            }
        }

        private void CreateStatCard(string title, string value, Color color, int x, int y)
        {
            Panel card = new Panel();
            card.Size = new Size(200, 100);
            card.Location = new Point(x, y);
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;

            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = new Font("Arial", 10, FontStyle.Bold);
            titleLabel.ForeColor = color;
            titleLabel.Location = new Point(10, 10);
            titleLabel.AutoSize = true;

            Label valueLabel = new Label();
            valueLabel.Text = value;
            valueLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            valueLabel.ForeColor = Color.Black;
            valueLabel.Location = new Point(10, 40);
            valueLabel.AutoSize = true;

            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);
            dashboardPanel.Controls.Add(card);
        }

        private void CreateTopProductsList(List<dynamic> topProducts, int x, int y)
        {
            Panel panel = new Panel();
            panel.Size = new Size(300, 200);
            panel.Location = new Point(x, y);
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;

            Label titleLabel = new Label();
            titleLabel.Text = "أفضل المنتجات مبيعاً";
            titleLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            titleLabel.Location = new Point(10, 10);
            titleLabel.AutoSize = true;

            ListView listView = new ListView();
            listView.View = View.Details;
            listView.Location = new Point(10, 40);
            listView.Size = new Size(280, 150);
            listView.Columns.Add("المنتج", 150);
            listView.Columns.Add("الكمية", 60);
            listView.Columns.Add("المبلغ", 60);

            foreach (var product in topProducts)
            {
                var item = new ListViewItem(product.Name.ToString());
                item.SubItems.Add(product.TotalSold.ToString());
                item.SubItems.Add(((decimal)product.TotalRevenue).ToString("C0"));
                listView.Items.Add(item);
            }

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(listView);
            dashboardPanel.Controls.Add(panel);
        }

        private void CreateWeeklySalesChart(List<ReportService.SalesReport> weeklySales, int x, int y)
        {
            Panel panel = new Panel();
            panel.Size = new Size(350, 200);
            panel.Location = new Point(x, y);
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;

            Label titleLabel = new Label();
            titleLabel.Text = "مبيعات الأسبوع";
            titleLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            titleLabel.Location = new Point(10, 10);
            titleLabel.AutoSize = true;

            // رسم بياني بسيط
            Panel chartPanel = new Panel();
            chartPanel.Location = new Point(10, 40);
            chartPanel.Size = new Size(330, 150);
            chartPanel.Paint += (s, e) => DrawSalesChart(e.Graphics, weeklySales, chartPanel.Size);

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(chartPanel);
            dashboardPanel.Controls.Add(panel);
        }

        private void DrawSalesChart(Graphics g, List<ReportService.SalesReport> sales, Size chartSize)
        {
            if (sales.Count == 0) return;

            var maxSales = sales.Max(s => s.NetSales);
            if (maxSales == 0) return;

            var barWidth = chartSize.Width / sales.Count;
            var scale = (chartSize.Height - 20) / (double)maxSales;

            for (int i = 0; i < sales.Count; i++)
            {
                var sale = sales[i];
                var barHeight = (int)(sale.NetSales * scale);
                var x = i * barWidth;
                var y = chartSize.Height - barHeight - 10;

                g.FillRectangle(Brushes.SkyBlue, x + 5, y, barWidth - 10, barHeight);
                g.DrawString(sale.Date.ToString("MM/dd"), new Font("Arial", 8), Brushes.Black, x + 2, chartSize.Height - 10);
            }
        }

        private void UpdateUserInterface()
        {
            // تحديث واجهة المستخدم بناءً على الصلاحيات
            this.Text = $"نظام إدارة متجر الملابس - {UserService.CurrentUser?.FullName ?? "غير محدد"}";
        }

        private string GetRoleName(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "مدير النظام",
                UserRole.Manager => "مدير المتجر",
                UserRole.Cashier => "كاشير",
                UserRole.Employee => "موظف",
                _ => "غير محدد"
            };
        }

        // معالجات الأحداث الجديدة
        private void Dashboard_Click(object sender, EventArgs e)
        {
            RefreshDashboard();
        }

        private void ChangePassword_Click(object sender, EventArgs e)
        {
            var changePasswordForm = new ChangePasswordForm();
            changePasswordForm.ShowDialog();
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            userService.Logout();
            this.Hide();
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                UpdateUserInterface();
                RefreshDashboard();
            }
            else
            {
                Application.Exit();
            }
        }

        private void Users_Click(object sender, EventArgs e)
        {
            var usersForm = new UsersForm();
            usersForm.ShowDialog();
        }

        private void ActivityLog_Click(object sender, EventArgs e)
        {
            var activityLogForm = new ActivityLogForm();
            activityLogForm.ShowDialog();
        }

        // معالجات الأحداث الأصلية
        private void POS_Click(object sender, EventArgs e)
        {
            POSForm posForm = new POSForm();
            posForm.Show();
        }

        private void Products_Click(object sender, EventArgs e)
        {
            ProductsForm productsForm = new ProductsForm();
            productsForm.Show();
        }

        private void Customers_Click(object sender, EventArgs e)
        {
            CustomersForm customersForm = new CustomersForm();
            customersForm.Show();
        }

        private void Suppliers_Click(object sender, EventArgs e)
        {
            SuppliersForm suppliersForm = new SuppliersForm();
            suppliersForm.Show();
        }

        private void Returns_Click(object sender, EventArgs e)
        {
            MessageBox.Show("سيتم تطوير وحدة المرتجعات قريباً", "قيد التطوير", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SalesReport_Click(object sender, EventArgs e)
        {
            MessageBox.Show("سيتم تطوير تقرير المبيعات قريباً", "قيد التطوير", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ProfitReport_Click(object sender, EventArgs e)
        {
            MessageBox.Show("سيتم تطوير تقرير الأرباح قريباً", "قيد التطوير", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InventoryReport_Click(object sender, EventArgs e)
        {
            MessageBox.Show("سيتم تطوير تقرير المخزون قريباً", "قيد التطوير", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LowStockReport_Click(object sender, EventArgs e)
        {
            MessageBox.Show("سيتم تطوير تقرير المنتجات منخفضة المخزون قريباً", "قيد التطوير", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            MessageBox.Show("سيتم تطوير شاشة الإعدادات قريباً", "قيد التطوير", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BackupDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Database files (*.db)|*.db";
                saveDialog.FileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    DatabaseHelper.BackupDatabase(saveDialog.FileName);
                    MessageBox.Show("تم إنشاء النسخة الاحتياطية بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إنشاء النسخة الاحتياطية: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestoreDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Database files (*.db)|*.db";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    var result = MessageBox.Show("هل أنت متأكد من استعادة النسخة الاحتياطية؟ سيتم فقدان جميع البيانات الحالية.", 
                        "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        DatabaseHelper.RestoreDatabase(openDialog.FileName);
                        MessageBox.Show("تم استعادة النسخة الاحتياطية بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في استعادة النسخة الاحتياطية: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}