using System;
using System.Drawing;
using System.Windows.Forms;
using ClothingStoreManager.Models;
using ClothingStoreManager.Services;

namespace ClothingStoreManager.Forms
{
    public partial class CustomersForm : Form
    {
        private CustomerService customerService;
        private DataGridView dgvCustomers;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private Customer selectedCustomer;

        public CustomersForm()
        {
            customerService = new CustomerService();
            InitializeComponent();
            LoadCustomers();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = "إدارة العملاء";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // شريط البحث
            Label lblSearch = new Label();
            lblSearch.Text = "البحث:";
            lblSearch.Location = new Point(700, 20);
            lblSearch.AutoSize = true;

            txtSearch = new TextBox();
            txtSearch.Location = new Point(450, 18);
            txtSearch.Size = new Size(200, 23);
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // الأزرار
            btnAdd = new Button();
            btnAdd.Text = "إضافة عميل";
            btnAdd.Location = new Point(350, 18);
            btnAdd.Size = new Size(80, 25);
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button();
            btnEdit.Text = "تعديل";
            btnEdit.Location = new Point(265, 18);
            btnEdit.Size = new Size(80, 25);
            btnEdit.Click += BtnEdit_Click;
            btnEdit.Enabled = false;

            btnDelete = new Button();
            btnDelete.Text = "حذف";
            btnDelete.Location = new Point(180, 18);
            btnDelete.Size = new Size(80, 25);
            btnDelete.Click += BtnDelete_Click;
            btnDelete.Enabled = false;

            btnRefresh = new Button();
            btnRefresh.Text = "تحديث";
            btnRefresh.Location = new Point(95, 18);
            btnRefresh.Size = new Size(80, 25);
            btnRefresh.Click += BtnRefresh_Click;

            // DataGridView للعملاء
            dgvCustomers = new DataGridView();
            dgvCustomers.Location = new Point(20, 60);
            dgvCustomers.Size = new Size(750, 480);
            dgvCustomers.AllowUserToAddRows = false;
            dgvCustomers.AllowUserToDeleteRows = false;
            dgvCustomers.ReadOnly = true;
            dgvCustomers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCustomers.MultiSelect = false;
            dgvCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCustomers.SelectionChanged += DgvCustomers_SelectionChanged;

            // إضافة الأعمدة
            dgvCustomers.Columns.Add("Id", "الرقم");
            dgvCustomers.Columns.Add("Name", "اسم العميل");
            dgvCustomers.Columns.Add("Phone", "رقم الهاتف");
            dgvCustomers.Columns.Add("Address", "العنوان");
            dgvCustomers.Columns.Add("CreatedDate", "تاريخ الإضافة");

            // إخفاء عمود الرقم
            dgvCustomers.Columns["Id"].Visible = false;

            // تنسيق عمود التاريخ
            dgvCustomers.Columns["CreatedDate"].DefaultCellStyle.Format = "yyyy/MM/dd";

            // إضافة الكونترولز للنافذة
            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnEdit);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(dgvCustomers);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadCustomers()
        {
            try
            {
                dgvCustomers.Rows.Clear();
                var customers = customerService.GetAllCustomers();

                foreach (var customer in customers)
                {
                    dgvCustomers.Rows.Add(
                        customer.Id,
                        customer.Name,
                        customer.Phone,
                        customer.Address,
                        customer.CreatedDate
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل العملاء: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                LoadCustomers();
                return;
            }

            try
            {
                dgvCustomers.Rows.Clear();
                var customers = customerService.SearchCustomers(txtSearch.Text);

                foreach (var customer in customers)
                {
                    dgvCustomers.Rows.Add(
                        customer.Id,
                        customer.Name,
                        customer.Phone,
                        customer.Address,
                        customer.CreatedDate
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count > 0)
            {
                int customerId = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells["Id"].Value);
                selectedCustomer = customerService.GetCustomerById(customerId);
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
            }
            else
            {
                selectedCustomer = null;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var addForm = new CustomerAddEditForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadCustomers();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (selectedCustomer != null)
            {
                var editForm = new CustomerAddEditForm(selectedCustomer);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadCustomers();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedCustomer != null)
            {
                var result = MessageBox.Show(
                    $"هل أنت متأكد من حذف العميل '{selectedCustomer.Name}'؟\nسيتم حذف جميع البيانات المرتبطة به.",
                    "تأكيد الحذف",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        if (customerService.DeleteCustomer(selectedCustomer.Id))
                        {
                            MessageBox.Show("تم حذف العميل بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadCustomers();
                        }
                        else
                        {
                            MessageBox.Show("فشل في حذف العميل", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في حذف العميل: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadCustomers();
        }
    }
}