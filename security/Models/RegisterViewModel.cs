using System.ComponentModel.DataAnnotations;

namespace security.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name ="Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100,ErrorMessage ="the {0} must be at least {2} charecter long",MinimumLength =6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

       [Required]
       [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage ="the password and confirmpassword should match")]
        public string ConfirmPassword { get; set; }
        public string Name { get; set; }
    } 
}
