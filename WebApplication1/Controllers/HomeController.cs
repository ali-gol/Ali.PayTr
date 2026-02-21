using Microsoft.AspNetCore.Mvc;
using PayTr.Payment.Abstractions.Interfaces;
using PayTr.Payment.Abstractions.Models;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController(IPayTrOrderService orderService) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
      
        public async Task<IActionResult> Privacy()
        {
            var request = new PayTrCreatePaymentRequest
            {
                ClientIp = "127.0.0.1",
                PaymentAmount = 10000,
                CustomerEmail = "user@example.com",
                CustomerFullName = "Ali Gol",
                CustomerAddress = "izmir alsancak konak",
                Currency = "TRY",
                UserId = Guid.NewGuid(),
                CustomerPhone = "5393239199",
                CorrelationId = Guid.NewGuid(),
                BasketItems = new List<PayTrBasketItem>
                {
                    new() { Name = "Item 1", Price = "10000", Quantity = 1 }
                },
            };
            var response = await orderService.CreateOrderAsync(request);
            if (response.IsSuccess)
            {
                // response.Token is the PayTR iframe token
                return Redirect(response.RedirectUrl);
            }
            return new BadRequestResult();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
