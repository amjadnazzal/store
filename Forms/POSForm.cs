using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClothingStoreManager.Models;
using ClothingStoreManager.Services;

namespace ClothingStoreManager.Forms
{
    public partial class POSForm : Form
    {
        private ProductService productService;
        private CustomerService customerService;
        private InvoiceService invoiceService;

        private TextBox txtBarcode;
        private ComboBox cmbCustomer;
        private DataGridView dgvInvoiceItems;
        private Label lblSubtotal, lblDiscount, lblTotal;
        private NumericUpDown nudDiscountPercent, nudDiscountAmount;
        private TextBox txtCustomerPhone;
        private Button btnAddCustomer, btnPay, btnClear, btnRemoveItem;

        private List<InvoiceItem> currentInvoiceItems;
        private decimal subtotal = 0;
        private decimal discountAmount = 0;
        private decimal finalTotal = 0;

        public POSForm()
        {
            productService = new ProductService();
            customerService = new CustomerService();
            invoiceService = new InvoiceService();
            currentInvoiceItems = new List<InvoiceItem>();
            
            InitializeComponent();
            LoadCustomers();
            UpdateTotals();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = "نقطة البيع";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // منطقة البحث والباركود
            GroupBox gbProduct = new GroupBox();
            gbProduct.Text = "إضافة منتج";
            gbProduct.Location = new Point(20, 20);
            gbProduct.Size = new Size(400, 80);

            Label lblBarcode = new Label();
            lblBarcode.Text = "الباركود:";
            lblBarcode.Location = new Point(320, 25);
            lblBarcode.AutoSize = true;

            txtBarcode = new TextBox();
            txtBarcode.Location = new Point(20, 23);
            txtBarcode.Size = new Size(280, 23);
            txtBarcode.KeyPress += TxtBarcode_KeyPress;

            gbProduct.Controls.Add(lblBarcode);
            gbProduct.Controls.Add(txtBarcode);

            // منطقة العميل
            GroupBox gbCustomer = new GroupBox();
            gbCustomer.Text = "معلومات العميل";
            gbCustomer.Location = new Point(450, 20);
            gbCustomer.Size = new Size(400, 80);

            Label lblCustomerPhone = new Label();
            lblCustomerPhone.Text = "رقم الهاتف:";
            lblCustomerPhone.Location = new Point(320, 25);
            lblCustomerPhone.AutoSize = true;

            txtCustomerPhone = new TextBox();
            txtCustomerPhone.Location = new Point(150, 23);
            txtCustomerPhone.Size = new Size(150, 23);
            txtCustomerPhone.KeyPress += TxtCustomerPhone_KeyPress;

            btnAddCustomer = new Button();
            btnAddCustomer.Text = "إضافة";
            btnAddCustomer.Location = new Point(20, 22);
            btnAddCustomer.Size = new Size(60, 25);
            btnAddCustomer.Click += BtnAddCustomer_Click;

            cmbCustomer = new ComboBox();
            cmbCustomer.Location = new Point(20, 50);
            cmbCustomer.Size = new Size(280, 23);
            cmbCustomer.DropDownStyle = ComboBoxStyle.DropDownList;

            gbCustomer.Controls.Add(lblCustomerPhone);
            gbCustomer.Controls.Add(txtCustomerPhone);
            gbCustomer.Controls.Add(btnAddCustomer);
            gbCustomer.Controls.Add(cmbCustomer);

            // جدول عناصر الفاتورة
            dgvInvoiceItems = new DataGridView();
            dgvInvoiceItems.Location = new Point(20, 120);
            dgvInvoiceItems.Size = new Size(830, 350);
            dgvInvoiceItems.AllowUserToAddRows = false;
            dgvInvoiceItems.AllowUserToDeleteRows = false;
            dgvInvoiceItems.ReadOnly = false;
            dgvInvoiceItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInvoiceItems.MultiSelect = false;
            dgvInvoiceItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvInvoiceItems.CellEndEdit += DgvInvoiceItems_CellEndEdit;

            // إضافة الأعمدة
            dgvInvoiceItems.Columns.Add("ProductId", "رقم المنتج");
            dgvInvoiceItems.Columns.Add("Barcode", "الباركود");
            dgvInvoiceItems.Columns.Add("ProductName", "اسم المنتج");
            dgvInvoiceItems.Columns.Add("UnitPrice", "سعر الوحدة");
            dgvInvoiceItems.Columns.Add("Quantity", "الكمية");
            dgvInvoiceItems.Columns.Add("TotalPrice", "المجموع");

            // إخفاء عمود رقم المنتج
            dgvInvoiceItems.Columns["ProductId"].Visible = false;

            // جعل عمود الكمية قابل للتعديل فقط
            dgvInvoiceItems.Columns["Barcode"].ReadOnly = true;
            dgvInvoiceItems.Columns["ProductName"].ReadOnly = true;
            dgvInvoiceItems.Columns["UnitPrice"].ReadOnly = true;
            dgvInvoiceItems.Columns["TotalPrice"].ReadOnly = true;

            // تنسيق الأعمدة
            dgvInvoiceItems.Columns["UnitPrice"].DefaultCellStyle.Format = "C2";
            dgvInvoiceItems.Columns["TotalPrice"].DefaultCellStyle.Format = "C2";

            // منطقة الحسابات
            GroupBox gbTotals = new GroupBox();
            gbTotals.Text = "المجاميع";
            gbTotals.Location = new Point(870, 120);
            gbTotals.Size = new Size(300, 350);

            Label lblSubtotalLabel = new Label();
            lblSubtotalLabel.Text = "المجموع الفرعي:";
            lblSubtotalLabel.Location = new Point(200, 30);
            lblSubtotalLabel.AutoSize = true;

            lblSubtotal = new Label();
            lblSubtotal.Text = "0.00";
            lblSubtotal.Location = new Point(20, 30);
            lblSubtotal.Size = new Size(100, 23);
            lblSubtotal.Font = new Font("Arial", 12, FontStyle.Bold);

            Label lblDiscountLabel = new Label();
            lblDiscountLabel.Text = "الخصم %:";
            lblDiscountLabel.Location = new Point(200, 70);
            lblDiscountLabel.AutoSize = true;

            nudDiscountPercent = new NumericUpDown();
            nudDiscountPercent.Location = new Point(100, 68);
            nudDiscountPercent.Size = new Size(80, 23);
            nudDiscountPercent.DecimalPlaces = 2;
            nudDiscountPercent.Maximum = 100;
            nudDiscountPercent.ValueChanged += NudDiscount_ValueChanged;

            Label lblDiscountAmountLabel = new Label();
            lblDiscountAmountLabel.Text = "مبلغ الخصم:";
            lblDiscountAmountLabel.Location = new Point(200, 110);
            lblDiscountAmountLabel.AutoSize = true;

            nudDiscountAmount = new NumericUpDown();
            nudDiscountAmount.Location = new Point(20, 108);
            nudDiscountAmount.Size = new Size(120, 23);
            nudDiscountAmount.DecimalPlaces = 2;
            nudDiscountAmount.Maximum = 999999;
            nudDiscountAmount.ValueChanged += NudDiscountAmount_ValueChanged;

            Label lblTotalLabel = new Label();
            lblTotalLabel.Text = "المجموع الإجمالي:";
            lblTotalLabel.Location = new Point(180, 150);
            lblTotalLabel.AutoSize = true;

            lblTotal = new Label();
            lblTotal.Text = "0.00";
            lblTotal.Location = new Point(20, 150);
            lblTotal.Size = new Size(120, 23);
            lblTotal.Font = new Font("Arial", 14, FontStyle.Bold);
            lblTotal.ForeColor = Color.DarkGreen;

            // الأزرار
            btnRemoveItem = new Button();
            btnRemoveItem.Text = "حذف العنصر";
            btnRemoveItem.Location = new Point(20, 200);
            btnRemoveItem.Size = new Size(100, 30);
            btnRemoveItem.Click += BtnRemoveItem_Click;

            btnClear = new Button();
            btnClear.Text = "مسح الكل";
            btnClear.Location = new Point(140, 200);
            btnClear.Size = new Size(100, 30);
            btnClear.Click += BtnClear_Click;

            btnPay = new Button();
            btnPay.Text = "دفع";
            btnPay.Location = new Point(20, 280);
            btnPay.Size = new Size(220, 50);
            btnPay.Font = new Font("Arial", 14, FontStyle.Bold);
            btnPay.BackColor = Color.Green;
            btnPay.ForeColor = Color.White;
            btnPay.Click += BtnPay_Click;

            gbTotals.Controls.Add(lblSubtotalLabel);
            gbTotals.Controls.Add(lblSubtotal);
            gbTotals.Controls.Add(lblDiscountLabel);
            gbTotals.Controls.Add(nudDiscountPercent);
            gbTotals.Controls.Add(lblDiscountAmountLabel);
            gbTotals.Controls.Add(nudDiscountAmount);
            gbTotals.Controls.Add(lblTotalLabel);
            gbTotals.Controls.Add(lblTotal);
            gbTotals.Controls.Add(btnRemoveItem);
            gbTotals.Controls.Add(btnClear);
            gbTotals.Controls.Add(btnPay);

            // إضافة الكونترولز للنافذة
            this.Controls.Add(gbProduct);
            this.Controls.Add(gbCustomer);
            this.Controls.Add(dgvInvoiceItems);
            this.Controls.Add(gbTotals);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadCustomers()
        {
            try
            {
                cmbCustomer.Items.Clear();
                cmbCustomer.Items.Add(new { Id = 0, Name = "-- عميل عام --" });

                var customers = customerService.GetAllCustomers();
                foreach (var customer in customers)
                {
                    cmbCustomer.Items.Add(new { Id = customer.Id, Name = $"{customer.Name} - {customer.Phone}" });
                }

                cmbCustomer.DisplayMember = "Name";
                cmbCustomer.ValueMember = "Id";
                cmbCustomer.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل العملاء: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                AddProductByBarcode(txtBarcode.Text.Trim());
                txtBarcode.Clear();
                txtBarcode.Focus();
            }
        }

        private void TxtCustomerPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SearchCustomerByPhone();
            }
        }

        private void SearchCustomerByPhone()
        {
            try
            {
                string phone = txtCustomerPhone.Text.Trim();
                if (!string.IsNullOrEmpty(phone))
                {
                    var customer = customerService.GetCustomerByPhone(phone);
                    if (customer != null)
                    {
                        // تحديد العميل في القائمة
                        for (int i = 0; i < cmbCustomer.Items.Count; i++)
                        {
                            dynamic item = cmbCustomer.Items[i];
                            if (item.Id == customer.Id)
                            {
                                cmbCustomer.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("العميل غير موجود. يمكنك إضافته كعميل جديد.", "عميل غير موجود", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في البحث عن العميل: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddProductByBarcode(string barcode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barcode))
                    return;

                var product = productService.GetProductByBarcode(barcode);
                if (product == null)
                {
                    MessageBox.Show("المنتج غير موجود", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (product.CurrentQuantity <= 0)
                {
                    MessageBox.Show("المنتج غير متوفر في المخزون", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // التحقق من وجود المنتج في الفاتورة الحالية
                var existingItem = currentInvoiceItems.FirstOrDefault(x => x.ProductId == product.Id);
                if (existingItem != null)
                {
                    // زيادة الكمية
                    if (existingItem.Quantity < product.CurrentQuantity)
                    {
                        existingItem.Quantity++;
                        existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
                    }
                    else
                    {
                        MessageBox.Show("الكمية المطلوبة تتجاوز المتوفر في المخزون", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    // إضافة منتج جديد
                    var newItem = new InvoiceItem
                    {
                        ProductId = product.Id,
                        ProductBarcode = product.Barcode,
                        ProductName = product.Name,
                        UnitPrice = product.SalePrice,
                        Quantity = 1,
                        TotalPrice = product.SalePrice
                    };
                    currentInvoiceItems.Add(newItem);
                }

                RefreshInvoiceGrid();
                UpdateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إضافة المنتج: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshInvoiceGrid()
        {
            dgvInvoiceItems.Rows.Clear();
            foreach (var item in currentInvoiceItems)
            {
                dgvInvoiceItems.Rows.Add(
                    item.ProductId,
                    item.ProductBarcode,
                    item.ProductName,
                    item.UnitPrice,
                    item.Quantity,
                    item.TotalPrice
                );
            }
        }

        private void UpdateTotals()
        {
            subtotal = currentInvoiceItems.Sum(x => x.TotalPrice);
            
            // حساب الخصم
            if (nudDiscountPercent.Value > 0)
            {
                discountAmount = subtotal * (nudDiscountPercent.Value / 100);
                nudDiscountAmount.Value = discountAmount;
            }
            else
            {
                discountAmount = nudDiscountAmount.Value;
            }

            finalTotal = subtotal - discountAmount;

            lblSubtotal.Text = subtotal.ToString("C2");
            lblTotal.Text = finalTotal.ToString("C2");
        }

        private void DgvInvoiceItems_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvInvoiceItems.Columns["Quantity"].Index)
            {
                try
                {
                    int newQuantity = Convert.ToInt32(dgvInvoiceItems.Rows[e.RowIndex].Cells["Quantity"].Value);
                    int productId = Convert.ToInt32(dgvInvoiceItems.Rows[e.RowIndex].Cells["ProductId"].Value);
                    
                    var product = productService.GetProductById(productId);
                    if (product == null || newQuantity > product.CurrentQuantity || newQuantity <= 0)
                    {
                        MessageBox.Show("كمية غير صحيحة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        RefreshInvoiceGrid();
                        return;
                    }

                    var item = currentInvoiceItems.FirstOrDefault(x => x.ProductId == productId);
                    if (item != null)
                    {
                        item.Quantity = newQuantity;
                        item.TotalPrice = item.Quantity * item.UnitPrice;
                        RefreshInvoiceGrid();
                        UpdateTotals();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في تحديث الكمية: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    RefreshInvoiceGrid();
                }
            }
        }

        private void NudDiscount_ValueChanged(object sender, EventArgs e)
        {
            if (nudDiscountPercent.Value > 0)
            {
                nudDiscountAmount.ValueChanged -= NudDiscountAmount_ValueChanged;
                UpdateTotals();
                nudDiscountAmount.ValueChanged += NudDiscountAmount_ValueChanged;
            }
            else
            {
                UpdateTotals();
            }
        }

        private void NudDiscountAmount_ValueChanged(object sender, EventArgs e)
        {
            nudDiscountPercent.ValueChanged -= NudDiscount_ValueChanged;
            nudDiscountPercent.Value = 0;
            nudDiscountPercent.ValueChanged += NudDiscount_ValueChanged;
            UpdateTotals();
        }

        private void BtnAddCustomer_Click(object sender, EventArgs e)
        {
            var customerForm = new CustomerAddEditForm();
            if (customerForm.ShowDialog() == DialogResult.OK)
            {
                LoadCustomers();
            }
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvInvoiceItems.SelectedRows.Count > 0)
            {
                int productId = Convert.ToInt32(dgvInvoiceItems.SelectedRows[0].Cells["ProductId"].Value);
                currentInvoiceItems.RemoveAll(x => x.ProductId == productId);
                RefreshInvoiceGrid();
                UpdateTotals();
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (currentInvoiceItems.Count > 0)
            {
                var result = MessageBox.Show("هل أنت متأكد من مسح جميع العناصر؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    currentInvoiceItems.Clear();
                    RefreshInvoiceGrid();
                    UpdateTotals();
                    nudDiscountPercent.Value = 0;
                    nudDiscountAmount.Value = 0;
                }
            }
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            if (currentInvoiceItems.Count == 0)
            {
                MessageBox.Show("لا توجد عناصر في الفاتورة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var paymentForm = new PaymentForm(finalTotal);
            if (paymentForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // إنشاء الفاتورة
                    var invoice = new Invoice
                    {
                        InvoiceNumber = invoiceService.GenerateInvoiceNumber(),
                        TotalAmount = subtotal,
                        DiscountAmount = discountAmount,
                        FinalAmount = finalTotal,
                        PaymentMethod = paymentForm.PaymentMethod,
                        Items = new List<InvoiceItem>(currentInvoiceItems)
                    };

                    // تحديد العميل
                    dynamic selectedCustomer = cmbCustomer.SelectedItem;
                    if (selectedCustomer != null && selectedCustomer.Id > 0)
                    {
                        invoice.CustomerId = selectedCustomer.Id;
                    }

                    // حفظ الفاتورة
                    int invoiceId = invoiceService.CreateInvoice(invoice);
                    
                    MessageBox.Show($"تم إنشاء الفاتورة بنجاح\nرقم الفاتورة: {invoice.InvoiceNumber}", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // مسح الفاتورة الحالية
                    currentInvoiceItems.Clear();
                    RefreshInvoiceGrid();
                    UpdateTotals();
                    nudDiscountPercent.Value = 0;
                    nudDiscountAmount.Value = 0;
                    cmbCustomer.SelectedIndex = 0;
                    txtCustomerPhone.Clear();
                    txtBarcode.Focus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في إنشاء الفاتورة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}