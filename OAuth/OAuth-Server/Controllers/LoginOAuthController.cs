using OAuth.Server.Models;
using OAuth.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
            Account account = db.Account.FirstOrDefault(fs => fs.UserName == user);
            IP userIP = db.IP.FirstOrDefault(fs => fs.Adress == HttpContext.Current.Request.UserHostAddress);
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
                userIP = db.IP.FirstOrDefault(fs => fs.Adress == HttpContext.Current.Request.UserHostAddress);
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
                    { "user", loginFirstStep.Token },
                    { "post", post }
                }));
            }

            loginFirstStep = new LoginFirstStep()
            {
                Account = account.ID,
                Date = DateTime.UtcNow,
                IPAdress = userIP.Adress
            };

            //Obtém um novo token até que não exista nenhum igual.
            do
            {
                loginFirstStep.Token = GenerateToken(TokenSize.Default);
                if (db.LoginFirstStep.FirstOrDefault(fs => fs.Token == loginFirstStep.Token) == null)
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
            return Redirect(GetPathQuery("OAuth/SecondStep", new Dictionary<string, string>
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
        public async Task<IHttpActionResult> LoginSecondStep(string pwd, string key, bool web_view, string post)
        {
            LoginFirstStep loginFirstStep = db.LoginFirstStep.FirstOrDefault(fs => fs.Token == key);
            IP userIP = db.IP.FirstOrDefault(fs => fs.Adress == HttpContext.Current.Request.UserHostAddress);
            Account account = db.Account.FirstOrDefault(fs => fs.ID == loginFirstStep.ID);
            HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.Redirect);
            bool existEquals = true;

            if (post==null)
            {
                post = string.Empty;
            }
            if (post==string.Empty||post=="none")
            {
                UriBuilder builder = new UriBuilder(Request.RequestUri);
                builder.Path = string.Empty;
                builder.Query = string.Empty;
                post = builder.ToString(); 
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
                userIP = db.IP.FirstOrDefault(fs => fs.Adress == HttpContext.Current.Request.UserHostAddress);
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
                return Redirect(GetPathQuery("OAuth/SecondStepFail", new Dictionary<string, string>
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
            if (!PasswordCompare(account.Password, pwd))
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
                if (db.Authentication.FirstOrDefault(fs => fs.Token == loginFirstStep.Token) == null)
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
            httpResponse.Headers.Add("Location",post);
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


        public enum TokenSize
        {
            Small = 17,
            Default = 36,
            Big = 72,
            VeryBig = 108,
        }
        public Uri GetUri(string path, string query)
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

        public static string GetQuery(IDictionary<string, string> keyValues)
        {
            throw new NotImplementedException();
        }
    }
}