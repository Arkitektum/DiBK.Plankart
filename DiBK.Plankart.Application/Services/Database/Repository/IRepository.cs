using System;
using System.Collections.Generic;

namespace DiBK.Plankart.Application.Services;

public interface IRepository<TEntity> where TEntity : class
{
    IEnumerable<TEntity> GetAll();
    
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
    void Add(TEntity entity);
    #nullable enable
    TEntity? GetById(object id);
    TEntity? Find(TEntity entity);
    TEntity? FirstOrDefault(Func<TEntity, bool> predicate);
    #nullable disable
}