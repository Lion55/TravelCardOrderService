using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TravelCardOrderService.Data;
using TravelCardOrderService.Models;
using MimeKit;
using MailKit.Net.Smtp;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelCardOrderService.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class OrdersController : Controller
    {
        private readonly IOrdersStorage _ordersStorage;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

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


        public OrdersController(IOrdersStorage ordersStorage, UserManager<ApplicationUser> userManager, IConfiguration config)
         {
            _ordersStorage = ordersStorage;
            _userManager = userManager;
            _config = config;
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

            FormFinalOrder(year, month);

            return View(ordersByDate);
        }

        [HttpGet("[controller]/[action]")]
        public IActionResult FormFinalOrder(int year, int month)
        {
            if (year == 0)
                year = DateTime.Now.Year;
            if (month == 0)
                month = DateTime.Now.Month + 1;

            Order final = new Order();
            List<Order> orders = _ordersStorage.GetAll();
            var userId = _userManager.GetUserId(User);

            foreach (var o in orders)
            {
                if (o.UserId == userId && o.Date.Year == year && o.Date.Month == month)
                {
                    final.Mb += o.Mb;
                    final.M46 += o.M46;
                    final.M62 += o.M62;

                    final.MAb += o.MAb;
                    final.MA46 += o.MA46;
                    final.MA62 += o.MA62;

                    final.MTRb += o.MTRb;
                    final.MTR46 += o.MTR46;
                    final.MTR62 += o.MTR62;

                    final.MTb += o.MTb;
                    final.MT46 += o.MT46;
                    final.MT62 += o.MT62;

                    final.Cost += o.Cost;
                    final.Amount += o.Amount;
                }
            }

            final.Date = new DateTime(year, month, 1);

            ViewData["finalOrder"] = final;

            return View(final);

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

            FormFinalOrder(year, month);

            return View(userOrders);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var date = DateTime.Today;
            if (date.Day > 15)
            {
                return RedirectToAction("OrderClosed", "Orders");
            }
            else
            {
                return View();
            }
        }

        [HttpGet("[controller]/[action]")]
        public IActionResult OrderClosed()
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
            var LIQPAY_PUBLIC_KEY = _config["LIQPAY_PUBLIC_KEY"];
            var LIQPAY_PRIVATE_KEY = _config["LIQPAY_PRIVATE_KEY"];

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

        [HttpGet("[controller]/[action]")]
        public IActionResult SuccessPay()
        {
            return View();
        }

        [HttpPost("[controller]/[action]")]
        [ValidateAntiForgeryToken]
        public IActionResult SuccessPay(HttpContext httpContext)
        {
            var userId = httpContext.Request.QueryString.Value[1];
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

        [HttpGet("[controller]/[action]")]
        public IActionResult SendEmail()
        {
            return View();
        }

        [HttpPost("[controller]/[action]")]
        public async Task<IActionResult> SendEmail(EmailInfo emailInfo)
        {
            var currentMonth = DateTime.Now.Date.ToString("MMMM", new CultureInfo("uk-UA"));

            var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month + 1, DateTime.Now.Day);
            var nextMonth = date.ToString("MMMM", new CultureInfo("uk-UA"));

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Admin", "brunets1997@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("Students", "brunets1997@gmail.com"));
            emailMessage.Subject = "Проїзні на " + nextMonth;
            emailMessage.Body = new TextPart("html")
            {
                Text = "Доброго дня!" + "<br>" + "Роздача проїзних на " + nextMonth + ": " + currentMonth + " " + emailInfo.Day + "-го з " +
                emailInfo.FromHour + " по " + emailInfo.ToHour + "<br>" +
                "Місце: " + emailInfo.Place + "<br>" +
                "Контактна особа Андрій - 093 23 23 432" + "<br>" +
                "https://www.facebook.com/andrew.brunets"
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                await client.AuthenticateAsync("brunets1997@gmail.com", _config["Gmail"]);
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }

            return RedirectToAction("GetByUser", "Orders");
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
