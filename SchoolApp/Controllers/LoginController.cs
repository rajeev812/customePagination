using SchoolApp.Entity;
using SchoolApp.Entity.Context;
using SchoolApp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SchoolApp.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        [HttpGet]
        public ActionResult Index()
        {
            return View(new User());
        }
        [HttpPost]
        public ActionResult Index(User objUser)
        {
            try
            {
                using (var context = new SchoolAppContext())
                {
                    var getUser = (from s in context.Users where s.UserName == objUser.UserName  select s).FirstOrDefault();
                    if (getUser != null)
                    {
                        var hashCode = getUser.VCode;
                        var encodingPasswordString = Helper.EncodePassword(objUser.Password, hashCode);
                        var query = (from s in context.Users where (s.UserName == objUser.UserName) && s.Password.Equals(encodingPasswordString) select s).FirstOrDefault();
                        if (query != null)
                        {
                            Session["UserType"] = getUser;
                            return RedirectToAction("DashBoard", "Login");
                        }
                        ViewBag.ErrorMessage = "Invallid User Name or Password";
                        return View();
                    }
                    ViewBag.ErrorMessage = "Invallid User Name or Password";
                    return View();
                }
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = " Error!!! " +e.Message;
                return View();
            }
        }

        public ActionResult DashBoard()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
           return View( new User());
        }
        [HttpPost]
        public ActionResult Register(User ObjUser)
        {
            try
            {
                using (var context = new SchoolAppContext())
                {
                    var chkUser = (from s in context.Users where s.UserName == ObjUser.UserName  select s).FirstOrDefault();
                    if (chkUser == null)
                    {
                        var keyNew = Helper.GeneratePassword(10);
                        var password = Helper.EncodePassword(ObjUser.Password, keyNew);
                        ObjUser.Password = password;
                        ObjUser.ConfirmPassword = password;
                        ObjUser.CreateDate = DateTime.Now;
                        ObjUser.ModifyDate = DateTime.Now;
                        ObjUser.VCode = keyNew;
                        ObjUser.UserType = "Admin";
                        context.Users.Add(ObjUser);
                        context.SaveChanges();
                        ModelState.Clear();
                        return RedirectToAction("Index", "Login");
                    }
                    ViewBag.ErrorMessage = "User Allredy Exixts!!!!!!!!!!";
                    return View(ObjUser);
                }
            }
            catch(Exception e)
            {
                ViewBag.ErrorMessage = "error" + e.Message;
                return View(ObjUser);
            }
        }
    }
}