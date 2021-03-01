using System.Web.Mvc;

namespace OAuth.Server.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
        public ActionResult Terms()
        {
            return View();
        }
        public ActionResult Policy()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
