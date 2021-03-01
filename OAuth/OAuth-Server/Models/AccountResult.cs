using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OAuth.Server.Models
{
    public class AccountResult
    {
        public int ID { get; set; }
        /// <summary>
        /// Unique Key for User.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Username for Login.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Account Create Date.
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// E-mail.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Valided E-mail.
        /// </summary>
        public bool IsValidEmail { get; set; }

        public AccountResult(Account account) {
            Key = account.Key;
            UserName = account.UserName;
            CreateDate = account.CreateDate;
            Email = account.Email;
            IsValidEmail = account.ValidLogin;
            ID = account.ID;
        }
    }
}