using System;
using System.Linq;

namespace OAuth.Server.Models
{
    public class LoginResult
    {
        public string Token { get; set; }
        public DateTime Date { get; set; }
        public string UserAgent { get; set; }
        public IPAdressResult IP { get; set; }
        public string AccountKey { get; set; }
        private OAuthEntities db = new OAuthEntities();

        public LoginResult(Authentication authentication)
        {
            Token = authentication.Token;
            Date = authentication.Date;
            UserAgent = authentication.User_Agent;
            AccountKey = authentication.LoginFirstStep1.Account1.Key;
            IP = new IPAdressResult(db.IP.FirstOrDefault(fs => fs.Adress == authentication.IPAdress));
        }
    }
}