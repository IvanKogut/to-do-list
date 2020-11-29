﻿using Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
  public interface IItemRepository
  {
    Task<Item?> GetByIdAndIdentityIdAsync(int id, int identityId);
    Task<int?> GetMaxItemPriorityAsync(int identityId);
    IQueryable<Item> All(int identityId);
    void Create(Item item);
    void Delete(Item item);
  }
}
