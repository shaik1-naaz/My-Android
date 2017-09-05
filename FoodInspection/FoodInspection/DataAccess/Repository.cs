using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Design;

namespace DataAccess
{
    public  class EntityRepository<T> : IRepository<T>, IDisposable where T
                                     : class
    {

        private DbSet<T> dbset;
        private readonly DbContext _context;
        //private readonly bool _lazyLoadingEnabled = true;


        //public EntityRepository(DbContext context, bool lazyLoadingEnabledEnabled)
        //    : this(context)
        //{
        //    _lazyLoadingEnabled = lazyLoadingEnabledEnabled;
        //}

        public EntityRepository(DbContext context)
        {
            _context = context;
            //_context.Configuration.LazyLoadingEnabled = _lazyLoadingEnabled;
            dbset = context.Set<T>();
        }

        public void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
           
            dbset.Add(entity);
            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            //var originalValues = FindOne(x => x.Id == entity.Id);
            //_context.Entry(originalValues).CurrentValues.SetValues(entity);
            if (entity == null)
                throw new ArgumentNullException("entity");
            this._context.SaveChanges();
        }

        public void Remove(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            dbset.Remove(entity);
            _context.SaveChanges();
        }

        public List<T> Find(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return dbset.Where(predicate).ToList();
        }

        public T FindOne(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return dbset.FirstOrDefault(predicate);
        }

        public List<T> FindAll()
        {
            return dbset.ToList();
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets a table
        /// </summary>
        public virtual IQueryable<T> Table
        {
            get
            {
                return this.Entities;
            }
        }

        /// <summary>
        /// Entities
        /// </summary>
        protected virtual IDbSet<T> Entities
        {
            get
            {
                if (dbset == null)
                    dbset = _context.Set<T>();
                return dbset;
            }
        }

    }
}
