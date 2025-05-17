using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public interface IEmployeeFacade
    {
        void CreateExpense(Expense expense);
        List<ExpenseCategory> LoadExpensesCategories();
    }
}
