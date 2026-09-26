# PayTR Hata Kodları

**PayTR servislerinden dönen Hata Kodlarına ait tablolar**

**ÖDEME İŞLEMİ SONRASI BANKAYA GİTMEDEN PAYTR TARAFINDAN BİLDİRİM URL ADRESİNİZE DÖNÜLEN HATA KODLARI VE AÇIKLAMALARI**

| failed\_reason\_code | failed\_reason\_msg                                                       | Açıklama                                                                                                                                                                          |
|----------------------|---------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0                    | DEĞİŞKEN (AÇIKLAMAYI OKUYUN)                                              | Ödemenin neden onaylanmadığına ilişkin,detaylı hata mesajı (Örneğin: Kartın limiti /bakiyesi yetersiz).                                                                           |
| 1                    | Kimlik Doğrulama yapılmadı. Lütfen tekrar deneyin ve işlemi tamamlayın.   | Müşteri, kimlik doğrulama adımında cep telefonu numarasını girmedi.                                                                                                               |
| 2                    | Kimlik Doğrulama başarısız. Lütfen tekrar deneyin ve şifreyi doğru girin. | Müşteri, cep telefonuna gelen şifreyi doğru girmedi.                                                                                                                              |
| 3                    | Güvenlik kontrolü sonrası onay verilmedi veya kontrol yapılamadı.         | Müşterinin işlemi PayTR tarafından güvenlik kontrolünden geçemedi veya kontrol yapılamadı.                                                                                        |
| 6                    | Müşteri ödeme yapmaktan vazgeçti ve ödeme sayfasından ayrıldı.            | Müşteri, kendisine tanınmış olan işlem süresinde(1. ADIM’da tanımlanan request\_exp\_date değeri) işlemini tamamlamadı veya müşteri ödeme sayfasını kapatarak işlemi sonlandırdı. |
| 8                    | Bu karta taksit yapılamamaktadır.                                         | Müşterinin kullanmakta olduğu kart ile seçmiş olduğu taksitli ödeme yöntemi kullanılamaz.                                                                                         |
| 9                    | Bu kart ile işlem yetkisi bulunmamaktadır.                                | Müşterinin kullanmakta olduğu kart için mağazanızın işlem yetkisi bulunmuyor.                                                                                                     |
| 10                   | Bu işlemde 3D Secure kullanılmalıdır.                                     | Müşteri, yapmış olduğu işlemde 3D Secure ile ödeme yapmalıdır.                                                                                                                    |
| 11                   | Güvenlik uyarısı. İşlem yapan müşterinizi kontrol edin.                   | Müşterinin işleminde fraud tespiti bulunuyor. Güvenliğiniz için müşterinin işlemlerini kontrol edin.                                                                              |
| 99                   | İşlem başarısız: Teknik entegrasyon hatası.                               | Teknik entegrasyon hatası varsa dönülecektir. (debug\_on değeri 0 ise)                                                                                                            |

**İADE API SERVİSİ ÜZERİNDEN DÖNÜLEN HATA KODLARI VE AÇIKLAMALARI**

| err\_no | err\_msg                                                     | Açıklama                                                                                                             |
|---------|--------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------|
| 000     | iade yapilamiyor, daha sonra tekrar deneyin                  | Ödeme çıkışı esnasında iade servisinin kilitlenmesi durumunda yaşanmaktadır. Bir süre sonra tekrar deneyebilirsiniz. |
| 001     | Gecersiz istek veya magaza aktif degil                       | İstekte merchant\_id bilgisinin iletilmemesi veya mağazanın aktif olmaması durumunda dönülmektedir.                  |
| 002     | Gecersiz merchant\_oid                                       | İstekte merchant\_oid alanını iletmemeniz durumunda dönülmektedir.                                                   |
| 003     | Gecersiz return\_amount                                      | İstekte return\_amount alanını iletmemeniz durumunda dönülmektedir.                                                  |
| 004     | paytr\_token gonderilmedi veya gecersiz                      | paytr\_token alanının iletilmemesi veya doğru olmaması durumunda dönülmektedir.                                      |
| 005     | merchant\_oid ile basarili odeme bulunamadi                  | İlettiğiniz sipariş numarası ile başarılı bir ödemenin olmadığı durumda dönülmektedir.                               |
| 007     | merchant\_oid bulundu ancak odeme henuz siteye bildirilmemis | İlettiğiniz sipariş numarasının bildirim akışının tamamlanmadığını belirtmektedir.                                   |
| 008     | XYZ odeme tipi iade desteklemiyor                            | İlettiğiniz ödeme tipi ile iade işlemi yapılmamaktadır.                                                              |
| 009     | Toplam iade tutari odeme tutarindan fazla olamaz             | İlettiğiniz iade tutarı işleme ait kalan tutardan fazla olması durumunda dönülmektedir.                              |
| 010     | Net bakiyeniz yetersiz                                       | Hesabınızda iade etmek istediğiniz tutar kadar bakiye bulunmaması durumunda döndülmektedir.                          |
| 011     | Bir yildan eski islemler icin iade islemi yapilamaz.         | Bir yıldan önceki bir işlem için iade işlemi denemeniz durumunda dönülmektedir.                                      |

**DURUM SORGU SERVİSİ ÜZERİNDEN DÖNÜLEN HATA KODLARI VE AÇIKLAMALARI**

| err\_no | err\_msg                                    | Açıklama                                                                                            |
|---------|---------------------------------------------|-----------------------------------------------------------------------------------------------------|
| 001     | Gecersiz istek veya magaza aktif degil      | İstekte merchant\_id bilgisinin iletilmemesi veya mağazanın aktif olmaması durumunda dönülmektedir. |
| 002     | Gecersiz merchant\_oid                      | İstekte merchant\_oid alanını iletmemeniz durumunda dönülmektedir.                                  |
| 003     | paytr\_token gonderilmedi veya gecersiz     | paytr\_token alanının iletilmemesi veya doğru olmaması durumunda dönülmektedir.                     |
| 004     | merchant\_oid ile islem bulunamadi          | İlettiğiniz sipariş numarasına ait bir işlem bulunamaması durumunda dönülmektedir.                  |
| 004     | merchant\_oid ile basarili odeme bulunamadi | İlettiğiniz sipariş numarası ile başarılı bir ödemenin olmadığı durumda dönülmektedir.              |

**PLATFORM TRANSFER TALEBİ SERVİSLERİ ÜZERİNDEN DÖNÜLEN HATA KODLARI VE AÇIKLAMALARI**

| err\_no | err\_msg                                                                                                  | Açıklama                                                                                                |
|---------|-----------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------|
| 001     | Gecersiz istek veya magaza aktif degil                                                                    | İstekte merchant\_id bilgisinin iletilmemesi veya mağazanın aktif olmaması durumunda dönülmektedir.     |
| 002     | Bu servis icin yetkiniz yok                                                                               | Mağazanız türünüz pazaryeri olmaması durumunda bu hata dönülmektedir.                                   |
| 003     | Gecersiz trans\_id                                                                                        | trans\_id değeri gönderilmemesi durumunda bu hata yanıtı dönülmektedir.                                 |
| 004     | paytr\_token gonderilmedi veya gecersiz                                                                   | paytr\_token alanının iletilmemesi veya doğru olmaması durumunda dönülmektedir.                         |
| 005     | Gecersiz merchant\_oid                                                                                    | İstekte merchant\_oid alanını iletmemeniz durumunda dönülmektedir.                                      |
| 006     | merchant\_oid ile basarili odeme bulunamadi                                                               | İlettiğiniz sipariş numarası ile başarılı bir ödemenin olmadığı durumda dönülmektedir.                  |
| 007     | merchant\_oid bulundu ancak odeme henuz siteye bildirilmemis                                              | İlettiğiniz sipariş numarasının bildirim akışının tamamlanmadığını belirtmektedir.                      |
| 008     | valor tarihi gecmeden transfer yapilamaz, (valör tarihi) sonrasi tekrar deneyin                           | İşlem için geçerli olan valör tarihinizden önce istekte bulunmanız durumunda dönülmektedir.             |
| 009     | trans\_id benzersiz olmalidir, bu trans\_id daha once kullanilmis.                                        | Daha önce iletilen bir trans\_id değeri iletmeniz durumunda dönülmektedir.                              |
| 010     | toplam transfer tutari, kalan tutardan fazla olamaz.                                                      | İletmiş olduğunuz transfer tutarı işlem için kalan tutardan fazla olması durumunda dönülmektedir.       |
| 012     | platform komisyonu sifirdan az olamaz                                                                     | Yapmış olduğunuz istekte PayTR komisyonu kadar tutar kalmaması durumunda dönülmektedir.                 |
| 091     | transfer\_iban degeri IBAN dogrulamasindan gecemedi, lutfen kontrol edin.                                 | Geçerli bir IBAN numarası girmemeniz durumunda dönülmektedir.                                           |
| 092     | transfer\_iban TR ile baslamalidir, bosluk veya '-' icermemeli ve 26 hane olmalidir, lutfen kontrol edin. | IBAN alanı doğru formatta iletilmemesi nedeniyle dönülmektedir.                                         |
| 095     | submerchant\_amount sifirdan kucuk olamaz.                                                                | Alt satıcınıza aktaracağınız tutar 0 dan küçük gönderilmesi nedeniyle dönülmektedir.                    |
| 096     | trans\_id alfanumerik olmalidir, ozel karakter iceremez                                                   | İletilen trans\_id alanının alfa numerik olmaması nedeniyle dönülmektedir.                              |
| 097     | transfer\_iban zorunludur, lutfen kontrol edin                                                            | transfer\_iban alanının iletilmemesi durumunda dönülmektedir.                                           |
| 098     | transfer\_name zorunludur, lutfen kontrol edin                                                            | transfer\_name alanının iletilmemesi durumunda dönülmektedir.                                           |
| 099     | total\_amount sifirdan buyuk ve sayisal olmalidir                                                         | total\_amount alanının doğru formatta iletilmemesi veya 0'dan küçük iletilmesi durumunda dönülmektedir. |
| 100     | transfer\_name icerisinde ad ve soyad arasinda bosluk olmalidir                                           | transfer\_name alanının doğru formatta iletilmemesi durumunda dönülmektedir.                            |
| 101     | transfer\_name icerisinde ad ve soyad en az 2 karakter olmalidir                                          | transfer\_name alanının doğru formatta iletilmemesi durumunda dönülmektedir.                            |
| 201     | paytr\_token gonderilmedi veya gecersiz                                                                   | paytr\_token alanının iletilmemesi veya doğru olmaması durumunda dönülmektedir.                         |
| 202     | trans\_id alfanumerik olmalidir, ozel karakter iceremez                                                   | İletilen trans\_id alanının alfa numerik olmaması nedeniyle dönülmektedir.                              |
| 203     | trans\_id benzersiz olmalidir, bu trans\_id daha once kullanilmis.                                        | Daha önce iletilen bir trans\_id değeri iletmeniz durumunda dönülmektedir.                              |
| 204     | trans\_info izin verilenden uzun, daha az kayit ile tekrar deneyin                                        | trans\_info alanında beklenenden fazla kayıt iletilmesi durumunda dönülmektedir.                        |
| 205     | trans\_info en az 2 islem en fazla 2000 islem icermelidir                                                 | trans\_info alanında beklenenden fazla veya az kayıt iletilmesi durumunda dönülmektedir.                |
| 206     | trans\_info gecerli bir JSON string degil                                                                 | trans\_info alanı beklenen formatta iletilmemesi durumunda dönülmektedir.                               |
| 301     | paytr\_token gonderilmedi veya gecersiz                                                                   | paytr\_token alanının iletilmemesi veya doğru olmaması durumunda dönülmektedir.                         |
| 302     | trans\_id alfanumerik olmalidir, ozel karakter iceremez                                                   | İletilen trans\_id alanının alfa numerik olmaması nedeniyle dönülmektedir.                              |
| 303     | trans\_id benzersiz olmalidir, bu trans\_id daha once kullanilmis.                                        | Daha önce iletilen bir trans\_id değeri iletmeniz durumunda dönülmektedir.                              |
| 305     | merchant\_oids en az X islem, en fazla Y islem icermelidir"                                               | İletilen değer merchant\_oids alanının alabileceği değerler olmaması durumunda dönülmektedir.           |
| 306     | merchant\_oids gecerli bir JSON string degil                                                              | merchant\_oids alanı beklenen formatta iletilmemesi durumunda dönülmektedir.                            |
| BLK     | merchant\_oid numarali islemde bloke mevcut, detayli bilgi icin bize ulasin                               | İletilen sipariş numarasında bloke olması durumunda dönülmektedir.                                      |