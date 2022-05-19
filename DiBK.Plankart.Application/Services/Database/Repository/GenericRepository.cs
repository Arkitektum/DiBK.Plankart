using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DiBK.Plankart.Application.Services;

public abstract class GenericRepository<TContext, TEntity> : IRepository<TEntity> where TEntity : class
    where TContext : DbContext
{
    protected readonly TContext DbContext;

    private DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    protected GenericRepository(TContext dbContext)
    {
        DbContext = dbContext;
    }

    public IEnumerable<TEntity> GetAll()
    {
        return DbSet.AsEnumerable();
    }

    public TEntity? GetById(object id)
    {
        return DbSet.Find(id);
    }

    public void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        DbSet.RemoveRange(entities);
    }

    public void Add(TEntity entity)
    {
        DbSet.Add(entity);
    }

    public TEntity? Find(TEntity entity)
    {
        return DbSet.Find(entity);
    }

    public TEntity? FirstOrDefault(Func<TEntity,bool> predicate)
    {
        return DbSet.FirstOrDefault(predicate);
    }
}