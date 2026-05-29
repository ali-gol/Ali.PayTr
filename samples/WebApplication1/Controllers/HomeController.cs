using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
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
      
        public async Task<IActionResult> Checkout()
        {
            var request = new PayTrCreatePaymentRequest
            {
                ClientIp = "127.0.0.1",
                PaymentAmount = 10000,
                CustomerEmail = "user@example.com",
                CustomerFullName = "Ali Gol",
                CustomerAddress = "izmir",
                Currency = "TRY",
                CustomerPhone = "05555555555",
                CorrelationId = Guid.NewGuid(),
                BasketItems = new List<PayTrBasketItem>
                {
                    new() { Name = "Item 1", Price = 1000, Quantity = 1 }
                },
                
            };
            var response = await orderService.CreateOrderAndGetPaymentUrlAsync(request);
            if (!response.IsSuccess)
            {
                return new BadRequestResult();
            }
            return Redirect(response!.RedirectUrl!);
        }

        public IActionResult ErrorReturnUrl(Guid correlationId)
        {
            //call your database or service to get error details by correlationId and pass to view if needed.
            object order = null; //get order details by correlationId.
            return View(order);
        }

        public IActionResult SuccessReturnUrl(Guid correlationId)
        {
            //call your database or service to get error details by correlationId and pass to view if needed.
            object order = null; //get order details by correlationId.
            return View(order);
        }
    }
}
