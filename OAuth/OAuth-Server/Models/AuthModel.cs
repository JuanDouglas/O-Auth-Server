using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OAuth.Server.Models
{
    public class AuthModel
    {
        [Required]
        public string User { get; set; }
        [Required]
        public string  Password { get; set; }
        public string Post { get; set; }
    }
}