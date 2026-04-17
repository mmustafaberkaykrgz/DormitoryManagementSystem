using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DormitoryManagementSystem.Data;
using DormitoryManagementSystem.Models;

namespace DormitoryManagementSystem.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private readonly AppDbContext _context;

        public MaintenanceController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            var query = _context.MaintenanceTickets.AsNoTracking().Include(m => m.Room).Include(m => m.Student).ThenInclude(s => s!.Room).AsQueryable();
            if (User.IsInRole("Student"))
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdStr, out int userId))
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                    if (student != null)
                    {
                        query = query.Where(m => m.StudentId == student.Id);
                        var now = DateTime.Now;
                        ViewBag.IsActiveStudent = (now >= student.MembStartDate && now <= student.MembEndDate);
                    }
                    else
                    {
                        query = query.Where(m => false);
                        ViewBag.IsActiveStudent = false;
                    }
                }
            }

            int totalItems = await query.CountAsync();
            int pendingTickets = await query.CountAsync(t => !t.IsResolved);
            int resolvedTickets = await query.CountAsync(t => t.IsResolved);

            var tickets = await query
                .OrderByDescending(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
            ViewBag.TotalCount = totalItems;
            ViewBag.PendingCount = pendingTickets;
            ViewBag.ResolvedCount = resolvedTickets;

            return View(tickets);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                var now = DateTime.Now;
                if (student == null || student.MembStartDate > now || student.MembEndDate < now)
                {
                    TempData["Error"] = "Only active students can create maintenance tickets. Expired or passive accounts are restricted.";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Students = _context.Students.ToList();
            ViewBag.Rooms = _context.Rooms.ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentId,RoomId,Issue")] MaintenanceTicket ticket)
        {
            if (User.IsInRole("Student"))
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdStr, out int userId))
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                    if (student != null)
                    {
                        var now = DateTime.Now;
                        if (student.MembStartDate > now || student.MembEndDate < now)
                        {
                            TempData["Error"] = "Only active students can create maintenance tickets. Expired or passive accounts are restricted.";
                            return RedirectToAction(nameof(Index));
                        }

                        ticket.StudentId = student.Id;
                        ticket.RoomId = student.RoomId; // Assume they report for their own room by default
                        ModelState.Remove("StudentId");
                        ModelState.Remove("RoomId");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                ticket.IsResolved = false;
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Maintenance ticket submitted successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Students = _context.Students.ToList();
            ViewBag.Rooms = _context.Rooms.ToList();
            return View(ticket);
        }

        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Resolve(int id)
        {
            var ticket = await _context.MaintenanceTickets.FindAsync(id);
            if (ticket != null)
            {
                ticket.IsResolved = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Maintenance ticket marked as resolved.";
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Staff,Student")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _context.MaintenanceTickets.Include(m => m.Room).Include(m => m.Student).FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null) return NotFound();

            if (User.IsInRole("Student"))
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                    if (student == null || ticket.StudentId != student.Id) return Unauthorized();
                }
            }
            return View(ticket);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _context.MaintenanceTickets.FindAsync(id);
            if (ticket == null) return NotFound();
            if (ticket.IsResolved) 
            {
                TempData["Error"] = "Resolved tickets cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null || ticket.StudentId != student.Id) return Unauthorized();
            }
            return View(ticket);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Issue")] MaintenanceTicket model)
        {
            if (id != model.Id) return NotFound();

            var ticket = await _context.MaintenanceTickets.FindAsync(id);
            if (ticket == null) return NotFound();
            if (ticket.IsResolved) return BadRequest("Resolved tickets cannot be edited.");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null || ticket.StudentId != student.Id) return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                ticket.Issue = model.Issue;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ticket updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _context.MaintenanceTickets.FindAsync(id);
            if (ticket != null)
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                    if (student != null && ticket.StudentId == student.Id)
                    {
                        _context.MaintenanceTickets.Remove(ticket);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Maintenance ticket successfully deleted.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                return Unauthorized();
            }
            return NotFound();
        }
    }
}
