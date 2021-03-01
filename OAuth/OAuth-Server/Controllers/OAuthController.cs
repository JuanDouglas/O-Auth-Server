using OAuth.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace OAuth.Server.Controllers
{
    public class OAuthController : ApiController
    {
        [HttpPost]
        [ResponseType(typeof(Authorization))]
        public async Task<Account> AutorizeApp() {
            LoginOAuthController.LoginInformations loginInformations = LoginOAuthController.GetLoginInformations();

            throw new NotImplementedException();
        }
    }
}