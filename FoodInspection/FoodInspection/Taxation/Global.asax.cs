using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace Taxation
{

    public class MvcApplication : System.Web.HttpApplication 
    {

        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
            //Response.Redirect(FormsAuthentication.DefaultUrl);
        }

        protected void Application_Start()
        {            
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// Logs the Application Level Errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Application_Error(object sender, EventArgs e)
        {

          //  Exception exc = Server.GetLastError();
          //  HttpContextBase currentContext = new HttpContextWrapper(HttpContext.Current);
          //  UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
          //  RouteData routeData = urlHelper.RouteCollection.GetRouteData(currentContext);
          //  string action = routeData.Values["action"] as string;
          //  string controller = routeData.Values["controller"] as string;

          //  LogFile(exc.Message, action, controller);
          //  Server.ClearError();
          //  if (Session["ProfileName"] == null)
          //  {
          //      Response.Redirect("~/Account/Login");
          //  }

          //  RouteData objRoute = new RouteData();
          //  objRoute.Values.Add("controller","Account");
          //  objRoute.Values.Add("action", "PageNotFound");

        
          //  Response.Clear();

          ////  HttpException httpException = exc as HttpException;
          
          //  //Enable Switch if needed show specific Errors
          //      //switch (httpException.GetHttpCode())
          //      //{
          //      //    case 404:
          //      //        // Page not found.
          //      //        objRoute.Values.Add("action", "HttpError404");
          //      //        break;
          //      //    case 500:
          //      //        // Server error.
          //      //        objRoute.Values.Add("action", "HttpError500");
          //      //        break;

          //      //    // Here you can handle Views to other error codes.
          //      //    // I choose a General error template  
          //      //    default:
          //      //        objRoute.Values.Add("action", "General");
          //      //        break;
          //      //}
            

          //  // Pass exception details to the target error View.
          //   //   routeData.Values.Add("error", exc);

          //  // Clear the error on server.
          //  Server.ClearError();

          //  // Avoid IIS7 getting in the middle
          //  Response.TrySkipIisCustomErrors = true;

          //  IController errorHandler = new Taxation.Controllers.AccountController();
          //  errorHandler.Execute(new RequestContext(new HttpContextWrapper(Context), objRoute));

        }

        /// <summary>
        /// Logs into the File with Exception, Action Occured, Controller Name
        /// </summary>
        /// <param name="sExceptionName"></param>
        /// <param name="sActionName"></param>
        /// <param name="sControlName"></param>
        public void LogFile(string sExceptionName, string sActionName, string sControlName)
        {

            StreamWriter log;
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var file= Path.GetDirectoryName(path).Replace("\\bin","");

            var todayFile=DateTime.Now.ToShortDateString().Replace("/","");
            var filePath=file+"\\Log\\"+todayFile+".txt";
            if (!System.IO.File.Exists(filePath))
            {
                log = new StreamWriter(filePath,true, System.Text.Encoding.ASCII);
            }
            else
            {
                log = System.IO.File.AppendText(filePath);
            }

            // Write to the file:
            log.WriteLine("*************************---------------------------**************************");

            log.WriteLine("Data Time:" + DateTime.Now);

            log.WriteLine("Exception Name:" + sExceptionName);

            log.WriteLine("Event Name:" + sActionName);

            log.WriteLine("Control Name:" + sControlName);

            // Close the stream:

            log.Close();

        }

    }    
}
