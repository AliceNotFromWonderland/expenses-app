namespace View
{
    partial class EmployeeMainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnCreateExpense;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnCreateExpense = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnCreateExpense
            // 
            this.btnCreateExpense.Location = new System.Drawing.Point(87, 37);
            this.btnCreateExpense.Name = "btnCreateExpense";
            this.btnCreateExpense.Size = new System.Drawing.Size(200, 30);
            this.btnCreateExpense.TabIndex = 0;
            this.btnCreateExpense.Text = "Создать расход";
            this.btnCreateExpense.Click += new System.EventHandler(this.btnCreateExpense_Click);
            // 
            // EmployeeMainForm
            // 
            this.ClientSize = new System.Drawing.Size(384, 211);
            this.Controls.Add(this.btnCreateExpense);
            this.Name = "EmployeeMainForm";
            this.Text = "Главное меню сотрудника";
            this.ResumeLayout(false);

        }
    }
}