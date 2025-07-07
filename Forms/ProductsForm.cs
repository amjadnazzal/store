using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClothingStoreManager.Models;
using ClothingStoreManager.Services;

namespace ClothingStoreManager.Forms
{
    public partial class ProductsForm : Form
    {
        private ProductService productService;
        private SupplierService supplierService;
        private DataGridView dgvProducts;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private Product selectedProduct;

        public ProductsForm()
        {
            productService = new ProductService();
            supplierService = new SupplierService();
            InitializeComponent();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = "إدارة المنتجات";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // شريط البحث
            Label lblSearch = new Label();
            lblSearch.Text = "البحث:";
            lblSearch.Location = new Point(900, 20);
            lblSearch.AutoSize = true;

            txtSearch = new TextBox();
            txtSearch.Location = new Point(650, 18);
            txtSearch.Size = new Size(200, 23);
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // الأزرار
            btnAdd = new Button();
            btnAdd.Text = "إضافة منتج";
            btnAdd.Location = new Point(550, 18);
            btnAdd.Size = new Size(80, 25);
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button();
            btnEdit.Text = "تعديل";
            btnEdit.Location = new Point(465, 18);
            btnEdit.Size = new Size(80, 25);
            btnEdit.Click += BtnEdit_Click;
            btnEdit.Enabled = false;

            btnDelete = new Button();
            btnDelete.Text = "حذف";
            btnDelete.Location = new Point(380, 18);
            btnDelete.Size = new Size(80, 25);
            btnDelete.Click += BtnDelete_Click;
            btnDelete.Enabled = false;

            btnRefresh = new Button();
            btnRefresh.Text = "تحديث";
            btnRefresh.Location = new Point(295, 18);
            btnRefresh.Size = new Size(80, 25);
            btnRefresh.Click += BtnRefresh_Click;

            // DataGridView للمنتجات
            dgvProducts = new DataGridView();
            dgvProducts.Location = new Point(20, 60);
            dgvProducts.Size = new Size(950, 480);
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.AllowUserToDeleteRows = false;
            dgvProducts.ReadOnly = true;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.MultiSelect = false;
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProducts.SelectionChanged += DgvProducts_SelectionChanged;

            // إضافة الأعمدة
            dgvProducts.Columns.Add("Id", "الرقم");
            dgvProducts.Columns.Add("Barcode", "الباركود");
            dgvProducts.Columns.Add("Name", "اسم المنتج");
            dgvProducts.Columns.Add("Size", "المقاس");
            dgvProducts.Columns.Add("Color", "اللون");
            dgvProducts.Columns.Add("SupplierName", "المورد");
            dgvProducts.Columns.Add("PurchasePrice", "سعر الشراء");
            dgvProducts.Columns.Add("SalePrice", "سعر البيع");
            dgvProducts.Columns.Add("CurrentQuantity", "الكمية");
            dgvProducts.Columns.Add("Profit", "الربح");

            // إخفاء عمود الرقم
            dgvProducts.Columns["Id"].Visible = false;

            // تنسيق الأعمدة
            dgvProducts.Columns["PurchasePrice"].DefaultCellStyle.Format = "C2";
            dgvProducts.Columns["SalePrice"].DefaultCellStyle.Format = "C2";
            dgvProducts.Columns["Profit"].DefaultCellStyle.Format = "C2";

            // إضافة الكونترولز للنافذة
            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnEdit);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(dgvProducts);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadProducts()
        {
            try
            {
                dgvProducts.Rows.Clear();
                var products = productService.GetAllProducts();

                foreach (var product in products)
                {
                    dgvProducts.Rows.Add(
                        product.Id,
                        product.Barcode,
                        product.Name,
                        product.Size,
                        product.Color,
                        product.SupplierName,
                        product.PurchasePrice,
                        product.SalePrice,
                        product.CurrentQuantity,
                        product.Profit
                    );

                    // تلوين الصفوف للمنتجات منخفضة المخزون
                    if (product.CurrentQuantity <= 5)
                    {
                        dgvProducts.Rows[dgvProducts.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightCoral;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المنتجات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                LoadProducts();
                return;
            }

            try
            {
                dgvProducts.Rows.Clear();
                var products = productService.SearchProducts(txtSearch.Text);

                foreach (var product in products)
                {
                    dgvProducts.Rows.Add(
                        product.Id,
                        product.Barcode,
                        product.Name,
                        product.Size,
                        product.Color,
                        product.SupplierName,
                        product.PurchasePrice,
                        product.SalePrice,
                        product.CurrentQuantity,
                        product.Profit
                    );

                    if (product.CurrentQuantity <= 5)
                    {
                        dgvProducts.Rows[dgvProducts.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightCoral;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                int productId = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["Id"].Value);
                selectedProduct = productService.GetProductById(productId);
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
            }
            else
            {
                selectedProduct = null;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var addForm = new ProductAddEditForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadProducts();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (selectedProduct != null)
            {
                var editForm = new ProductAddEditForm(selectedProduct);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadProducts();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProduct != null)
            {
                var result = MessageBox.Show(
                    $"هل أنت متأكد من حذف المنتج '{selectedProduct.Name}'؟",
                    "تأكيد الحذف",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        if (productService.DeleteProduct(selectedProduct.Id))
                        {
                            MessageBox.Show("تم حذف المنتج بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadProducts();
                        }
                        else
                        {
                            MessageBox.Show("فشل في حذف المنتج", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في حذف المنتج: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }
    }
}