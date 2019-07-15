﻿using Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
  public class UserRepository : IUserRepository
  {
    private readonly AppDbContext dbContext;

    public UserRepository(AppDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public Task<User> GetByIdAsync(int id)
    {
      return dbContext
        .Users
        .SingleOrDefaultAsync(u => u.Id == id);
    }

    public IQueryable<User> GetAll()
    {
      return dbContext
        .Users
        .AsNoTracking();
    }

    public void Delete(User user)
    {
      dbContext.Remove(user);
    }
  }
}
