namespace Ali.PayTr.Core.Utilities;

public static class MerchantOidConverter
{
    public static string ToMerchantOid(Guid correlationId) => correlationId.ToString("N");
}
