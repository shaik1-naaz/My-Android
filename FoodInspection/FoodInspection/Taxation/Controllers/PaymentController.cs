using Core;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Taxation.Filters;
using TwoCheckout;

namespace Taxation.Controllers
{
    [AuthenticateFileter]
    public class PaymentController : BaseController
    {
        #region Repositories

        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Subscription> _subscription;
        private readonly IRepository<Plan> _plans;

        #endregion

        #region CTor

        public PaymentController()
        {
            this._userRepository = new EntityRepository<User>(new TaxationEntities());
            this._subscription = new EntityRepository<Subscription>(new TaxationEntities());
            this._plans = new EntityRepository<Plan>(new TaxationEntities());
        }

        #endregion

        #region Payment Processing Methods

        public ActionResult PaywithCreditCard(UserModel objCard)
        {
            TwoCheckoutConfig.SellerID = "901325328";
            TwoCheckoutConfig.PrivateKey = "F94D20FF-0DA7-4177-BD6E-B3140DD59326";
            TwoCheckoutConfig.Sandbox = true;   // Set Mode to use your 2Checkout sandbox account
            TwoCheckoutConfig.ApiUsername = "gandhamvenkatanagasai";
            TwoCheckoutConfig.ApiPassword = "Skginfo~123";
            TwoCheckoutConfig.Demo = true;
            TwoCheckoutConfig.SecretWord = "Yzc4NzIwZDUtODg0MC00N2YwLTk3MGMtNjk2ZGFhZjVkNGUx";
            TwoCheckoutConfig.SandboxUrl = "https://sandbox.2checkout.com/checkout/api/1/" + TwoCheckoutConfig.SellerID + "/rs/authService";
            //TwoCheckoutConfig.BaseUrl = "https://www.2checkout.com/checkout/api/1/" + TwoCheckoutConfig.SellerID + "/rs/authService";
            var subscriptionDetails = new DataAccess.Subscription();
            try
            {
                #region Payment Sample Code

                //var LineItemOptions = new AuthLineitemOption();
                //LineItemOptions.optName = "Sample Option";
                //LineItemOptions.optSurcharge = (decimal)1.00;
                //LineItemOptions.optValue = "1";

                //var LineItemOptionsList = new List<AuthLineitemOption>();
                //LineItemOptionsList.Add(LineItemOptions);

                var Items = new AuthLineitem();
                Items.name = objCard.PlanName;
                //Items.options = LineItemOptionsList;
                Items.price = Convert.ToDecimal(objCard.Amount);
                Items.productId = "123";
                Items.quantity = 1;
                Items.type = "product";
                Items.recurrence = "N";
                Items.tangible = "N";

                var ItemsList = new List<AuthLineitem>();
                ItemsList.Add(Items);

                var user = this._userRepository.Find(x => x.UserId == UserId).FirstOrDefault();

                var Shipping= new AuthShippingAddress();
               
                var Billing = new AuthBillingAddress();
                Billing.addrLine2 = user.Address;
                Billing.addrLine1 = user.Address;
                Billing.city = user.Address;
                Billing.zipCode = user.PinCode;
                Billing.state = Enum.GetName(typeof(Core.States), user.State);
                Billing.country = Enum.GetName(typeof(Core.States), user.State);
                Billing.name = user.FirstName + " " + user.LastName;
                Billing.email = user.Email;
                Billing.phoneNumber = user.MobileNumber;
                
                
           
                #endregion End of Region


                var Auth = new ChargeAuthorizeServiceOptions();
                Auth.currency = "USD";
                Auth.merchantOrderId = "456";               
                Auth.token = objCard.token;
                Auth.total = (decimal) Convert.ToDecimal(objCard.Amount);
                Auth.billingAddr = Billing; 
                Auth.lineItems = ItemsList;
                //Auth.shippingAddr = Shipping;
                
                
                var Charge = new ChargeService();                
                var result = Charge.Authorize(Auth);

                subscriptionDetails.Status = result.responseCode == "APPROVED" ? true : false;
                subscriptionDetails.ReferenceNumber = result.orderNumber.ToString();
                SubscriptionStatus(objCard, subscriptionDetails);
                TempData["IsSucess"] = true;
            }
            catch (TwoCheckoutException e)
            {
                subscriptionDetails.Status =  false;
                SubscriptionStatus(objCard, subscriptionDetails);
                TempData["IsSucess"] = false;
            }
                            
            return RedirectToAction("CardDetails","Account");

        }

        /// <summary>
        /// Adds the Subscription Status into Database
        /// </summary>
        /// <param name="objCard"></param>
        /// <param name="subscriptionDetails"></param>
        private void SubscriptionStatus(UserModel objCard, Subscription subscriptionDetails)
        {
            subscriptionDetails.UserId = UserId;
            subscriptionDetails.PlanId = this._plans.FindOne(x => x.PlanName == objCard.PlanName).PlanId;
            subscriptionDetails.CreatedOn = DateTime.Now.Date;
            subscriptionDetails.IsTrail = false;
            subscriptionDetails.IsExpired = false;
            subscriptionDetails.ValidTill = DateTime.Now.AddDays(this._plans.FindOne(x => x.PlanName == objCard.PlanName).DaysValid);

            this._subscription.Add(subscriptionDetails);
        }


        #endregion

    }
}