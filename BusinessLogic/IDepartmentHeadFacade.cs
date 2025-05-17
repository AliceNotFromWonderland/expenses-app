using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public interface IDepartmentHeadFacade
    {
        List<Expense> LoadPendingExpenses();
        void UpdateExpenseStatus(int id, string status);
    }
}
