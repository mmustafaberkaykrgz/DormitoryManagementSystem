using System.ComponentModel.DataAnnotations;

namespace DormitoryManagementSystem.Models
{
    public class SettingsViewModel
    {
        public GlobalSettingsViewModel GlobalSettings { get; set; } = new();
        public ProfileSettingsViewModel ProfileSettings { get; set; } = new();
        public List<AuditLog> RecentLogs { get; set; } = new();
        public List<Staff> StaffList { get; set; } = new();
    }

    public class GlobalSettingsViewModel
    {
        [Required]
        [Display(Name = "Dormitory Name")]
        public string DormitoryName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Dormitory Address")]
        public string DormitoryAddress { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Contact Info (Phone/Email)")]
        public string ContactInfo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Default Monthly Due (₺)")]
        [Range(0, 100000)]
        public decimal DefaultMonthlyDue { get; set; } = 0;

        [Required]
        [Display(Name = "Late Penalty Fee (₺)")]
        [Range(0, 50000)]
        public decimal LatePenaltyFee { get; set; } = 0;
    }

    public class ProfileSettingsViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
