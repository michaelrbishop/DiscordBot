using BishHouse2.Repository.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BishHouse2.Repository.Factory
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public RepositoryFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T CreateRepository<T>() where T : class
        {
            var instance = _serviceProvider.GetService(typeof(T));
            return instance is not T repository
                ? throw new InvalidCastException($"The created instance is not assignable to type {typeof(T).FullName}.")
                : repository;
        }
    }
}

