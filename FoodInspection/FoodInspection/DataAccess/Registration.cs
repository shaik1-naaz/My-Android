using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
   public class Registration : IRegistration , IDisposable
    {
      private TaxationEntities objTaxation ;

       public Registration(TaxationEntities objContext)
       {
           this.objTaxation = objContext;
       }

       public void SaveUser(User objUserData)
        {
            objTaxation.Users.Add(objUserData);   
        }


       public void Save() 
       {
           objTaxation.SaveChanges();
       }



        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    objTaxation.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    
   }
}
