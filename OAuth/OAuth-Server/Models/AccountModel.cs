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
        [Display(Name = "É ou representa um Empresa ?")]
        public bool IsCompany { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name ="Nome de usuário")]
        public string UserName { get; set; }
        /// <summary>
        /// E-mail.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name ="E-mail", Description = "Seu e-mail de contato.")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Senha")]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Confirme sua Senha")]
        public string ConfirmPassword { get; set; }

        public bool AcceptTerms { get; set; }
    }
}