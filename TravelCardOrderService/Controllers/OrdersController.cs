using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TravelCardOrderService.Data;
using TravelCardOrderService.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelCardOrderService.Controllers
{
    [Authorize(Roles = "Admin,User")]
    //[Route("[controller]")]
    public class OrdersController : Controller
    {
        private readonly IOrdersStorage _ordersStorage;
        private readonly UserManager<ApplicationUser> _userManager;

        //Prices
        private int MbPrice = 191;
        private int M46Price = 96;
        private int M62Price = 121;

        private int MAbPrice = 256;
        private int MA46Price = 171;
        private int MA62Price = 191;

        private int MTRbPrice = 256;
        private int MTR46Price = 171;
        private int MTR62Price = 191;

        private int MTbPrice = 256;
        private int MT46Price = 171;
        private int MT62Price = 191;


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
            foreach (var o in orders)
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
                order.UserId = _userManager.GetUserId(User);

                order.UserName = _userManager.GetUserAsync(User).Result.Name;

                var today = DateTime.UtcNow.Date;
                order.Date = new DateTime(today.Year, today.Month + 1, today.Day);

                order.Amount = order.Mb + order.M46 + order.M62 + order.MAb + order.MA46 + order.MA62 + order.MTRb + order.MTR46 + order.MTR62 + order.MTb + order.MT46 + order.MT62;

                order.Cost = order.Mb * MbPrice + order.M46 * M46Price + order.M62 * M62Price + order.MAb * MAbPrice + order.MA46 * MA46Price + order.MA62 * MA62Price + order.MTRb * MTRbPrice + order.MTR46 * MTR46Price + order.MTR62 * MTR62Price + order.MTb * MTbPrice + order.MT46 * MT46Price + order.MT62 * MT62Price;

                _ordersStorage.Add(order);
                await _ordersStorage.SaveChangesAsync();
                return RedirectToAction("Payment", "Orders", new { id = order.Id });
            }

            return View(order);
        }

        [HttpGet("[controller]/[action]")]
        public async Task<IActionResult> Payment(int? id)
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
            var LIQPAY_PUBLIC_KEY = "";
            var LIQPAY_PRIVATE_KEY = "";

            Dictionary<string, string> orderPay = new Dictionary<string, string>();
            orderPay.Add("version", "3");
            orderPay.Add("action", "pay");
            orderPay.Add("amount", order.Cost.ToString());
            orderPay.Add("currency", "UAH");
            orderPay.Add("description", order.ToString());
            orderPay.Add("order_id", id.ToString());
            orderPay.Add("sandbox", "1");
            orderPay.Add("public_key", LIQPAY_PUBLIC_KEY);
            orderPay.Add("result_url", "https://localhost:44382/Orders/SuccessPay");
            orderPay.Add("server_url", "https://localhost:44382/Orders/SuccessPay");

            var json = JsonConvert.SerializeObject(orderPay);
            var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            var signature = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(LIQPAY_PRIVATE_KEY + data + LIQPAY_PRIVATE_KEY)));

            ViewData["data"] = data;
            ViewData["signature"] = signature;

            return View(order);
        }

        [HttpPost("[controller]/[action]")]
        public IActionResult Callback(string data, string signature, string order_id, string status)
        {
            Console.WriteLine(data);
            Console.WriteLine(signature);
            Console.WriteLine(order_id);
            Console.WriteLine(status);

            return NoContent();
        }

        [HttpGet("[controller]/[action]")]
        public IActionResult SuccessPay()
        {
            return View();
        }

        [HttpPost, ActionName("Payment")]
        [ValidateAntiForgeryToken]
        public IActionResult PaymentPost(int id)
        {
            var LIQPAY_PUBLIC_KEY = "i10448010334";
            var LIQPAY_PRIVATE_KEY = "TlGHffdG2zcec7YdMjUkBifUfwdKxTXfMCqW50VG";

            Dictionary<string, string> order = new Dictionary<string, string>();
            order.Add("version", "3");
            order.Add("action", "pay");
            order.Add("amount", "1");
            order.Add("currency", "UAH");
            order.Add("description", "description text");
            order.Add("order_id", id.ToString());
            order.Add("sandbox", "1");
            order.Add("public_key", LIQPAY_PUBLIC_KEY);

            var json = JsonConvert.SerializeObject(order);
            var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            var signature = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(LIQPAY_PRIVATE_KEY + data + LIQPAY_PRIVATE_KEY)));

            ViewData["data"] = data;
            ViewData["signature"] = signature;

            return View();
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
                order.UserId = _userManager.GetUserId(User);
                
                order.UserName = _userManager.GetUserAsync(User).Result.Name;

                var today = DateTime.UtcNow.Date;
                order.Date = new DateTime(today.Year, today.Month + 1, today.Day);

                order.Amount = order.Mb + order.M46 + order.M62 + order.MAb + order.MA46 + order.MA62 + order.MTRb + order.MTR46 + order.MTR62 + order.MTb + order.MT46 + order.MT62;

                order.Cost = order.Mb * MbPrice + order.M46 * M46Price + order.M62 * M62Price + order.MAb * MAbPrice + order.MA46 * MA46Price + order.MA62 * MA62Price + order.MTRb * MTRbPrice + order.MTR46 * MTR46Price + order.MTR62 * MTR62Price + order.MTb * MTbPrice + order.MT46 * MT46Price + order.MT62 * MT62Price;

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
