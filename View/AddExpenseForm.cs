using BusinessLogic;
using Model;
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
    public partial class AddExpenseForm : Form
    {
        private readonly IEmployeeFacade _facade;
        private readonly int _userID;
        private readonly int _department;
        public AddExpenseForm(IEmployeeFacade facade, int userID, int department)
        {
            InitializeComponent();
            _facade = facade;
            _userID = userID;
            _department = department;

            LoadCategories();
        }

        // Загрузка категорий в выпадающий список
        private void LoadCategories()
        {
            try
            {
                var categories = _facade.LoadExpensesCategories();
                cboCategory.DisplayMember = "ExpenseCategoryName"; // Отображаем Название
                cboCategory.ValueMember = "ExpenseCategoryID"; // Храним ID_категории_расходов
                cboCategory.DataSource = categories;
                cboCategory.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}");
            }
        }

        // Обработчик для кнопки "Сохранить расход"
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка на правильность ввода суммы
                if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Пожалуйста, введите корректную сумму больше нуля.");
                    return;
                }

                // Проверка на выбор категории
                if (cboCategory.SelectedIndex == -1)
                {
                    MessageBox.Show("Пожалуйста, выберите категорию.");
                    return;
                }

                // Проверка на пустое описание
                if (string.IsNullOrWhiteSpace(txtDescription.Text))
                {
                    MessageBox.Show("Пожалуйста, введите описание.");
                    return;
                }

                // Считываем данные с формы
                var expense = new Expense
                {
                    CategoryID = (int)cboCategory.SelectedValue, // Используем ID категории
                    Amount = amount,
                    Description = txtDescription.Text,
                    Date = DateTime.Now,
                    Status = "На рассмотрении",
                    EmployeeID = _userID,
                    Department = _department
                };

                // Передаем данные в фасад для сохранения
                _facade.CreateExpense(expense);

                // Сообщаем пользователю об успешном добавлении расхода
                MessageBox.Show("Расход успешно добавлен!");

                // Очищаем поля формы
                cboCategory.SelectedIndex = -1;
                txtAmount.Clear();
                txtDescription.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }


    }
}
