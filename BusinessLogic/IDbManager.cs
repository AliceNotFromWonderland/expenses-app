using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public interface IDbManager
    {
        // Расходы
        void CreateExpense(Expense expense);
        List<Expense> LoadApprovedExpenses(string filter);
        List<Expense> LoadPendingExpenses();
        void UpdateExpenseStatus(int id, string status);

        bool CheckFundsAvailability(decimal sum, int departmentId); 
        void DoPaymentTransaction(int expenseId);


        List<Expense> GetAllExpenses();
        List<ExpenseCategory> LoadExpensesCategories();

        (string role, int userID, int department) TryLoginAndGetUserInfo(string login, string password);

        //// Пользователи и отделы (для администратора)
        //List<User> LoadUsers();
        //List<Department> LoadDepartments();
        //void AddDepartment(Department department);
        //void UpdateDepartment(Department department);
        //void DeleteDepartment(int departmentId);
    }
}
