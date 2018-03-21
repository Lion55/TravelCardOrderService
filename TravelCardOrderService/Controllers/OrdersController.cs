﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelCardOrderService.Data;
using TravelCardOrderService.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelCardOrderService.Controllers
{
    [Authorize(Roles ="Admin,User")]
    public class OrdersController : Controller
    {
        private readonly IOrdersStorage _ordersStorage;
        private readonly UserManager<ApplicationUser> _userManager;


        public OrdersController(IOrdersStorage ordersStorage, UserManager<ApplicationUser> userManager)
        {
            _ordersStorage = ordersStorage;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Index(int year, int month)
        {
            if (year == 0)
                year = DateTime.Now.Year;
            if (month == 0)
                month = DateTime.Now.Month;

            var orders = _ordersStorage.GetAll();
            var ordersByDate = new List<Order>();

            foreach (var o in orders)
            {
                if (o.Date.Year == year && o.Date.Month == month)
                {
                    ordersByDate.Add(o);
                }
            }

            ordersByDate.Sort((x, y) => x.UserName.CompareTo(y.UserName));
            return View(ordersByDate);
        }

        [HttpGet]
        public IActionResult GetByUser(int year, int month)
        {
            if (year == 0)
                year = DateTime.Now.Year;
            if (month == 0)
                month = DateTime.Now.Month;

            var orders = _ordersStorage.GetAll();
            var userOrders = new List<Order>();
            var userId = _userManager.GetUserId(User);
            foreach(var o in orders)
            {
                if (o.UserId.Equals(userId) && o.Date.Year == year && o.Date.Month == month)
                {
                    userOrders.Add(o);
                }
            }
            return View(userOrders);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                order.UserId = userId;

                var userName = _userManager.GetUserAsync(User).Result.Name;
                order.UserName = userName;

                var today = DateTime.UtcNow.Date;
                var date = new DateTime(today.Year, today.Month + 1, today.Day);
                order.Date = date;

                _ordersStorage.Add(order);
                await _ordersStorage.SaveChangesAsync();
                return RedirectToAction(nameof(GetByUser));
            }

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var order = await _ordersStorage.SingleOrDefaultAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var order = await _ordersStorage.SingleOrDefaultAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                order.UserId = userId;

                var userName = _userManager.GetUserAsync(User).Result.Name;
                order.UserName = userName;

                _ordersStorage.Update(order);
                await _ordersStorage.SaveChangesAsync();
                return RedirectToAction(nameof(GetByUser));
            }
            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var order = await _ordersStorage.SingleOrDefaultAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveOrder(int id)
        {
            var order = await _ordersStorage.SingleOrDefaultAsync(id);
            _ordersStorage.Remove(order);
            await _ordersStorage.SaveChangesAsync();
            return RedirectToAction(nameof(GetByUser));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ordersStorage.Dispose();
            }
        }
    }
}
