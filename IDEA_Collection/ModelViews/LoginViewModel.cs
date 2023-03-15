using System;
using System.ComponentModel.DataAnnotations;

namespace IDEA_Collection.ModelViews
{
    public class LoginViewModel
    {
        [Key]
        [MaxLength(100)]
        [Required(ErrorMessage = ("Please enter Email"))]
        [Display(Name = "Email address")]
        [EmailAddress(ErrorMessage = "Email format wrong")]
        public string UserName { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Please enter a password")]
        [MinLength(5, ErrorMessage = "You need to set a password of at least 5 characters")]
        public string Password { get; set; }
    }
}
