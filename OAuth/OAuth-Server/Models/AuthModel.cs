using System.ComponentModel.DataAnnotations;

namespace OAuth.Server.Models
{
    public class AuthModel
    {
        [Required]
        public string User { get; set; }
        [Required]
        public string Password { get; set; }
        public string Post { get; set; }
        public string Key { get; set; }
    }
}