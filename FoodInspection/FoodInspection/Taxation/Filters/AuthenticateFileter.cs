using System.Web.Mvc;
using Taxation.Controllers;

namespace Taxation.Filters
{
    public class AuthenticateFileter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null && filterContext.HttpContext == null)
                return;
        
            if (filterContext.IsChildAction)
                return;

            var data = filterContext.Controller as BaseController;
            if (data != null && data.UserId == 0)
            {
                var controller = (BaseController)filterContext.Controller;
                filterContext.Result = controller.RedirectToAction("Login", "Account");                
            }
        }

    }
}