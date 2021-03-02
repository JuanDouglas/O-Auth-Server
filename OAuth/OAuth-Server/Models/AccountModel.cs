using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OAuth.Server.Models
{
    public class AccountModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public bool IsCompany { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string UserName { get; set; }
        /// <summary>
        /// E-mail.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }

        public bool AcceptTerms { get; set; }
    }
}