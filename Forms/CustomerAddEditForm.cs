using System;
using System.Drawing;
using System.Windows.Forms;
using ClothingStoreManager.Models;
using ClothingStoreManager.Services;

namespace ClothingStoreManager.Forms
{
    public partial class CustomerAddEditForm : Form
    {
        private CustomerService customerService;
        private Customer currentCustomer;
        private bool isEditMode;

        private TextBox txtName, txtPhone, txtAddress;
        private Button btnSave, btnCancel;

        public CustomerAddEditForm(Customer customer = null)
        {
            customerService = new CustomerService();
            currentCustomer = customer;
            isEditMode = customer != null;
            InitializeComponent();
            
            if (isEditMode)
            {
                LoadCustomerData();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = isEditMode ? "تعديل عميل" : "إضافة عميل جديد";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            int yPosition = 20;
            int labelWidth = 100;
            int controlWidth = 200;
            int spacing = 35;

            // اسم العميل
            Label lblName = new Label();
            lblName.Text = "اسم العميل *:";
            lblName.Location = new Point(280, yPosition);
            lblName.Size = new Size(labelWidth, 23);

            txtName = new TextBox();
            txtName.Location = new Point(50, yPosition);
            txtName.Size = new Size(controlWidth, 23);

            yPosition += spacing;

            // رقم الهاتف
            Label lblPhone = new Label();
            lblPhone.Text = "رقم الهاتف:";
            lblPhone.Location = new Point(280, yPosition);
            lblPhone.Size = new Size(labelWidth, 23);

            txtPhone = new TextBox();
            txtPhone.Location = new Point(50, yPosition);
            txtPhone.Size = new Size(controlWidth, 23);

            yPosition += spacing;

            // العنوان
            Label lblAddress = new Label();
            lblAddress.Text = "العنوان:";
            lblAddress.Location = new Point(280, yPosition);
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
                lblAddress, txtAddress,
                btnSave, btnCancel
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadCustomerData()
        {
            if (currentCustomer != null)
            {
                txtName.Text = currentCustomer.Name;
                txtPhone.Text = currentCustomer.Phone;
                txtAddress.Text = currentCustomer.Address;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                var customer = new Customer
                {
                    Name = txtName.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Address = txtAddress.Text.Trim()
                };

                bool success;
                if (isEditMode)
                {
                    customer.Id = currentCustomer.Id;
                    success = customerService.UpdateCustomer(customer);
                }
                else
                {
                    success = customerService.AddCustomer(customer) > 0;
                }

                if (success)
                {
                    MessageBox.Show(
                        isEditMode ? "تم تعديل العميل بنجاح" : "تم إضافة العميل بنجاح",
                        "نجح",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        isEditMode ? "فشل في تعديل العميل" : "فشل في إضافة العميل",
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
                MessageBox.Show("اسم العميل مطلوب", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
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