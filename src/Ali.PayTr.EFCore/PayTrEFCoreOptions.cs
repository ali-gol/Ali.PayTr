namespace Ali.PayTr.EFCore;

public class PayTrEFCoreOptions
{
    public string TablePrefix { get; set; } = "PayTr_";
    public string? Schema { get; set; } = null; // null ise default schema (genelde dbo) kullanılır
}
