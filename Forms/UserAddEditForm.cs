using System;
using System.Drawing;
using System.Windows.Forms;
using ClothingStoreManager.Models;
using ClothingStoreManager.Services;

namespace ClothingStoreManager.Forms
{
    public partial class UserAddEditForm : Form
    {
        private UserService userService;
        private User editingUser;
        private bool isEditing;

        private TextBox txtUsername, txtFullName, txtEmail, txtPhone, txtPassword, txtConfirmPassword;
        private ComboBox cmbRole;
        private CheckBox chkIsActive;
        private Button btnSave, btnCancel;
        private Label lblUsername, lblFullName, lblEmail, lblPhone, lblPassword, lblConfirmPassword, lblRole, lblStatus;

        public UserAddEditForm(User user = null)
        {
            userService = new UserService();
            editingUser = user;
            isEditing = user != null;
            InitializeComponent();
            
            if (isEditing)
            {
                LoadUserData();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = isEditing ? "تعديل مستخدم" : "إضافة مستخدم جديد";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // اسم المستخدم
            lblUsername = new Label();
            lblUsername.Text = "اسم المستخدم:";
            lblUsername.Location = new Point(350, 30);
            lblUsername.Size = new Size(100, 23);
            lblUsername.Font = new Font("Arial", 10);

            txtUsername = new TextBox();
            txtUsername.Location = new Point(150, 28);
            txtUsername.Size = new Size(180, 25);
            txtUsername.Font = new Font("Arial", 10);

            // الاسم الكامل
            lblFullName = new Label();
            lblFullName.Text = "الاسم الكامل:";
            lblFullName.Location = new Point(350, 70);
            lblFullName.Size = new Size(100, 23);
            lblFullName.Font = new Font("Arial", 10);

            txtFullName = new TextBox();
            txtFullName.Location = new Point(150, 68);
            txtFullName.Size = new Size(180, 25);
            txtFullName.Font = new Font("Arial", 10);

            // البريد الإلكتروني
            lblEmail = new Label();
            lblEmail.Text = "البريد الإلكتروني:";
            lblEmail.Location = new Point(350, 110);
            lblEmail.Size = new Size(100, 23);
            lblEmail.Font = new Font("Arial", 10);

            txtEmail = new TextBox();
            txtEmail.Location = new Point(150, 108);
            txtEmail.Size = new Size(180, 25);
            txtEmail.Font = new Font("Arial", 10);

            // الهاتف
            lblPhone = new Label();
            lblPhone.Text = "الهاتف:";
            lblPhone.Location = new Point(350, 150);
            lblPhone.Size = new Size(100, 23);
            lblPhone.Font = new Font("Arial", 10);

            txtPhone = new TextBox();
            txtPhone.Location = new Point(150, 148);
            txtPhone.Size = new Size(180, 25);
            txtPhone.Font = new Font("Arial", 10);

            // كلمة المرور
            lblPassword = new Label();
            lblPassword.Text = isEditing ? "كلمة مرور جديدة:" : "كلمة المرور:";
            lblPassword.Location = new Point(350, 190);
            lblPassword.Size = new Size(120, 23);
            lblPassword.Font = new Font("Arial", 10);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(150, 188);
            txtPassword.Size = new Size(180, 25);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Font = new Font("Arial", 10);

            // تأكيد كلمة المرور
            lblConfirmPassword = new Label();
            lblConfirmPassword.Text = "تأكيد كلمة المرور:";
            lblConfirmPassword.Location = new Point(350, 230);
            lblConfirmPassword.Size = new Size(120, 23);
            lblConfirmPassword.Font = new Font("Arial", 10);

            txtConfirmPassword = new TextBox();
            txtConfirmPassword.Location = new Point(150, 228);
            txtConfirmPassword.Size = new Size(180, 25);
            txtConfirmPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.Font = new Font("Arial", 10);

            // الصلاحية
            lblRole = new Label();
            lblRole.Text = "الصلاحية:";
            lblRole.Location = new Point(350, 270);
            lblRole.Size = new Size(100, 23);
            lblRole.Font = new Font("Arial", 10);

            cmbRole = new ComboBox();
            cmbRole.Location = new Point(150, 268);
            cmbRole.Size = new Size(180, 25);
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.Font = new Font("Arial", 10);
            cmbRole.Items.Add("مدير النظام");
            cmbRole.Items.Add("مدير المتجر");
            cmbRole.Items.Add("كاشير");
            cmbRole.Items.Add("موظف");

            // الحالة
            lblStatus = new Label();
            lblStatus.Text = "الحالة:";
            lblStatus.Location = new Point(350, 310);
            lblStatus.Size = new Size(100, 23);
            lblStatus.Font = new Font("Arial", 10);

            chkIsActive = new CheckBox();
            chkIsActive.Text = "نشط";
            chkIsActive.Location = new Point(150, 310);
            chkIsActive.Size = new Size(60, 23);
            chkIsActive.Font = new Font("Arial", 10);
            chkIsActive.Checked = true;

            // الأزرار
            btnSave = new Button();
            btnSave.Text = "حفظ";
            btnSave.Location = new Point(260, 360);
            btnSave.Size = new Size(80, 35);
            btnSave.BackColor = Color.Green;
            btnSave.ForeColor = Color.White;
            btnSave.Font = new Font("Arial", 10, FontStyle.Bold);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button();
            btnCancel.Text = "إلغاء";
            btnCancel.Location = new Point(170, 360);
            btnCancel.Size = new Size(80, 35);
            btnCancel.BackColor = Color.Gray;
            btnCancel.ForeColor = Color.White;
            btnCancel.Font = new Font("Arial", 10, FontStyle.Bold);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Click += BtnCancel_Click;

            // إضافة العناصر للنافذة
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblFullName);
            this.Controls.Add(txtFullName);
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(lblPhone);
            this.Controls.Add(txtPhone);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblConfirmPassword);
            this.Controls.Add(txtConfirmPassword);
            this.Controls.Add(lblRole);
            this.Controls.Add(cmbRole);
            this.Controls.Add(lblStatus);
            this.Controls.Add(chkIsActive);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            // إضافة ملاحظة للتعديل
            if (isEditing)
            {
                Label noteLabel = new Label();
                noteLabel.Text = "ملاحظة: اتركه فارغاً للإبقاء على كلمة المرور الحالية";
                noteLabel.Location = new Point(30, 255);
                noteLabel.Size = new Size(300, 15);
                noteLabel.Font = new Font("Arial", 8);
                noteLabel.ForeColor = Color.Gray;
                this.Controls.Add(noteLabel);
            }

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadUserData()
        {
            if (editingUser != null)
            {
                txtUsername.Text = editingUser.Username;
                txtFullName.Text = editingUser.FullName;
                txtEmail.Text = editingUser.Email ?? "";
                txtPhone.Text = editingUser.Phone ?? "";
                cmbRole.SelectedIndex = (int)editingUser.Role - 1;
                chkIsActive.Checked = editingUser.IsActive;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // التحقق من صحة البيانات
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("يرجى إدخال اسم المستخدم", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("يرجى إدخال الاسم الكامل", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            if (cmbRole.SelectedIndex == -1)
            {
                MessageBox.Show("يرجى اختيار الصلاحية", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbRole.Focus();
                return;
            }

            // التحقق من كلمة المرور للمستخدمين الجدد
            if (!isEditing)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("يرجى إدخال كلمة المرور", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (txtPassword.Text.Length < 6)
                {
                    MessageBox.Show("كلمة المرور يجب أن تكون 6 أحرف على الأقل", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("كلمة المرور وتأكيدها غير متطابقان", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassword.Focus();
                    return;
                }
            }
            else if (!string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                // إذا كان يريد تغيير كلمة المرور في التعديل
                if (txtPassword.Text.Length < 6)
                {
                    MessageBox.Show("كلمة المرور يجب أن تكون 6 أحرف على الأقل", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("كلمة المرور وتأكيدها غير متطابقان", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassword.Focus();
                    return;
                }
            }

            try
            {
                btnSave.Enabled = false;
                btnSave.Text = "جاري الحفظ...";

                var user = new User
                {
                    Username = txtUsername.Text.Trim(),
                    FullName = txtFullName.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Role = (UserRole)(cmbRole.SelectedIndex + 1),
                    IsActive = chkIsActive.Checked
                };

                bool success;
                if (isEditing)
                {
                    user.Id = editingUser.Id;
                    success = userService.UpdateUser(user);
                    
                    // تغيير كلمة المرور إذا كانت مدخلة
                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        // هنا يجب إضافة دالة لتغيير كلمة المرور بواسطة المدير
                        // userService.ChangePasswordByAdmin(user.Id, txtPassword.Text);
                    }
                }
                else
                {
                    success = userService.AddUser(user, txtPassword.Text) > 0;
                }

                if (success)
                {
                    MessageBox.Show(isEditing ? "تم تحديث المستخدم بنجاح" : "تم إضافة المستخدم بنجاح", 
                        "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("فشل في حفظ المستخدم", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ المستخدم: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSave.Enabled = true;
                btnSave.Text = "حفظ";
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}