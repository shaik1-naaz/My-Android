using Core;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Taxation.Filters;


namespace Taxation.Controllers
{
    [AuthenticateFileter]
    public class TestController : BaseController
    {
        #region Repositories

        private readonly IRepository<CourseTest> _courseTest;
        private readonly IRepository<TestQuestion> _testQuestions;
        private readonly IRepository<Form> _formRepository;
        private readonly IRepository<QuestionGroup> _questionGroups;
        private readonly IRepository<UserResult> _userResults;



        #endregion

        #region CTor

        /// <summary>
        /// Assigns the Course repository to the table
        /// </summary>
        public TestController()
        {
            this._courseTest = new EntityRepository<CourseTest>(new TaxationEntities());
            this._testQuestions = new EntityRepository<TestQuestion>(new TaxationEntities());
            this._formRepository = new EntityRepository<Form>(new TaxationEntities());
            this._questionGroups = new EntityRepository<QuestionGroup>(new TaxationEntities());
            this._userResults = new EntityRepository<UserResult>(new TaxationEntities());

        }


        #endregion

        /// <summary>
        /// Gets the Answers of the Form
        /// </summary>
        /// <param name="objCourse"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TakeTest(CourseInsurance objCourse)
        {
            var FormId = this._courseTest.FindOne(x => x.CourseId == objCourse.CourseId).FormId;
            var FormName = this._formRepository.FindOne(x => x.FormId == FormId).Name;

            var Answers = this._userResults.FindAll().Where(x => x.UserId == UserId);
            var CorrectAns = new StringBuilder();
            foreach (var item in Answers)
            {
                CorrectAns.Append("$('#" + item.TestQuestion.FormPlaceHolder.DivId + "').val('" + item.TestQuestion.Answer + "');");
            }

            TempData["List"] = CorrectAns.ToString();
            return View(FormName);
        }


        public JsonResult UpdateAnswer(string Answer,string Id)
        {
            var id = Convert.ToInt32(Id);
            var count = this._userResults.Find(x => x.UserId == UserId && x.TestQuestionId == id).Count();
            if (count == 0)
            {
                UserResult objResult = new UserResult();
                objResult.TestQuestionId = id;
                objResult.UserId = UserId;
                objResult.IsCorrect = true;
                this._userResults.Add(objResult);                
            }
            return Json("");
        }

        ///// <summary>
        ///// Gets the Answers of the Form
        ///// </summary>
        ///// <param name="objCourse"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult TakeTest(CourseInsurance objCourse)
        //{           
        //    var FormId = this._courseTest.FindOne(x => x.CourseId == objCourse.CourseId).FormId;
        //    var FormName = this._formRepository.FindOne(x => x.FormId == FormId).Name;

        //    var data = new TestQuestionsData()
        //    {
        //        FormId = FormId.Value,
        //        FormName = FormName,
        //        //Groups = this._questionGroups.FindAll()
        //        //             .Where(z => z.FormId == FormId).Count() != 0 ?

        //        //            GetGroupedAnswers(FormId) :  GetAnswers(FormId)
        //        Groups =  GetGroupedAnswers(FormId) 
                              
        //    };

        //    var AnsList = new List<TestQuestionsData>();
        //    AnsList.Add(data);

        //    return View(FormName, AnsList);
        //}

        //private List<AnswerGroups> GetGroupedAnswers(int? FormId)
        //{
        //    //Gets the Answers of the Form along with the Group Name
        //    var data= this._questionGroups.FindAll().Where(z => z.FormId == FormId)
        //                                 .Select(z =>
        //                                 {
        //                                     var grp = new AnswerGroups()
        //                                     {
        //                                         GroupId = z.QuestionGroupId,
        //                                         GroupName = z.GroupName,
        //                                         Answers = this._testQuestions.FindAll().
        //                                         Where(y=>y.QuestionGroupId==z.QuestionGroupId).Select(y =>
        //                                         {
        //                                             var ans = new Answers()
        //                                             {
        //                                                 AnswerId = y.TestQuestionsId,
        //                                                 Answer = y.Answer,
        //                                                 FormPlaceHolder=y.FormPlaceHolder.DivId,
        //                                                 FormPlaceHolderId = y.FormPlaceHolderId.Value,
        //                                                 QuestionTypeId = y.TestQuestionTypeId.Value
        //                                             };
        //                                             return ans;
        //                                         }).ToList()
        //                                     };
        //                                     return grp;
        //                                 }).ToList();

        //    //Gets only the Answers if they doesn't belongs to any Group
        //    var answer = this._testQuestions.FindAll().Where(y => y.FormId == FormId && y.QuestionGroupId==null).Select(y =>
        //    {
        //        var ans = new Answers()
        //        {
        //            AnswerId = y.TestQuestionsId,
        //            Answer = y.Answer,
        //            FormPlaceHolder = y.FormPlaceHolder.DivId,
        //            FormPlaceHolderId = y.FormPlaceHolderId.Value,
        //            QuestionTypeId = y.TestQuestionTypeId.Value
        //        };
        //        return ans;
        //    }).ToList();

        //    var group = new AnswerGroups();
        //    group.Answers = answer;
        //    data.Add(group);
        //    return data;
        //}

        //private List<AnswerGroups> GetAnswers(int? FormId)
        //{
        //    var listgrp = new List<AnswerGroups>();
        //    var grp = new AnswerGroups();
        //    var answer = this._testQuestions.FindAll().Where(y => y.FormId == FormId).Select(y =>
        //    {
        //        var ans = new Answers()
        //        {
        //            AnswerId = y.TestQuestionsId,
        //            Answer = y.Answer,
        //            FormPlaceHolderId = y.FormPlaceHolderId.Value,
        //            QuestionTypeId = y.TestQuestionTypeId.Value
        //        };
        //        return ans;
        //    }).ToList();

        //    grp.Answers = answer;
        //    listgrp.Add(grp);
        //    return listgrp; 
        //}

    }
}