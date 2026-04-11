using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormitoryManagementSystem.Models
{
    public class Student : BaseEntity
    {
        public int UserId { get; set; }
        public User? User { get; set; }

        // ─────────────── DATA VALIDATION & METADATA ───────────────
        // [Required] ensures the data is strictly personal in the DB level
        // [Display] controls the label text shown in Automated Razor Views
        [Required]
        [Display(Name = "First Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [StringLength(7, MinimumLength = 7)]
        [Display(Name = "Dormitory Registration Number")]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "E-Mail")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public int RoomId { get; set; }
        public Room? Room { get; set; }

        [Required]
        [Display(Name = "Contract Start")]
        public DateTime MembStartDate { get; set; }

        [Required]
        [Display(Name = "Contract End")]
        public DateTime MembEndDate { get; set; }

        // Computed property — no changes needed in views/controllers
        [NotMapped]
        public string FullName => $"{Name} {Surname}";

        // For display with Registration Number disambiguation
        [NotMapped]
        public string FullNameWithRegNo => $"{Name} {Surname} (ID: {StudentId})";
    }
}