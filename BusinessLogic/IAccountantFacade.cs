using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public interface IAccountantFacade
    {
        List<Expense> LoadApprovedExpenses(string filter);
        bool CheckFundsAvailability(decimal sum, int departmentId);
        void DoPaymentTransaction(int departmentId);
        void UpdateExpenseStatus(int id, string status);
    }
}
