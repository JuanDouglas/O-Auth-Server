using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace OAuth.Server.Controllers
{
    [RoutePrefix("api/OAuth/Login")]
    public class LoginsController : ApiController
    {
        public IHttpActionResult LoginFirstStep(string username) { 
        
        }
    }
}