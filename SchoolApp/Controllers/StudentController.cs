using SchoolApp.Entity;
using SchoolApp.Entity.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Rotativa.MVC;
using Rotativa.Core;
using ClosedXML.Excel;
using System.IO;
using System.Transactions;

namespace SchoolApp.Controllers
{
    public class StudentController : Controller
    {
        private SchoolAppContext db = new SchoolAppContext();
        // GET: Student
        public ActionResult Index()
        {
            Student model = new Student();
            User loginuser = Session["UserType"] as User;
            model.schoolName = db.Schools
                 .Where(x => x.ParentId == 0)
                 .Select(x => new { x.SchoolId, x.SchoolName })
                 .ToDictionary(x => x.SchoolId, x => x.SchoolName);

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(Student studentEntryCls)
        {
            studentEntryCls.IsBranch = !studentEntryCls.IsBranch;
            studentEntryCls.SchoolsName = db.Schools.Where(x => x.SchoolId == studentEntryCls.SchooldId).Select(x => x.SchoolName).FirstOrDefault();
            studentEntryCls.SchoolsChildName = db.Schools.Where(x => x.SchoolId == studentEntryCls.SchooldChildId).Select(x => x.SchoolName).FirstOrDefault();
            if (ModelState.IsValid)
            {
                using (var context = new SchoolAppContext())
                {
                    context.Students.Add(studentEntryCls);
                    await context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            if (studentEntryCls.schoolName == null)
            {
                ModelState.AddModelError(string.Empty, "Please select school name");
                using (var context = new SchoolAppContext())
                {
                    studentEntryCls.schoolName = db.Schools
                    .Where(x => x.ParentId == 0)
                    .Select(x => new { x.SchoolId, x.SchoolName })
                    .ToDictionary(x => x.SchoolId, x => x.SchoolName);
                }
                // return View(studentEntryCls);
            }

            return View(studentEntryCls);
        }

        [HttpPost]
        public ActionResult UploadExcel(Student obj, HttpPostedFileBase FileUpload)
        {
            obj.SchoolsName = db.Schools.Where(x => x.SchoolId == obj.SchooldId).Select(x => x.SchoolName).FirstOrDefault();
            obj.SchoolsChildName = db.Schools.Where(x => x.SchoolId == obj.SchooldChildId).Select(x => x.SchoolName).FirstOrDefault();
            if (obj.SchooldId == 0)
            {
                TempData["Message"] = " Error!!! Please select school name ";
                //return RedirectToAction("Index");
                return new RedirectResult(@"~\Student\Index");
            }
            List<string> data = new List<string>();
            if (FileUpload != null)
            {
                // tdata.ExecuteCommand("truncate table OtherCompanyAssets");  
                if (FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string filename = FileUpload.FileName;
                    string targetpath = Server.MapPath("~/Doc/");
                    FileUpload.SaveAs(targetpath + filename);
                    string pathToExcelFile = targetpath + filename;
                    try
                    {
                        using (XLWorkbook workBook = new XLWorkbook(pathToExcelFile))
                        {
                            //Read the first Sheet from Excel file.
                            IXLWorksheet workSheet = workBook.Worksheet(1);

                            //Create a new DataTable.
                            DataTable dt = new DataTable();

                            //Loop through the Worksheet rows.
                            bool firstRow = true;
                            foreach (IXLRow row in workSheet.Rows())
                            {
                                //Use the first row to add columns to DataTable.
                                if (row.IsEmpty())
                                {
                                    row.Delete();
                                }


                                if (firstRow)
                                {
                                    foreach (IXLCell cell in row.Cells())
                                    {
                                        dt.Columns.Add(cell.Value.ToString());
                                    }
                                    firstRow = false;
                                }
                                else
                                {
                                    //Add rows to DataTable.
                                    dt.Rows.Add();
                                    int i = 0;
                                    foreach (IXLCell cell in row.Cells())
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                                        i++;
                                    }
                                }
                            }
                            //insert to db code
                            //List<DataRow> list = dt.AsEnumerable().ToList();
                            //List<DataRow> list = new List<DataRow>(dt.Select());
                            List<Student> ExcelData = (from DataRow dr in dt.Rows
                                                       select new Student()
                                                       {
                                                           StudentName = dr["StudentName"].ToString(),
                                                           StudentRegNumber = dr["StudentRegNumber"].ToString(),
                                                           FatherName = dr["FatherName"].ToString(),
                                                           FatherMobileNo = dr["FatherMobileNo"].ToString(),
                                                           MotherName = dr["MotherName"].ToString(),
                                                           MotherMobileNo = dr["MotherMobileNo"].ToString(),
                                                           EmailId = dr["EmailId"].ToString(),
                                                           AddCls = dr["AddCls"].ToString(),
                                                           SchoolsName = obj.SchoolsName,
                                                           SchooldId = obj.SchooldId,
                                                           SchooldChildId = obj.SchooldChildId
                                                       }).ToList();

                            using (TransactionScope scope = new TransactionScope())
                            {
                                SchoolAppContext context = null;
                                try
                                {
                                    context = new SchoolAppContext();
                                    context.Configuration.AutoDetectChangesEnabled = false;
                                    context.Configuration.ValidateOnSaveEnabled = false;
                                    int count = 0;
                                    foreach (var entityToInsert in ExcelData)
                                    {
                                        ++count;
                                        context = AddToContext(context, entityToInsert, count, 100, true);
                                    }

                                    context.SaveChanges();
                                }
                                finally
                                {
                                    if (context != null)
                                        context.Dispose();
                                }

                                scope.Complete();
                            }




                            //using (var context = new SchoolAppContext())
                            //{
                            //    context.Configuration.AutoDetectChangesEnabled = false;
                            //    context.Configuration.ValidateOnSaveEnabled = false;

                            //    for (var i = 0; i <= 100; i++)
                            //    {
                            //        context.Students.Add(ExcelData);
                            //    }

                            //    await context.SaveChangesAsync();
                            //    //return RedirectToAction("Index");
                            //}

                            //foreach (var a in ExcelData)
                            //{
                            //    try
                            //    {
                            //        if (a.StudentName != "" && a.StudentRegNumber != "" && a.AddCls != "" && a.FatherName != "" && a.MotherName != "" && a.FatherMobileNo != "")
                            //        {
                            //            Student TU = new Student();
                            //            TU.StudentName = a.StudentName;
                            //            TU.SchooldId = a.SchooldId;
                            //            TU.SchooldChildId = a.SchooldChildId;
                            //            TU.StudentRegNumber = a.StudentRegNumber;
                            //            TU.AddCls = a.AddCls;
                            //            TU.FatherName = a.FatherName;
                            //            TU.FatherMobileNo = a.FatherMobileNo;
                            //            TU.MotherName = a.MotherName;
                            //            TU.MotherMobileNo = a.MotherMobileNo;
                            //            TU.EmailId = a.EmailId;
                            //            TU.SchoolsName = obj.SchoolsName;


                            //            using (var context = new SchoolAppContext())
                            //            {
                            //                context.Configuration.AutoDetectChangesEnabled = false;
                            //                context.Students.Add(TU);
                            //                await context.SaveChangesAsync();
                            //                //return RedirectToAction("Index");
                            //            }
                            //        }
                            //        else
                            //        {
                            //            data.Add("<ul>");
                            //            if (a.StudentRegNumber == "" || a.StudentRegNumber == null) TempData["Message"] = "registration number is required";
                            //            if (a.StudentName == "" || a.StudentName == null) TempData["Message"] = "Student name is required";
                            //            if (a.FatherMobileNo == "" || a.FatherMobileNo == null) TempData["Message"] = "ContactNo is required";

                            //            return new RedirectResult(@"~\Student\Index");
                            //            // return Json(data, JsonRequestBehavior.AllowGet);
                            //        }
                            //    }

                            //    catch (DbEntityValidationException ex)
                            //    {
                            //        foreach (var entityValidationErrors in ex.EntityValidationErrors)
                            //        {

                            //            foreach (var validationError in entityValidationErrors.ValidationErrors)
                            //            {

                            //                Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);

                            //            }

                            //        }
                            //    }
                            //}
                        }
                    }
                    catch (Exception e)
                    {
                        // System.IO.File.Create(Server.MapPath("~/ExcelTemplates/LoginController.txt"));
                        StreamWriter sw = new StreamWriter(Server.MapPath("~/ExcelTemplates/Log.txt"), true);
                        sw.WriteLine(e.InnerException.Message);
                        sw.Flush();
                        sw.Close();
                    }
                    //CatchBlock
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    // return Json("success", JsonRequestBehavior.AllowGet);
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Message"] = "Only Excel file format is allowed";
                    return new RedirectResult(@"~\Student\Index");
                    // return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                TempData["Message"] = "Please choose Excel file";
                return new RedirectResult(@"~\Student\Index");
                //return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Details()
        {
            Student model = new Student();
            User loginuser = Session["UserType"] as User;
            var School = db.Schools.Where(x => x.SchRegNo.Equals(loginuser.UserName)).FirstOrDefault();
            //if (School.BranchId == null)
            //{

            //}
            model.SchooldId = School.SchoolId;
            model.SchooldChildId = School.ParentId;
            return View(model);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult GetGridContent(int currentPage, int pageSize, string stName, string SchName, string MobileNo, int SchooldId, int chldId)
        {
            Expression<Func<Student, object>> orderBy = x => x.StudentName;
            Expression<Func<Student, bool>> predicate = GetWherePrediction(stName, SchName, MobileNo, SchooldId, chldId);
            var retval = this.GetPredicateQueryable(predicate);
            //var retval = GetGridData(currentPage, pageSize, orderBy).ToList();

            int totalCount = 0;
            var dataRetval = GetGridData(retval, out totalCount, currentPage, pageSize, orderBy).ToList();

            var data = new GenericGridModel<Student>
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

        private Expression<Func<Student, bool>> GetWherePrediction(string studentname, string SchoolName, string mobile, int SchooldId, int chldId)
        {
            Expression<Func<Student, bool>> predicate = null;
            if (!string.IsNullOrEmpty(studentname))
            {
                predicate = x => x.StudentName.Equals(studentname);
            }
            if (!string.IsNullOrEmpty(mobile))
            {
                predicate = x => x.FatherMobileNo.Contains(mobile);
            }
            if (SchoolName!="0")
            {
                int childId = Convert.ToInt32(SchoolName);
                predicate = x => x.SchooldChildId.Equals(childId);
            }
            else
            {
                if (chldId == 0)
                {
                    predicate = x => x.SchooldId.Equals(SchooldId);
                }
                if (chldId != 0)
                {
                    predicate = x => x.SchooldChildId.Equals(SchooldId);
                }
            }


            return predicate;
        }


        public virtual IQueryable<Student> GetPredicateQueryable(Expression<Func<Student, bool>> predicate)
        {
            IQueryable<Student> dataQuery = null;
            if (predicate == null)
            {
                dataQuery = db.Students;
            }
            else
            {
                dataQuery = db.Students.Where(predicate);
            }
            return dataQuery;
        }

        public virtual IEnumerable<Student> GetGridData(IQueryable<Student> dataQuery, out int total, int currentPage, int pageSize, Expression<Func<Student, object>> orderBy, bool isDescending = false)
        {
            IEnumerable<Student> data = null;
            // var dataQuery = db.Students;
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
        public async Task<JsonResult> DeleteStudent(Student obj)
        {
            // var result;
            Student st = await db.Students.FindAsync(obj.StudentId);
            db.Students.Remove(st);
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
            //return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<JsonResult> EditStudent(Student obj)
        {
            if (ModelState.IsValid)
            {
                db.Entry(obj).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetBranchSchool(int SchoolId = 0)
        {
            var result = db.Schools.Where(x => x.ParentId == SchoolId).Select(x => new { SchooldChildId = x.SchoolId, SchoolsChildName = x.SchoolName }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        ///
        ///Pdf
        ///
        public ActionResult Print()
        {
            string footer = "--footer-right \"Date: [date] [time]\" " + "--footer-center \"Page: [page] of [toPage]\" --footer-line --footer-font-size \"9\" --footer-spacing 5 --footer-font-name \"calibri light\"";
            List<Student> result = db.Students.ToList();
            if (result == null)
            {
                result = new List<Student>();
            }
            // return new ActionAsPdf("PdfStudent", result);
            return new ViewAsPdf("PdfStudent", result)
            {
                FileName = "Student.pdf",
                RotativaOptions = new DriverOptions() { CustomSwitches = footer }
            };
        }

        public ActionResult PdfStudent()
        {
            return View();
        }

        public void DownloadExcelStudent()
        {
            List<Student> data = db.Students.ToList();
            if (data == null)
            {
                data = new List<Student>();
            }

            var tempdata = data.Select((value, i) => new { SlNo = i + 1, value.StudentName, value.StudentRegNumber, value.AddCls, value.FatherName, value.FatherMobileNo, value.MotherName, value.MotherMobileNo, value.EmailId, value.SchoolsName }).ToList();
            DataTable objds = ToDataTableFromList.ToDataTable(tempdata);
            string TemplateName = Server.MapPath(@"~/ExcelTemplates/StudentDetailsDetails.xlsx");
            string FileName = "StudentDetailsDetails" + DateTime.Now.ToShortDateString();
            string SheetName = "StudentDetailsDetails";
            int StartIndexData = 5;

            using (XLWorkbook wb = new XLWorkbook(TemplateName))
            {
                if (objds != null)
                {
                    ExcelGenertae(objds, wb, StartIndexData, FileName, SheetName);

                }
                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    memoryStream.Close();
                }
                //Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=" + FileName + ".xlsx");
                Response.End();
            }
        }

        private void ExcelGenertae(DataTable objds, XLWorkbook wb, int startIndexData, string fileName, string sheetName)
        {
            var ws = wb.Worksheets.Worksheet(sheetName);
            string Range = "";
            char StartCharData1 = ' ';
            char StartCharData2 = 'A';
            string StartCharData = StartCharData1.ToString().Trim() + StartCharData2.ToString();
            //StartIndexData;// = 7;// header section will take 6 rows after data will come

            for (int i = 0; i < objds.Rows.Count; i++)
            {

                for (int j = 0; j < objds.Columns.Count; j++)
                {
                    StartCharData = StartCharData1.ToString().Trim() + StartCharData2.ToString();
                    string DataCell = StartCharData.ToString() + startIndexData;
                    var a = objds.Rows[i][j];//.ToString();                               

                    ws.Cell(DataCell).SetValue(a);
                    ws.Cell(DataCell).Style.Alignment.WrapText = true;
                    if (StartCharData2 == 'Z')
                    {
                        StartCharData1 = 'A';
                        StartCharData2 = 'A';
                    }
                    else
                    {
                        StartCharData2++;
                    }
                }
                StartCharData1 = ' ';
                StartCharData2 = 'A';
                startIndexData++;
            }

            int RowIndex = 3;

            int ColIndex = objds.Columns.Count;
            //  formatRange.EntireRow.Font.Bold = true;
            // xlWorkSheet.Cells[1, 5] = "Bold";
            var DateFillRange = GetExcelColumnName(RowIndex, ColIndex);
            ws.Cell(DateFillRange).Value = "Date : " + DateTime.Now.Date.ToShortDateString();
            //ws.Cell("A2").Value = "School Name :" + branchname;
            //ws.Cell("A3").Value = "S Adddress :" + branchAddress;
            var LastChar = StartCharData;
            // string financicialYear = "B3";
            int TotalRows = objds.Rows.Count + 4;
            var dateFillrange = LastChar + 3;
            //var financialYear = "2016 - 17";
            //ws.Cell(financicialYear).Value = "Financial Year: " + financialYear;

            Range = "A4:" + LastChar + TotalRows;
            string RangeBorder = "A4:" + LastChar + TotalRows;
            ws.Range(Range).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Range(Range).Style.Border.RightBorder = XLBorderStyleValues.Thin;
            ws.Range(Range).Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Range(Range).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("A4").Select();
        }
        public string GetExcelColumnName(int RowIndex, int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
                //columnName = columnName + RowIndex;
            }

            return columnName + RowIndex;
        }
        private SchoolAppContext AddToContext(SchoolAppContext context, Student entity, int count, int commitCount, bool recreateContext)
        {
            context.Set<Student>().Add(entity);

            if (count % commitCount == 0)
            {
                context.SaveChanges();
                if (recreateContext)
                {
                    context.Dispose();
                    context = new SchoolAppContext();
                    context.Configuration.AutoDetectChangesEnabled = false;
                    context.Configuration.ValidateOnSaveEnabled = false;
                }
            }

            return context;
        }

        //private JsonResult SearchStudent(string StName,string SchName, string MobileNo)
        //{
        //    if (StName!=null)
        //    {

        //    }
        //    return new List<Student>();
        //}
    }
}