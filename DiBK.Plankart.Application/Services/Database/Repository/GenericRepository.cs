using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DiBK.Plankart.Application.Services;

public class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly CesiumIonResourceDbContext _dbContext;

    private DbSet<T> DbSet => _dbContext.Set<T>();
    public IQueryable<T> Entities => DbSet;

    public GenericRepository(CesiumIonResourceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }

    public void Add(T entity)
    {
        DbSet.Add(entity);
    }

    public T? Find(T entity)
    {
        return DbSet.Find(entity);
    }

    public T? Find(Func<T,bool> predicate)
    {
        return DbSet.FirstOrDefault(predicate);
    }
}