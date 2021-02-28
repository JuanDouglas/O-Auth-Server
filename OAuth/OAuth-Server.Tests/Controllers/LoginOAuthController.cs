using Microsoft.VisualStudio.TestTools.UnitTesting;
using OAuth.Server.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OAuth_Server.Tests.Controllers
{
    [TestClass]
    public class LoginOAuthControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Organizar
            string token = null;

            // Agir
            token = LoginOAuthController.GenerateToken(LoginOAuthController.TokenSize.Big);
            // Declarar

            Assert.IsNotNull(token);
        }
    }
}
