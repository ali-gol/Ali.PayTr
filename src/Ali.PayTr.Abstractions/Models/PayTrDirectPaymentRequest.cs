using System;
using System.Collections.Generic;

namespace Ali.PayTr.Abstractions.Models;

public class PayTrDirectPaymentRequest
{
    public required string UserIp { get; set; }
    public required string MerchantOid { get; set; }
    public required string Email { get; set; }
    public required decimal PaymentAmount { get; set; }
    public string PaymentType { get; set; } = "card";
    public int InstallmentCount { get; set; } = 0;
    public string Currency { get; set; } = "TL";
    public bool? TestMode { get; set; }
    public bool Non3D { get; set; } = false;
    public bool Non3DTestFailed { get; set; } = false;
    
    // Optional request expiration timestamp in minutes (defaults to 30 mins by PayTR if null)
    public int? RequestExpDate { get; set; }

    // Card Details
    public required string CcOwner { get; set; }
    public required string CardNumber { get; set; }
    public required string ExpiryMonth { get; set; }
    public required string ExpiryYear { get; set; }
    public required string Cvv { get; set; }
    public string? CardType { get; set; }

    // Return URLs
    public required string MerchantOkUrl { get; set; }
    public required string MerchantFailUrl { get; set; }

    // User details
    public required string UserName { get; set; }
    public required string UserAddress { get; set; }
    public required string UserPhone { get; set; }

    // User Basket
    public List<PayTrBasketItem> UserBasket { get; set; } = new();

    public bool DebugOn { get; set; } = false;
    
    // Sync mode settings
    public bool SyncMode { get; set; } = false;
    
    // Language
    public string ClientLang { get; set; } = "tr";
}
