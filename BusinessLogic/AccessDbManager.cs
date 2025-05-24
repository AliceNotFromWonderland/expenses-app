using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class AccessDbManager : IDbManager
    {
        private readonly string _connectionString;

        public AccessDbManager()
        {
            _connectionString = System.Configuration.ConfigurationManager
                .ConnectionStrings["OfficeExpensesDb"].ConnectionString;

        }

        public (string role, int userID, int department) TryLoginAndGetUserInfo(string login, string password)
        {
            var query = "SELECT ID_Пользователя, Роль, ID_Отдела FROM Пользователи WHERE Логин = ? AND Пароль = ?";
            using (var connection = new OleDbConnection(_connectionString))
            {
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("?", login);
                command.Parameters.AddWithValue("?", password);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return (reader["Роль"].ToString(), Convert.ToInt32(reader["ID_Пользователя"]), Convert.ToInt32(reader["ID_Отдела"]));
                    }
                }
            }

            return (null, 0, 0); 
        }


        public void CreateExpense(Expense expense)
        {
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));

            if (expense.Amount <= 0)
                throw new ArgumentException("Сумма должна быть больше нуля.");

            if (string.IsNullOrWhiteSpace(expense.Description))
                throw new ArgumentException("Описание не может быть пустым.");

            if (expense.CategoryID <= 0)
                throw new ArgumentException("CategoryID должен быть выбран (больше нуля).");

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand(
                    @"INSERT INTO Расходы (ID_Сотрудника, ID_Отдела, Дата, Сумма, Категория, Описание, Статус)
              VALUES (?, ?, ?, ?, ?, ?, ?)", connection);

                command.Parameters.AddWithValue("?", expense.EmployeeID);
                command.Parameters.AddWithValue("?", expense.Department);
                command.Parameters.AddWithValue("?", expense.Date);
                command.Parameters.AddWithValue("?", expense.Amount);
                command.Parameters.AddWithValue("?", expense.CategoryID);
                command.Parameters.AddWithValue("?", expense.Description);
                command.Parameters.AddWithValue("?", expense.Status);

                command.ExecuteNonQuery();
            }
        }


        public List<ExpenseCategory> LoadExpensesCategories()
        {
            List<ExpenseCategory> categories = new List<ExpenseCategory>();

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                string query = "SELECT ID_категории_расходов, Название FROM Категории_расходов";

                OleDbCommand command = new OleDbCommand(query, connection);

                connection.Open();
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new ExpenseCategory
                        {
                            ExpenseCategoryID = Convert.ToInt32(reader["ID_категории_расходов"]),
                            ExpenseCategoryName = reader["Название"].ToString()
                        });
                    }
                }
            }

            return categories;
        }



        public List<Expense> GetAllExpenses()
        {
            var expenses = new List<Expense>();

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT * FROM Расходы", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        expenses.Add(ReadExpense(reader));
                    }
                }
            }

            return expenses;
        }

        public List<Expense> LoadApprovedExpenses(string filter)
        {
            var expenses = new List<Expense>();

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand(
                    "SELECT * FROM Expenses WHERE Status = 'Approved' AND Department LIKE ?", connection);
                command.Parameters.AddWithValue("?", "%" + filter + "%");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        expenses.Add(ReadExpense(reader));
                    }
                }
            }

            return expenses;
        }

        public List<Expense> LoadPendingExpenses()
        {
            var expenses = new List<Expense>();

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand(
                    "SELECT * FROM Расходы WHERE Статус = 'На рассмотрении'", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        expenses.Add(ReadExpense(reader));
                    }
                }
            }

            return expenses;
        }

      
        public bool CheckFundsAvailability(decimal sum, int departmentId)
        {
            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                string query = "SELECT Остаток FROM Остатки_бюджета WHERE ID_Отдела = ?";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("?", departmentId);

                connection.Open();
                object result = command.ExecuteScalar();

                if (result != null && decimal.TryParse(result.ToString(), out decimal remaining))
                {
                    return remaining >= sum;
                }

                return false;
            }
        }
        public decimal GetCurrentBudget(int departmentId)
        {
            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                string query = "SELECT Остаток FROM Остатки_бюджета WHERE ID_Отдела = ?";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("?", departmentId);

                connection.Open();
                var result = command.ExecuteScalar();
                return result != null ? Convert.ToDecimal(result) : 0;
            }
        }

        public void DoPaymentTransaction(int expenseId)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();

                string selectQuery = @"SELECT Сумма, ID_Отдела FROM Расходы WHERE ID_Расхода = ? AND Статус = 'Одобрено'";

                using (var selectCmd = new OleDbCommand(selectQuery, connection))
                {
                    selectCmd.Parameters.AddWithValue("?", expenseId);

                    using (var reader = selectCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new InvalidOperationException("Заявка не найдена или ещё не одобрена.");

                        decimal amount = Convert.ToDecimal(reader["Сумма"]);
                        int departmentId = Convert.ToInt32(reader["ID_Отдела"]);

                        // Проверка бюджета
                        if (!CheckFundsAvailability(amount, departmentId))
                            throw new InvalidOperationException("Недостаточно средств в отделе.");

                        // Списание средств
                        PayExpense(amount, departmentId);

                        // Обновление статуса заявки
                        MarkExpenseAsPaid(expenseId);
                    }
                }
            }
        }


        public void PayExpense(decimal sum, int departmentId)
        {
            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                string query = "UPDATE Остатки_бюджета SET Остаток = Остаток - ? WHERE ID_Отдела = ?";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("?", sum);
                command.Parameters.AddWithValue("?", departmentId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public void MarkExpenseAsPaid(int expenseId)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                string updateStatusQuery = "UPDATE Расходы SET Статус = 'Оплачено' WHERE ID_Расхода = ?";

                using (var statusCmd = new OleDbCommand(updateStatusQuery, connection))
                {
                    statusCmd.Parameters.AddWithValue("?", expenseId);
                    statusCmd.ExecuteNonQuery();
                }
            }
        }

        public string GetExpenseStatus(int expenseId)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT Статус FROM Расходы WHERE ID_Расхода = ?", connection);
                command.Parameters.AddWithValue("?", expenseId);
                var result = command.ExecuteScalar();
                return result?.ToString();
            }
        }
        public int GetDepartmentIdByExpenseId(int expenseId)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT ID_Отдела FROM Расходы WHERE ID_Расхода = ?", connection);
                command.Parameters.AddWithValue("?", expenseId);

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        public decimal GetExpenseAmount(int expenseId)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var cmd = new OleDbCommand("SELECT Сумма FROM Расходы WHERE ID_Расхода = ?", connection);
                cmd.Parameters.AddWithValue("?", expenseId);
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        public void UpdateExpenseStatus(int expenseId, string status)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var cmd = new OleDbCommand("UPDATE Расходы SET Статус = ? WHERE ID_Расхода = ?", connection);
                cmd.Parameters.AddWithValue("?", status);
                cmd.Parameters.AddWithValue("?", expenseId);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateDepartmentBudget(int departmentId, decimal budget)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var cmd = new OleDbCommand("UPDATE Остатки_бюджета SET Остаток = ? WHERE ID_Отдела = ?", connection);
                cmd.Parameters.AddWithValue("?", budget);
                cmd.Parameters.AddWithValue("?", departmentId);
                cmd.ExecuteNonQuery();
            }
        }       

        public void DeleteExpenseById(int expenseId)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var cmd = new OleDbCommand("DELETE FROM Расходы WHERE ID_Расхода = ?", connection);
                cmd.Parameters.AddWithValue("?", expenseId);
                cmd.ExecuteNonQuery();
            }
        }
        public int GetLastInsertedExpenseId()
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var cmd = new OleDbCommand(
                    "SELECT MAX(ID_Расхода) FROM Расходы", connection);
                var result = cmd.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public Expense GetExpenseById(int id)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var cmd = new OleDbCommand(
                    "SELECT * FROM Расходы WHERE ID_Расхода = ?", connection);
                cmd.Parameters.AddWithValue("?", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ReadExpense(reader); 
                    }
                }
            }

            return null;
        }





        private Expense ReadExpense(OleDbDataReader reader)
        {
            return new Expense
            {
                ExpenseID = Convert.ToInt32(reader["ID_Расхода"]),
                EmployeeID = Convert.ToInt32(reader["ID_Сотрудника"]),
                Department = Convert.ToInt32(reader["ID_Отдела"]),
                CategoryID = Convert.ToInt32(reader["Категория"]),
                Amount = Convert.ToDecimal(reader["Сумма"]),
                Status = reader["Статус"].ToString(),
                Date = Convert.ToDateTime(reader["Дата"]),
                Description = reader["Описание"].ToString()
            };
        }
    }
}
