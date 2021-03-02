using OAuth.Server.Controllers;
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
        /// Valor que define se é pessoa fisíca ou jurídica.
        /// </summary>
        [Required]
        [Display(Name = "É ou representa um Empresa ?")]
        public bool IsCompany { get; set; }
        /// <summary>
        /// Nome de usuário.
        /// </summary>
        [Required]
        [Display(Name ="Nome de usuário")]
        [StringLength(50,MinimumLength = 3)]
        public string UserName { get; set; }
        /// <summary>
        /// E-mail.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name ="E-mail", Description = "Seu e-mail de contato.")]
        [StringLength(254, MinimumLength = 5)]
        public string Email { get; set; }
        /// <summary>
        /// Senah da conta.
        /// </summary>
        [Required]
        [Display(Name = "Senha")]
        [StringLength(30, MinimumLength = 8)]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Confirme sua Senha", Prompt = "Confirme sua senha.")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        public bool AcceptTerms { get; set; }

        public string post { get; set; }

        public Account GetAccount() {
            return new Account()
            {
                CreateDate = DateTime.UtcNow,
                Email = Email,
                UserName = UserName,
                Password = LoginOAuthController.CryptographyString(Password)
            };
        }
    }
}