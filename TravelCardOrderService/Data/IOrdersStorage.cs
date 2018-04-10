using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelCardOrderService.Models;

namespace TravelCardOrderService.Data
{
    public interface IOrdersStorage
    {
        void Add(object obj);

        Task<int> SaveChangesAsync();

        Task<Order> SingleOrDefaultAsync(int? id);

        List<Order> GetAll();

        void Update(Order order);

        void Remove(Order order);

        int Count();

        void Dispose();
    }
}
