using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.IO;

namespace Taxation.Filters
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class EncryptedActionParameterAttribute : ActionFilterAttribute
    {
       
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            Dictionary<string, object> decryptedParameters = new Dictionary<string, object>();          

            if (HttpContext.Current.Request.QueryString.Get("q") != null)
            {
                string encryptedQueryString = HttpContext.Current.Request.QueryString.Get("q");
                HttpContext.Current.Session["URL"] = encryptedQueryString;
                string decrptedString = Helpers.MyExtensions.Decrypt(encryptedQueryString.ToString());
                string[] paramsArrs = decrptedString.Split('?');

                for (int i = 0; i < paramsArrs.Length; i++)
                {
                    string[] paramArr = paramsArrs[i].Split('=');
                    decryptedParameters.Add(paramArr[0], (paramArr[1]));
                }

                var obj = Activator.CreateInstance(filterContext.ActionParameters.ElementAt(0).Value.GetType());

                for (int i = 0; i < decryptedParameters.Count; i++)
                {
                    obj.GetType().GetProperty(decryptedParameters.Keys.ElementAt(i)).SetValue(obj,  Convertion(decryptedParameters.Values.ElementAt(i)));
                }
                filterContext.ActionParameters[filterContext.ActionParameters.ElementAt(0).Key.ToString()] = obj;
              
            }

            base.OnActionExecuting(filterContext);

        }

        private dynamic Convertion(dynamic value)
        {
            dynamic val;
            try
            {
                val = Convert.ToInt32(value);
            }
            catch
            {
                val = value;
            }
           
            return val;
        }

        //private string Decrypt(string encryptedText)
        //{
        //    string key = "jdsg432387#";
        //    byte[] DecryptKey = { };
        //    byte[] IV = { 55, 34, 87, 64, 87, 195, 54, 21 };
        //    byte[] inputByte = new byte[encryptedText.Length];

        //    DecryptKey = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, 8));
        //    DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        //    inputByte = Convert.FromBase64String(encryptedText.ToString().Replace(" ","+"));
        //    MemoryStream ms = new MemoryStream();
        //    CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(DecryptKey, IV), CryptoStreamMode.Write);
        //    cs.Write(inputByte, 0, inputByte.Length);
        //    cs.FlushFinalBlock();
        //    System.Text.Encoding encoding = System.Text.Encoding.UTF8;
        //    return encoding.GetString(ms.ToArray());
        //}

    }
}