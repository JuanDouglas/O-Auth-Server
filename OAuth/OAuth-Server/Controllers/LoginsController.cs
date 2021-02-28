using OAuth.Server.Models;
using OAuth.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace OAuth.Server.Controllers
{
    [RoutePrefix("api/OAuth/Login")]
    public class LoginsController : ApiController
    {
        private OAuthEntities db = new OAuthEntities();
        /// <summary>
        /// First Step for Authentication.
        /// </summary>
        /// <param name="user">Account Username</param>
        /// <returns></returns>
        [HttpGet]
        [Route("FirstStep")]
        [ResponseType(typeof(LoginFirstStepResult))]
        public async Task<IHttpActionResult> LoginFirstStep(string user, bool is_api,string post)
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
                    AttempType = (int)AttempType.FirstStepAttemp
                });
                await db.SaveChangesAsync();

                if (is_api)
                {
                    return NotFound();
                }

                return Redirect(GetUri("OAuth/FirstStepFail", $"user={user}&post={post}"));
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
                if (db.LoginFirstStep.FirstOrDefault(fs=>fs.Token== loginFirstStep.Token)==null)
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
            if (is_api)
            {
                return Ok(new LoginFirstStepResult(loginFirstStep));
            }
    
            return Redirect(GetUri("OAuth/FirstStep",$"first_step_key={loginFirstStep.Token}&post={post}"));
        }

        public async Task<IHttpActionResult> LoginSecondStep(string first_step_key, string post) {


            throw new NotImplementedException();
        }

        public enum TokenSize
        {
            Small = 6,
            Default = 12,
            Big = 24,
            VeryBig = 48,
            ForWhat = 86
        }
        public Uri GetUri(string path,string query) {
            UriBuilder uriBuilder = new UriBuilder(Request.RequestUri);
            uriBuilder.Path = path;
            uriBuilder.Query = query;
            return new Uri(uriBuilder.ToString());
        }
        public static string GenerateToken(TokenSize tokenSize)
        {
            int size = (int)tokenSize;
            throw new NotImplementedException();
        }
    }
}