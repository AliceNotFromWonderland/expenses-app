using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class FacadeBase : IDepartmentHeadFacade, IAdminFacade, IEmployeeFacade, IAccountantFacade
    {
        private IDbManager _dbManager;

        public FacadeBase(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public (string role, int userID, int department) TryLoginAndGetUserInfo(string login, string password)
        {
            return _dbManager.TryLoginAndGetUserInfo(login, password);
        }     

        public void CreateExpense(Expense expense)
        {
            _dbManager.CreateExpense(expense);
        }
        public List<ExpenseCategory> LoadExpensesCategories()
        {
            return _dbManager.LoadExpensesCategories();
        }
        public List<Expense> GetAllExpenses()
        {
            return _dbManager.GetAllExpenses();
        }
        public List<Expense> LoadApprovedExpenses(string filter)
        {
            return _dbManager.LoadApprovedExpenses(filter);
        }

        public List<Expense> LoadPendingExpenses()
        {
            return _dbManager.LoadPendingExpenses();
        }

        public void UpdateExpenseStatus(int id, string status)
        {
            _dbManager.UpdateExpenseStatus(id, status);
        }      

        public bool CheckFundsAvailability(decimal sum, int departmentId)
        {
            return _dbManager.CheckFundsAvailability(sum, departmentId);
        }

        public void DoPaymentTransaction(int departmentId)
        {
            _dbManager.DoPaymentTransaction(departmentId);
        }

    }
}
