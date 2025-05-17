using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Expense
    {
        public int ExpenseID { get; set; }
        public int EmployeeID { get; set; }
        public int Department { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int CategoryID { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

    }
}
