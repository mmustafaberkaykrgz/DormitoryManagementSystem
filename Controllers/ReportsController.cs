using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DormitoryManagementSystem.Data;
using DormitoryManagementSystem.Models;

namespace DormitoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var allDues = await _context.DuesAndPenalties
                .Include(d => d.Student)
                    .ThenInclude(s => s.Room)
                .ToListAsync();

            // Key Financial Metrics
            var collectedRevenue = allDues.Where(d => d.IsPaid).Sum(d => d.Amount);
            var totalOverdue = allDues.Where(d => !d.IsPaid).Sum(d => d.Amount);
            var totalTarget = collectedRevenue + totalOverdue;
            var collectionRate = totalTarget > 0
                ? Math.Round((double)(collectedRevenue / totalTarget) * 100, 2)
                : 0;

            // Highest overdue type
            var penaltyOverdue = allDues.Where(d => !d.IsPaid && d.Description.ToLower().Contains("penalty")).Sum(d => d.Amount);
            var feeOverdue = allDues.Where(d => !d.IsPaid && !d.Description.ToLower().Contains("penalty")).Sum(d => d.Amount);
            var highestOverdueArea = penaltyOverdue >= feeOverdue ? "Penalty" : "Monthly Fee";

            ViewBag.CollectedRevenue = collectedRevenue;
            ViewBag.TotalOverdue = totalOverdue;
            ViewBag.TotalTarget = totalTarget;
            ViewBag.CollectionRate = collectionRate;
            ViewBag.HighestOverdueArea = highestOverdueArea;

            // Monthly revenue data for chart (last 6 months)
            var monthlyData = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var month = DateTime.Now.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var accommodation = allDues
                    .Where(d => d.IsPaid && !d.Description.ToLower().Contains("penalty")
                        && d.DueDate >= monthStart && d.DueDate < monthEnd)
                    .Sum(d => d.Amount);

                var miscFees = allDues
                    .Where(d => d.IsPaid && d.Description.ToLower().Contains("penalty")
                        && d.DueDate >= monthStart && d.DueDate < monthEnd)
                    .Sum(d => d.Amount);

                monthlyData.Add(new
                {
                    Month = month.ToString("MMM"),
                    Accommodation = accommodation,
                    MiscFees = miscFees
                });
            }
            ViewBag.MonthlyData = monthlyData;

            // Overdue Payments List
            var overdueList = allDues
                .Where(d => !d.IsPaid)
                .OrderByDescending(d => (DateTime.Now - d.DueDate).TotalDays)
                .Take(10)
                .Select(d => new
                {
                    StudentName = d.Student?.FullName ?? "Unknown",
                    Room = d.Student?.Room?.RoomNumber ?? "N/A",
                    Type = d.Description,
                    Amount = d.Amount,
                    DaysOverdue = (int)Math.Max(0, (DateTime.Now - d.DueDate).TotalDays),
                    DueDate = d.DueDate,
                    StudentId = d.StudentId,
                    DuesId = d.Id
                })
                .ToList();
            ViewBag.OverdueList = overdueList;

            // Summary for management
            var avgOverdueDays = overdueList.Any()
                ? overdueList.Average(o => o.DaysOverdue)
                : 0;
            ViewBag.AvgOverdueDays = Math.Round(avgOverdueDays, 0);
            ViewBag.TopOverdueDept = highestOverdueArea;
            ViewBag.Q1Target = totalTarget * 3; // quarterly projection

            ViewBag.DataAsOf = DateTime.Now.ToString("MMMM d, yyyy");

            return View();
        }
    }
}
