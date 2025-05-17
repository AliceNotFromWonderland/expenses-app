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
                var command = new OleDbCommand("SELECT * FROM Expenses", connection);
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
                    "SELECT * FROM Expenses WHERE Status = 'Pending'", connection);

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

        public void UpdateExpenseStatus(int id, string status)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand(
                    "UPDATE Expenses SET Status = ? WHERE ExpenseID = ?", connection);

                command.Parameters.AddWithValue("?", status);
                command.Parameters.AddWithValue("?", id);

                command.ExecuteNonQuery();
            }
        }      

        public bool CheckFundsAvailability(decimal sum, int departmentId)
        {
            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                string query = "SELECT RemainingBudget FROM Departments WHERE DepartmentID = ?";
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

        public void DoPaymentTransaction(decimal sum, int departmentId)
        {
            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                string query = "UPDATE Departments SET RemainingBudget = RemainingBudget - ? WHERE DepartmentID = ?";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("?", sum);
                command.Parameters.AddWithValue("?", departmentId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }


        private Expense ReadExpense(OleDbDataReader reader)
        {
            return new Expense
            {
                ExpenseID = Convert.ToInt32(reader["ExpenseID"]),
                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                Department = Convert.ToInt32(reader["Department"]),
                CategoryID = Convert.ToInt32(reader["Category"]),
                Amount = Convert.ToDecimal(reader["Amount"]),
                Status = reader["Status"].ToString(),
                Date = Convert.ToDateTime(reader["Date"]),
                Description = reader["Description"].ToString()
            };
        }
    }
}
