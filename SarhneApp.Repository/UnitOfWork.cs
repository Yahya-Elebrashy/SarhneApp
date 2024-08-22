using Microsoft.EntityFrameworkCore;
using SarhneApp.Core;
using SarhneApp.Core.Repositories.Contract;
using SarhneApp.Repository.Data;
using SarhneApp.Repository.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarhneApp.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private Hashtable _repositories;
        private SarhneDBContext _sarhneDBContext;
        public UnitOfWork(SarhneDBContext sarhneDBContext)
        {
            _repositories = new Hashtable();
            _sarhneDBContext = sarhneDBContext;
        }
        public Task<int> CompleteAsync()
        {
            return _sarhneDBContext.SaveChangesAsync();
        }
        public IGenericRepository<T> Repositry<T>() where T : class
        {
            var key = typeof(T).Name;

            if (!_repositories.ContainsKey(key))
            {
                var repository = new GenericRepository<T>(_sarhneDBContext);
                _repositories.Add(key, repository);
            }

            return _repositories[key] as IGenericRepository<T>;
        }
    }
}
