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
    public partial class EmployeeMainForm : Form
    {
        private readonly IEmployeeFacade _facade;
        private readonly int _userID;
        private readonly int _department;

        // Обновленный конструктор для передачи UserID, роли и отдела
        public EmployeeMainForm(IEmployeeFacade facade, int userID, int department)
        {
            InitializeComponent();
            _facade = facade;
            _userID = userID;
            _department = department;
        }

        // Обработчик для кнопки "Создать расход"
        private void btnCreateExpense_Click(object sender, EventArgs e)
        {
            // Создаем форму для добавления расхода, передаем нужные данные
            var form = new AddExpenseForm(_facade, _userID, _department);
            form.ShowDialog();
        }
    }
}
