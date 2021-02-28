﻿using OAuth.Server.Models;
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
    public class LoginOAuthController : ApiController
    {
        private OAuthEntities db = new OAuthEntities();
        #region LoginStep´s

        #region FirstStep
        /// <summary>
        /// FirstStep fort Login attemp.
        /// </summary>
        /// <param name="user">User Login.</param>
        /// <param name="is_api">Request result for API(true) or WebService(false).</param>
        /// <param name="post">Post Url</param>
        /// <returns></returns>
        [HttpGet]
        [Route("FirstStep")]
        [ResponseType(typeof(LoginFirstStepResult))]
        public async Task<IHttpActionResult> LoginFirstStepAsync(string user, bool is_api, string post)
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
            if (is_api)
            {
                return Ok(new LoginFirstStepResult(loginFirstStep));
            }

            return Redirect(GetUri("OAuth/FirstStep", $"first_step_key={loginFirstStep.Token}&post={post}"));
        }
        #endregion

        #region SecondStep
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pwd">Password</param>
        /// <param name="key">First Step Key</param>
        /// <param name="is_api">Request result for API(true) or WebService(false).</param>
        /// <param name="post"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SecondStep")]
        [ResponseType(typeof(LoginFirstStepResult))]
        public async Task<IHttpActionResult> LoginSecondStep(string pwd, string key, bool is_api, string post)
        {
            LoginFirstStep loginFirstStep = db.LoginFirstStep.FirstOrDefault(fs => fs.Token == key);
            IP userIP = db.IP.FirstOrDefault(fs => fs.Adress == HttpContext.Current.Request.UserHostAddress);
            bool existEquals = true;


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

                if (is_api)
                {
                    return NotFound();
                }
                return Redirect(GetUri("OAuth/SecondStepFail", $"post={post}"));
            }





            throw new NotImplementedException();
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
    }
}