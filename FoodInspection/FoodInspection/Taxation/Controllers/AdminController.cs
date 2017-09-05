using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core;
using DataAccess;
using Taxation.Filters;
using System.IO;
using Taxation.Helpers;

namespace Taxation.Controllers
{
    [AuthenticateFileter]
    [AuthorizeFilter]
    public class AdminController : BaseController
    {

        #region Repositories

        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<UserCourseMapping> _userCourseMapRepository;
        private readonly IRepository<CourseTest> _CourseTestRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<CourseSection> _coursesection;
        private readonly IRepository<CourseSectionContent> _coursesectioncontent;
        private readonly IRepository<CourseTest> _CourseTest;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<SectionQuestion> _sectionQuestions;
        private readonly IRepository<SectionAnswer> _sectionAnswers;
        private readonly IRepository<UserCredential> _userCredentialRepository;
        private readonly IRepository<Reference> _reference;
        private readonly IRepository<ReferenceNote> _referenceNotes;
        private readonly IRepository<Subscription> _subscription;
        private readonly IRepository<CourseStatu> _courseStatus;
        private readonly IRepository<Form> _formRepository;


        #endregion

        #region CTor

        /// <summary>
        /// Assigns the Course repository to the table
        /// </summary>
        public AdminController()
        {
            this._courseRepository = new EntityRepository<Course>(new TaxationEntities());
            this._userCourseMapRepository = new EntityRepository<UserCourseMapping>(new TaxationEntities());
            this._CourseTestRepository = new EntityRepository<CourseTest>(new TaxationEntities());
            this._userRepository = new EntityRepository<User>(new TaxationEntities());
            this._coursesection = new EntityRepository<CourseSection>(new TaxationEntities());
            this._coursesectioncontent = new EntityRepository<CourseSectionContent>(new TaxationEntities());
            this._CourseTest = new EntityRepository<CourseTest>(new TaxationEntities());
            this._testResultRepository = new EntityRepository<TestResult>(new TaxationEntities());
            this._sectionQuestions = new EntityRepository<SectionQuestion>(new TaxationEntities());
            this._sectionAnswers = new EntityRepository<SectionAnswer>(new TaxationEntities());
            this._userCredentialRepository = new EntityRepository<UserCredential>(new TaxationEntities());
            this._reference = new EntityRepository<Reference>(new TaxationEntities());
            this._referenceNotes = new EntityRepository<ReferenceNote>(new TaxationEntities());
            this._subscription = new EntityRepository<Subscription>(new TaxationEntities());
            this._courseStatus = new EntityRepository<CourseStatu>(new TaxationEntities());
            this._formRepository = new EntityRepository<Form>(new TaxationEntities());
        }

        #endregion

        #region Users Methods

        /// <summary>
        /// Gets the Users View
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Users()
        {
            var userdata = this._userRepository.FindAll()
                .Where(d => d.UserCredentials.Select(s => s.IsActive).FirstOrDefault() == true
                   && d.IsDeleted.Value == false && d.IsReference == false )
               .Select(x =>
               {
                   var users = new UserModel
                   {
                       Id = x.UserId,
                       Name = x.FirstName + " " + x.LastName,
                       Email = x.Email,
                       UserTypeName = ((UserType)(x.UserCredentials.Where(w => w.UserId == x.UserId).Select(s => s.RoleId).FirstOrDefault() ?? 0)).ToString(),
                       ModulesTrained = 20,
                       TestsTaken = 20,

                   };
                   return users;
               });

            ViewBag.Active = "liUsers";

            return View(userdata.ToList());
        }

        /// <summary>
        /// Course Results of the TaxCourse
        /// </summary>
        /// <returns></returns>    
        public ActionResult UserResults(CourseInsurance objSearch)
        {
            var Resultdata = this._testResultRepository.FindAll()
              .Where(x => x.Course.CatalogId == (int)Core.CourseCatalog.Tax
                  && objSearch.CourseName != null ? x.Course.CourseName.Contains(objSearch.CourseName) : true
                  && objSearch.TestDate != new DateTime() ? x.CreatedDate == objSearch.TestDate : true
              ).Select(x =>
              {
                  var Tax = new CourseInsurance
                  {
                      Id = x.CourseId.Value,
                      CourseName = x.Course.CourseName,
                      TestDate = x.CreatedDate.Value,
                      Score = x.TotalScore.Value.ToString()
                  };
                  return Tax;
              });
            ViewBag.Active = "liUsers";
            return View(Resultdata.ToList());
        }

        /// <summary>
        /// Deletes the User
        /// </summary>
        /// <param name="objCourseInsurance"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UserDelete(UserModel objUserData)
        {
            var data = this._userRepository.FindOne(x => x.UserId == objUserData.Id);
            data.IsDeleted = true;
            this._userRepository.Update(data);
            ViewBag.Active = "liUsers";
            return RedirectToAction("Users", "Admin");
        }

        [HttpGet]
        public ActionResult AddUser()
        {
            ViewBag.Active = "liUsers";
            return View();
        }

        /// <summary>
        /// Customer Registration Posts the Data to Database
        /// </summary>
        /// <returns></returns>
        [ActionName("AddUser")]
        [HttpPost]
        public ActionResult AddUser_Post(UserModel objUserMetaData)
        {
            if (ModelState.IsValid)
            {
                User objUser = new User();
                UserCredential objUC = new UserCredential();

                objUser.FirstName = objUserMetaData.FirstName;
                objUser.LastName = objUserMetaData.LastName;
                objUser.Email = objUserMetaData.Email;
                objUser.Gender = objUserMetaData.Gender == "Male" ? true : false;
                objUser.IsDeleted = false;
                objUC.Username = objUserMetaData.UserName;
                objUC.Password = MyExtensions.EncodePasswordMd5(objUserMetaData.Password); ;
                objUC.RoleId = objUserMetaData.UserType;
                objUC.IsActive = true;


                this._userRepository.Add(objUser);
                objUC.UserId = objUser.UserId;
                this._userCredentialRepository.Add(objUC);


                if (objUserMetaData.UserType != (int)UserType.Administrator)
                {
                    Subscription objSubscription = new Subscription();
                    objSubscription.CreatedOn = DateTime.Now;
                    objSubscription.IsExpired = false;
                    objSubscription.IsTrail = true;
                    objSubscription.PlanId = 1;
                    objSubscription.ValidTill = DateTime.Now.AddDays(30);
                    objSubscription.UserId = objUser.UserId;
                    this._subscription.Add(objSubscription);
                }
                

                Taxation.Controllers.UserController.AsyncMethodCaller caller = new Taxation.Controllers.UserController.AsyncMethodCaller(Taxation.Controllers.UserController.SendMailToReferences);
                //Sends the Mail to the Refere as been Refered
                caller.BeginInvoke(objUser.Email, string.Empty, string.Empty, "You have been Added as User",
                    "You Have been added as User Please find the Credentials, UserName: " + objUserMetaData.UserName + ", Password : " + objUserMetaData.Password, null, null);


                return RedirectToAction("Users");
            }
            return View();
        }


        #endregion

        #region Insurance

        #region GET Methods

        /// <summary>
        /// Gets the Insurance View
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Insurance()
        {
            var Insurancedata = this._courseRepository.FindAll()
              .Where(x => x.CatalogId == (int)Core.CourseCatalog.Insurance && x.IsDeleted == false)
              .Select(x =>
              {
                  var Insurance = new CourseInsurance
                  {
                      Id = x.CourseId,
                      CourseName = x.CourseName,
                      CatalogId=x.CatalogId.Value,
                      Steps = x.CourseSections.Count.ToString(),
                      Users = x.UserCourseMappings.Count
                  };
                  return Insurance;
              });

            ViewBag.Active = "liInsurance";

            return View(Insurancedata.ToList());
        }

        /// <summary>
        /// Gets the View to add the Course
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddInsuranceCourse()
        {
            ViewBag.Active = "liInsurance";
            return View();
        }


        #endregion

        #region POST Methods


        /// <summary>
        /// Post the Course details to the DB
        /// </summary>
        /// <param name="objAddCourseMetaData"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddInsuranceCourse")]
        public ActionResult AddInsuranceCourse_Post(CourseInsurance objAddCourseMetaData)
        {
            if (ModelState.IsValid)
            {
                Course objCourse = new Course();
                objCourse.CourseName = objAddCourseMetaData.CourseName;
                objCourse.CatalogId = (int)Core.CourseCatalog.Insurance;
                objCourse.IsDeleted = false;
                this._courseRepository.Add(objCourse);

                Core.TaxCourse objInsuranceCourse = new TaxCourse();
                objInsuranceCourse.Id = objCourse.CourseId;
                objInsuranceCourse.CatalogId = (int)Core.CourseCatalog.Insurance;
                return RedirectToAction("AddNewSection", objInsuranceCourse);
            }
            return View();
        }



        #endregion


        #endregion

        #region Tax Course

        #region GET Methods

        /// <summary>
        /// Gets the TaxCourse View
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TaxCourse()
        {          
            var tax = this._courseRepository.FindAll()
                .Where(x => x.CatalogId == (int)Core.CourseCatalog.Tax && x.IsDeleted == false)
                .Select(x =>
                {
                    var Taxdata = new CourseInsurance
                    {
                        Id = x.CourseId,
                        CourseName = x.CourseName,
                        CatalogId = x.CatalogId.Value,
                        Steps = x.CourseSections.Count.ToString(),
                        Users = x.UserCourseMappings.Count,                        
                    };
                    return Taxdata;
                });
            ViewBag.Active = "liTaxCourse";
            return View(tax.ToList());
        }

        /// <summary>
        /// Gets the Add Tax course view
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public ActionResult AddTaxCourse()
        {
            ViewBag.Active = "liTaxCourse";
            return View();
        }

        /// <summary>
        /// Course Results of the TaxCourse
        /// </summary>
        /// <returns></returns>       
        public ActionResult CourseResults(CourseInsurance objSearch)
        {
            var Resultdata = this._testResultRepository.FindAll()
              .Where(x => x.Course.CatalogId == (int)Core.CourseCatalog.Tax
                  && objSearch.CourseName != null ? x.User.UserCredentials.Where(y => y.UserId == x.UserId).Select(y => y.Username).FirstOrDefault().Contains(objSearch.CourseName) : true
                  && objSearch.TestDate != new DateTime() ? x.CreatedDate == objSearch.TestDate : true
              ).Select(x =>
              {
                  var Tax = new CourseInsurance
                  {
                      Id = x.TestResultId,
                      CourseName = x.User.FirstName + " " + x.User.LastName,
                      TestDate = x.CreatedDate.Value,
                      Score = x.TotalScore.Value.ToString()
                  };
                  return Tax;
              });
            ViewBag.Active = "liTaxCourse";
            return View(Resultdata.ToList());
        }

        /// <summary>
        /// Gets the View of Course Steps
        /// </summary>
        /// <param name="courseid"></param>
        /// <returns></returns>
        public void CourseSteps(TaxCourse objCourse)
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
                      CatalogId=x.Course.CatalogId.Value,
                      SectionType = x.CourseSectionContents.Where(y => y.CourseSectionID == x.CourseSectionId).Select(y => y.CourseContentTypeId).FirstOrDefault().Value
                  };
                  return taxcourse;
              });

            List<TaxCourse> objtaxcourse = new List<TaxCourse>();
            if (CourseSections.ToList().Count == 0)
            {
                TaxCourse taxcourse = new TaxCourse();
                taxcourse.Id = objCourse.Id;
                taxcourse.CourseName = objCourse.CourseName;
                taxcourse.CatalogId = objCourse.CatalogId;
                objtaxcourse.Add(taxcourse);
            }
            ViewBag.CourseSections = CourseSections.ToList().Count != 0 ? CourseSections.ToList() : objtaxcourse;

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

        }


        /// <summary>
        /// Deletes the Course of Course Insurance and Tax 
        /// </summary>
        /// <param name="objCourseInsurance"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CourseDelete(CourseInsurance objCourseInsurance)
        {
            var data = this._courseRepository.FindOne(x => x.CourseId == objCourseInsurance.Id);
            data.IsDeleted = true;
            this._courseRepository.Update(data);

            return RedirectToAction(objCourseInsurance.PageName, "Admin");
        }

        #endregion

        #region POST Methods

        /// <summary>
        /// Post the Course details to the DB
        /// </summary>
        /// <param name="objAddCourseMetaData"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddTaxCourse")]
        public ActionResult AddTaxCourse_Post(CourseInsurance objAddCourseMetaData)
        {
            if (ModelState.IsValid)
            {
                Course objCourse = new Course();
                objCourse.CourseName = objAddCourseMetaData.CourseName;
                objCourse.CatalogId = (int)Core.CourseCatalog.Tax;
                objCourse.IsDeleted = false;
                this._courseRepository.Add(objCourse);
                //TempData["courseid"] = objCourse.CourseId;
                Core.TaxCourse objTaxCourse = new TaxCourse();
                objTaxCourse.Id = objCourse.CourseId;
                objTaxCourse.CatalogId = (int)Core.CourseCatalog.Tax;
                return RedirectToAction("AddNewSection", objTaxCourse);
            }
            return View("AddNewSection");
        }

        #endregion

        #endregion

        #region Course Methods

        #region GET Methods

        /// <summary>
        /// Gets the View of Add New Section
        /// </summary>
        /// <param name="courseid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddNewSection(TaxCourse objCourse)
        {
            if (objCourse.Id != 0)
            {
                //TempData["courseid"] = objCourse.Id;
                CourseSteps(objCourse);
            }

            return View();    
           
        }


       

        /// <summary>
        /// Gets the Content type and redirects based on the Content Type
        /// </summary>
        /// <param name="objTaxCourse"></param>
        /// <returns></returns>
        public ActionResult GetContent(TaxCourse objTaxCourse)
        {
            CourseSteps(objTaxCourse);
            var data = this._coursesectioncontent.FindOne(x => x.CourseSectionID == objTaxCourse.SectionID);
            TaxCourse objCourse = GetSectionContent(data);

            switch (Convert.ToInt32(objCourse.Type))
            {
                case (int)SectionType.Content:
                    return View("Description", objCourse);
                case (int)SectionType.Video:
                    return View("Videos", objCourse);
                default:
                    return View("Description", objCourse);
            }
        }


        #region Commneted Code
        ///// <summary>
        ///// Gets the Description of the Course
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult Description(TaxCourse objCourse)
        //{
        //    if (objCourse.Id != 0)
        //    {
        //        TempData["courseid"] = objCourse.Id;
        //        CourseSteps(objCourse);
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
        //        CourseSteps(objCourse);
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
            return objTaxCourse;
        }

        #endregion

        #region POST Methods


        /// <summary>
        /// Posts the Data of Course Steps
        /// </summary>
        /// <param name="courseid"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CourseSteps")]
        public ActionResult CourseSteps_Post(TaxCourse courseid)
        {
            return View("AddNewSection");
        }


        /// <summary>
        /// Posts the Data to the Server
        /// </summary>
        /// <param name="taxcourse"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddNewSection")]
        public ActionResult AddNewSection_Post(TaxCourse taxcourse)
        {
            CourseSection objcoursesection = new CourseSection();
            CourseSectionContent objcourcesectioncontent = new CourseSectionContent();

            objcoursesection.CourseSectionName = taxcourse.SectionTitle;          
            objcoursesection.CourseId = taxcourse.Id;
            objcoursesection.IsDeleted = false;

            this._coursesection.Add(objcoursesection);

            objcourcesectioncontent.CourseSectionID = objcoursesection.CourseSectionId;
            objcourcesectioncontent.CourseContentTypeId = taxcourse.SectionType;
            objcourcesectioncontent.IsDeleted = false;
            
            switch (taxcourse.SectionType)
            {
                case (int)Core.SectionType.Content:
                    objcourcesectioncontent.CourseSectionContent1 = taxcourse.Text;
                    break;
                case (int)Core.SectionType.Video:
                    objcourcesectioncontent.CourseSectionContent1 = taxcourse.File;
                    break;
            }            
            
           
            this._coursesectioncontent.Add(objcourcesectioncontent);
            taxcourse.Text = string.Empty;
            taxcourse.SectionID = objcoursesection.CourseSectionId;
            taxcourse.Type = Convert.ToInt32(taxcourse.SectionType);
      
            return RedirectToAction("GetContent", taxcourse);

        }

        /// <summary>
        /// Gets the View of Test Add
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddNewTest(TaxCourse objCourse)
        {
            if (objCourse.Id != 0)
            {
                CourseSteps(objCourse);
            }
            
            var FormList = this._formRepository.FindAll().Select(x =>
            {
                var data = new SelectListItem()
                {
                    Value = x.FormId.ToString(),
                    Text = x.Name,
                };
                return data;
            }).ToList();

            var obj = new TaxCourse();
            obj.Forms = FormList;            
            obj.FormId=this._CourseTest.Find(x => x.CourseId == objCourse.Id ).Count != 0 ? 
                this._CourseTest.FindOne(x => x.CourseId == objCourse.Id ).FormId.Value : 0;
            return View(obj);
        }


        /// <summary>
        /// Post the data to add new Test
        /// </summary>
        /// <param name="taxcourse"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddNewTest")]
        public ActionResult AddNewTest_Post(TaxCourse taxcourse)
        {
            
           var CourseTest = this._CourseTest.Find(x => x.CourseId == taxcourse.Id ).FirstOrDefault();          
            

            if (CourseTest != null)
            {
                CourseTest.FormId = taxcourse.FormId;
                this._CourseTest.Update(CourseTest);
            }
            else
            {
                CourseTest newTest = new CourseTest();
                newTest.FormId = taxcourse.FormId;
                newTest.CourseId = taxcourse.Id;
                this._CourseTest.Add(newTest);
            }            
            return RedirectToAction("AddNewTest", taxcourse);
        }

      
        /// <summary>
        /// Deletes the Post method
        /// </summary>
        /// <param name="objCourse"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Description")]
        public ActionResult Description_Post(TaxCourse objCourse, string Command)
        {
            var objContentData = this._coursesectioncontent.FindOne(x => x.CourseSectionID == objCourse.SectionID);
            switch (Command)
            {
                case "Update":
                    objContentData.CourseSectionContent1 = objCourse.Text;
                    this._coursesectioncontent.Update(objContentData);
                    break;
                
                case "Delete":
                   
                    objContentData.IsDeleted = true;
                    this._coursesectioncontent.Update(objContentData);
                    var objSection = this._coursesection.FindOne(x => x.CourseSectionId == objContentData.CourseSectionID);
                    objSection.IsDeleted = true;
                    this._coursesection.Update(objSection);
                    break;
            }
            objCourse.Text = string.Empty;            
            return RedirectToAction("AddNewSection", objCourse);   
        }

        #endregion


        #endregion

        #region Section Test Methods

        /// <summary>
        /// Gets the Question of the Each section
        /// </summary>
        /// <param name="objSectionData"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddSectionTest(TaxCourse objSectionData)
        {
            if (objSectionData.Id != 0)
            {
                CourseSteps(objSectionData);
            }
            var Model = new Core.QuestionsModel();
            var questions= this._sectionQuestions.FindAll()
                .Where(x=>x.SectionId==objSectionData.SectionID)
                .Select(x=>
                {
                    var Qus = new Questions
                    {
                        QuestionId = x.QuestionId,
                        Question = x.Question,
                        Answers = x.SectionAnswers.Select(y =>
                        {
                            var Ans = new Answers()
                            {
                                AnswerId=y.AnswerId,
                                Answer = y.Answer,
                                IsCorrect = y.IsCorrect.HasValue ? y.IsCorrect.Value : false
                            };
                        return Ans;
                        })
                    };
                    return Qus;
                });
            Model.Questions = questions.OrderByDescending(x => x.QuestionId).ToList();
            Model.SectionID = objSectionData.SectionID;
            Model.Id = objSectionData.Id;
            return View(Model);
        }

        #region POST Methods

        /// <summary>
        /// Saves the New Question and Answers into the Database
        /// </summary>
        /// <param name="objQuestionData"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddSectionTest")]
        public ActionResult AddSectionTest_Post(QuestionsModel objQuestionData)
        {
            if (objQuestionData.Id != 0)
            {
                CourseSteps(objQuestionData);
            }

            switch (objQuestionData.Text)
            {
                case "Add Question":
                    AddNewQuestion(objQuestionData);
                    break;
                case "Update Question":
                    UpdateQuestion(objQuestionData);
                    break;
            }           

            TaxCourse objSectionData = new TaxCourse();
            objSectionData.Id = objQuestionData.Id;
            objSectionData.SectionID = objQuestionData.SectionID;

            return RedirectToAction("AddSectionTest", objSectionData);
        }

        /// <summary>
        /// Updates the Question, Description and Options
        /// </summary>
        /// <param name="objQuestionData"></param>
        private void UpdateQuestion(QuestionsModel objQuestionData)
        {
            var Question = this._sectionQuestions.Find(x => x.QuestionId == objQuestionData.QuestionId).FirstOrDefault();
            Question.Question = objQuestionData.NewQuestion;
            Question.Description = objQuestionData.Description;
            this._sectionQuestions.Update(Question);

            var Answers = this._sectionAnswers.Find(x => x.QuestionId == objQuestionData.QuestionId).AsEnumerable();
            foreach (var item in Answers)
            {
                this._sectionAnswers.Remove(item);                
            }

            AddAnswerOption(objQuestionData.Option1, Question.QuestionId, objQuestionData.CorrectAnswer == "Option1" ? true : false);
            AddAnswerOption(objQuestionData.Option2, Question.QuestionId, objQuestionData.CorrectAnswer == "Option2" ? true : false);
            AddAnswerOption(objQuestionData.Option3, Question.QuestionId, objQuestionData.CorrectAnswer == "Option3" ? true : false);
            AddAnswerOption(objQuestionData.Option4, Question.QuestionId, objQuestionData.CorrectAnswer == "Option4" ? true : false);
        }

        /// <summary>
        /// Adds the New Question into the Database
        /// </summary>
        /// <param name="objQuestionData"></param>
        private void AddNewQuestion(QuestionsModel objQuestionData)
        {
            var Question = new SectionQuestion();
            Question.Question = objQuestionData.NewQuestion;
            Question.IsDeteled = false;
            Question.SectionId = objQuestionData.SectionID;
            Question.Description = objQuestionData.Description;
            this._sectionQuestions.Add(Question);

            AddAnswerOption(objQuestionData.Option1, Question.QuestionId, objQuestionData.CorrectAnswer == "Option1" ? true : false);
            AddAnswerOption(objQuestionData.Option2, Question.QuestionId, objQuestionData.CorrectAnswer == "Option2" ? true : false);
            AddAnswerOption(objQuestionData.Option3, Question.QuestionId, objQuestionData.CorrectAnswer == "Option3" ? true : false);
            AddAnswerOption(objQuestionData.Option4, Question.QuestionId, objQuestionData.CorrectAnswer == "Option4" ? true : false);
        }

        /// <summary>
        /// Adds the Answer option into the Database
        /// </summary>
        /// <param name="Option"></param>
        /// <param name="QuestionId"></param>
        /// <param name="IsCorrectAnswer"></param>
        private void AddAnswerOption(string Option, int QuestionId, bool IsCorrectAnswer)
        {
            var Answers = new SectionAnswer();
            Answers.QuestionId = QuestionId;
            Answers.Answer = Option;
            Answers.IsCorrect = IsCorrectAnswer;
            this._sectionAnswers.Add(Answers);
        }


        public JsonResult GetQuestion(Questions objQus)
        {
            var QusInfo = this._sectionQuestions.Find(x => x.QuestionId == objQus.QuestionId).FirstOrDefault();

            var objData = new Questions();
            objData.Question = QusInfo.Question;
            objData.Id = QusInfo.QuestionId;
            objData.Answers = QusInfo.SectionAnswers.Where(x => x.QuestionId == QusInfo.QuestionId).Select(x => new Answers { AnswerId = x.AnswerId, Answer = x.Answer }).ToList();

            return Json(new { data = objData }, JsonRequestBehavior.AllowGet);
 
        }

        public JsonResult DeleteQuestion(Questions objQus)
        {
            var QusInfo = this._sectionQuestions.Find(x => x.QuestionId == objQus.QuestionId).FirstOrDefault();
            var Answers = this._sectionAnswers.Find(x => x.QuestionId == objQus.QuestionId).AsEnumerable();
            foreach (var item in Answers)
            {
                this._sectionAnswers.Remove(item);
            }
            this._sectionQuestions.Remove(QusInfo);
            return Json(new { data = "Sucess" }, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #endregion

        #region Reference Methods

     
        /// <summary>
        /// Gets the Total Referers overview
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Referals()
        {
            var objData = GetAllReferals(null,null);

            ViewBag.Active = "liReference";
            UpdateFlagBit();
            //HttpContext.Cache.Insert("Reference", 0);
            return View(objData);
        }

        private List<AgentReferences> GetAllReferals(DateTime? FromDate, DateTime? ToDate)
        {
            var objData = this._reference.FindAll().Where( x=>x.CreatedDate >= (FromDate != null ? FromDate:x.CreatedDate) 
                && x.CreatedDate <= ( ToDate != null ? ToDate:x.CreatedDate))
               .Select(x =>
               {
                   var data = new AgentReferences()
                   {
                       ReferenceId = x.Id,
                       FirstName = x.User.FirstName,
                       LastName = x.User.LastName,
                       Name = this._userRepository.FindOne(y => y.UserId == x.RefferedBy).FirstName + " " + this._userRepository.FindOne(y => y.UserId == x.RefferedBy).LastName,
                       Email = x.User.Email,
                       Phone=x.User.MobileNumber,
                       Address=x.User.Address,
                       State = x.User.State.HasValue ? x.User.State.Value.ToString() : "0",
                       Status= x.Status,
                       IsContacted = x.IsContacted.HasValue ? x.IsContacted.Value : false,
                       IsInstersted = x.IsIntersted.HasValue ? x.IsIntersted.Value ? 1 : 0 : 2,
                       IsAccepted = x.IsResponded.HasValue ? x.IsResponded.Value : false,
                       IsPaid = x.IsCommissionPaid.HasValue ? x.IsCommissionPaid.Value : false
                   };
                   return data;
               }).OrderByDescending(x => x.ReferenceId).ToList();
            return objData;
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
                        Name = this._userRepository.FindOne(y => y.UserId == x.RefferedBy).FirstName + " " + this._userRepository.FindOne(y => y.UserId == x.RefferedBy).LastName,
                        Email = x.User.Email,
                        Phone = x.User.MobileNumber,
                        Address = x.User.Address,
                        State = x.User.State.HasValue ? x.User.State.Value.ToString() : "0",
                        Status= x.Status,
                        Note = x.Note,
                        IsContacted = x.IsContacted,
                        IsInstersted = x.IsIntersted.HasValue ? x.IsIntersted.Value ? 1 : 0 : 2,
                        IsAccepted = x.IsResponded,
                        IsPaid = x.IsCommissionPaid,
                        Notes = this._referenceNotes.FindAll().Where(y => y.ReferenceId == x.Id).OrderByDescending(y=>y.Id)
                        .Select(y => 
                        {
                            var notedata = new AdminNotes()
                            {
                                NotesId= y.Id,
                                CreatedOn= y.CreatedOn.ToString(),
                                Notes= y.Note
                            };
                            return notedata;
                        }).ToList()
                    };
                    return info;
                }).FirstOrDefault();

            ViewBag.Active = "liReference";
            UpdateFlagBit(objData.ReferenceId);
            return View(data);
        }

        /// <summary>
        /// Saves the User Data to the database
        /// </summary>
        /// <param name="objdata"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("RefereralInfo")]
        public ActionResult RefereralInfo_Post(AgentReferences objdata)
        {
            var data = this._reference.FindOne(x => x.Id == objdata.ReferenceId);
            data.User.Email = objdata.Email;
            data.User.MobileNumber = objdata.Phone;
            data.User.Address = objdata.Address;
            data.User.State = Convert.ToInt32(objdata.State);
            this._reference.Update(data);
            AgentReferences objRef = new AgentReferences();
            objRef.ReferenceId = objdata.ReferenceId;
            return RedirectToAction("RefereralInfo", objRef);

        }

        /// <summary>
        /// Saves the Admin Note to the Database
        /// </summary>
        /// <param name="objData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddAdminNote(AgentReferences objData)
        {
            var data = new DataAccess.ReferenceNote();
            data.Note = objData.Note;
            data.ReferenceId = objData.ReferenceId;
            data.CreatedOn = DateTime.Now;
            this._referenceNotes.Add(data);
            AgentReferences objAgentRef = new AgentReferences();
            objAgentRef.ReferenceId = objData.ReferenceId;
            return RedirectToAction("RefereralInfo", objAgentRef);
 
        }

        /// <summary>
        /// Updates the Column in the Data Base of Referals ( IsContacted, Is Accepted, Is Paid, Is Intersted )
        /// </summary>
        /// <param name="objData"></param>
        /// <returns></returns>
        public JsonResult UpdateColumn(AgentReferences objData)
        {
            try
            {
                var data = this._reference.Find(x => x.Id == objData.ReferenceId).FirstOrDefault();
                data.IsContacted = objData.IsContacted;
                data.IsResponded = objData.IsAccepted;
                data.IsCommissionPaid = objData.IsPaid;
                if (objData.IsInstersted!=2)
                {
                    data.IsIntersted = objData.IsInstersted == 1 ?true:false ;                    
                }
                data.Status = objData.Status;
                //data.GetType().GetProperty(objData.Column).SetValue(data, objData.IsChecked);
                this._reference.Update(data);
                return Json("Success");
            }
            catch
            {
                return Json("Fail");

            }
        }

        /// <summary>
        /// Exports the Data of Referals to the Excel between the Selected Dates. Default will be all,incase no date selected.
        /// </summary>
        /// <param name="objdata"></param>
        /// <returns></returns>
        public ActionResult ExportToExcell(AgentReferences objdata)
        {     

            var objData = GetAllReferals(objdata.FromDate != null ? Convert.ToDateTime(objdata.FromDate): (DateTime?)null,
                objdata.ToDate != null ? Convert.ToDateTime(objdata.ToDate) : (DateTime?)null);

             System.Data.DataTable dt = new System.Data.DataTable();
             dt.Columns.Add("FirstName",typeof(string));
             dt.Columns.Add("LastName", typeof(string));
             dt.Columns.Add("ReferedBy", typeof(string));
             dt.Columns.Add("Email", typeof(string));
             dt.Columns.Add("Phone", typeof(string));
             dt.Columns.Add("Address", typeof(string));
             dt.Columns.Add("State", typeof(string));
             dt.Columns.Add("Status", typeof(string));

             foreach (var item in objData)
             {
                 var rec = dt.NewRow();
                 rec["FirstName"] = item.FirstName;
                 rec["LastName"] = item.LastName;
                 rec["ReferedBy"] = item.Name;
                 rec["Email"] = item.Email;
                 rec["Phone"] = item.Phone.ToString();
                 rec["Address"] = item.Address;
                 rec["State"] = item.State;
                 rec["Status"] = item.Status;
                 dt.Rows.Add(rec);
             }
            
            var grid = new System.Web.UI.WebControls.GridView();
            grid.DataSource = dt;
            grid.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=MyExcelFile.xls");
            Response.ContentType = "application/ms-excel";

            Response.Charset = "";
            StringWriter sw = new StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);

            grid.RenderControl(htw);

            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.Clear();
            Response.End();

            return RedirectToAction("Referals");
        }

        #endregion
        
        #region Notifications Methods

        /// <summary>
        /// Gets the notification Count still Pending
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public int GetNotificationCount()
        {
            return this._reference.FindAll().Where(x => x.Flag == false).Count();
        }

        /// <summary>
        /// Gets the Notifications on Hover the Icon
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Notifications()
        {
            var data = this._reference.FindAll()
                .Where(x => x.Flag == false)
                .Select(x =>
                {
                    var flagdata = new AgentReferences()
                    {
                        ReferenceId = x.Id,
                        ReferedBy = this._userRepository.FindOne(y => y.UserId == x.RefferedBy).FirstName + " " + this._userRepository.FindOne(y => y.UserId == x.RefferedBy).LastName,
                        Referee = x.User.FirstName + " " + x.User.LastName,
                        Date = x.CreatedDate.ToString()
                    };
                    return flagdata;
                }).OrderByDescending(x => x.Id).ToList();

            return PartialView("_Notifications", data);
        }

        /// <summary>
        /// Updates the Flag bit for Notifications
        /// </summary>
        protected void UpdateFlagBit()
        {
            var data = this._reference.FindAll().Where(x => x.Flag == false);
            foreach (var item in data)
            {
                item.Flag = true;
                this._reference.Update(item);
            }
        }

        /// <summary>
        /// Updates the Flag bit of Passed Reference ID
        /// </summary>
        /// <param name="UserId"></param>
        protected void UpdateFlagBit(int UserId)
        {
            var data = this._reference.FindOne(x => x.Id == UserId);
            data.Flag = true;
            this._reference.Update(data);
        }

        #endregion
        
    }
}