using Ali.PayTr.Abstractions.Models;

namespace Ali.PayTr.Core.Services
{
    public class PayTrFailReasonService
    {
        private Dictionary<int, PayTrFeedbackFailedReasonItem> FailReasons = new()
        {
                { -1, null },
                { 0, new PayTrFeedbackFailedReasonItem(0, "DEĞİŞKEN (AÇIKLAMAYI OKUYUN)","Ödemenin neden onaylanmadığına ilişkin, detaylı hata mesajı (Örneğin: Kartın limiti / bakiyesi yetersiz).") },
                { 1, new PayTrFeedbackFailedReasonItem(1, "Kimlik Doğrulama yapılmadı. Lütfen tekrar deneyin ve işlemi tamamlayın.","Müşteri, kimlik doğrulama adımında cep telefonu numarasını girmedi.") },
                { 2, new PayTrFeedbackFailedReasonItem(2, "Kimlik Doğrulama başarısız. Lütfen tekrar deneyin ve şifreyi doğru girin.", "Müşteri, cep telefonuna gelen şifreyi doğru girmedi.") },
                { 3, new PayTrFeedbackFailedReasonItem(3, "Güvenlik kontrolü sonrası onay verilmedi veya kontrol yapılamadı.", "Müşterinin işlemi PayTR tarafından güvenlik kontrolünden geçemedi veya kontrol yapılamadı.") },
                { 6, new PayTrFeedbackFailedReasonItem(6, "Müşteri ödeme yapmaktan vazgeçti ve ödeme sayfasından ayrıldı.", "Müşteri, kendisine tanınmış olan işlem süresinde (1.ADIM’da tanımlanan timeout_limit değeri) işlemini tamamlamadı veya müşteri ödeme sayfasını kapatarak işlemi sonlandırdı.") },
                { 8, new PayTrFeedbackFailedReasonItem(8, "Bu karta taksit yapılamamaktadır.", "Müşterinin kullanmakta olduğu kart ile seçmiş olduğu taksitli ödeme yöntemi kullanılamaz.") },
                { 9, new PayTrFeedbackFailedReasonItem(9, "Bu kart ile işlem yetkisi bulunmamaktadır.", "Müşterinin kullanmakta olduğu kart için mağazanızın işlem yetkisi bulunmuyor.") },
                { 10, new PayTrFeedbackFailedReasonItem(10, "Bu işlemde 3D Secure kullanılmalıdır.", "Müşteri, yapmış olduğu işlemde 3D Secure ile ödeme yapmalıdır.") },
                { 11, new PayTrFeedbackFailedReasonItem(11, "Güvenlik uyarısı. İşlem yapan müşterinizi kontrol edin.", "Müşterinin işleminde fraud tespiti bulunuyor. Güvenliğiniz için müşterinin işlemlerini kontrol edin.") },
                { 99, new PayTrFeedbackFailedReasonItem(99, "İşlem başarısız: Teknik entegrasyon hatası.", "Teknik entegrasyon hatası varsa dönülecektir. (debug_on değeri 0 ise)") },
         };

        public PayTrFeedbackFailedReasonItem GetFailedReasonByReasonCode(int reasonCode)
        {
            return FailReasons[reasonCode];
        }
    }
}
