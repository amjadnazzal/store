using System;
using System.Drawing;
using System.Windows.Forms;
using ClothingStoreManager.Models;
using ClothingStoreManager.Services;

namespace ClothingStoreManager.Forms
{
    public partial class UsersForm : Form
    {
        private UserService userService;
        private ListView listView;
        private Button btnAdd, btnEdit, btnDeactivate, btnRefresh, btnClose;

        public UsersForm()
        {
            userService = new UserService();
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // إعدادات النافذة
            this.Text = "إدارة المستخدمين";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // قائمة المستخدمين
            listView = new ListView();
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.Location = new Point(20, 20);
            listView.Size = new Size(720, 450);
            listView.Font = new Font("Arial", 10);

            listView.Columns.Add("الرقم", 60);
            listView.Columns.Add("اسم المستخدم", 120);
            listView.Columns.Add("الاسم الكامل", 150);
            listView.Columns.Add("البريد الإلكتروني", 150);
            listView.Columns.Add("الهاتف", 100);
            listView.Columns.Add("الصلاحية", 100);
            listView.Columns.Add("الحالة", 80);
            listView.Columns.Add("تاريخ الإنشاء", 120);
            listView.Columns.Add("آخر دخول", 120);

            // الأزرار
            btnAdd = new Button();
            btnAdd.Text = "إضافة مستخدم";
            btnAdd.Location = new Point(760, 50);
            btnAdd.Size = new Size(100, 35);
            btnAdd.BackColor = Color.Green;
            btnAdd.ForeColor = Color.White;
            btnAdd.Font = new Font("Arial", 9, FontStyle.Bold);
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button();
            btnEdit.Text = "تعديل";
            btnEdit.Location = new Point(760, 100);
            btnEdit.Size = new Size(100, 35);
            btnEdit.BackColor = Color.Blue;
            btnEdit.ForeColor = Color.White;
            btnEdit.Font = new Font("Arial", 9, FontStyle.Bold);
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Click += BtnEdit_Click;

            btnDeactivate = new Button();
            btnDeactivate.Text = "إلغاء تفعيل";
            btnDeactivate.Location = new Point(760, 150);
            btnDeactivate.Size = new Size(100, 35);
            btnDeactivate.BackColor = Color.Orange;
            btnDeactivate.ForeColor = Color.White;
            btnDeactivate.Font = new Font("Arial", 9, FontStyle.Bold);
            btnDeactivate.FlatStyle = FlatStyle.Flat;
            btnDeactivate.Click += BtnDeactivate_Click;

            btnRefresh = new Button();
            btnRefresh.Text = "تحديث";
            btnRefresh.Location = new Point(760, 200);
            btnRefresh.Size = new Size(100, 35);
            btnRefresh.BackColor = Color.Gray;
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Font = new Font("Arial", 9, FontStyle.Bold);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Click += BtnRefresh_Click;

            btnClose = new Button();
            btnClose.Text = "إغلاق";
            btnClose.Location = new Point(760, 480);
            btnClose.Size = new Size(100, 35);
            btnClose.BackColor = Color.DarkRed;
            btnClose.ForeColor = Color.White;
            btnClose.Font = new Font("Arial", 9, FontStyle.Bold);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Click += BtnClose_Click;

            // إضافة العناصر للنافذة
            this.Controls.Add(listView);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnEdit);
            this.Controls.Add(btnDeactivate);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(btnClose);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadUsers()
        {
            try
            {
                listView.Items.Clear();
                var users = userService.GetAllUsers();

                foreach (var user in users)
                {
                    var item = new ListViewItem(user.Id.ToString());
                    item.SubItems.Add(user.Username);
                    item.SubItems.Add(user.FullName);
                    item.SubItems.Add(user.Email ?? "");
                    item.SubItems.Add(user.Phone ?? "");
                    item.SubItems.Add(GetRoleName(user.Role));
                    item.SubItems.Add(user.IsActive ? "نشط" : "غير نشط");
                    item.SubItems.Add(user.CreatedDate.ToString("yyyy/MM/dd"));
                    item.SubItems.Add(user.LastLogin?.ToString("yyyy/MM/dd HH:mm") ?? "لم يسجل دخول");
                    
                    // تلوين المستخدمين غير النشطين
                    if (!user.IsActive)
                    {
                        item.BackColor = Color.LightGray;
                        item.ForeColor = Color.DarkGray;
                    }

                    item.Tag = user;
                    listView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المستخدمين: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetRoleName(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "مدير النظام",
                UserRole.Manager => "مدير المتجر",
                UserRole.Cashier => "كاشير",
                UserRole.Employee => "موظف",
                _ => "غير محدد"
            };
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var addUserForm = new UserAddEditForm();
            if (addUserForm.ShowDialog() == DialogResult.OK)
            {
                LoadUsers();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("يرجى اختيار مستخدم للتعديل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedUser = (User)listView.SelectedItems[0].Tag;
            var editUserForm = new UserAddEditForm(selectedUser);
            if (editUserForm.ShowDialog() == DialogResult.OK)
            {
                LoadUsers();
            }
        }

        private void BtnDeactivate_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("يرجى اختيار مستخدم لإلغاء تفعيله", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedUser = (User)listView.SelectedItems[0].Tag;
            
            // منع إلغاء تفعيل المستخدم الحالي
            if (selectedUser.Id == UserService.CurrentUser?.Id)
            {
                MessageBox.Show("لا يمكن إلغاء تفعيل المستخدم الحالي", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // منع إلغاء تفعيل مدير النظام الوحيد
            if (selectedUser.Role == UserRole.Admin)
            {
                var adminCount = userService.GetAllUsers().FindAll(u => u.Role == UserRole.Admin && u.IsActive).Count;
                if (adminCount <= 1)
                {
                    MessageBox.Show("لا يمكن إلغاء تفعيل مدير النظام الوحيد", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            var result = MessageBox.Show($"هل أنت متأكد من إلغاء تفعيل المستخدم '{selectedUser.FullName}'؟", 
                "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    bool success = userService.DeactivateUser(selectedUser.Id);
                    if (success)
                    {
                        MessageBox.Show("تم إلغاء تفعيل المستخدم بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadUsers();
                    }
                    else
                    {
                        MessageBox.Show("فشل في إلغاء تفعيل المستخدم", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في إلغاء تفعيل المستخدم: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}