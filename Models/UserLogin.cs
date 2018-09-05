using System.ComponentModel.DataAnnotations;

namespace ahydrax.Servitor.Models
{
    public class UserLogin
    {
        [Required]
        [Display(Name = "Login")]
        public string Login { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public string Reason { get; set; }
    }
}
