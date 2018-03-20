using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelCardOrderService.Models;

namespace TravelCardOrderService.Data
{
    public class DbOrdersStorage : IOrdersStorage
    {
        private readonly ApplicationDbContext _db;

        public DbOrdersStorage(ApplicationDbContext db)
        {
            _db = db;
        }

        public void Add(object obj)
        {
            _db.Add(obj);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }

        public async Task<Order> SingleOrDefaultAsync(int? id)
        {
            return await _db.Orders.SingleOrDefaultAsync(m => m.Id == id);
        }

        public List<Order> GetAll()
        {
            return _db.Orders.ToList();
        }

        public void Update(Order order)
        {
            _db.Update(order);
        }

        public void Remove(Order order)
        {
            _db.Orders.Remove(order);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
