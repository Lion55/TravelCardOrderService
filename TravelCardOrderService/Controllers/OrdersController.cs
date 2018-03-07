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

        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;


        public OrdersController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _db = db;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Index(string token)
        {
            return View(_db.Orders.ToList());
        }

        [HttpGet]
        public IActionResult GetByUser()
        {
            var orders = _db.Orders.ToList();
            var userOrders = new List<Order>();
            var userId = _userManager.GetUserId(User);
            foreach(var o in orders)
            {
                if (o.UserId.Equals(userId))
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

                _db.Add(order);
                await _db.SaveChangesAsync();
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
            var order = await _db.Orders.SingleOrDefaultAsync(m => m.Id == id);
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
            var order = await _db.Orders.SingleOrDefaultAsync(m => m.Id == id);
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

                _db.Update(order);
                await _db.SaveChangesAsync();
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
            var order = await _db.Orders.SingleOrDefaultAsync(m => m.Id == id);
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
            var order = await _db.Orders.SingleOrDefaultAsync(m => m.Id == id);
            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(GetByUser));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
        }
    }
}
