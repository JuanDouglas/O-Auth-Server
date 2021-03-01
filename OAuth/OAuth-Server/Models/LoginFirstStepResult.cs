using System;
using System.Linq;

namespace OAuth.Server.Models
{
    public class LoginFirstStepResult
    {
        /// <summary>
        /// Date for First Step.
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// First Step Token.
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// IP for original request.
        /// </summary>
        public IPAdressResult IP { get; set; }
        /// <summary>
        /// Indicates whether the token is still valid.
        /// </summary>
        public bool IsValid { get; set; }
        private OAuthEntities db = new OAuthEntities();

        public LoginFirstStepResult()
        {
        }
        public LoginFirstStepResult(LoginFirstStep loginFirstStep)
        {
            Date = loginFirstStep.Date;
            Token = loginFirstStep.Token;
            IsValid = loginFirstStep.Valid;
            IP = new IPAdressResult(db.IP.FirstOrDefault(fs => fs.Adress == loginFirstStep.IPAdress));
        }
    }
}