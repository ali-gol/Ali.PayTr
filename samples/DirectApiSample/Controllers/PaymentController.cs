using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Core.Entities;
using Ali.PayTr.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DirectApiSample.Controllers;

public class PaymentController : Controller
{
    private readonly IPayTrDirectClient _payTrDirectClient;
    private readonly IPayTrRepository _repository;

    public PaymentController(IPayTrDirectClient payTrDirectClient, IPayTrRepository repository)
    {
        _payTrDirectClient = payTrDirectClient;
        _repository = repository;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(string cardNumber, string expireMonth, string expireYear, string cvv, string cardHolderName)
    {
        var correlationId = Guid.NewGuid();
        var merchantOid = $"ORD-{correlationId:N}";
        var amount = 100.50m;
        
        // Save the order to DB so Webhook can find it
        await _repository.AddOrderAsync(new PayTrOrder
        {
            Id = Guid.NewGuid(),
            CorrelationId = correlationId,
            TotalAmount = amount,
            CreatedDate = DateTimeOffset.UtcNow,
            Status = "Created",
            Currency = "TL",
            CustomerEmail = "test@example.com",
            CustomerFullName = "Test User",
            ClientIp = "1.2.3.4",
            BasketJson = "[]",
            InstallmentCount = 1
        });
        await _repository.SaveChangesAsync();

        // Now initiate the direct payment
        var request = new PayTrDirectPaymentRequest
        {
            MerchantOid = merchantOid,
            Email = "test@example.com",
            PaymentAmount = amount,
            Currency = "TL",
            UserIp = "1.2.3.4",
            CcOwner = cardHolderName,
            CardNumber = cardNumber,
            ExpiryMonth = expireMonth,
            ExpiryYear = expireYear,
            Cvv = cvv,
            UserName = "Test User",
            UserAddress = "Test Address, Istanbul",
            UserPhone = "05320000000",
            MerchantOkUrl = Url.Action("Success", "Payment", null, Request.Scheme)!,
            MerchantFailUrl = Url.Action("Fail", "Payment", null, Request.Scheme)!,
            UserBasket = new List<PayTrBasketItem>
            {
                new PayTrBasketItem { Name = "Test Product", Price = 100.50m, Quantity = 1 }
            },
            TestMode = true,
            Non3D = true // True for non-3d, false for 3D
        };

        var result = await _payTrDirectClient.CreatePaymentAsync(request);

        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Success));
        }

        ViewBag.Error = result.Message;
        return View("Index");
    }

    [HttpGet]
    public IActionResult Success()
    {
        return Content("Payment initiated/completed successfully. Waiting for webhook (if wait_callback).");
    }

    [HttpGet]
    public IActionResult Fail()
    {
        return Content("Payment failed.");
    }
}
