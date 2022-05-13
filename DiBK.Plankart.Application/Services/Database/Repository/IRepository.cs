using System;
using System.Collections.Generic;
using System.Linq;

namespace DiBK.Plankart.Application.Services;

public interface IRepository<T> where T : class
{
    IQueryable<T> Entities { get; }
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    void Add(T entity);
    #nullable enable
    T? Find(T entity);
    T? Find(Func<T, bool> predicate);
    #nullable disable
}