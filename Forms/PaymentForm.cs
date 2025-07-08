using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClothingStoreManager.Forms
{
    public partial class PaymentForm : Form
    {
        private decimal totalAmount;
        private RadioButton rbCash, rbCard;
        private NumericUpDown nudPaidAmount;
        private Label lblChange, lblChangeAmount;
        private Button btnConfirm, btnCancel;

        public string PaymentMethod { get; private set; }
        public decimal PaidAmount { get; private set; }
        public decimal ChangeAmount { get; private set; }

        public PaymentForm(decimal total)
        {
            totalAmount = total;
            InitializeComponent();
            nudPaidAmount.Value = totalAmount;
            CalculateChange();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = "طريقة الدفع";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // عرض المبلغ الإجمالي
            Label lblTotal = new Label();
            lblTotal.Text = $"المبلغ الإجمالي: {totalAmount:C2}";
            lblTotal.Location = new Point(50, 20);
            lblTotal.Size = new Size(300, 25);
            lblTotal.Font = new Font("Arial", 12, FontStyle.Bold);
            lblTotal.ForeColor = Color.DarkBlue;

            // طرق الدفع
            GroupBox gbPaymentMethod = new GroupBox();
            gbPaymentMethod.Text = "طريقة الدفع";
            gbPaymentMethod.Location = new Point(50, 60);
            gbPaymentMethod.Size = new Size(300, 60);

            rbCash = new RadioButton();
            rbCash.Text = "نقدي";
            rbCash.Location = new Point(200, 25);
            rbCash.Checked = true;
            rbCash.CheckedChanged += PaymentMethod_CheckedChanged;

            rbCard = new RadioButton();
            rbCard.Text = "بطاقة";
            rbCard.Location = new Point(100, 25);
            rbCard.CheckedChanged += PaymentMethod_CheckedChanged;

            gbPaymentMethod.Controls.Add(rbCash);
            gbPaymentMethod.Controls.Add(rbCard);

            // المبلغ المدفوع
            Label lblPaidAmount = new Label();
            lblPaidAmount.Text = "المبلغ المدفوع:";
            lblPaidAmount.Location = new Point(250, 140);
            lblPaidAmount.AutoSize = true;

            nudPaidAmount = new NumericUpDown();
            nudPaidAmount.Location = new Point(50, 138);
            nudPaidAmount.Size = new Size(150, 23);
            nudPaidAmount.DecimalPlaces = 2;
            nudPaidAmount.Maximum = 999999;
            nudPaidAmount.ValueChanged += NudPaidAmount_ValueChanged;

            // الباقي
            lblChange = new Label();
            lblChange.Text = "الباقي:";
            lblChange.Location = new Point(250, 180);
            lblChange.AutoSize = true;

            lblChangeAmount = new Label();
            lblChangeAmount.Text = "0.00";
            lblChangeAmount.Location = new Point(50, 180);
            lblChangeAmount.Size = new Size(150, 23);
            lblChangeAmount.Font = new Font("Arial", 12, FontStyle.Bold);
            lblChangeAmount.ForeColor = Color.DarkGreen;

            // الأزرار
            btnConfirm = new Button();
            btnConfirm.Text = "تأكيد الدفع";
            btnConfirm.Location = new Point(200, 220);
            btnConfirm.Size = new Size(100, 30);
            btnConfirm.BackColor = Color.Green;
            btnConfirm.ForeColor = Color.White;
            btnConfirm.Click += BtnConfirm_Click;

            btnCancel = new Button();
            btnCancel.Text = "إلغاء";
            btnCancel.Location = new Point(90, 220);
            btnCancel.Size = new Size(100, 30);
            btnCancel.Click += BtnCancel_Click;

            // إضافة الكونترولز
            this.Controls.Add(lblTotal);
            this.Controls.Add(gbPaymentMethod);
            this.Controls.Add(lblPaidAmount);
            this.Controls.Add(nudPaidAmount);
            this.Controls.Add(lblChange);
            this.Controls.Add(lblChangeAmount);
            this.Controls.Add(btnConfirm);
            this.Controls.Add(btnCancel);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void PaymentMethod_CheckedChanged(object sender, EventArgs e)
        {
            if (rbCard.Checked)
            {
                // في حالة البطاقة، المبلغ المدفوع يساوي المجموع تماماً
                nudPaidAmount.Value = totalAmount;
                nudPaidAmount.Enabled = false;
            }
            else
            {
                // في حالة النقد، يمكن تعديل المبلغ المدفوع
                nudPaidAmount.Enabled = true;
            }
            CalculateChange();
        }

        private void NudPaidAmount_ValueChanged(object sender, EventArgs e)
        {
            CalculateChange();
        }

        private void CalculateChange()
        {
            ChangeAmount = nudPaidAmount.Value - totalAmount;
            lblChangeAmount.Text = ChangeAmount.ToString("C2");
            
            if (ChangeAmount < 0)
            {
                lblChangeAmount.ForeColor = Color.Red;
                btnConfirm.Enabled = false;
            }
            else
            {
                lblChangeAmount.ForeColor = Color.DarkGreen;
                btnConfirm.Enabled = true;
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (nudPaidAmount.Value < totalAmount)
            {
                MessageBox.Show("المبلغ المدفوع أقل من المطلوب", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PaymentMethod = rbCash.Checked ? "نقدي" : "بطاقة";
            PaidAmount = nudPaidAmount.Value;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}