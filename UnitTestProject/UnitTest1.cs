using BusinessLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using System;
using System.Data.OleDb;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        private AccessDbManager _db;
        private string _connectionString;

        [TestInitialize]
        public void Setup()
        {
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["OfficeExpensesDb"].ConnectionString;
            _db = new AccessDbManager();
        }

        [TestMethod]
        public void TryLoginAndGetUserInfo_ValidCredentials_ReturnsRoleAndUser()
        {
            var (role, userID, department) = _db.TryLoginAndGetUserInfo("user0", "123");
            Assert.IsNotNull(role);
            Assert.IsTrue(userID > 0);
            Assert.IsTrue(department > 0);
        }


        [TestMethod]
        public void CreateExpense_AddsExpenseSuccessfully()
        {
            int beforeId = _db.GetLastInsertedExpenseId();

            var expense = new Expense
            {
                EmployeeID = 1,
                Department = 4,
                Date = DateTime.Today,
                Amount = 123456,
                CategoryID = 5,
                Description = "Тестовый расход",
                Status = "На рассмотрении"
            };

            int afterId = 0;

            try
            {
                _db.CreateExpense(expense);
                afterId = _db.GetLastInsertedExpenseId();

                Assert.IsTrue(afterId > beforeId, "Новый ID должен быть больше предыдущего.");

                var inserted = _db.GetExpenseById(afterId);
                Assert.IsNotNull(inserted);
                Assert.AreEqual(expense.Amount, inserted.Amount);
                Assert.AreEqual(expense.Description, inserted.Description);
                Assert.AreEqual(expense.Status, inserted.Status);
            }
            finally
            {
                if (afterId > 0)
                {
                    _db.DeleteExpenseById(afterId); 
                }
            }
        }




        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateExpense_NullExpense_ThrowsArgumentNullException()
        {
            _db.CreateExpense(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateExpense_NegativeAmount_ThrowsArgumentException()
        {
            var expense = new Expense
            {
                EmployeeID = 1,
                Department = 4,
                Date = DateTime.Today,
                Amount = -100, // некорректно
                CategoryID = 5,
                Description = "Отрицательная сумма",
                Status = "На рассмотрении"
            };

            _db.CreateExpense(expense);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateExpense_EmptyDescription_ThrowsArgumentException()
        {
            var expense = new Expense
            {
                EmployeeID = 1,
                Department = 4,
                Date = DateTime.Today,
                Amount = 100,
                CategoryID = 5,
                Description = "", // некорректно
                Status = "На рассмотрении"
            };

            _db.CreateExpense(expense);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateExpense_ZeroCategoryID_ThrowsArgumentException()
        {
            var expense = new Expense
            {
                EmployeeID = 1,
                Department = 4,
                Date = DateTime.Today,
                Amount = 100,
                CategoryID = 0, // некорректно
                Description = "Категория = 0",
                Status = "На рассмотрении"
            };

            _db.CreateExpense(expense);
        }


        [TestMethod]
        public void LoadExpensesCategories_ReturnsAtLeastOneCategory()
        {
            var categories = _db.LoadExpensesCategories();
            Assert.IsTrue(categories.Count > 0);
        }



        [TestMethod]
        public void DoPaymentTransaction_ShouldMarkExpenseAsPaidAndUpdateBudget()
        {
            var dbManager = new AccessDbManager();
            int expenseId = 14;

            //сохраняем исходные значения, чтобы потом восстановить
            string originalStatus = _db.GetExpenseStatus(expenseId);
            int departmentId = _db.GetDepartmentIdByExpenseId(expenseId);
            decimal originalBudget = _db.GetCurrentBudget(departmentId);
            decimal amount = _db.GetExpenseAmount(expenseId); 

            try
            {
                // целевое действие
                dbManager.DoPaymentTransaction(expenseId);

                // проверки
                string expectedStatus = "Оплачено";
                decimal expectedBudgetAfterPayment = originalBudget - amount;

                string actualStatus = _db.GetExpenseStatus(expenseId);
                decimal actualBudget = _db.GetCurrentBudget(departmentId);

                Assert.AreEqual(expectedStatus, actualStatus);
                Assert.AreEqual(expectedBudgetAfterPayment, actualBudget);
            }
            finally
            {
                // откат
                _db.UpdateExpenseStatus(expenseId, originalStatus);
                _db.UpdateDepartmentBudget(departmentId, originalBudget);
            }
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DoPaymentTransaction_ShouldThrowIfNotEnoughBudget()
        {
            var dbManager = new AccessDbManager();
            int expenseId = 15;

            dbManager.DoPaymentTransaction(expenseId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DoPaymentTransaction_ExpenseNotFound_ThrowsException()
        {
            var dbManager = new AccessDbManager();

            int nonExistingExpenseId = 99999; 

            dbManager.DoPaymentTransaction(nonExistingExpenseId);

        }

    }
}
