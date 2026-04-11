using System.ComponentModel.DataAnnotations;

namespace DormitoryManagementSystem.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Please select your role.")]
        public string SelectedRole { get; set; } = "Admin";

        public string? Username { get; set; } // For Admin/Staff

        [StringLength(7, MinimumLength = 7, ErrorMessage = "Registration Number must be exactly 7 digits.")]
        public string? StudentId { get; set; } // For Student
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Password must be at least 4 characters long.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
