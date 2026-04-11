using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DormitoryManagementSystem.Data;
using DormitoryManagementSystem.Models;

namespace DormitoryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────── LOGIN ───────────────
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard", "Home");
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Skip server-side Required on Username — it depends on role
            ModelState.Remove("Username");
            ModelState.Remove("StudentId");

            if (!ModelState.IsValid)
                return View(model);

            User? user = null;

            if (model.SelectedRole == "Student")
            {
                // Students authenticate using their unique Dormitory Registration Number
                if (string.IsNullOrWhiteSpace(model.StudentId))
                {
                    ModelState.AddModelError("StudentId", "Dormitory Registration Number is required.");
                    return View(model);
                }

                var student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.StudentId == model.StudentId);

                if (student?.User != null && student.User.PasswordHash == model.Password)
                {
                    if (!student.User.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "User account is now Inactive.");
                        return View(model);
                    }
                    user = student.User;
                }
            }
            else
            {
                // Staff or Admins authenticate using their chosen Username
                if (string.IsNullOrWhiteSpace(model.Username))
                {
                    ModelState.AddModelError("Username", "Username is required.");
                    return View(model);
                }

                var potentialUsers = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role != null && u.Role.RoleName == model.SelectedRole)
                    .ToListAsync();
                
                var potentialUser = potentialUsers.FirstOrDefault(u => 
                    u.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase));
                
                if (potentialUser != null && potentialUser.PasswordHash == model.Password)
                {
                    if (!potentialUser.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "User account is now Inactive.");
                        return View(model);
                    }
                    user = potentialUser;
                }
            }

            if (user != null)
            {
                // ─────────────── IDENTITY & CLAIMS ───────────────
                // Creating claims to store the user's role and ID in the encrypted cookie
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Student")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                // Signing in the user with the specified authentication properties
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties { IsPersistent = model.RememberMe });

                // Log Admin and Staff logins to Recent Activity Logs
                if (user.Role?.RoleName == "Admin" || user.Role?.RoleName == "Staff")
                {
                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = user.Id,
                        ActionDesc = "Logged into the system",
                        Timestamp = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Dashboard", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid credentials. Please check your information and try again.");
            return View(model);
        }

        // ─────────────── REGISTER ───────────────
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check username uniqueness
            var existingUser = await _context.Users.AnyAsync(u => u.Username.ToLower() == model.Username.ToLower());
            if (existingUser)
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            // Find or create role
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == model.SelectedRole);
            if (role == null)
            {
                ModelState.AddModelError("SelectedRole", "Selected role is invalid.");
                return View(model);
            }

            var newUser = new User
            {
                Username = model.Username,
                PasswordHash = model.Password,
                RoleId = role.Id,
                IsActive = true
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            if (model.SelectedRole == "Admin")
            {
                var newAdmin = new Admin
                {
                    UserId = newUser.Id,
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email
                };
                _context.Admins.Add(newAdmin);
            }
            else if (model.SelectedRole == "Staff")
            {
                var newStaff = new Staff
                {
                    UserId = newUser.Id,
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email
                };
                _context.Staffs.Add(newStaff);
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Account created successfully! You can now log in as {model.SelectedRole}.";
            return RedirectToAction("Login");
        }

        // ─────────────── FORGOT PASSWORD ───────────────
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            ModelState.Remove("Username");
            ModelState.Remove("StudentId");

            if (!ModelState.IsValid) return View(model);

            User? user = null;
            if (model.SelectedRole == "Student")
            {
                if (string.IsNullOrWhiteSpace(model.StudentId))
                {
                    ModelState.AddModelError("StudentId", "Registration Number is required.");
                    return View(model);
                }
                var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.StudentId == model.StudentId);
                user = student?.User;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.Username))
                {
                    ModelState.AddModelError("Username", "Username is required.");
                    return View(model);
                }
                user = await _context.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.ToLower() && u.Role != null && u.Role.RoleName == model.SelectedRole);
            }

            if (user != null)
            {
                // Generate Token
                user.ResetToken = Guid.NewGuid().ToString();
                user.ResetTokenExpiration = DateTime.Now.AddHours(1);
                _context.Update(user);
                await _context.SaveChangesAsync();

                // Redirect to simulation page
                return RedirectToAction("ForgotPasswordSimulation", new { token = user.ResetToken });
            }

            ModelState.AddModelError(string.Empty, "User not found with the provided information.");
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordSimulation(string token)
        {
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");
            ViewBag.Token = token;
            // Generate the full URL for the simulation link
            ViewBag.ResetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);
            return View();
        }

        // ─────────────── RESET PASSWORD ───────────────
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiration > DateTime.Now);
            if (user == null)
            {
                TempData["Error"] = "The reset link is invalid or has expired.";
                return RedirectToAction("Login");
            }

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == model.Token && u.ResetTokenExpiration > DateTime.Now);
            if (user == null)
            {
                TempData["Error"] = "The reset link is invalid or has expired.";
                return RedirectToAction("Login");
            }

            // Update Password
            user.PasswordHash = model.NewPassword;
            user.ResetToken = null;
            user.ResetTokenExpiration = null;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your password has been reset successfully. You can now log in with your new password.";
            return RedirectToAction("Login");
        }

        // ─────────────── LOGOUT ───────────────
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
