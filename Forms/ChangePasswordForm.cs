using System;
using System.Drawing;
using System.Windows.Forms;
using ClothingStoreManager.Services;

namespace ClothingStoreManager.Forms
{
    public partial class ChangePasswordForm : Form
    {
        private UserService userService;
        private TextBox txtOldPassword, txtNewPassword, txtConfirmPassword;
        private Button btnChange, btnCancel;
        private Label lblOldPassword, lblNewPassword, lblConfirmPassword;

        public ChangePasswordForm()
        {
            userService = new UserService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = "تغيير كلمة المرور";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // كلمة المرور الحالية
            lblOldPassword = new Label();
            lblOldPassword.Text = "كلمة المرور الحالية:";
            lblOldPassword.Location = new Point(250, 30);
            lblOldPassword.Size = new Size(120, 23);
            lblOldPassword.Font = new Font("Arial", 10);

            txtOldPassword = new TextBox();
            txtOldPassword.Location = new Point(50, 28);
            txtOldPassword.Size = new Size(180, 25);
            txtOldPassword.UseSystemPasswordChar = true;
            txtOldPassword.Font = new Font("Arial", 10);

            // كلمة المرور الجديدة
            lblNewPassword = new Label();
            lblNewPassword.Text = "كلمة المرور الجديدة:";
            lblNewPassword.Location = new Point(250, 70);
            lblNewPassword.Size = new Size(120, 23);
            lblNewPassword.Font = new Font("Arial", 10);

            txtNewPassword = new TextBox();
            txtNewPassword.Location = new Point(50, 68);
            txtNewPassword.Size = new Size(180, 25);
            txtNewPassword.UseSystemPasswordChar = true;
            txtNewPassword.Font = new Font("Arial", 10);

            // تأكيد كلمة المرور
            lblConfirmPassword = new Label();
            lblConfirmPassword.Text = "تأكيد كلمة المرور:";
            lblConfirmPassword.Location = new Point(250, 110);
            lblConfirmPassword.Size = new Size(120, 23);
            lblConfirmPassword.Font = new Font("Arial", 10);

            txtConfirmPassword = new TextBox();
            txtConfirmPassword.Location = new Point(50, 108);
            txtConfirmPassword.Size = new Size(180, 25);
            txtConfirmPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.Font = new Font("Arial", 10);

            // الأزرار
            btnChange = new Button();
            btnChange.Text = "تغيير";
            btnChange.Location = new Point(160, 160);
            btnChange.Size = new Size(80, 35);
            btnChange.BackColor = Color.Green;
            btnChange.ForeColor = Color.White;
            btnChange.Font = new Font("Arial", 10, FontStyle.Bold);
            btnChange.FlatStyle = FlatStyle.Flat;
            btnChange.Click += BtnChange_Click;

            btnCancel = new Button();
            btnCancel.Text = "إلغاء";
            btnCancel.Location = new Point(70, 160);
            btnCancel.Size = new Size(80, 35);
            btnCancel.BackColor = Color.Gray;
            btnCancel.ForeColor = Color.White;
            btnCancel.Font = new Font("Arial", 10, FontStyle.Bold);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Click += BtnCancel_Click;

            // إضافة العناصر للنافذة
            this.Controls.Add(lblOldPassword);
            this.Controls.Add(txtOldPassword);
            this.Controls.Add(lblNewPassword);
            this.Controls.Add(txtNewPassword);
            this.Controls.Add(lblConfirmPassword);
            this.Controls.Add(txtConfirmPassword);
            this.Controls.Add(btnChange);
            this.Controls.Add(btnCancel);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void BtnChange_Click(object sender, EventArgs e)
        {
            string oldPassword = txtOldPassword.Text;
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            // التحقق من الحقول
            if (string.IsNullOrEmpty(oldPassword))
            {
                MessageBox.Show("يرجى إدخال كلمة المرور الحالية", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOldPassword.Focus();
                return;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("يرجى إدخال كلمة المرور الجديدة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("كلمة المرور يجب أن تكون 6 أحرف على الأقل", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("كلمة المرور الجديدة وتأكيدها غير متطابقان", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            try
            {
                btnChange.Enabled = false;
                btnChange.Text = "جاري التحديث...";

                var currentUser = UserService.CurrentUser;
                if (currentUser == null)
                {
                    MessageBox.Show("لم يتم العثور على المستخدم الحالي", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool success = userService.ChangePassword(currentUser.Id, oldPassword, newPassword);

                if (success)
                {
                    MessageBox.Show("تم تغيير كلمة المرور بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("كلمة المرور الحالية غير صحيحة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtOldPassword.Clear();
                    txtOldPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تغيير كلمة المرور: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnChange.Enabled = true;
                btnChange.Text = "تغيير";
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}