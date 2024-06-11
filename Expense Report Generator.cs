using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ExpenseReportGenerator
{
    public class Expense
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
    }

    public class ExpenseDbContext : DbContext
    {
        public DbSet<Expense> Expenses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("YourConnectionString");
        }
    }

    public enum ReportFormat
    {
        Text,
        CSV,
        Excel,
        PDF
    }

    public class ReportGenerator
    {
        private readonly ExpenseDbContext dbContext;

        public ReportGenerator()
        {
            dbContext = new ExpenseDbContext();
        }

        public void GenerateExpenseReport(DateTime startDate, DateTime endDate, string category, string outputPath, ReportFormat format)
        {
            var expenses = dbContext.Expenses.Where(e =>
                e.Date >= startDate && e.Date <= endDate &&
                (string.IsNullOrEmpty(category) || e.Category == category))
                .OrderBy(e => e.Date)
                .ToList();

            string reportContent = GenerateReportContent(expenses, startDate, endDate, category, format);

            WriteReportToFile(outputPath, reportContent);
        }

        private string GenerateReportContent(List<Expense> expenses, DateTime startDate, DateTime endDate, string category, ReportFormat format)
        {
            switch (format)
            {
                case ReportFormat.Text:
                    return GenerateTextReportContent(expenses, startDate, endDate, category);
                case ReportFormat.CSV:
                    return GenerateCSVReportContent(expenses);
                default:
                    throw new ArgumentException("Invalid report format.");
            }
        }

        private string GenerateTextReportContent(List<Expense> expenses, DateTime startDate, DateTime endDate, string category)
        {
            string reportContent = $"Expense Report\nPeriod: {startDate.ToShortDateString()} to {endDate.ToShortDateString()}\nCategory: {category}\n\nDate\t\tAmount\t\tCategory\n";
            foreach (var expense in expenses)
            {
                reportContent += $"{expense.Date.ToShortDateString()}\t{expense.Amount}\t\t{expense.Category}\n";
            }
            return reportContent;
        }

        private string GenerateCSVReportContent(List<Expense> expenses)
        {
            string reportContent = "Date,Amount,Category\n";
            foreach (var expense in expenses)
            {
                reportContent += $"{expense.Date.ToShortDateString()},{expense.Amount},{expense.Category}\n";
            }
            return reportContent;
        }

        private void WriteReportToFile(string outputPath, string reportContent)
        {
            File.WriteAllText(outputPath, reportContent);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var reportGenerator = new ReportGenerator();
            DateTime startDate = new DateTime(2024, 1, 1);
            DateTime endDate = new DateTime(2024, 12, 31);
            string category = "Business";
            string outputPath = "ExpenseReport.txt";
            ReportFormat format = ReportFormat.Text;
            reportGenerator.GenerateExpenseReport(startDate, endDate, category, outputPath, format);
            Console.WriteLine("Expense report generated successfully.");
        }
    }
}