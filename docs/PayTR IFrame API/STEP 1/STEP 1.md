

<!-- Start of picture text -->
———<br><!-- End of picture text -->

PAYTR IFRAME API INTEGRATION 

STEP 1 

Display the Payment Form in an iFrame 

Merchant should first request an iframe_token. A server-side POST request is needed. 

# *** Request URL:** https://www.paytr.com/odeme/api/get-token 

# *** POST REQUEST FIELDS AND VALUES:** 

|**Mandatory**|<br>**Token**<br>**Calculation**|<sup>**Description**</sup>|**Field name / type**|**Limitations & Notes**|
|---|---|---|---|---|
|✓|✓|Merchant ID: Your Merchant ID<br>(Mağaza no) provided by PayTR|merchant_id<br>(integer)||
|✓|✓|User ip: User IP received during<br>the request<br>(Important: Make sure you send<br>the external IP address when you<br>run tests on your local machine)|<br>user_ip (string)|Up to 39 characters<br>(ipv4)|
|✓|✓|Merchant order id: The unique<br>order id you set for the<br>transaction.<br>(Note: Order number is posted<br>back within callback notification<br>- on STEP 2)|merchant_oid (string)|<sup>Up to 64 characters,</sup><br>**Alpha numeric**|
|✓|✓|User email address: The email<br>address which;<br>a) the user registered with<br>on your system<br>b) or you received via the<br>order form|email (string)|Up to 100 characters|
|✓|✓|Payment amount: The total<br>amount of the order.<br>(Multiply the amount by 100)|payment_amount<br>(integer)|**For example, 3456**<br>**should be sent for 34.56**<br>**(34.56 * 100 = 3456)**|
|✓|✓|Currency|currency(string)|**TL (or TRY), EUR, USD,**<br>**GBP, RUB (TL is assumed**<br>**if not sent)**|
|✓|✓|User basket/order contents|user_basket (string)|**Please check the sample**<br>**codes for structure**|
|✓|✓|Do not display the installment<br>option: If you send as 1, the<br>installment options are not<br>displayed (example usage:<br>installment ban for mobile<br>phone sales)|no_installment (int)|0 or 1|
|✓|✓|Maximum number of<br>installments: Specifies the<br>maximum number of<br>installments to be displayed<br>(example usage: up to 4<br>installments is allowed for<br>jewellery expenditures)|max_installment (int)|0,2,3,4,5,6,7,8,9,10,11,12<br>If zero (0) is sent, the<br>maximum available<br>installment number is<br>used|
|✓||Paytr_token: It is used to ensure|paytr_token (string)|**Please check the sample**|



Sayfa **1** / **2** 

||that the request comes from you<br>and the content did not change||**codes for calculation**|
|---|---|---|---|
|✓|User name and surname: First<br>and last name of the user that<br>you have on your system or<br>received via the order form|user_name (string)|Up to 60 characters|
|✓|User address: The address of the<br>user that you have on your<br>system or received via the order<br>form|user_address (string)|Up to 400 characters|
|✓|User phone number: The phone<br>number of the user that you<br>have on your system or received<br>via the order form|user_phone (string)|Up to 20 characters|
|✓|The page the user will be<br>redirected to after successful<br>payment (e.g. Order status / my<br>orders page)<br>(Warning: the payment may not<br>have been approved yet when<br>the user reaches this page)|merchant_ok_url|Up to 400 characters|
|✓|The page that the user will be<br>redirected to if something<br>unexpected occurs|merchant_fail_url|Up to 400 characters|
|✓|When the merchant is in live<br>mode, it can be sent as 1 to run a<br>test|<br>test_mode|0 or 1|
||Display errors: If the value is 1,<br>when wrong or incomplete<br>information is transmitted to the<br>API, error message is displayed<br>on the page.|debug_on (int)|0 or 1<br>**(Be sure to send 1 to**<br>**detect errors during the**<br>**integration and testing**<br>**process)**|
||If a value other than zero is sent,<br>payment must be completed<br>within that time. (e.g. You can<br>use it for security purposes in<br>case of price updates etc.)|timeout_limit(int)|In minutes<br>(30 minutes is assumed if<br>not sent)|
||Language to be used on pages<br>during payment process|lang(string)|**tr**for Turkish or**en**for<br>English<br>(tr is assumed if not sent)|



# *** The response to the iframe_token request is in JSON format:** 

- Successful response example: (includes **iframe_token** ) 

{"status":"success","token":"28cc613c3d7633cfa4ed0956fdf901e05cf9d9cc0c2ef8db54fa"} 

- Failed response example: 

{"status": "failed", "reason": "Required field value invalid: merchant_id"} 

The following HTML code block should be used to open the payment form. The **iframe_token** received in the successful response (explained above) is used in “src” attribute of iFrame. 

Sayfa **2** / **2** 

<script src= _"https://www.paytr.com/js/iframeResizer.min.js"_ ></script> 

<iframe src=" _https://www.paytr.com/odeme/guvenli/_ **iframe_token** " id= _"paytriframe"_ <u>frameborder=</u> _"0"_ <u>scrolling=</u> _"no"_ style="width: _100%_ ;"></iframe> 

<script>iFrameResize({},'#paytriframe');</script> 

Upon completion of the steps described above, the payment form should appear on the screen. This step concludes the part of the payment process which the user will interact with. **<u>HOWEVER</u>** <u>; the integration is not yet complete.</u> **STEP 2** must be completed in order to receive the payment result (success / failed) and to confirm / cancel the order. 

To complete the integration, please see the document inside **STEP 2** folder. 

Sayfa **3** / **2** 

