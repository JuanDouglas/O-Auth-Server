using OAuth.Server.Models;
using OAuth.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace OAuth.Server.Controllers
{
    [RoutePrefix("api/OAuth/Login")]
    public class LoginOAuthController : ApiController
    {
        private OAuthEntities db = new OAuthEntities();

        #region Login Step´s

        #region FirstStep
        /// <summary>
        ///// FirstStep fort Login attemp.
        /// </summary>
        /// <param name="user">User Login.</param>
        /// <param name="web_view">Request result for API(true) or WebService(false).</param>
        /// <param name="post">Post Url</param>
        /// <returns></returns>
        [HttpGet]
        [Route("FirstStep")]
        [ResponseType(typeof(LoginFirstStepResult))]
        public async Task<IHttpActionResult> LoginFirstStepAsync(string user, bool web_view, string post)
        {
            Account account = await db.Account.FirstOrDefaultAsync(fs => fs.UserName == user);
            IP userIP = await db.IP.FirstOrDefaultAsync(fs => fs.Adress == HttpContext.Current.Request.UserHostAddress);
            LoginFirstStep loginFirstStep = null;
            bool existEquals = true;

            /* 
             * Valida se o IP já esta cadastrado na base de dados.
             * Caso não esteja adicionado irá adicionar ao banco de dados.
             * Caso contrário não faz nada.
             */
            if (userIP == null)
            {
                db.IP.Add(new IP()
                {
                    Adress = HttpContext.Current.Request.UserHostAddress,
                    AlreadyBeenBanned = false,
                    Confiance = (int)IPConfiance.NONE
                });

                await db.SaveChangesAsync();

                //Atualiza o valor do IP usado no método.
                userIP = await db.IP.FirstOrDefaultAsync(fs => fs.Adress == HttpContext.Current.Request.UserHostAddress);
            }

            /*
             * Valida se a conta buscada existe.
             * Caso não exista irá retornar 'NotFound' é adicionar a tentativa ao banco de dados.
             * Caso exista continua a execução.
             */
            if (account == null)
            {
                db.FailAttemp.Add(new FailAttemp()
                {
                    Date = DateTime.UtcNow,
                    IPAdress = userIP.Adress,
                    AttempType = (int)AttempType.UserNotFound
                });
                await db.SaveChangesAsync();

                if (!web_view)
                {
                    return NotFound();
                }

                return Redirect(GetPathQuery("OAuth/FirstStepFail", new Dictionary<string, string>
                {
                    { "post", post }
                }));
            }

            loginFirstStep = new LoginFirstStep()
            {
                Account = account.ID,
                Date = DateTime.UtcNow,
                IPAdress = userIP.Adress,
                Valid = true
            };

            //Obtém um novo token até que não exista nenhum igual.
            do
            {
                loginFirstStep.Token = GenerateToken(TokenSize.Default);
                if ((await db.LoginFirstStep.FirstOrDefaultAsync(fs => fs.Token == loginFirstStep.Token)) == null)
                {
                    existEquals = false;
                }
            } while (existEquals);

            //Salva no banco esse passo da autenticação.
            db.LoginFirstStep.Add(loginFirstStep);
            await db.SaveChangesAsync();

            /*
             * Valida o tipo de resposta requisitada.
             * Caso seja de API irá retornar o Objeto: 'LoginFirstStep'.
             * Caso contrário continua o método.
             */
            if (!web_view)
            {
                return Ok(new LoginFirstStepResult(loginFirstStep));
            }
            return Redirect(GetPathQuery("Login/SecondStep", new Dictionary<string, string>
            {
                { "key", loginFirstStep.Token },
                { "post", post }
            }));
        }

        public Uri GetPathQuery(string path, Dictionary<string, string> keys)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (var item in keys)
            {
                query.Add(item.Key, item.Value);
            }
            return GetUri(path, query.ToString());
        }
        #endregion

        #region SecondStep
        /// <summary>
        /// Second Step for Authentication.
        /// </summary>
        /// <param name="pwd">Password</param>
        /// <param name="key">First Step Key</param>
        /// <param name="web_view">Request result for API(true) or WebService(false).</param>
        /// <param name="post">Defines '{user-key}'</param>
        /// <returns></returns>
        [HttpGet]
        [Route("SecondStep")]
        [ResponseType(typeof(LoginFirstStepResult))]
        public async Task<IHttpActionResult> LoginSecondStepAsync(string pwd, string key, bool web_view, string post)
        {
            LoginFirstStep loginFirstStep = await db.LoginFirstStep.FirstOrDefaultAsync(fs => fs.Token == key);
            IP userIP = await db.IP.FirstOrDefaultAsync(fs => fs.Adress == HttpContext.Current.Request.UserHostAddress);
            Account account = await db.Account.FirstOrDefaultAsync(fs => fs.ID == loginFirstStep.ID);
            HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.Redirect);
            bool existEquals = true;

            if (post == null)
            {
                post = string.Empty;
            }
            if (post == string.Empty || post == "none")
            {
                UriBuilder builder = new UriBuilder(Request.RequestUri);
                builder.Path = string.Empty;
                builder.Query = string.Empty;
                post = builder.ToString();
            }

            if (loginFirstStep.Date.AddMinutes(15)>DateTime.UtcNow.AddMinutes(-15))
            {
                loginFirstStep.Valid = false;
                db.Entry(loginFirstStep).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            /* 
             * Valida se o IP já esta cadastrado na base de dados.
             * Caso não esteja adicionado irá adicionar ao banco de dados.
             * Caso contrário não faz nada.
             */
            if (userIP == null)
            {
                db.IP.Add(new IP()
                {
                    Adress = HttpContext.Current.Request.UserHostAddress,
                    AlreadyBeenBanned = false,
                    Confiance = (int)IPConfiance.NONE
                });

                await db.SaveChangesAsync();

                //Atualiza o valor do IP usado no método.
                userIP = await db.IP.FirstOrDefaultAsync(fs => fs.Adress == HttpContext.Current.Request.UserHostAddress);
            }

            /*
             * Valida se o usuário iniciou o Login.
             * Caso tenho começado desconsidere.
             * Caso contrário irá adicionar a tentativa falha ao banco de dados.
             */
            if (loginFirstStep == null)
            {
                db.FailAttemp.Add(new FailAttemp()
                {
                    Date = DateTime.UtcNow,
                    IPAdress = userIP.Adress,
                    AttempType = (int)AttempType.SecondStepAttemp
                });
                await db.SaveChangesAsync();

                if (!web_view)
                {
                    return NotFound();
                }
                return Redirect(GetPathQuery("Login/StepFail", new Dictionary<string, string>
            {
                { "post", post }
            }));
            }

            /*
             * Valida se o passo ainda é válido.
             * Caso seja o mesmo desconsidere.
             * Caso contrário irá retornar não autorizado.
             */
            if (!loginFirstStep.Valid)
            {
                db.FailAttemp.Add(new FailAttemp()
                {
                    Date = DateTime.UtcNow,
                    IPAdress = userIP.Adress,
                    AttempType = (int)AttempType.FirstStepInvalid
                });
                await db.SaveChangesAsync();

                if (!web_view)
                {
                    return NotFound();
                }
                return Redirect(GetPathQuery("Login/StepFail", new Dictionary<string, string>
            {
                { "post", post }
            }));
            }


            /*
             * Valida se o IP fornecido é o mesmo do primeiro passo.
             * Caso seja o mesmo desconsidere.
             * Caso contrário irá retornar não autorizado.
             */
            if (loginFirstStep.IPAdress != userIP.Adress)
            {
                db.FailAttemp.Add(new FailAttemp()
                {
                    Date = DateTime.UtcNow,
                    IPAdress = userIP.Adress,
                    AttempType = (int)AttempType.SecondStepAttemp
                });

                await db.SaveChangesAsync();

                if (!web_view)
                {
                    return Unauthorized();
                }
                return Redirect(GetPathQuery("OAuth/StepFail", new Dictionary<string, string>
            {

                { "post", post }
            }));
            }

            /*
             * Valida a senha fornecida.
             * Caso seja igual a do banco continua a execução0.
             * Caso contrário irá retornar 'Unathorized' é adicionar a tentativa falha ao banco de dados.
             */
            if (!PasswordCompare(loginFirstStep.Account1.Password, pwd))
            {
                db.FailAttemp.Add(new FailAttemp()
                {
                    Date = DateTime.UtcNow,
                    IPAdress = userIP.Adress,
                    AttempType = (int)AttempType.IncorrectPassword
                });

                await db.SaveChangesAsync();

                if (!web_view)
                {
                    return Unauthorized();
                }

                return Redirect(GetPathQuery("OAuth/StepFail", new Dictionary<string, string>
            {
                { "post", post }
            })); ;
            }


            Authentication authentication = new Authentication()
            {
                Date = DateTime.UtcNow,
                IPAdress = userIP.Adress,
                LoginFirstStep = loginFirstStep.ID,
                User_Agent = HttpContext.Current.Request.UserAgent
            };

            //Obtém um novo token até que não exista nenhum igual.
            do
            {
                authentication.Token = GenerateToken(TokenSize.Default);
                if ((await db.Authentication.FirstOrDefaultAsync(fs => fs.Token == loginFirstStep.Token)) == null)
                {
                    existEquals = false;
                }
            } while (existEquals);

            if (!web_view)
            {
                return Ok(new LoginResult(authentication));
            }

            httpResponse.Headers.AddCookies(
                new List<CookieHeaderValue>() {
                    new CookieHeaderValue("Authentication", new NameValueCollection() {
                        {"Token", authentication.Token},
                        {"AccountKey",authentication.LoginFirstStep1.Account1.Key }
                    })
                });
            httpResponse.Headers.Add("Location", post);
            return ResponseMessage(httpResponse);
        }

        #endregion

        #endregion

        #region Cryptography services

        public static string CryptographyString(string value)
        {
            return HashGeneration(value);
        }
        public static string HashGeneration(string password)
        {
            // Configurations
            int workfactor = 10; // 2 ^ (10) = 1024 iterations.

            string salt = BCrypt.Net.BCrypt.GenerateSalt(workfactor);
            string hash = BCrypt.Net.BCrypt.HashPassword(password, salt);

            return hash;
        }
        public static bool PasswordCompare(string hash, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        #endregion

        #region Aux
        public enum TokenSize
        {
            Small = 17,
            Default = 36,
            Big = 72,
            VeryBig = 108,
        }
        internal Uri GetUri(string path, string query)
        {
            UriBuilder uriBuilder = new UriBuilder(Request.RequestUri);
            uriBuilder.Path = path;
            uriBuilder.Query = query;
            return new Uri(uriBuilder.ToString());
        }
        public static string GenerateToken(TokenSize tokenSize)
        {
            List<Guid> guids = new List<Guid>();
            string result = string.Empty;
            for (int i = 0; i < (int)((int)tokenSize / 36) + 1; i++)
            {
                guids.Add(Guid.NewGuid());
            }

            for (int i = 0; result.Length < (int)tokenSize && guids.Count > i; i++)
            {
                string guidString = guids[i].ToString();
                foreach (char item in guidString)
                {
                    if (item != '-')
                    {
                        result += item;
                    }

                    if (result.Length >= (int)tokenSize)
                    {
                        break;
                    }
                }
            }
            return result;
        }
        #endregion

        #region ValidUser

        internal async static Task<LoginInformations> ValidLoginAsync(string token, string user_key)
        {
            if (token == null)
            {
                token = string.Empty;
            }
            if (user_key == null)
            {
                user_key = string.Empty;
            }
            OAuthEntities db = new OAuthEntities();
            var context = System.Web.HttpContext.Current;

            //Obtém a autenticação deste usuário
            Authentication logToken = await db.Authentication.FirstOrDefaultAsync(fs => fs.IPAdress == context.Request.UserHostAddress);
            if (logToken == null)
            {
                return new LoginInformations(user_key, token) { IsValid = false, Message = "This 'login token' does not exist or does not exist on the system or is wrong." };
            }

            if (!PasswordCompare(logToken.Token, token.ToString()))
            {
                return new LoginInformations(user_key, token) { IsValid = false, Message = "This 'login token' does not exist or does not exist on the system or is wrong." };
            }

            //Valida o IP fornecido
            if (context.Request.UserHostAddress != logToken.IPAdress)
            {
                return new LoginInformations(user_key, token) { IsValid = false, Message = "This 'login token' does not validing for your IP." };
            }

            //Valida a chave de verificação da tabela User.
            if (logToken.LoginFirstStep1.Account1.Key != user_key)
            {
                return new LoginInformations(user_key, token) { IsValid = false, Message = "This 'login token' does not validing for your IP." };
            }

            return new LoginInformations(user_key, token) { IsValid = true, Message = "This login validation result is success" };
        }
        internal static async Task<LoginInformations> ValidLoginAsync()
        {
            try
            {
                var loginInformations = GetLoginInformations();

                return await ValidLoginAsync(loginInformations.LoginToken, loginInformations.UserKey);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static LoginInformations ValidLogin()
        {
            try
            {
                var loginInformations = GetLoginInformations();

                return ValidLogin(loginInformations.LoginToken, loginInformations.UserKey);

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        internal static LoginInformations ValidLogin(string token, string user_valid_key)
        {
            OAuthEntities db = new OAuthEntities();
            var context = System.Web.HttpContext.Current;
            //Obtém a autenticação deste usuário
            Authentication logToken = db.Authentication.FirstOrDefault(fs => fs.IPAdress == context.Request.UserHostAddress);

            if (logToken == null)
            {
                return new LoginInformations(user_valid_key, token) { IsValid = false, Message = "This 'login token' does not exist or does not exist on the system or is wrong." };
            }

            if (!PasswordCompare(logToken.Token, token.ToString()))
            {
                return new LoginInformations(user_valid_key, token) { IsValid = false, Message = "This 'login token' does not exist or does not exist on the system or is wrong." };
            }

            //Valida o IP fornecido
            if (context.Request.UserHostAddress != logToken.IPAdress)
            {
                return new LoginInformations(user_valid_key, token) { IsValid = false, Message = "This 'login token' does not validing for your IP." };
            }

            //Valida a chave de verificação da tabela User.
            if (logToken.LoginFirstStep1.Account1.Key != user_valid_key)
            {
                return new LoginInformations(user_valid_key, token) { IsValid = false, Message = "This 'login token' does not validing for your IP." };
            }

            return new LoginInformations(user_valid_key, token) { IsValid = true, Message = "This login validation result is success" };
        }
        internal static LoginInformations GetLoginInformations()
        {
            string userKey = string.Empty;
            string loginToken = string.Empty;
            try
            {
                loginToken = HttpContext.Current.Request.Cookies.Get("Autorization").Values.Get("Token");
                userKey = HttpContext.Current.Request.Cookies.Get("Autorization").Values.Get("AccountKey");
            }
            catch (NullReferenceException)
            {
                try
                {
                    loginToken = System.Web.HttpContext.Current.Request.Headers.Get("Autorization");
                    userKey = System.Web.HttpContext.Current.Request.Headers.Get("Account");
                }
                catch (NullReferenceException)
                {
                    try
                    {
                        userKey = System.Web.HttpContext.Current.Request.QueryString.Get("account_key");
                        loginToken = System.Web.HttpContext.Current.Request.QueryString.Get("autorization_token");
                    }
                    catch (NullReferenceException)
                    {
                        userKey = string.Empty;
                        loginToken = string.Empty;
                    }
                }
            }
            return new LoginInformations(userKey, loginToken);
        }
        #endregion

        public class LoginInformations
        {
            public string UserKey { get; set; }
            public string LoginToken { get; set; }
            public bool IsValid { get; set; }
            public string Message { get; set; }
            public LoginInformations(string userKey, string loginToken)
            {
                UserKey = userKey;
                LoginToken = loginToken;
                IsValid = false;
                Message = "No Message here!";
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}