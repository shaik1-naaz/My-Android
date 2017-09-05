using System.Linq;
using System.Web.Mvc;
using Core;
using DataAccess;
using System.Collections.Generic;
using System.Configuration;

namespace Taxation.Controllers
{
    public class BaseController : Controller
    {
        public new RedirectToRouteResult RedirectToAction(string action, string controllerName)
        {
            return base.RedirectToAction(action, controllerName);
        }
        

        #region Ctor

        public BaseController()
        {
           
        }

        #endregion
        
        #region Base Properties


        public string UserName { 
            get
            {
                if (Session["UserName"] != null)
                {
                    return (string)Session["UserName"].ToString(); 
                }
                return null;
            }
            set
            {
                Session["UserName"]= value;
            } 
        }

        public UserType UserRole
        {
            get
            {
                if (Session["UserRole"] != null)
                {
                    return (UserType)Session["UserRole"];
                }
                return 0;
            }
            set
            {
                Session["UserRole"] = value;
            }
        }

        public bool IsAdmin
        {
            get
            {
                if (Session["IsAdmin"] != null)
                {
                    return (bool)Session["IsAdmin"];
                }
                return false;
            }
            set
            {
                Session["IsAdmin"] = value;
            }
        }

        public string Email
        {
            get
            {
                if (Session["Email"] != null)
                {
                    return (string)Session["Email"].ToString();
                }
                return null;
            }
            set
            {
                Session["Email"] = value;
            }
        }
      
        public int UserId
        {
            get
            {
                if (Session["UserId"] != null)
                {
                    return (int)Session["UserId"];
                }
                return 0;
            }
            set
            {
                Session["UserId"] = value;
            }
        }

        
        #endregion
          
    }
}