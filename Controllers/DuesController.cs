using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using DormitoryManagementSystem.Data;
using DormitoryManagementSystem.Models;

namespace DormitoryManagementSystem.Controllers
{
    [Authorize]
    public class DuesController : Controller
    {
        private readonly AppDbContext _context;

        public DuesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            var query = _context.DuesAndPenalties.AsNoTracking()
                .Include(d => d.Student!).ThenInclude(s => s!.Room)
                .AsQueryable();

            if (User.IsInRole("Student"))
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdStr, out int userId))
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                    if (student != null)
                        query = query.Where(d => d.StudentId == student.Id);
                    else
                        query = query.Where(d => false);
                }
            }

            // Summary totals (all records, not just current page)
            var allDues = await query.ToListAsync();
            ViewBag.TotalPaid   = allDues.Where(x => x.IsPaid).Sum(x => x.Amount);
            ViewBag.TotalUnpaid = allDues.Where(x => !x.IsPaid).Sum(x => x.Amount);
            ViewBag.TotalCount  = allDues.Count;

            // Pagination
            int totalRecords = allDues.Count;
            int totalPages   = (int)Math.Ceiling(totalRecords / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages  = totalPages;

            var paged = allDues
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return View(paged);
        }

        // GET: Dues/Create
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Create()
        {
            var now = DateTime.Now;
            var students = await _context.Students.AsNoTracking()
                .Where(s => s.MembStartDate <= now && s.MembEndDate >= now)
                .ToListAsync();
            ViewData["Students"] = students.Select(s => new SelectListItem 
            { 
                Value = s.Id.ToString(), 
                Text = s.FullNameWithRegNo 
            }).ToList();
            
            return View(new DuesAndPenalty());
        }

        // POST: Dues/Create
        [HttpPost]
        [Authorize(Roles = "Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentId,DueDate,Description")] DuesAndPenalty due)
        {
            // Parse Amount manually to support tr-TR format (14.500,00)
            var amountStr = Request.Form["Amount"].ToString().Trim();
            var trCulture = CultureInfo.GetCultureInfo("tr-TR");
            if (decimal.TryParse(amountStr, NumberStyles.Number, trCulture, out decimal parsedAmount))
            {
                due.Amount = parsedAmount;
                ModelState.Remove("Amount");
            }
            else
            {
                ModelState.AddModelError("Amount", "Please enter a valid amount (e.g. 14.500,00)");
            }

            if (ModelState.IsValid)
            {
                _context.Add(due);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Due/Penalty record created successfully.";
                return RedirectToAction(nameof(Index));
            }
            var now2 = DateTime.Now;
            var students = await _context.Students.AsNoTracking()
                .Where(s => s.MembStartDate <= now2 && s.MembEndDate >= now2)
                .ToListAsync();
            ViewData["Students"] = students.Select(s => new SelectListItem 
            { 
                Value = s.Id.ToString(), 
                Text = s.FullNameWithRegNo 
            }).ToList();

            return View(due);
        }

        // POST: Dues/MarkAsPaid/5
        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var due = await _context.DuesAndPenalties.FindAsync(id);
            if (due != null)
            {
                due.IsPaid = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Record marked as paid.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Dues/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var due = await _context.DuesAndPenalties
                .AsNoTracking()
                .Include(d => d.Student!).ThenInclude(s => s!.Room)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (due == null) return NotFound();
            return View(due);
        }

        // GET: Dues/Edit/5
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Edit(int id)
        {
            var due = await _context.DuesAndPenalties
                .Include(d => d.Student)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (due == null) return NotFound();
            return View(due);
        }

        // POST: Dues/Edit/5
        [HttpPost]
        [Authorize(Roles = "Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentId,DueDate,Description,IsPaid")] DuesAndPenalty due)
        {
            if (id != due.Id) return NotFound();

            // Parse Amount manually to support tr-TR format (14.500,00)
            var amountStr = Request.Form["Amount"].ToString().Trim();
            var trCulture = CultureInfo.GetCultureInfo("tr-TR");
            if (decimal.TryParse(amountStr, NumberStyles.Number, trCulture, out decimal parsedAmount))
            {
                due.Amount = parsedAmount;
                ModelState.Remove("Amount");
            }
            else
            {
                ModelState.AddModelError("Amount", "Please enter a valid amount (e.g. 14.500,00)");
            }

            if (ModelState.IsValid)
            {
                _context.Update(due);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Record updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            var existing = await _context.DuesAndPenalties
                .Include(d => d.Student)
                .FirstOrDefaultAsync(d => d.Id == id);
            return View(existing ?? due);
        }
    }
}
