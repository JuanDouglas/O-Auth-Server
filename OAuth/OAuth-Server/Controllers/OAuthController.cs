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
[RoutePrefix("api/OAuth")]
    public class OAuthController : ApiController
    {
        [Route("Authorize")]
        [HttpGet]
        [ResponseType(typeof(Authorization))]
        public async Task<IHttpActionResult> Authorize(string app_key) {
            LoginOAuthController.LoginInformations loginInformations = LoginOAuthController.GetLoginInformations();
            if (!loginInformations.IsValid)
            {
                var parseQuery = HttpUtility.ParseQueryString(string.Empty);
                parseQuery["post"] = Request.RequestUri.ToString();
                UriBuilder builder = new UriBuilder(Request.RequestUri)
                {
                    Path = "Login/FirstStep",
                    Query = parseQuery.ToString()
                };

                return Redirect(builder.ToString());
            }
            throw new NotImplementedException();
        }
    }
}