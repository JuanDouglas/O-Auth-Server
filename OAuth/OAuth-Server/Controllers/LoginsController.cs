using OAuth.Server.Models;
using OAuth.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

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
        public async Task<IHttpActionResult> LoginFirstStepAsync(string user)
        {
            Account account = db.Account.FirstOrDefault(fs => fs.UserName == user);
            IP userIP = db.IP.FirstOrDefault(fs => fs.Adress == HttpContext.Current.Request.UserHostAddress);

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
                    IPAdress = userIP.Adress
                });
                await db.SaveChangesAsync();
                return NotFound();
            }
            throw new NotImplementedException();
        }
    }
}