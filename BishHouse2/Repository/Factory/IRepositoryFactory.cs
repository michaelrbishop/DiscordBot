using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BishHouse2.Repository.Factory
{
    public interface IRepositoryFactory
    {
        //T CreateRepository<T>(TKey1 contextFactory) where T : class;
        //T CreateRepository<T>(TKey1 contextFactory, TKey2 memoryCache) where T : class;
        T CreateRepository<T>() where T : class;
    }
}
