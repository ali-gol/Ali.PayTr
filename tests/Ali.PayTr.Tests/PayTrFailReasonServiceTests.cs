using Ali.PayTr.Core.Services;

namespace Ali.PayTr.Tests;

public class PayTrFailReasonServiceTests
{
    [Fact]
    public void GetFailedReasonByReasonCode_ShouldReturnCorrectReason()
    {
        // Arrange
        var service = new PayTrFailReasonService();

        // Act
        var result1 = service.GetFailedReasonByReasonCode(1);
        var result6 = service.GetFailedReasonByReasonCode(6);

        // Assert
        Assert.NotNull(result1);
        Assert.Equal("Kimlik Doğrulama yapılmadı. Lütfen tekrar deneyin ve işlemi tamamlayın.", result1.failed_reason_msg);
        
        Assert.NotNull(result6);
        Assert.Equal("Müşteri ödeme yapmaktan vazgeçti ve ödeme sayfasından ayrıldı.", result6.failed_reason_msg);
    }

    [Fact]
    public void GetFailedReasonByReasonCode_ShouldThrowKeyNotFoundException_WhenCodeIsInvalid()
    {
        // Arrange
        var service = new PayTrFailReasonService();

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => service.GetFailedReasonByReasonCode(999));
    }
}
