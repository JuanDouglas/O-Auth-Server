using OAuth.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OAuth.Server.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        [HttpGet]
        public ActionResult Index()
        {
            return FirstStep("none");
        }

        [HttpGet]
        public ActionResult FirstStep(string post)
        {
            ViewBag.Post = post;
            return View("FirstStep");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FirstStep([Bind(Include = "Post,User")]AuthModel model) {
            return RedirectToAction("FirstStep", "api/OAuth/Login", new { post = model.Post, user=model.User, web_view=true});
        }
        public ActionResult FirstStepFail(string user, string post)
        {
            ViewBag.Post = post;
            return View("FirstStep");
        }
    }
}