using System;
using System.Drawing;
using System.Windows.Forms;
using ClothingStoreManager.Models;
using ClothingStoreManager.Services;

namespace ClothingStoreManager.Forms
{
    public partial class SupplierAddEditForm : Form
    {
        private SupplierService supplierService;
        private Supplier currentSupplier;
        private bool isEditMode;

        private TextBox txtName, txtPhone, txtEmail, txtAddress;
        private Button btnSave, btnCancel;

        public SupplierAddEditForm(Supplier supplier = null)
        {
            supplierService = new SupplierService();
            currentSupplier = supplier;
            isEditMode = supplier != null;
            InitializeComponent();
            
            if (isEditMode)
            {
                LoadSupplierData();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = isEditMode ? "تعديل مورد" : "إضافة مورد جديد";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            int yPosition = 20;
            int labelWidth = 120;
            int controlWidth = 200;
            int spacing = 35;

            // اسم المورد
            Label lblName = new Label();
            lblName.Text = "اسم المورد *:";
            lblName.Location = new Point(260, yPosition);
            lblName.Size = new Size(labelWidth, 23);

            txtName = new TextBox();
            txtName.Location = new Point(50, yPosition);
            txtName.Size = new Size(controlWidth, 23);

            yPosition += spacing;

            // رقم الهاتف
            Label lblPhone = new Label();
            lblPhone.Text = "رقم الهاتف:";
            lblPhone.Location = new Point(260, yPosition);
            lblPhone.Size = new Size(labelWidth, 23);

            txtPhone = new TextBox();
            txtPhone.Location = new Point(50, yPosition);
            txtPhone.Size = new Size(controlWidth, 23);

            yPosition += spacing;

            // البريد الإلكتروني
            Label lblEmail = new Label();
            lblEmail.Text = "البريد الإلكتروني:";
            lblEmail.Location = new Point(260, yPosition);
            lblEmail.Size = new Size(labelWidth, 23);

            txtEmail = new TextBox();
            txtEmail.Location = new Point(50, yPosition);
            txtEmail.Size = new Size(controlWidth, 23);

            yPosition += spacing;

            // العنوان
            Label lblAddress = new Label();
            lblAddress.Text = "العنوان:";
            lblAddress.Location = new Point(260, yPosition);
            lblAddress.Size = new Size(labelWidth, 23);

            txtAddress = new TextBox();
            txtAddress.Location = new Point(50, yPosition);
            txtAddress.Size = new Size(controlWidth, 23);
            txtAddress.Multiline = true;
            txtAddress.Height = 50;

            yPosition += 60;

            // الأزرار
            btnSave = new Button();
            btnSave.Text = "حفظ";
            btnSave.Location = new Point(200, yPosition);
            btnSave.Size = new Size(80, 30);
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button();
            btnCancel.Text = "إلغاء";
            btnCancel.Location = new Point(100, yPosition);
            btnCancel.Size = new Size(80, 30);
            btnCancel.Click += BtnCancel_Click;

            // إضافة الكونترولز
            this.Controls.AddRange(new Control[] {
                lblName, txtName,
                lblPhone, txtPhone,
                lblEmail, txtEmail,
                lblAddress, txtAddress,
                btnSave, btnCancel
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSupplierData()
        {
            if (currentSupplier != null)
            {
                txtName.Text = currentSupplier.Name;
                txtPhone.Text = currentSupplier.Phone;
                txtEmail.Text = currentSupplier.Email;
                txtAddress.Text = currentSupplier.Address;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                var supplier = new Supplier
                {
                    Name = txtName.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Address = txtAddress.Text.Trim()
                };

                bool success;
                if (isEditMode)
                {
                    supplier.Id = currentSupplier.Id;
                    success = supplierService.UpdateSupplier(supplier);
                }
                else
                {
                    success = supplierService.AddSupplier(supplier) > 0;
                }

                if (success)
                {
                    MessageBox.Show(
                        isEditMode ? "تم تعديل المورد بنجاح" : "تم إضافة المورد بنجاح",
                        "نجح",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        isEditMode ? "فشل في تعديل المورد" : "فشل في إضافة المورد",
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
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("اسم المورد مطلوب", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }

            // التحقق من صحة البريد الإلكتروني إذا تم إدخاله
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                if (!IsValidEmail(txtEmail.Text.Trim()))
                {
                    MessageBox.Show("البريد الإلكتروني غير صحيح", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}