
using Newtonsoft.Json.Linq; 
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace WebApplication3
{
    public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        // ####################### REQUIRED FIELDS TO BE REGULATED #######################
        //
        // API - Information Integration  - You can get them from the information page after logging into the PayTR Merchant Panel.
        string merchant_id = "XXXXXX";
        string merchant_key = "YYYYYYYYYYYYYY";
        string merchant_salt = "ZZZZZZZZZZZZZZ";
        //
        // the e-mail address of the customer who will make the payment 
        string emailstr = "ZZZZZZZZZZZZZZ";
        //
        // 14.45 * 100 = 1445 for 14.45 TL (multiplied by 100 and must be sent as integer.)
        int payment_amountstr = ;
        //
        // Merchant order id: The unique order id you set for the transaction.
        string merchant_oid = "";
        //
        // User name and surname: First and last name of the user that you have on your system or received via the order form
        string user_namestr = "";
        //
        // User address: The address of the user that you have on your system or received via the order form
        string user_addressstr = "";
        //
        // User phone number: The phone number of the user that you have on your system or received via the order form
        string user_phonestr = "";
        //
        //The page the user will be redirected to after successful payment (e.g. Order status / my orders page)
        string merchant_ok_url = "http://www.siteniz.com/basarili";
        //
        // The page that the user will be redirected to if something unexpected occurs
        string merchant_fail_url = "http://www.siteniz.com/basarisiz";
        //        
        //User ip: User IP received during the request.(Important: Make sure you send the external IP address when you run tests on your local machine)
        string user_ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
        if (user_ip == "" || user_ip == null)
        {
            user_ip = Request.ServerVariables["REMOTE_ADDR"];
        }
        //
        //  User basket/order contents
        object[][] user_basket = {
            new object[] {"Örnek ürün 1", "18.00", 1}, // 1. ürün (Ürün Ad - Birim Fiyat - Adet)
            new object[] {"Örnek ürün 2", "33.25", 2}, // 2. ürün (Ürün Ad - Birim Fiyat - Adet)
            new object[] {"Örnek ürün 3", "45.42", 1}, // 3. ürün (Ürün Ad - Birim Fiyat - Adet)
            };
        /* ############################################################################################ */

	/* EXAMPLE $user_basket creation - You can duplicate arrays per each product
	$user_basket = base64_encode(json_encode(array(
		array("Sample Product 1", "18.00", 1), // 1st Product (Product Name - Unit Price - Piece)
		array("Sample Product 2", "33.25", 2), // 2nd Product (Product Name - Unit Price - Piece)
    	array("Sample Product 3", "45.42", 1)  // 3rd Product (Product Name - Unit Price - Piece)
	)));
	 */

        // Transaction timeout - in minutes
        string timeout_limit = "30";
        //
        //Send as 1 for integration and test errors
        string debug_on = "1";
        //
        // When the merchant is in live mode, it can be sent as 1 to run a test
        string test_mode = "1";
        //
        // Do not display the installment option: If you send as 1, the installment options are not displayed (example usage: installment ban for mobile phone sales)
        string no_installment = "0";
        //
        // Maximum number of installments: Specifies the maximum number of installments to be displayed (example usage: up to 4 installments is allowed for jewellery expenditures)
        string max_installment = "0";
        //
        //  Currency / TL (or TRY), EUR, USD, GBP, RUB (TL is assumed if not sent)
        string currency = "TL";
        //
        // Language to be used on pages during payment process. tr for Turkish or en for English (tr is assumed if not sent)
        string lang = "";


        // Creating data to be sent in that part
        NameValueCollection data = new NameValueCollection();
        data["merchant_id"] = merchant_id;
        data["user_ip"] = user_ip;
        data["merchant_oid"] = merchant_oid;
        data["email"] = emailstr;
        data["payment_amount"] = payment_amountstr.ToString();
        //
        // Basket function, It should not be changed.
        JavaScriptSerializer ser = new JavaScriptSerializer();
        string user_basket_json = ser.Serialize(user_basket);
        string user_basketstr = Convert.ToBase64String(Encoding.UTF8.GetBytes(user_basket_json));
        data["user_basket"] = user_basketstr;
        //
        // Token function, It should not be changed.
        string Birlestir = string.Concat(merchant_id, user_ip, merchant_oid, emailstr, payment_amountstr.ToString(), user_basketstr, no_installment, max_installment, currency, test_mode, merchant_salt);
        HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
        byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));
        data["paytr_token"] = Convert.ToBase64String(b);
        //
        data["debug_on"] = debug_on;
        data["test_mode"] = test_mode;
        data["no_installment"] = no_installment;
        data["max_installment"] = max_installment;
        data["user_name"] = user_namestr;
        data["user_address"] = user_addressstr;
        data["user_phone"] = user_phonestr;
        data["merchant_ok_url"] = merchant_ok_url;
        data["merchant_fail_url"] = merchant_fail_url;
        data["timeout_limit"] = timeout_limit;
        data["currency"] = currency;
        data["lang"] = lang;

        using (WebClient client = new WebClient())
        {
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            byte[] result = client.UploadValues("https://www.paytr.com/odeme/api/get-token", "POST", data);
            string ResultAuthTicket = Encoding.UTF8.GetString(result);
            dynamic json = JValue.Parse(ResultAuthTicket);

            if (json.status == "success")
            { 
                paytriframe.Attributes["src"] = "https://www.paytr.com/odeme/guvenli/" + json.token;
                paytriframe.Visible = true;
            }
            else
            {
                Response.Write("PAYTR IFRAME failed. reason:" + json.reason + "");
            }
        }
    }
}

}


