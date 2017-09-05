using System.Linq;
using System.Web.Mvc;
using Core;
using DataAccess;
using System.Web;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text;
using System.Security.Cryptography;
using System;
using System.Web.Security;
using Taxation.Helpers;
using Taxation.Filters;

namespace Taxation.Controllers
{

    public class AccountController : BaseController
    {
        // GET: Login

        #region Repositories

        private readonly IRepository<User> _userRepository;
        private readonly IRepository<CardDetail> _cardDetails;
        private readonly IRepository<UserCredential> _userCredentialRepository;
        private readonly IRepository<Plan> _plans;
        private readonly IRepository<Subscription> _subscription;



        #endregion
        
        #region CTor

        public AccountController()
        {
            this._userRepository = new EntityRepository<User>(new TaxationEntities());
            this._cardDetails = new EntityRepository<CardDetail>(new TaxationEntities());
            this._userCredentialRepository = new EntityRepository<UserCredential>(new TaxationEntities());
            this._plans = new EntityRepository<Plan>(new TaxationEntities());
            this._subscription = new EntityRepository<Subscription>(new TaxationEntities());

         }


        #endregion
        
        #region Register

        /// <summary>
        /// Customer Registration Gets the View
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [OutputCache(CacheProfile = "cache1Hour")]
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Customer Registration Posts the Data to Database
        /// </summary>
        /// <returns></returns>
        [ActionName("Register")]
        [HttpPost]
        public ActionResult Register_Post(UserModel objUserMetaData)
        {
            if (ModelState.IsValid)
            {
                User objUser = new User();
                UserCredential objUC = new UserCredential();
                Subscription objSubscription = new Subscription();

                objUser.FirstName = objUserMetaData.FirstName;
                objUser.LastName = objUserMetaData.LastName;
                objUser.Email = objUserMetaData.Email;
                objUser.Gender = objUserMetaData.Gender=="Male"? true: false;
                objUser.IsDeleted = false;
                objUC.Username = objUserMetaData.UserName;
                objUC.Password = MyExtensions.EncodePasswordMd5(objUserMetaData.Password);
                objUC.RoleId = objUserMetaData.UserType;
                objUC.IsActive = true;
                objSubscription.CreatedOn = DateTime.Now;
                objSubscription.IsExpired = false;
                objSubscription.IsTrail = true;                
                objSubscription.PlanId = 1;
                objSubscription.ValidTill = DateTime.Now.AddDays(30);
               

                this._userRepository.Add(objUser);
                objUC.UserId = objUser.UserId;
                this._userCredentialRepository.Add(objUC);
                objSubscription.UserId = objUser.UserId;
                this._subscription.Add(objSubscription);

                objUserMetaData.IsValidUserName = true;
                objUserMetaData.Id = objUser.UserId;

                Taxation.Controllers.UserController.AsyncMethodCaller caller = new Taxation.Controllers.UserController.AsyncMethodCaller(Taxation.Controllers.UserController.SendMailToReferences);
                //Sends the Mail to the Referee, stating been Refered
                caller.BeginInvoke(objUser.Email, string.Empty, string.Empty, "You have been Registered", "Test Body", null, null);


                return View("Login",objUserMetaData);
            }
            return View();
        }


        /// <summary>
        /// Gets the Profile Photo
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [OutputCache(CacheProfile = "cache1Hour", VaryByParam = "userId")]        
        public FileContentResult Photo(int userId)
        {
            try
            {
                var ProfilePicture = this._userRepository.FindOne(x => x.UserId == userId).EmployeePhoto;
                return new FileContentResult(ProfilePicture, "image/jpeg");

            }
            catch (System.Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// Uploads the Profile Photo to the Database
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadImage()
        {
            var image = HttpContext.Request.Files[0];
            if (ModelState.IsValid)
            {
                var User = this._userRepository.Find(x => x.UserId == UserId).FirstOrDefault();

                if (image != null)
                {
                    User.EmployeePhoto = new byte[image.ContentLength];
                    image.InputStream.Read(User.EmployeePhoto, 0, image.ContentLength);
                }
                try
                {
                    this._userRepository.Update(User);
                    return RedirectToAction("Profile");
                }
                catch
                {
                    return Json("Failure");
                }
            }
            return Json("Failure");
        }

        /// <summary>
        /// Gets the Profile Details
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AuthenticateFileter]
        new public ActionResult Profile()
        {
            var objUser = this._userRepository.FindOne(x => x.UserId == UserId);

            UserModel objUserData = new UserModel();
            objUserData.Password = objUser.UserCredentials.Where(x => x.UserId == UserId).Select(x => x.Password).FirstOrDefault();
            objUserData.Email = objUser.Email;
            objUserData.UserName = objUser.UserCredentials.Where(x => x.UserId == UserId).Select(x => x.Username).FirstOrDefault(); ;
            objUserData.Phone = objUser.MobileNumber;
            objUserData.FirstName = objUser.FirstName;
            objUserData.MiddleName = objUser.MiddleName;
            objUserData.LastName = objUser.LastName;
            objUserData.Address = objUser.Address;
            objUserData.State = objUser.State.HasValue ? objUser.State.Value.ToString() : "0";
            objUserData.ZipCode = objUser.PinCode;
            objUserData.Image = objUser.EmployeePhoto;
           
            if (!IsAdmin)
            {
                UpdateSubscriptionExpiry();
                objUserData.PlanName = objUser.Subscriptions.Where(x => x.UserId == UserId).OrderByDescending(x => x.Id).FirstOrDefault().Plan.PlanName;
                objUserData.Support = objUser.Subscriptions.Where(x => x.UserId == UserId).OrderByDescending(x => x.Id).FirstOrDefault().Plan.Support;
                objUserData.TestAccessbility = objUser.Subscriptions.Where(x => x.UserId == UserId).OrderByDescending(x => x.Id).FirstOrDefault().Plan.TestsAccessibility;
                objUserData.Amount = objUser.Subscriptions.Where(x => x.UserId == UserId).OrderByDescending(x => x.Id).FirstOrDefault().Plan.Cost.ToString();
                objUserData.ValidTill = objUser.Subscriptions.Where(x => x.UserId == UserId).OrderByDescending(x => x.Id).FirstOrDefault().ValidTill.ToShortDateString();
                objUserData.SubscribedOn = objUser.Subscriptions.Where(x => x.UserId == UserId).OrderByDescending(x => x.Id).FirstOrDefault().CreatedOn.ToShortDateString();
                objUserData.IsExpired = objUser.Subscriptions.Where(x => x.UserId == UserId).OrderByDescending(x => x.Id).FirstOrDefault().IsExpired;
                objUserData.IsTrailPeriod = objUser.Subscriptions.Where(x => x.UserId == UserId).OrderByDescending(x => x.Id).FirstOrDefault().IsTrail;
           
            }
            return View(objUserData);
        }

        /// <summary>
        /// Updates the User Subscription on Login and Profile Call
        /// </summary>
        private void UpdateSubscriptionExpiry()
        {            
                var subscriptionDetails = this._subscription.Find(x => x.UserId == UserId).OrderByDescending(x => x.Id).FirstOrDefault();
                if ((subscriptionDetails.ValidTill - subscriptionDetails.CreatedOn).Days <= 0)
                {
                    subscriptionDetails.IsExpired = true;
                    this._subscription.Update(subscriptionDetails);
                }            
        }

        /// <summary>
        /// Updates the Profile Details
        /// </summary>
        /// <param name="objUserData"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Profile")]
        public ActionResult Profile_Post(UserModel objUserData)
        {
            var objUser = this._userRepository.FindOne(x => x.UserId == UserId);
            
            objUser.MobileNumber = objUserData.Phone;
            objUser.FirstName = objUserData.FirstName;
            objUser.MiddleName = objUserData.MiddleName;
            objUser.LastName = objUserData.LastName;
            objUser.Address = objUserData.Address;
            objUser.State = Convert.ToInt32(objUserData.State);
            objUser.PinCode = objUserData.ZipCode;

            this._userRepository.Update(objUser);

            if (objUserData.NewPassword != null)
            {
                var objUserCredentials = this._userCredentialRepository.FindOne(x => x.UserId == UserId);
                objUserCredentials.Password = MyExtensions.EncodePasswordMd5(objUserData.NewPassword);
                this._userCredentialRepository.Update(objUserCredentials);
            }

            objUserData.Password = objUser.UserCredentials.Where(x => x.UserId == UserId).Select(x => x.Password).FirstOrDefault();
            return RedirectToAction("Profile");
        }


        #endregion
        
        #region Card Methods

        /// <summary>
        /// Gets the Details of all the Cards that Saved
        /// </summary>
        /// <returns></returns>
        [AuthenticateFileter]
        [HttpGet] 
        public ActionResult CardDetails(CardDetails objPlan)
        {
            //var Cardnum = Helpers.MyExtensions.Decrypt(objUser.CardNumber, objUser.SaltKey); To Decrypt the Card
            var objUserCards = this._cardDetails.FindAll()
                .Where(x => x.UserId == UserId)
                .Select(x =>
                {
                    var data = new CardDetails
                    {
                        CardId = x.Id,
                        CardNumber = "XXXX-XXXX-XXXX-" + x.LastFourDigits,
                        NameOnCard = x.NameOnCard,
                        CardType = x.CardType,
                        ExpirationMonth = Convert.ToInt32(x.ExpiryMonth),
                        ExpirationYear = Convert.ToInt32(x.ExpiryYear),
                        CVV = "XX" + x.CVV1.Substring(x.CVV1.Length - 1, 1),
                        IsDefault = x.IsDefaultCard.Value,
                        IsAutoRenewable = x.IsAutoRenewable.Value
                    };
                    return data;
                }).ToList();

            TempData["PlanName"] = objPlan.PlanName;
            TempData["Amount"] = objPlan.Amount;
            TempData["Validity"] = objPlan.ValidTill;
            TempData["IsSucess"] = TempData["IsSucess"] != null ? TempData["IsSucess"] : null;
            return View("_CardDetails", objUserCards);
        }

        public JsonResult GetCardNumber(string data)
        {
            // To Decrypt the Card
            var id = Convert.ToInt32(data);
            var objUserCards = this._cardDetails.FindOne(x=>x.Id==id);
              var Cardnum = Helpers.MyExtensions.Decrypt(objUserCards.CardNumber, objUserCards.SaltKey);
              var CVV = objUserCards.CVV1;
              return Json(new { Card=Cardnum,Cvv=CVV });
        }

        /// <summary>
        /// Saves the Card Details to the Database
        /// </summary>
        /// <param name="objData"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CardDetails")]
        public ActionResult CardDetails_Post(CardDetails objData)
        {
            if (objData.Operation == "AddCard")
            {
                var CheckDuplicates = this._cardDetails.Find(x => x.LastFourDigits == objData.CardNumber.Substring(objData.CardNumber.Length - 4)
                    && x.CVV1 == objData.CVV
                    && x.ExpiryMonth == objData.ExpirationMonth.ToString()
                    && x.ExpiryYear == objData.ExpirationYear.ToString()
                    && x.UserId == UserId).Count;

                if (CheckDuplicates != 0)
                {
                    TempData["CardExist"] = true;
                    return RedirectToAction("Profile");
                }

                var SaltKey = Helpers.MyExtensions.GenerateSalt(objData.CardNumber.Length);
                var EncodedNumber = Helpers.MyExtensions.Encrypt(objData.CardNumber, SaltKey);

                CardDetail objCard = new CardDetail();
                objCard.Id = objData.CardId;
                objCard.UserId = UserId;
                objCard.SaltKey = SaltKey;
                objCard.CardNumber = EncodedNumber;
                objCard.NameOnCard = objData.NameOnCard;
                objCard.CardType = objData.CardType;
                objCard.ExpiryMonth = objData.ExpirationMonth.ToString();
                objCard.ExpiryYear = objData.ExpirationYear.ToString();
                objCard.CVV1 = objData.CVV;
                objCard.IssuedNumber = objData.IssuedNumber;
                objCard.IssuedMonth = objData.IssuedMonth;
                objCard.IssuedYear = objData.IssuedYear;
                objCard.IsDefaultCard = objData.IsDefault;
                if (objData.IsDefault)
                {
                    var DefaultedCard = this._cardDetails.FindOne(x => x.UserId == UserId && x.IsDefaultCard == true);
                    DefaultedCard.IsDefaultCard = false;
                    this._cardDetails.Update(DefaultedCard);
                }
                objCard.IsAutoRenewable = objData.IsAutoRenewable;
                objCard.LastFourDigits = objData.CardNumber.Substring(objData.CardNumber.Length - 4);
                this._cardDetails.Add(objCard);
                //return RedirectToAction("Profile");
            }
            return RedirectToAction("CardDetails");
        }

        /// <summary>
        /// Ajax call to Delete the card Saved
        /// </summary>
        /// <param name="objData"></param>
        /// <returns></returns>
        public JsonResult DeleteCard(CardDetails objData)
        {
            try
            {
                var data = this._cardDetails.FindOne(x => x.Id == objData.CardId);
                if (data.IsDefaultCard.Value)
                {
                    var defaultCard = this._cardDetails.FindOne(x => x.UserId == UserId && x.Id != objData.CardId);
                    defaultCard.IsDefaultCard = true;
                    this._cardDetails.Update(defaultCard);
                }
                this._cardDetails.Remove(data);
                return Json("Sucess");
            }
            catch
            {
                return Json("Fail");
            }
        }
        

        #endregion
        
        #region Login Methods

        /// <summary>
        /// Gets the View of the Login
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(CacheProfile = "cache1Hour")]
        public ActionResult Login()
        {
            if (UserId != 0)            
                return RedirectToLandingPage();
            
            Session.RemoveAll();
            return View();
        }
        
        /// <summary>
        /// Validates to the User
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Login")]
        public ActionResult Login_Post(UserModel objUserData)
        {
           
                //Gets the Excrypted Password to Compare
                //1. MD5 Encryption
                objUserData.Password = MyExtensions.EncodePasswordMd5(objUserData.Password).Replace("-", "");
                var UserData = this._userCredentialRepository.Find(x => x.Username == objUserData.UserName && x.Password == objUserData.Password).FirstOrDefault();
                if (UserData != null)
                {

                    base.UserName = UserData.Username;
                    base.Email = UserData.User.Email;
                    base.UserRole = (UserType)UserData.Role.RoleId;
                    base.IsAdmin = UserData.Role.RoleId == (int)Core.UserType.Administrator;
                    base.UserId = UserData.UserId.Value;
                    Session["ProfileName"] = this._userRepository.Table.Where(x => x.UserId == UserData.UserId.Value).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();

                    if (!IsAdmin)
                    {
                        UpdateSubscriptionExpiry();  
                    }                                 

                    return RedirectToLandingPage();                  
                }
                objUserData.IsValidUserName = false;
            return View(objUserData);      
        }

        /// <summary>
        /// Gets to the Default Landing Page based on the User Role.
        /// If the user is Admin then Users page in Admin, if Other than Admin TaxCourse in User
        /// </summary>
        /// <returns></returns>
        private ActionResult RedirectToLandingPage()
        {
            return IsAdmin ? RedirectToAction("Users", "Admin") : RedirectToAction("TaxCourses", "User");           
        }
        
        /// <summary>
        /// Logs out from the Sessions
        /// </summary>
        /// <returns></returns>
        [OutputCache(CacheProfile = "cache1Hour")]        
        public ActionResult Logout()
        {
            UserId = 0;
            UserName = string.Empty;
            IsAdmin = false;
            UserName = string.Empty;
            UserRole = 0;
            Session.Remove("ProfileName");

            return RedirectToAction("Login", "Account");
        }        

        #endregion

        #region Plans

        /// <summary>
        /// Gets the All Plans and the Current Plan of the User
        /// </summary>
        /// <returns></returns>
        [AuthenticateFileter]
        [HttpGet]
        public ActionResult Plans()
        {
            var CurrentPlanId = this._subscription.Find(x => x.UserId == UserId).OrderByDescending(x => x.Id).FirstOrDefault().PlanId;
            var data = this._plans.FindAll()
                .Select(x => {
                    var plans = new SubscriptionPlans() 
                    {
                        Id=x.PlanId,
                        PlanName= x.PlanName,
                        Amount=x.Cost,
                        DaysValid= x.DaysValid.ToString(),
                        SupportType=x.Support,
                        TestAccessbility=x.TestsAccessibility,
                        IsActive = x.PlanId == CurrentPlanId? true : false
                    };
                    return plans;
                });
         
            return View(data.ToList());
        }

        #endregion

        /// <summary>
        /// Redirects to the PageNotFound if any Error occurs while Processing the request
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PageNotFound()
        {
            return View();
        }
    }
}