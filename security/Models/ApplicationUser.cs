using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace security.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
    }
}
