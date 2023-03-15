using System;
using System.ComponentModel.DataAnnotations;

namespace IDEA_Collection.ModelViews
{
    public class ChangePasswordViewModel
    {
        [Key]
        public int CustomerId { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Please email login")]
        public string Email { get; set; }
        [Display(Name = "Avata")]
        public string Avata { get; set; }

        [Display(Name = "current password")]
        [Required(ErrorMessage = "Please enter your current password")]
        public string PasswordNow { get; set; }

        [Display(Name = "A new password")]
        [Required(ErrorMessage = "Please enter a new password")]
        [MinLength(5, ErrorMessage = "You need to set a password of at least 5 characters")]
        public string Password { get; set; }

        [MinLength(5, ErrorMessage = "You need to set a password of at least 5 characters")]
        [Display(Name = "Enter the password again")]
        [Compare("Password", ErrorMessage = "Password incorrect, please try again")]
        public string ConfirmPassword { get; set; }
    }
}
