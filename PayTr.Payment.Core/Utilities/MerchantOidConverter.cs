namespace PayTr.Payment.Core.Utilities;

public static class MerchantOidConverter
{
    public static string ToMerchantOid(Guid correlationId) => correlationId.ToString().Replace("-", "x");

    public static Guid ToGuid(string merchantOid) => Guid.Parse(merchantOid.Replace("x", "-"));
}
