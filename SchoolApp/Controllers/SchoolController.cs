using SchoolApp.Entity;
using SchoolApp.Entity.Context;
using SchoolApp.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SchoolApp.Controllers
{
    public class SchoolController : Controller
    {
        // GET: School
        private SchoolAppContext db = new SchoolAppContext();
        public ActionResult Index()
        {
            School obj = new School();
            obj.SchoolList = db.Schools
                     .Where(x => x.ParentId == 0)
                     .Select(x => new { x.SchoolId, x.SchoolName })
                     .ToDictionary(x => x.SchoolId, x => x.SchoolName);
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(School obj)
        {
            if (obj.SchoolList == null)
            {
                obj.SchoolList = db.Schools
                    .Where(x => x.ParentId == 0)
                    .Select(x => new { x.SchoolId, x.SchoolName })
                    .ToDictionary(x => x.SchoolId, x => x.SchoolName);

            }
            //if (ModelState.IsValid)
            //{
            if (obj.IsHavingBranch)
            {
                obj.IsHavingBranch = false;
                obj.ParentId = obj.SchoolId;
                var currentvalue = db.Schools.Where(x => x.ParentId == obj.SchoolId).Max(x => x.BranchId);
                if (currentvalue == null)
                {
                    obj.BranchId = 1;
                }
                else
                {
                    obj.BranchId = db.Schools.Max(x => x.BranchId) + 1;
                }

            }
            else { obj.IsHavingBranch = true; }
            using (var context = new SchoolAppContext())
            {
                if (obj.IsHavingBranch==false)
                {
                    var schAbb = obj.SchoolName.Replace(".", string.Empty).Substring(0, 2);
                    var value = Convert.ToInt32(db.Schools.Max(x => (int?)x.SchoolId) ?? 0) + 1;
                    obj.SchRegNo = schAbb.ToUpper() + value.ToString("D5")+"C"+obj.BranchId;
                }
                else
                {
                    var schAbb = obj.SchoolName.Replace(".", string.Empty).Substring(0, 2);
                    var value = Convert.ToInt32(db.Schools.Max(x => (int?)x.SchoolId) ?? 0) + 1;
                    obj.SchRegNo = schAbb.ToUpper() + value.ToString("D5");
                }
               
                if (!context.Schools.Any(x => x.SchoolName == obj.SchoolName))
                {
                    context.Schools.Add(obj);
                    await context.SaveChangesAsync();
                    //User Adding for school
                    var chkUser = (from s in context.Users where s.UserName == obj.SchRegNo select s).FirstOrDefault();
                    if (chkUser == null)
                    {
                            User ObjUser = new Entity.User();
                            var keyNew = Helper.GeneratePassword(10);
                            var password = Helper.EncodePassword(obj.SchRegNo, keyNew);
                            ObjUser.Password = password;
                            ObjUser.ConfirmPassword = password;
                            ObjUser.CreateDate = DateTime.Now;
                            ObjUser.ModifyDate = DateTime.Now;
                            ObjUser.VCode = keyNew;
                            ObjUser.UserName = obj.SchRegNo;
                        ObjUser.UserType = "School";
                        db.Users.Add(ObjUser);
                            await db.SaveChangesAsync();
                        ModelState.Clear();
                        //return RedirectToAction("Index", "Login");
                    }
                    TempData["Message"] = obj.SchRegNo+ " Added Succesfully!";
                    //return RedirectToAction("Index");
                    return new RedirectResult(@"~\School\Index");
                    //return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Duplicate School Name");
                }

            }
            //}
            //else
            //{
            //    var errorlist= ModelState.Values.SelectMany(v => v.Errors);
            //}

            return View(obj);
        }

        public ActionResult Details()
        {
            ViewBag.SchoolList = db.Schools
                    .Where(x => x.ParentId == 0)
                    .Select(x => new { x.SchoolId, x.SchoolName })
                    .ToDictionary(x => x.SchoolId, x => x.SchoolName);
            return View();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult GetGridContent(int currentPage, int pageSize, string SchoolName,string SchoolAddress, string SchoolReg)
        {
            Expression<Func<School, object>> orderBy = x => x.SchoolName;
            Expression<Func<School, bool>> predicate = GetWherePrediction(SchoolName, SchoolAddress, SchoolReg);
            var retval = this.GetPredicateQueryable(predicate);
            int totalCount = 0;
            var dataRetval = GetGridData(retval, out totalCount, currentPage, pageSize, orderBy).ToList();

            var data = new GenericGridModel<School>
            {
                ItemDetails = dataRetval,
                TotalCount = totalCount,
                currentPage = currentPage
            };

            if (retval == null)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private Expression<Func<School, bool>> GetWherePrediction(string SchoolName, string SchoolAddress, string SchoolReg)
        {
            Expression<Func<School, bool>> predicate = null;
            if (!string.IsNullOrEmpty(SchoolName))
            {
                predicate = x => x.SchoolName.Equals(SchoolName);
            }
            if (!string.IsNullOrEmpty(SchoolAddress))
            {
                predicate = x => x.Address.Contains(SchoolAddress);
            }
            if (!string.IsNullOrEmpty(SchoolReg))
            {
                predicate = x => x.SchRegNo.Equals(SchoolReg);
            }
            return predicate;
        }

        public virtual IQueryable<School> GetPredicateQueryable(Expression<Func<School, bool>> predicate)
        {
            IQueryable<School> dataQuery = null;
            if (predicate == null)
            {
                dataQuery = db.Schools;
            }
            else
            {
                dataQuery = db.Schools.Where(predicate);
            }
            return dataQuery;
        }

        public virtual IEnumerable<School> GetGridData(IQueryable<School> dataQuery, out int total, int currentPage, int pageSize, Expression<Func<School, object>> orderBy, bool isDescending = false)
        {
            IEnumerable<School> data = null;
            if (isDescending)
            {
                data = dataQuery.OrderByDescending(orderBy).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                data = dataQuery.OrderByDescending(orderBy).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            }
            total = dataQuery.Count();
            return data;
        }

        
        //delete code
        [HttpPost]
        public async Task<JsonResult> DeleteSchool(School obj)
        {
            School sch = await db.Schools.FindAsync(obj.SchoolId);
            List<School> schList = db.Schools.Where(x => x.ParentId == sch.SchoolId).ToList();
            if (schList.Count > 0)
            {
                foreach (var item in schList)
                {
                    db.Schools.Remove(item);
                }
            }
            db.Schools.Remove(sch);
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> EditSchool(School obj)
        {
            if (ModelState.IsValid)
            {
                var schAbb = obj.SchoolName.Split(' ').ToList();
                foreach (var item in schAbb)
                {
                    obj.SchRegNo = obj.SchRegNo + item[0];
                }
                var value = Convert.ToInt32(db.Schools.Where(x => x.SchoolId == obj.SchoolId).Max(x => (int?)x.SchoolId) ?? 0) + 1;
                obj.SchRegNo = obj.SchRegNo.ToUpper() + value.ToString("D3");

                db.Entry(obj).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
    }
}