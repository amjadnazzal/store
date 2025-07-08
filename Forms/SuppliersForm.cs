using System;
using System.Drawing;
using System.Windows.Forms;
using ClothingStoreManager.Models;
using ClothingStoreManager.Services;

namespace ClothingStoreManager.Forms
{
    public partial class SuppliersForm : Form
    {
        private SupplierService supplierService;
        private DataGridView dgvSuppliers;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private Supplier selectedSupplier;

        public SuppliersForm()
        {
            supplierService = new SupplierService();
            InitializeComponent();
            LoadSuppliers();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = "إدارة الموردين";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // شريط البحث
            Label lblSearch = new Label();
            lblSearch.Text = "البحث:";
            lblSearch.Location = new Point(800, 20);
            lblSearch.AutoSize = true;

            txtSearch = new TextBox();
            txtSearch.Location = new Point(550, 18);
            txtSearch.Size = new Size(200, 23);
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // الأزرار
            btnAdd = new Button();
            btnAdd.Text = "إضافة مورد";
            btnAdd.Location = new Point(450, 18);
            btnAdd.Size = new Size(80, 25);
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button();
            btnEdit.Text = "تعديل";
            btnEdit.Location = new Point(365, 18);
            btnEdit.Size = new Size(80, 25);
            btnEdit.Click += BtnEdit_Click;
            btnEdit.Enabled = false;

            btnDelete = new Button();
            btnDelete.Text = "حذف";
            btnDelete.Location = new Point(280, 18);
            btnDelete.Size = new Size(80, 25);
            btnDelete.Click += BtnDelete_Click;
            btnDelete.Enabled = false;

            btnRefresh = new Button();
            btnRefresh.Text = "تحديث";
            btnRefresh.Location = new Point(195, 18);
            btnRefresh.Size = new Size(80, 25);
            btnRefresh.Click += BtnRefresh_Click;

            // DataGridView للموردين
            dgvSuppliers = new DataGridView();
            dgvSuppliers.Location = new Point(20, 60);
            dgvSuppliers.Size = new Size(850, 480);
            dgvSuppliers.AllowUserToAddRows = false;
            dgvSuppliers.AllowUserToDeleteRows = false;
            dgvSuppliers.ReadOnly = true;
            dgvSuppliers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSuppliers.MultiSelect = false;
            dgvSuppliers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSuppliers.SelectionChanged += DgvSuppliers_SelectionChanged;

            // إضافة الأعمدة
            dgvSuppliers.Columns.Add("Id", "الرقم");
            dgvSuppliers.Columns.Add("Name", "اسم المورد");
            dgvSuppliers.Columns.Add("Phone", "رقم الهاتف");
            dgvSuppliers.Columns.Add("Email", "البريد الإلكتروني");
            dgvSuppliers.Columns.Add("Address", "العنوان");
            dgvSuppliers.Columns.Add("CreatedDate", "تاريخ الإضافة");

            // إخفاء عمود الرقم
            dgvSuppliers.Columns["Id"].Visible = false;

            // تنسيق عمود التاريخ
            dgvSuppliers.Columns["CreatedDate"].DefaultCellStyle.Format = "yyyy/MM/dd";

            // إضافة الكونترولز للنافذة
            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnEdit);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(dgvSuppliers);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSuppliers()
        {
            try
            {
                dgvSuppliers.Rows.Clear();
                var suppliers = supplierService.GetAllSuppliers();

                foreach (var supplier in suppliers)
                {
                    dgvSuppliers.Rows.Add(
                        supplier.Id,
                        supplier.Name,
                        supplier.Phone,
                        supplier.Email,
                        supplier.Address,
                        supplier.CreatedDate
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الموردين: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                LoadSuppliers();
                return;
            }

            try
            {
                dgvSuppliers.Rows.Clear();
                var suppliers = supplierService.SearchSuppliers(txtSearch.Text);

                foreach (var supplier in suppliers)
                {
                    dgvSuppliers.Rows.Add(
                        supplier.Id,
                        supplier.Name,
                        supplier.Phone,
                        supplier.Email,
                        supplier.Address,
                        supplier.CreatedDate
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvSuppliers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSuppliers.SelectedRows.Count > 0)
            {
                int supplierId = Convert.ToInt32(dgvSuppliers.SelectedRows[0].Cells["Id"].Value);
                selectedSupplier = supplierService.GetSupplierById(supplierId);
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
            }
            else
            {
                selectedSupplier = null;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var addForm = new SupplierAddEditForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadSuppliers();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (selectedSupplier != null)
            {
                var editForm = new SupplierAddEditForm(selectedSupplier);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadSuppliers();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedSupplier != null)
            {
                var result = MessageBox.Show(
                    $"هل أنت متأكد من حذف المورد '{selectedSupplier.Name}'؟\nسيتم حذف جميع البيانات المرتبطة به.",
                    "تأكيد الحذف",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        if (supplierService.DeleteSupplier(selectedSupplier.Id))
                        {
                            MessageBox.Show("تم حذف المورد بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadSuppliers();
                        }
                        else
                        {
                            MessageBox.Show("فشل في حذف المورد", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في حذف المورد: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadSuppliers();
        }
    }
}