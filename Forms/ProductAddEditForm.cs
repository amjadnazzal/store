using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClothingStoreManager.Models;
using ClothingStoreManager.Services;

namespace ClothingStoreManager.Forms
{
    public partial class ProductAddEditForm : Form
    {
        private ProductService productService;
        private SupplierService supplierService;
        private Product currentProduct;
        private bool isEditMode;

        private TextBox txtBarcode, txtName, txtDescription, txtSize, txtColor;
        private ComboBox cmbSupplier;
        private NumericUpDown nudPurchasePrice, nudSalePrice, nudQuantity;
        private Button btnSave, btnCancel, btnSelectImage;
        private PictureBox pbImage;
        private string selectedImagePath;

        public ProductAddEditForm(Product product = null)
        {
            productService = new ProductService();
            supplierService = new SupplierService();
            currentProduct = product;
            isEditMode = product != null;
            InitializeComponent();
            LoadSuppliers();
            
            if (isEditMode)
            {
                LoadProductData();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = isEditMode ? "تعديل منتج" : "إضافة منتج جديد";
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            int yPosition = 20;
            int labelWidth = 100;
            int controlWidth = 250;
            int spacing = 35;

            // الباركود
            Label lblBarcode = new Label();
            lblBarcode.Text = "الباركود *:";
            lblBarcode.Location = new Point(380, yPosition);
            lblBarcode.Size = new Size(labelWidth, 23);

            txtBarcode = new TextBox();
            txtBarcode.Location = new Point(120, yPosition);
            txtBarcode.Size = new Size(controlWidth, 23);

            yPosition += spacing;

            // اسم المنتج
            Label lblName = new Label();
            lblName.Text = "اسم المنتج *:";
            lblName.Location = new Point(380, yPosition);
            lblName.Size = new Size(labelWidth, 23);

            txtName = new TextBox();
            txtName.Location = new Point(120, yPosition);
            txtName.Size = new Size(controlWidth, 23);

            yPosition += spacing;

            // الوصف
            Label lblDescription = new Label();
            lblDescription.Text = "الوصف:";
            lblDescription.Location = new Point(380, yPosition);
            lblDescription.Size = new Size(labelWidth, 23);

            txtDescription = new TextBox();
            txtDescription.Location = new Point(120, yPosition);
            txtDescription.Size = new Size(controlWidth, 23);

            yPosition += spacing;

            // المقاس
            Label lblSize = new Label();
            lblSize.Text = "المقاس:";
            lblSize.Location = new Point(380, yPosition);
            lblSize.Size = new Size(labelWidth, 23);

            txtSize = new TextBox();
            txtSize.Location = new Point(120, yPosition);
            txtSize.Size = new Size(controlWidth, 23);

            yPosition += spacing;

            // اللون
            Label lblColor = new Label();
            lblColor.Text = "اللون:";
            lblColor.Location = new Point(380, yPosition);
            lblColor.Size = new Size(labelWidth, 23);

            txtColor = new TextBox();
            txtColor.Location = new Point(120, yPosition);
            txtColor.Size = new Size(controlWidth, 23);

            yPosition += spacing;

            // المورد
            Label lblSupplier = new Label();
            lblSupplier.Text = "المورد:";
            lblSupplier.Location = new Point(380, yPosition);
            lblSupplier.Size = new Size(labelWidth, 23);

            cmbSupplier = new ComboBox();
            cmbSupplier.Location = new Point(120, yPosition);
            cmbSupplier.Size = new Size(controlWidth, 23);
            cmbSupplier.DropDownStyle = ComboBoxStyle.DropDownList;

            yPosition += spacing;

            // سعر الشراء
            Label lblPurchasePrice = new Label();
            lblPurchasePrice.Text = "سعر الشراء *:";
            lblPurchasePrice.Location = new Point(380, yPosition);
            lblPurchasePrice.Size = new Size(labelWidth, 23);

            nudPurchasePrice = new NumericUpDown();
            nudPurchasePrice.Location = new Point(120, yPosition);
            nudPurchasePrice.Size = new Size(controlWidth, 23);
            nudPurchasePrice.DecimalPlaces = 2;
            nudPurchasePrice.Maximum = 999999;

            yPosition += spacing;

            // سعر البيع
            Label lblSalePrice = new Label();
            lblSalePrice.Text = "سعر البيع *:";
            lblSalePrice.Location = new Point(380, yPosition);
            lblSalePrice.Size = new Size(labelWidth, 23);

            nudSalePrice = new NumericUpDown();
            nudSalePrice.Location = new Point(120, yPosition);
            nudSalePrice.Size = new Size(controlWidth, 23);
            nudSalePrice.DecimalPlaces = 2;
            nudSalePrice.Maximum = 999999;

            yPosition += spacing;

            // الكمية
            Label lblQuantity = new Label();
            lblQuantity.Text = "الكمية *:";
            lblQuantity.Location = new Point(380, yPosition);
            lblQuantity.Size = new Size(labelWidth, 23);

            nudQuantity = new NumericUpDown();
            nudQuantity.Location = new Point(120, yPosition);
            nudQuantity.Size = new Size(controlWidth, 23);
            nudQuantity.Maximum = 999999;

            yPosition += spacing;

            // الصورة
            Label lblImage = new Label();
            lblImage.Text = "صورة المنتج:";
            lblImage.Location = new Point(380, yPosition);
            lblImage.Size = new Size(labelWidth, 23);

            btnSelectImage = new Button();
            btnSelectImage.Text = "اختيار صورة";
            btnSelectImage.Location = new Point(270, yPosition);
            btnSelectImage.Size = new Size(100, 25);
            btnSelectImage.Click += BtnSelectImage_Click;

            pbImage = new PictureBox();
            pbImage.Location = new Point(120, yPosition);
            pbImage.Size = new Size(100, 80);
            pbImage.SizeMode = PictureBoxSizeMode.StretchImage;
            pbImage.BorderStyle = BorderStyle.FixedSingle;

            yPosition += 90;

            // الأزرار
            btnSave = new Button();
            btnSave.Text = "حفظ";
            btnSave.Location = new Point(300, yPosition);
            btnSave.Size = new Size(80, 30);
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button();
            btnCancel.Text = "إلغاء";
            btnCancel.Location = new Point(200, yPosition);
            btnCancel.Size = new Size(80, 30);
            btnCancel.Click += BtnCancel_Click;

            // إضافة الكونترولز
            this.Controls.AddRange(new Control[] {
                lblBarcode, txtBarcode,
                lblName, txtName,
                lblDescription, txtDescription,
                lblSize, txtSize,
                lblColor, txtColor,
                lblSupplier, cmbSupplier,
                lblPurchasePrice, nudPurchasePrice,
                lblSalePrice, nudSalePrice,
                lblQuantity, nudQuantity,
                lblImage, btnSelectImage, pbImage,
                btnSave, btnCancel
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSuppliers()
        {
            try
            {
                cmbSupplier.Items.Clear();
                cmbSupplier.Items.Add(new { Id = 0, Name = "-- اختر المورد --" });

                var suppliers = supplierService.GetAllSuppliers();
                foreach (var supplier in suppliers)
                {
                    cmbSupplier.Items.Add(new { Id = supplier.Id, Name = supplier.Name });
                }

                cmbSupplier.DisplayMember = "Name";
                cmbSupplier.ValueMember = "Id";
                cmbSupplier.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الموردين: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProductData()
        {
            if (currentProduct != null)
            {
                txtBarcode.Text = currentProduct.Barcode;
                txtName.Text = currentProduct.Name;
                txtDescription.Text = currentProduct.Description;
                txtSize.Text = currentProduct.Size;
                txtColor.Text = currentProduct.Color;
                nudPurchasePrice.Value = currentProduct.PurchasePrice;
                nudSalePrice.Value = currentProduct.SalePrice;
                nudQuantity.Value = currentProduct.CurrentQuantity;
                selectedImagePath = currentProduct.ImagePath;

                // تحديد المورد
                if (currentProduct.SupplierId.HasValue)
                {
                    for (int i = 0; i < cmbSupplier.Items.Count; i++)
                    {
                        dynamic item = cmbSupplier.Items[i];
                        if (item.Id == currentProduct.SupplierId.Value)
                        {
                            cmbSupplier.SelectedIndex = i;
                            break;
                        }
                    }
                }

                // تحميل الصورة
                LoadImage();
            }
        }

        private void LoadImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(selectedImagePath) && System.IO.File.Exists(selectedImagePath))
                {
                    pbImage.Image = Image.FromFile(selectedImagePath);
                }
                else
                {
                    pbImage.Image = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الصورة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                pbImage.Image = null;
            }
        }

        private void BtnSelectImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            openFileDialog.Title = "اختر صورة المنتج";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedImagePath = openFileDialog.FileName;
                LoadImage();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                var product = new Product
                {
                    Barcode = txtBarcode.Text.Trim(),
                    Name = txtName.Text.Trim(),
                    Description = txtDescription.Text.Trim(),
                    Size = txtSize.Text.Trim(),
                    Color = txtColor.Text.Trim(),
                    PurchasePrice = nudPurchasePrice.Value,
                    SalePrice = nudSalePrice.Value,
                    CurrentQuantity = (int)nudQuantity.Value,
                    ImagePath = selectedImagePath ?? ""
                };

                // تحديد المورد
                dynamic selectedSupplier = cmbSupplier.SelectedItem;
                if (selectedSupplier != null && selectedSupplier.Id > 0)
                {
                    product.SupplierId = selectedSupplier.Id;
                }

                bool success;
                if (isEditMode)
                {
                    product.Id = currentProduct.Id;
                    success = productService.UpdateProduct(product);
                }
                else
                {
                    success = productService.AddProduct(product) > 0;
                }

                if (success)
                {
                    MessageBox.Show(
                        isEditMode ? "تم تعديل المنتج بنجاح" : "تم إضافة المنتج بنجاح",
                        "نجح",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        isEditMode ? "فشل في تعديل المنتج" : "فشل في إضافة المنتج",
                        "خطأ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("الباركود مطلوب", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBarcode.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("اسم المنتج مطلوب", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }

            if (nudPurchasePrice.Value <= 0)
            {
                MessageBox.Show("سعر الشراء يجب أن يكون أكبر من صفر", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudPurchasePrice.Focus();
                return false;
            }

            if (nudSalePrice.Value <= 0)
            {
                MessageBox.Show("سعر البيع يجب أن يكون أكبر من صفر", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudSalePrice.Focus();
                return false;
            }

            if (nudQuantity.Value < 0)
            {
                MessageBox.Show("الكمية لا يمكن أن تكون سالبة", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudQuantity.Focus();
                return false;
            }

            // التحقق من تكرار الباركود
            if (!isEditMode || txtBarcode.Text.Trim() != currentProduct.Barcode)
            {
                var existingProduct = productService.GetProductByBarcode(txtBarcode.Text.Trim());
                if (existingProduct != null)
                {
                    MessageBox.Show("هذا الباركود موجود مسبقاً", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtBarcode.Focus();
                    return false;
                }
            }

            return true;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}