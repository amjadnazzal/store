using System;
using System.Drawing;
using System.Windows.Forms;
using ClothingStoreManager.Services;

namespace ClothingStoreManager.Forms
{
    public partial class LoginForm : Form
    {
        private UserService userService;
        private TextBox txtUsername, txtPassword;
        private Button btnLogin, btnExit;
        private Label lblTitle, lblUsername, lblPassword;
        private Panel loginPanel;

        public LoginForm()
        {
            userService = new UserService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = "تسجيل الدخول - نظام إدارة متجر الملابس";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.FromArgb(240, 248, 255);

            // لوحة تسجيل الدخول
            loginPanel = new Panel();
            loginPanel.Size = new Size(350, 280);
            loginPanel.Location = new Point((this.Width - loginPanel.Width) / 2, (this.Height - loginPanel.Height) / 2);
            loginPanel.BackColor = Color.White;
            loginPanel.BorderStyle = BorderStyle.FixedSingle;

            // العنوان
            lblTitle = new Label();
            lblTitle.Text = "نظام إدارة متجر الملابس";
            lblTitle.Font = new Font("Arial", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.DarkBlue;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point((loginPanel.Width - 250) / 2, 30);

            // اسم المستخدم
            lblUsername = new Label();
            lblUsername.Text = "اسم المستخدم:";
            lblUsername.Location = new Point(250, 80);
            lblUsername.Size = new Size(80, 23);
            lblUsername.Font = new Font("Arial", 10);

            txtUsername = new TextBox();
            txtUsername.Location = new Point(50, 78);
            txtUsername.Size = new Size(180, 25);
            txtUsername.Font = new Font("Arial", 10);

            // كلمة المرور
            lblPassword = new Label();
            lblPassword.Text = "كلمة المرور:";
            lblPassword.Location = new Point(250, 120);
            lblPassword.Size = new Size(80, 23);
            lblPassword.Font = new Font("Arial", 10);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(50, 118);
            txtPassword.Size = new Size(180, 25);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Font = new Font("Arial", 10);
            txtPassword.KeyPress += TxtPassword_KeyPress;

            // أزرار العمل
            btnLogin = new Button();
            btnLogin.Text = "دخول";
            btnLogin.Location = new Point(140, 170);
            btnLogin.Size = new Size(80, 35);
            btnLogin.BackColor = Color.DodgerBlue;
            btnLogin.ForeColor = Color.White;
            btnLogin.Font = new Font("Arial", 10, FontStyle.Bold);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Click += BtnLogin_Click;

            btnExit = new Button();
            btnExit.Text = "خروج";
            btnExit.Location = new Point(50, 170);
            btnExit.Size = new Size(80, 35);
            btnExit.BackColor = Color.Gray;
            btnExit.ForeColor = Color.White;
            btnExit.Font = new Font("Arial", 10, FontStyle.Bold);
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.Click += BtnExit_Click;

            // معلومات إضافية
            Label lblInfo = new Label();
            lblInfo.Text = "المستخدم الافتراضي: admin\nكلمة المرور: admin123";
            lblInfo.Location = new Point(50, 220);
            lblInfo.Size = new Size(250, 40);
            lblInfo.Font = new Font("Arial", 8);
            lblInfo.ForeColor = Color.Gray;

            // إضافة العناصر للوحة
            loginPanel.Controls.Add(lblTitle);
            loginPanel.Controls.Add(lblUsername);
            loginPanel.Controls.Add(txtUsername);
            loginPanel.Controls.Add(lblPassword);
            loginPanel.Controls.Add(txtPassword);
            loginPanel.Controls.Add(btnLogin);
            loginPanel.Controls.Add(btnExit);
            loginPanel.Controls.Add(lblInfo);

            // إضافة اللوحة للنافذة
            this.Controls.Add(loginPanel);

            // تركيز على حقل اسم المستخدم
            this.Load += (s, e) => txtUsername.Focus();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void TxtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("يرجى إدخال اسم المستخدم", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("يرجى إدخال كلمة المرور", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            try
            {
                btnLogin.Enabled = false;
                btnLogin.Text = "جاري التحقق...";

                var user = userService.AuthenticateUser(username, password);
                
                if (user != null)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة", "خطأ في تسجيل الدخول", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تسجيل الدخول: {ex.Message}", "خطأ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "دخول";
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}