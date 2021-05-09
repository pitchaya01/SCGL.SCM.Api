using Lazarus.Common.DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SCGL.SCM.User.Api.Infrastructure
{
    public class RepositoryBase<TEntity> : IDisposable, IRepositoryBase<TEntity> where TEntity : class
    {
        public DbDataContext _db;
        public DbReadDataContext _DbRead;

        public RepositoryBase(DbDataContext db, DbReadDataContext DbRead)
        {
            _DbRead = DbRead;
            _db = db;
        }
        public void Dispose()
        {
            //   Db.Dispose();
        }

        public RepositoryBase()
        {

        }
        public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
        {
            return _db.Set<TEntity>().Where(predicate).AsQueryable();
        }
        public void Delete(TEntity entity)
        {
            _db.Set<TEntity>().Remove(entity);

        }
        public TEntity Add(TEntity item)
        {
            try
            {
                _db.Add(item);
                return item;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw ex;

            }

        }

        public TEntity GetById(int id)
        {
            return _db.Set<TEntity>().Find(id);
        }

        public List<TEntity> Get()
        {

            return _db.Set<TEntity>().ToList();
        }
        public void DetachAllEntities()
        {
            var changedEntriesCopy = _db.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }

        public bool Update(TEntity item)
        {

            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {

                    _db.Entry(item).State = EntityState.Modified;
                    //_db.SaveChanges();
                    return true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;


                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());

                }
            } while (saveFailed);
            return true;

        }
        public IQueryable<TEntity> Query()
        {
            return _db.Set<TEntity>();
        }

        public int Count()
        {
            return _db.Set<TEntity>().Count();
        }

        public void Commit()
        {
            try
            {
                Validate();
                _db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw ex;
            }
        }
        public void Validate()
        {
            var entities = _db.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity);

            foreach (var entity in entities)
            {
                var validationContext = new ValidationContext(entity);
                Validator.ValidateObject(entity, validationContext, validateAllProperties: true);
            }
        }

        public IQueryable<TEntity> GetRead(Expression<Func<TEntity, bool>> predicate)
        {
            return _DbRead.Set<TEntity>().Where(predicate).AsQueryable();
        }
    }
}
