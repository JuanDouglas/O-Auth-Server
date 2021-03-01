using Microsoft.VisualStudio.TestTools.UnitTesting;
using OAuth.Server.Controllers;

namespace OAuth.Server.Tests.Controllers
{
    [TestClass]
    public class LoginOAuthControllerTest
    {
        [TestMethod]
        public void GenerateToken()
        {
            // Organizar
            string token = null;

            // Agir
            token = LoginOAuthController.GenerateToken(LoginOAuthController.TokenSize.Big);


            // Declarar

            Assert.IsNotNull(token);
            Assert.IsTrue(token != string.Empty);
            Assert.IsTrue(token.Length != (int)LoginOAuthController.TokenSize.Big);
        }

        public void ValidLogin() {
        }
    }
}
