using System;
using System.Drawing;
using System.Windows.Forms;
using ClothingStoreManager.Forms;

namespace ClothingStoreManager
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
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
            fileMenu.DropDownItems.Add("نسخة احتياطية", null, BackupDatabase_Click);
            fileMenu.DropDownItems.Add("استعادة", null, RestoreDatabase_Click);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("إعدادات", null, Settings_Click);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("خروج", null, Exit_Click);

            // قائمة المخزون
            ToolStripMenuItem inventoryMenu = new ToolStripMenuItem("المخزون");
            inventoryMenu.DropDownItems.Add("إدارة المنتجات", null, Products_Click);
            inventoryMenu.DropDownItems.Add("إدارة الموردين", null, Suppliers_Click);

            // قائمة المبيعات
            ToolStripMenuItem salesMenu = new ToolStripMenuItem("المبيعات");
            salesMenu.DropDownItems.Add("نقطة البيع", null, POS_Click);
            salesMenu.DropDownItems.Add("إدارة العملاء", null, Customers_Click);
            salesMenu.DropDownItems.Add("المرتجعات", null, Returns_Click);

            // قائمة التقارير
            ToolStripMenuItem reportsMenu = new ToolStripMenuItem("التقارير");
            reportsMenu.DropDownItems.Add("تقرير المبيعات", null, SalesReport_Click);
            reportsMenu.DropDownItems.Add("تقرير الأرباح", null, ProfitReport_Click);
            reportsMenu.DropDownItems.Add("تقرير المخزون", null, InventoryReport_Click);
            reportsMenu.DropDownItems.Add("منتجات منخفضة المخزون", null, LowStockReport_Click);

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(inventoryMenu);
            menuStrip.Items.Add(salesMenu);
            menuStrip.Items.Add(reportsMenu);

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

            ToolStripStatusLabel dateLabel = new ToolStripStatusLabel();
            dateLabel.Text = $"التاريخ: {DateTime.Now:yyyy/MM/dd}";

            ToolStripStatusLabel timeLabel = new ToolStripStatusLabel();
            timeLabel.Text = $"الوقت: {DateTime.Now:HH:mm}";

            statusStrip.Items.Add(dateLabel);
            statusStrip.Items.Add(new ToolStripStatusLabel() { Spring = true });
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
            // سيتم إنشاء لوحة التحكم هنا
            Panel dashboardPanel = new Panel();
            dashboardPanel.Dock = DockStyle.Fill;
            dashboardPanel.BackColor = Color.LightGray;

            Label welcomeLabel = new Label();
            welcomeLabel.Text = "مرحباً بك في نظام إدارة متجر الملابس";
            welcomeLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            welcomeLabel.ForeColor = Color.DarkBlue;
            welcomeLabel.AutoSize = true;
            welcomeLabel.Location = new Point(50, 50);

            dashboardPanel.Controls.Add(welcomeLabel);
            this.Controls.Add(dashboardPanel);
        }

        // معالجات الأحداث
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