using BusinessLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace View
{
    public partial class LoginForm : Form
    {
        private readonly FacadeBase _facade;

        public LoginForm(FacadeBase facade)
        {
            InitializeComponent();
            _facade = facade;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Получаем роль, UserID и отдел
            var (role, userID, department) = _facade.TryLoginAndGetUserInfo(login, password);

            if (role != null)
            {
                // В зависимости от роли пользователя показываем соответствующую форму
                Form mainMenu = null;

                switch (role)
                {
                    case "Сотрудник":
                        mainMenu = new EmployeeMainForm(_facade as IEmployeeFacade, userID, department);
                        break;
                    case "Начальник":
                        // mainMenu = new DepartmentHeadMainForm(_facade as IDepartmentHeadFacade, userID, department);
                        break;
                    case "Бухгалтер":
                        // mainMenu = new AccountantMainForm(_facade as IAccountantFacade, userID, department);
                        break;
                    case "Администратор":
                        // mainMenu = new AdminMainForm(_facade as IAdminFacade, userID, department);
                        break;
                }

                mainMenu?.Show();
                this.Hide(); // Прячем форму авторизации
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.");
            }
        }

    }
}
