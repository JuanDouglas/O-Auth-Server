using OAuth.Server.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OAuth.Server.Controllers
{
    public class LoginController : Controller
    {
        private OAuthEntities db = new OAuthEntities();
        // GET: Login
        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("FirstStep","Login",new { post="none"});
        }

        [HttpGet]
        public ActionResult FirstStep(string post)
        {
            ViewBag.Post = post;
            return View("FirstStep");
        }

        [HttpGet]
        public ActionResult FirstStepFail(string post)
        {
            ViewBag.Post = post;
            return View("FirstStepFail");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FirstStep([Bind(Include = "Post,User")] AuthModel model)
        {
            return RedirectToAction("FirstStep", "api/OAuth/Login", new { post = model.Post, user = model.User, web_view = true });
        }
        public ActionResult FirstStepFail(string user, string post)
        {
            ViewBag.Post = post;
            return View("FirstStep");
        }

        [HttpGet]
        public ActionResult SecondStep(string key, string post)
        {
            ViewBag.Post = post;
            ViewBag.Key = key;
            return View("SecondStep");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SecondStep([Bind(Include = "Post,Password,Key")] AuthModel model)
        {
            return RedirectToAction("SecondStep", "api/OAuth/Login", new { post = model.Post, pwd = model.Password, web_view = true, key = model.Key });
        }

        public ActionResult Create(string post) {
           
            return View(new AccountModel() { post = post});
        }
        [HttpPost]
        public async Task<ActionResult> Create([Bind(Include = "IsCompany,UserName,Email,Password,ConfirmPassword,AcceptTerms,post")]AccountModel accountModel)
        {
            if (db.Account.FirstOrDefault(fs=>fs.Email == accountModel.Email)!=null)
            {
                ModelState.AddModelError("Email","Já existe um usuário com esse e-mail.");
            }

            if (db.Account.FirstOrDefault(fs => fs.Email == accountModel.UserName) != null)
            {
                ModelState.AddModelError("UserName", "Já existe um usuário com esse nome de usuário.");
            }

            if (!ModelState.IsValid)
            {
                return View(accountModel);
            }

            Account account = accountModel.GetAccount();

            bool exist = true;
            do
            {
                account.Key = LoginOAuthController.GenerateToken(LoginOAuthController.TokenSize.VeryBig);
                if (db.Account.FirstOrDefault(fs=>fs.Key==account.Key)==null)
                {
                    exist = false;
                }
            } while (exist);

            db.Account.Add(account);
            await db.SaveChangesAsync();

            throw new NotImplementedException();
        }
    }
}