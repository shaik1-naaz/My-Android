using Core;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using Taxation.Filters;

namespace Taxation.Controllers
{
    [AuthenticateFileter]
    public class UserController : BaseController
    {
        
        #region Repositories

        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<UserCourseMapping> _userCourseMapRepository;
        private readonly IRepository<CourseTest> _CourseTestRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<CourseSection> _coursesection;
        private readonly IRepository<CourseSectionContent> _coursesectioncontent;
        private readonly IRepository<SectionQuestion> _sectionQuestions;
        private readonly IRepository<SectionAnswer> _sectionAnswer;
        private readonly IRepository<CourseStatu> _courseStatus;
        private readonly IRepository<DataAccess.ImageTracking> _imageTracking;
        private readonly IRepository<Reference> _reference;



        #endregion

        #region CTor

        /// <summary>
        /// Assigns the Course repository to the table
        /// </summary>
        public UserController()
        {
            this._courseRepository = new EntityRepository<Course>(new TaxationEntities());
            this._userCourseMapRepository = new EntityRepository<UserCourseMapping>(new TaxationEntities());
            this._CourseTestRepository = new EntityRepository<CourseTest>(new TaxationEntities());
            this._userRepository = new EntityRepository<User>(new TaxationEntities());
            this._testResultRepository = new EntityRepository<TestResult>(new TaxationEntities());
            this._coursesection = new EntityRepository<CourseSection>(new TaxationEntities());
            this._coursesectioncontent = new EntityRepository<CourseSectionContent>(new TaxationEntities());
            this._sectionQuestions = new EntityRepository<SectionQuestion>(new TaxationEntities());
            this._sectionAnswer = new EntityRepository<SectionAnswer>(new TaxationEntities());
            this._courseStatus = new EntityRepository<CourseStatu>(new TaxationEntities());            
            this._imageTracking = new EntityRepository<DataAccess.ImageTracking>(new TaxationEntities());
            this._reference = new EntityRepository<Reference>(new TaxationEntities());


        }


        #endregion

        #region Insurance

        #region GET Methods


        /// <summary>
        /// Gets the Data of User Innsurance Information
        /// </summary>
        /// <returns></returns>
        
        public ActionResult Insurance()
        {
            var CurrentCourseId = this._courseStatus.Find(x => x.UserId == UserId).Count != 0 ?
                this._courseStatus.Find(x => x.UserId == UserId).OrderByDescending(x => x.CourseStatusId).FirstOrDefault().CourseId : 0;

            var Insurancedata = this._courseRepository.FindAll()
            .Where(x => x.CatalogId == (int)Core.CourseCatalog.Insurance)
              // && x.UserCourseMappings.Where(y => y.UserId == base.UserId).Select(y => y.CourseId).FirstOrDefault() == x.CourseId)
            .Select(x =>
            {
                var Insurance = new CourseInsurance
                {
                    Id = x.CourseId,
                    CourseName = x.CourseName,
                    CatalogId=x.CatalogId.Value,
                    Steps = x.CourseSections.Count.ToString(),
                    IsCompleted = x.CourseStatus.Where(f => f.CourseId == x.CourseId && f.UserId == base.UserId).Select(f => f.IsCompleted).Any(),
                    CurrentCourse = x.CourseId == CurrentCourseId ? true : false

                };
                return Insurance;
            });

            ViewBag.Active = "liInsurance";

            #region Different Method of Lambda Expression Code
            //var data= this._userCourseMapRepository.FindAll()
            //    .Where(x=>x.UserId==base.UserId && 
            //    x.Course.CatalogId == 1
            //    ).Select(x =>
            //    {
            //        var Insurance = new CourseInsurance
            //        {
            //            Id = x.CourseId.Value,
            //            CourseName = x.Course.CourseName,
            //            Steps = x.Course.CourseSections.Count.ToString(),
            //            IsCompleted = (x.Course.CourseStatus.Where(y=>y.UserId==base.UserId && y.CourseId==x.CourseId).Count()) == 0 ? false :
            //            x.Course.CourseStatus.Where(y => y.UserId == base.UserId && y.CourseId == x.CourseId).Select(y=>y.IsCompleted).Any()
            //        };
            //        return Insurance;
            //    });
            #endregion

            return View(Insurancedata.ToList());
        }

      
        #endregion

        #region POST Methods
        #endregion

        #endregion

        #region Tax Course

        #region GET Methods

        /// <summary>
        /// Gets the View of Tax Courses of the User
        /// </summary>
        public ActionResult TaxCourses()
        {
            var CurrentCourseId = this._courseStatus.Find(x => x.UserId == UserId).Count !=0?
                this._courseStatus.Find(x => x.UserId == UserId).OrderByDescending(x => x.CourseStatusId).FirstOrDefault().CourseId : 0 ;

            var Taxdata = this._courseRepository.FindAll()
            .Where(x => x.CatalogId == (int)Core.CourseCatalog.Tax)
               //&& x.UserCourseMappings.Where(y => y.UserId == base.UserId).Select(y => y.CourseId).FirstOrDefault() == x.CourseId)
            .Select(x =>
            {
                var Tax = new CourseInsurance
                {
                    Id = x.CourseId,
                    CourseName = x.CourseName,
                    Tests = x.CourseTests.Count,
                    CatalogId = x.CatalogId.Value,                    
                    Score = x.TestResults.Where(z => z.UserId == base.UserId && z.CourseId == x.CourseId).Select(z => z.TotalScore.Value).FirstOrDefault().ToString(),
                    IsCompleted = x.CourseSections.Where(z => z.CourseId == x.CourseId).Select(z => z.CourseSectionId).Count()
                     == x.CourseStatus.Where(z => z.UserId == base.UserId && z.CourseId == x.CourseId).Select(z => z.CourseSectionId).Count()
                   ? true : false,
                   CurrentCourse= x.CourseId == CurrentCourseId ? true: false

                };
                return Tax;
            });

            var TotalTests = Taxdata.Sum(x => x.Tests);
            var completedTests = Taxdata.Where(w => w.IsCompleted == true).Sum(s => s.Tests);
            int d=0;
            if (TotalTests!=0)
            {
              d   = Convert.ToInt32(((double)completedTests / (double)TotalTests) * 100);               
            }
            ViewBag.GraphPercent = d;
           
            ViewBag.Active = "liTaxCourse";

            return View(Taxdata.ToList());
        }
        
    
        /// <summary>
        /// Gets the Section Names on Expanding
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public JsonResult GetSections(string data)
        {
            var Id = Convert.ToInt32(data);
            var sections = this._coursesection.Find(x => x.CourseId == Id)
                .Select(x =>
                {
                    var obj = new CourseInsurance()
                    {
                        Id = x.CourseId.Value,
                        SectionTitle = x.CourseSectionName,
                        SectionID = x.CourseSectionId,
                        CourseName = x.Course.CourseName,
                        CatalogId = x.Course.CatalogId.Value,
                        IsCompleted = x.CourseStatus.Where(y => y.CourseSectionId == x.CourseSectionId && y.UserId == UserId).Any(),
                        SectionType = x.CourseSectionContents.Where(y => y.CourseSectionID == x.CourseSectionId).Select(y => y.CourseContentTypeId).FirstOrDefault().Value
                    };
                    return obj;
                });
            return Json(sections);
        }

        #endregion

        #region POST Methods
        #endregion

        #endregion

        #region Training Methods

        /// <summary>
        /// Gets the Training View
        /// </summary>
        /// <param name="objCourse"></param>
        /// <returns></returns>
        [HttpGet]
        //[EncryptedActionParameter]
        public ActionResult Training(CourseInsurance objCourse)
        {
            if (objCourse.Id != 0)
            {
                //TempData["courseid"] = objCourse.Id;
                var Obj = TrainingSteps(objCourse);
               return RedirectToAction("GetContent",Obj[0]);
            }

            return View();
        }


        /// <summary>
        /// Gets the View of Training Steps
        /// </summary>
        /// <param name="courseid"></param>
        /// <returns></returns>
        //[EncryptedActionParameter]
        public List<TaxCourse> TrainingSteps(TaxCourse objCourse)
        {
            var CourseSections = this._coursesection.FindAll()
              .Where(w => w.CourseId == objCourse.Id && w.IsDeleted == false)
              .Select(x =>
              {
                  var taxcourse = new TaxCourse
                  {
                      Id = x.CourseId.Value,
                      SectionTitle = x.CourseSectionName,
                      SectionID = x.CourseSectionId,
                      CourseName = x.Course.CourseName,
                      CatalogId = x.Course.CatalogId.Value,
                      IsCompleted = x.CourseStatus.Where(y => y.CourseSectionId == x.CourseSectionId && y.UserId == UserId).Any(),
                      SectionType = x.CourseSectionContents.Where(y => y.CourseSectionID == x.CourseSectionId).Select(y => y.CourseContentTypeId).FirstOrDefault().Value
                  };
                  return taxcourse;
              });

            List<TaxCourse> objtaxcourse = new List<TaxCourse>();
            if (CourseSections.ToList().Count == 0)
            {
                TaxCourse taxcourse = new TaxCourse();
                taxcourse.Id = objCourse.Id;
                objtaxcourse.Add(taxcourse);
            }
            ViewBag.CourseSections = CourseSections.ToList().Count != 0 ? CourseSections.ToList() : objtaxcourse;

            GetRecentImages();// TO get the Latest Images Opened

            var CatalogId = CourseSections.ToList().Count != 0 ? CourseSections.FirstOrDefault().CatalogId : objtaxcourse[0].CatalogId;
            switch (CatalogId)
            {
                case (int)Core.CourseCatalog.Insurance:
                    ViewBag.Active = "liInsurance";
                    break;
                case (int)Core.CourseCatalog.Tax:
                    ViewBag.Active = "liTaxCourse";
                    break;
            }

            return CourseSections.ToList().Count != 0 ? CourseSections.ToList() : objtaxcourse;
        }

        
        /// <summary>
        /// Gets the Content type and redirects based on the Content Type
        /// </summary>
        /// <param name="objTaxCourse"></param>
        /// <returns></returns>
        //[EncryptedActionParameter]
        [OutputCache(CacheProfile = "cache1Hour", VaryByParam = "SectionID")]
        public ActionResult GetContent(TaxCourse objTaxCourse)
        {
            TrainingSteps(objTaxCourse);
            var data = this._coursesectioncontent.FindOne(x => x.CourseSectionID == objTaxCourse.SectionID);
            TaxCourse objCourse = GetSectionContent(data);

            switch (Convert.ToInt32(objTaxCourse.SectionType))
            {
                case (int)SectionType.Content:
                    return View("Description", objCourse);
                case (int)SectionType.Video:
                    return View("Videos", objCourse);
                default:
                    return View("Description", objCourse);
            }
        }

        #region Commented Code
        ///// <summary>
        ///// Gets the Description of the Course
        ///// </summary>
        ///// <returns></returns>
        // //[EncryptedActionParameter]
        //public ActionResult Description(TaxCourse objCourse)
        //{
        //    if (objCourse.Id != 0)
        //    {
        //        TempData["courseid"] = objCourse.Id;
        //        TrainingSteps(objCourse);
        //        var data = this._coursesectioncontent.FindOne(x => x.CourseSectionID == objCourse.SectionID);
        //        TaxCourse objTaxCourse = GetSectionContent(data);
        //        return View(objTaxCourse);
        //    }
        //    return View();
        //}


        ///// <summary>
        ///// Gets the Videos of the Course
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult Videos(TaxCourse objCourse)
        //{
        //    if (objCourse.Id != 0)
        //    {
        //        TempData["courseid"] = objCourse.Id;
        //        TrainingSteps(objCourse);
        //        var data = this._coursesectioncontent.FindOne(x => x.CourseSectionID == objCourse.SectionID);
        //        TaxCourse objTaxCourse = GetSectionContent(data);
        //        return View(objTaxCourse);
        //    }
        //    return View();
        //}
        #endregion

        /// <summary>
        /// Gets the Content of the Section both video or Text
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private TaxCourse GetSectionContent(CourseSectionContent data)
        {
            TaxCourse objTaxCourse = new TaxCourse();
            objTaxCourse.Id = data.CourseSection.CourseId.Value;            
            objTaxCourse.SectionID = data.CourseSectionID.Value;
            objTaxCourse.SectionTitle = this._coursesection.FindOne(x => x.CourseSectionId == objTaxCourse.SectionID).CourseSectionName;
            objTaxCourse.Type = Convert.ToInt32(data.CourseContentTypeId.Value);            
            objTaxCourse.Text = data.CourseSectionContent1;
            if (objTaxCourse.Type == ((int)Core.SectionType.Video))
            {
                objTaxCourse.Text = Convert.ToString(objTaxCourse.Text==null?string.Empty:objTaxCourse.Text)
                    .Contains(".com") ? objTaxCourse.Text : "../../Images/no-video-available.jpg";                 
            }
           
            return objTaxCourse;
        }


        #endregion

        #region Results

        #region GET Methods


        /// <summary>
        /// Gets the Results of the Users
        /// </summary>
        /// <returns></returns>
        public ActionResult Results()
        {
            var Resultdata = this._testResultRepository.FindAll()
                .Where(x => x.UserId == base.UserId &&
                x.Course.CatalogId == (int)Core.CourseCatalog.Tax
                ).Select(x =>
                {
                    var Insurance = new CourseInsurance
                    {
                        Id = x.TestResultId,
                        CourseName = x.CourseTest.Form.Name,
                        TestDate = x.CreatedDate.Value,
                        Score = x.TotalScore.Value.ToString()
                    };
                    return Insurance;
                });

            ViewBag.Active = "liResults";


            return View(Resultdata.ToList());
        }


        #endregion
        
        #region POST Methods
        #endregion

        #endregion     
        
        #region Section Test

        /// <summary>
        /// Gets the Questions of the Section On the Next Click, 
        ///   If no test found ,will automatically redirects to the Next Section
        /// </summary>
        /// <param name="objCourse"></param>
        /// <returns></returns>
        [HttpGet]
        //[EncryptedActionParameter]
        [OutputCache(CacheProfile = "cache1Hour", VaryByParam = "SectionID;IsCompleted")]
        public ActionResult SectionTest(TaxCourse objCourse)
        {            
            //To Get the Next Section Data And to Transfer to the Next section If no test Available.
            var obj = new TaxCourse();
            GetNextSectionId(objCourse, obj);

            //Gets the Questions of each Section
            var data = GetTestQustions(objCourse);

            // Gets the View of the Section Test if the test or Section Questions Available
            if(data.Select(x=>x.Question).Count() != 0)            
                return View(data.ToList());
                       

            //Other wise redirects to the view of Next Section
            //Add the Section as Completed if no Section Test is available
            AddCourseStatus(objCourse.SectionID);
            //Return to the Next Section 
            return RedirectToAction("GetContent",obj);
 
        }

        /// <summary>
        /// Gets the Next Section Id to the Current Section
        /// </summary>
        /// <param name="objCourse"></param>
        /// <param name="obj"></param>
        private void GetNextSectionId(TaxCourse objCourse, TaxCourse obj)
        {
            if (objCourse.Id != 0)
            {
                var objReturned = TrainingSteps(objCourse);

                foreach (var item in objReturned)
                {
                    if (item.SectionID > objCourse.SectionID)
                    {
                        obj.SectionID = item.SectionID;
                        obj.Id = item.Id;
                        obj.SectionType = item.SectionType;
                        obj.SectionTitle = item.SectionTitle;
                       
                        break;
                    }
                    else
                    {
                        obj.SectionID = item.SectionID;
                        obj.Id = item.Id;
                        obj.SectionType = item.SectionType;
                        obj.SectionTitle = item.SectionTitle;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Multiple Choice Questions for the Current Section
        /// </summary>
        /// <param name="objCourse"></param>
        /// <returns></returns>
        private IEnumerable<Questions> GetTestQustions(TaxCourse objCourse)
        {
            var data = this._sectionQuestions.FindAll()
                    .Where(x => x.SectionId == objCourse.SectionID)
                    .Select(x =>
                    {
                        var Qus = new Questions
                        {
                            Question = x.Question,
                            QuestionId = x.QuestionId,
                            SectionID = x.SectionId.Value,
                            Description = x.Description,
                            Answers = x.SectionAnswers.Where(y => y.QuestionId == x.QuestionId).Select(y =>
                            {
                                var Ans = new Answers
                                {
                                    Answer = y.Answer,
                                    AnswerId = y.AnswerId
                                };
                                return Ans;
                            })
                        };
                        return Qus;
                    });
            return data;
        }

        /// <summary>
        /// Gets the Right Answer and Wrong Answers
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public JsonResult SectionTest(List<Result> data, int SectionId)
        {
            int QuestionCount = 0, AnswersCount = 0 ;
           
            StringBuilder CorrectQus = new StringBuilder();
            StringBuilder WrongQus = new StringBuilder();

            foreach (var item in data)
            {
                QuestionCount++;
                var Results = this._sectionAnswer.FindAll().Where(x => x.QuestionId == item.QuestionId && x.IsCorrect==true);
                if (Results.Select(x=>x.AnswerId).FirstOrDefault()==item.AnswerId)
                {
                    AnswersCount++;
                    CorrectQus.Append("#rit_" + item.QuestionId + ", ");
                }              
                else 
                {
                    WrongQus.Append("#wrng_" + item.QuestionId + ", ");
                }
            }

            AddCourseStatus(SectionId);

            return Json(new { SectionId = SectionId, Correct = CorrectQus.ToString().TrimEnd(new char[] { ',', ' ' }), Wrong = WrongQus.ToString().TrimEnd(new char[] { ',', ' ' }) });
        }

        /// <summary>
        /// Adds the Section Completed Status to DataBase
        /// </summary>
        /// <param name="SectionId"></param>
        private void AddCourseStatus(int SectionId)
        {
            var CourseId = this._coursesection.FindOne(x => x.CourseSectionId == SectionId).CourseId;
            var Status = this._courseStatus.Find(x => x.CourseSectionId == SectionId && x.CourseId == CourseId && x.UserId == UserId).Count;
            if (Status == 0)
            {
                CourseStatu objStatus = new CourseStatu();
                objStatus.UserId = UserId;
                objStatus.IsCompleted = true;
                objStatus.CourseSectionId = SectionId;
                objStatus.CourseId = CourseId.Value;
                this._courseStatus.Add(objStatus);
            }
        }



        #endregion

        #region Image Tracking Methods

        /// <summary>
        /// Saves the Record of Recently Opened Link
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public JsonResult ImageTracking(Core.ImageTracking data)
        {
         
           var  objData = this._imageTracking.FindAll()
                .OrderByDescending(x => x.TrackId)
                .GroupBy(x => x.ImagePath).Select(x => x.First())
                .Take(10).ToList();

            bool Found = false;
            foreach (var item in objData)
            {
                if (item.ImagePath==data.ImagePath)
                {
                    item.UserId = data.UserId;
                    this._imageTracking.Update(item);
                    Found = true;
                }               
            }

            if (!Found)
            {
                DataAccess.ImageTracking obj = new DataAccess.ImageTracking();
                obj.ImagePath = data.ImagePath;
                obj.UserId = data.UserId;
                obj.Class = data.Class;
                this._imageTracking.Add(obj);
                return Json("Sucess");
            }
            return Json("Fail");
        }

        /// <summary>
        /// Gets the Records that Opened Recently
        /// </summary>
        public void GetRecentImages()
        {
            var Images = this._imageTracking.FindAll().OrderByDescending(x => x.TrackId).GroupBy(x => x.ImagePath).Select(x => x.First()).Take(10).ToList();
            ViewBag.Images = Images;
        }

        #endregion

        #region Reference Methods


        /// <summary>
        /// Gets the Reference Screen
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Reference()
        {
            var objData = this._reference.FindAll()
                .Where(x => x.RefferedBy == UserId)
                .Select(x =>
                {
                    var data = new AgentReferences()
                    {
                        ReferenceId = x.Id,
                        Name = x.User.FirstName + " " + x.User.LastName,
                        Email = x.User.Email,
                        Phone = x.User.MobileNumber,
                        State = x.User.State.ToString(),
                        Status=x.Status
                    };
                    return data;                   
                }).OrderByDescending(x=>x.ReferenceId).ToList();

            ViewBag.TotalReference = this._reference.FindAll().Where(x => x.RefferedBy == UserId).Count();
            ViewBag.Contacted = this._reference.FindAll().Where(x => x.RefferedBy == UserId && x.IsContacted==true).Count();
            ViewBag.DealAccepted = this._reference.FindAll().Where(x => x.RefferedBy == UserId && x.IsResponded == true).Count();
            ViewBag.Paid = this._reference.FindAll().Where(x => x.RefferedBy == UserId && x.IsCommissionPaid == true).Count();
            ViewBag.NotContacted = this._reference.FindAll().Where(x => x.RefferedBy == UserId && (x.IsContacted == null || x.IsContacted==false)).Count();
            ViewBag.Active = "liReference";
            return View(objData);
        }

        /// <summary>
        /// Gets the Screen to Add New Reference
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(CacheProfile = "cache1Hour")]
        public ActionResult AddReference(AgentReferences objData)
        {
            if (objData.Id!=0)
            {
                var objAgentData = this._reference.FindOne(x => x.Refferee == objData.ReferenceId);
                objData.FirstName = objAgentData.User.FirstName;
                objData.LastName = objAgentData.User.LastName;
                objData.Address = objAgentData.User.Address;
                objData.State = objAgentData.User.State.HasValue ? objAgentData.User.State.Value.ToString() : "0"; 
                objData.ZipCode = objAgentData.User.PinCode;                  
            }
            ViewBag.Active = "liReference";
            return View(objData);
        }

        /// <summary>
        /// Saves the Data to the Server when new reference Added, And sends the Mail Asychrnously.
        /// </summary>
        /// <param name="objData"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddReference")]
        public ActionResult AddReference_Post(AgentReferences objData)
        {
            DataAccess.Reference objRef = new DataAccess.Reference();
            DataAccess.User objuser = new DataAccess.User();
            //Saves the Details of Referee to the User Table
            objuser.FirstName = objData.FirstName;
            objuser.LastName = objData.LastName;
            objuser.Address = objData.Address;
            objuser.State =  Convert.ToInt32(objData.State);
            objuser.City = objData.City;
            objuser.PinCode = objData.ZipCode;
            objuser.MobileNumber = objData.Phone;
            objuser.IsDeleted = false;
            objuser.Email = objData.Email;
            objuser.IsReference = true;
            this._userRepository.Add(objuser);

            //Saves the Reference Details along with the UserID obtained into the Reference Table
            objRef.Note = objData.Note;
            objRef.RefferedBy = UserId;
            objRef.Refferee = objuser.UserId;
            objRef.IsContacted = false;
            objRef.Flag = false;
            objRef.Status = "Not Yet Contacted";
            objRef.CreatedDate = DateTime.Now;
            this._reference.Add(objRef);

            #region Mailing Asynchronously

            //Asynchronously calls the SendMailToReferences method to send Mail
            AsyncMethodCaller caller = new AsyncMethodCaller(SendMailToReferences);
            //Sends the Mail to the Referee, stating been Refered
            caller.BeginInvoke(objuser.Email, base.Email ,string.Empty,"You have been Refererd","Test Body",null,null);
            //Sends the Mail to the Admin Regarding new notification
            caller.BeginInvoke(Core.BaseModel.AdminMail, string.Empty, string.Empty, "Notification About Referance", "Test Body", null, null);

            #endregion


            ViewBag.Active = "liReference";
            return RedirectToAction("Reference");
        }

        #region Delegate to Call Mailing Functionality Asychronously
        
        /// <summary>
        /// Takes the Parameter and calls the Methods assigned for
        /// </summary>
        /// <param name="To"></param>
        /// <param name="Cc"></param>
        /// <param name="Bcc"></param>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public delegate string AsyncMethodCaller(string To, string Cc, string Bcc, string Subject, string Body);

        #endregion


        [HttpPost]
        [ActionName("RefereralInfo")]
        public ActionResult RefereralInfo_Post(AgentReferences objdata)
        {
            var data = this._reference.FindOne(x => x.Id == objdata.ReferenceId);
            data.User.Email = objdata.Email;
            data.User.MobileNumber = objdata.Phone;
            data.User.Address = objdata.Address;
            data.User.State =  Convert.ToInt32(objdata.State);
            this._reference.Update(data);
            AgentReferences objRef = new AgentReferences();
            objRef.ReferenceId = objdata.ReferenceId;
            return RedirectToAction("RefereralInfo", objRef);
        }

        /// <summary>
        /// Gets the Details View of the each referer
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult RefereralInfo(AgentReferences objData)
        {
            var data = this._reference.Find(x => x.Id == objData.ReferenceId)
                .Select(x =>
                {
                    var info = new AgentReferences()
                    {
                        ReferenceId = x.Id,
                        FirstName = x.User.FirstName,
                        LastName = x.User.LastName,
                        Email = x.User.Email,
                        Phone = x.User.MobileNumber,
                        Address = x.User.Address,
                        State = x.User.State.HasValue ? x.User.State.Value.ToString() : "0", 
                        Note = x.Note,
                        IsContacted = x.IsContacted,
                        IsInstersted = x.IsIntersted.HasValue ? x.IsIntersted.Value ? 1 : 0 : 2,
                        IsAccepted = x.IsResponded,
                        IsPaid = x.IsCommissionPaid
                    };
                    return info;
                }).FirstOrDefault();

            ViewBag.Active = "liReference";
            return View(data);
        }


        /// <summary>
        /// Sends Mail to the Details Given
        /// </summary>
        /// <param name="To"></param>
        /// <param name="Cc"></param>
        /// <param name="Bcc"></param>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public static string SendMailToReferences(string To,string Cc, string Bcc, string Subject, string Body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress(Core.BaseModel.FromMail);
                mail.To.Add(To);
                if (Cc != string.Empty)
                {
                    mail.CC.Add(Cc);
                }
                if (Bcc != string.Empty)
                {
                    mail.Bcc.Add(Cc);
                }
                mail.Subject = Subject;
                mail.Body = Body;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(Core.BaseModel.FromMailUsername, Core.BaseModel.FromMailPassword);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                
            }
            catch
            {               
            }
            return "";
        }

        #endregion
        
    }
}