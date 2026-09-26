

PAYTR DIRECT API INTEGRATION 

STEP 1 

- 1) The customer starts a new order on the merchant webpage. 

- 2) Merchant redirects the customer to the page where the payment options are presented. 

- 3) After the customer inputs and confirms the information for the payment, the merchant should produce a security token. The data to be used in token production is listed below. Please check sample codes for clarification on token production. 

# **Data to be used in token production** 

|**Mandatory**|**Description**|**Field name / type**|**Limitations & Notes**|
|---|---|---|---|
|✓|Merchant ID: Your Merchant ID (Mağaza no) provided by<br>PayTR|merchant_id (integer)||
|✓|User ip: User IP received during the request<br>(Important: Make sure you send the external IP address when<br>you run tests on your local machine)|user_ip (string)|Up to 39 characters (ipv4)|
|✓|Merchant order id: The unique order id you set for the<br>transaction.<br>(Note: Order number is posted back within callback<br>notification - on STEP 2)|merchant_oid (string)|Up to 64 characters,<br>Alpha numeric|
|✓|User email address: The email address which;<br>a) the user registered with on your system<br>b) or you received via the order form|email (string)|Up to 100 characters|
|✓|Payment amount: The total amount of the order.|payment_amount<br>(integer)|Brackets only send as dot<br>(.)|
|✓|Payment type|payment_type(string)|('card', 'card_points')|
|✓|Installment count|installment_count(int)|0, 2, 3, 4, 5, 6, 7, 8, 9, 10,<br>11, 12|
||Currency|currency(string)|TL, EUR, USD, GBP, RUB (TL<br>is assumed if not sent)|
||When the merchant is in live mode, it can be sent as 1 to run a<br>test|test_mode|0 or 1|
||For Non3D transactions send this value as 1|non_3d|0 or 1|
||If a value other than zero is sent, payment must be completed<br>within that time. (e.g. You can use it for security purposes in<br>case of price updates etc.)|request_exp_date(int)|Timestamp<br>(30 minutes is assumed if<br>not sent)|



- 4) The Merchant system sends the token, and the data used to produce the token, and the data which is not used for the token production but required for payment proccess, to https://www.paytr.com/odeme with POST method ( **Important Notice** : The form on the merchant page must POST only to PayTR URL because it contains sensitive card information. POST should never be sent to the server of the merchant.) 

|**Mandatory**|**Description**|**Field name / type**|**Limitations & Notes**|
|---|---|---|---|
|✓|Merchant ID: Your Merchant ID (Mağaza no) provided by<br>PayTR|merchant_id (integer)||
|✓|Paytr_token: It is used to ensure that the request comes<br>from you and the content did not change|paytr_token (string)|Please check the sample codes<br>for calculation|
|✓|User ip: User IP received during the request<br>(Important: Make sure you send the external IP address<br>when you run tests on your local machine)|user_ip (string)|Up to 39 characters (ipv4)|
|✓|Merchant order id: The unique order id you set for the<br>transaction.<br>(Note: Order number is posted back within callback<br>notification - on STEP 2)|merchant_oid (string)|<sup>Up to 64 characters,</sup><br>Alpha numeric|
|✓|User email address: The email address which;<br>a) the user registered with on your system|email (string)|Up to 100 characters|



Page **2** / **4** 

|✓|b) or you received via the order form<br>Payment type|payment_type(string)|('card', 'card_points')|
|---|---|---|---|
|✓|Payment amount: The total amount of the order.|payment_amount<br>(double), decimal (.)<br>and two digits after<br>the point.|For example: 100.99 or 150 or<br>1500.35|
|✓|Installment count|installment_count(int)|<sup>0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11,</sup><br>12|
||Card type (For installment transactions)|card_type(string)|advantage, axess, combo,<br>bonus, cardfinans, maximum,<br>paraf, world, saglamkart|
||Currency|currency(string)|TL(or TRY), EUR, USD (TL is<br>assumed if not sent)|
||Language to be used on payment process|client_lang(string)|**tr**for Turkish or**en**for English<br>(tr is assumed if not sent)|
||When the merchant is in live mode, it can be sent as 1 to run<br>a test|test_mode|0 or 1|
||For Non3D transactions send this value as 1|non_3d|0 or 1|
||If you need to test failed Non3D transaction send 1 (non_3d<br>and test_mode values must be both 1)|non3d_test_failed|0or1|
|✓|Card holder name|cc_owner(string)|Up to 50 characters|
|✓|Card number|card_number(string)|Up to 16 characters|
|✓|Card expiry date (Month)|expiry_month(string)|1, 2, 3, .. , 11, 12|
|✓|Card expiry date (Year)|expiry_year(string)|18, 19, 20,…|
|✓|Card security code|cvv(string)|Up to 4 characters|
||The page the user will be redirected to after successful|||
|✓|payment (e.g. Order status / my orders page)<br>(Warning: the payment may not have been approved yet<br>when the user reaches this page)|merchant_ok_url|Up to 400 characters|
|✓|The page that the user will be redirected to if something<br>unexpected occurs|merchant_fail_url|Up to 400 characters|
|✓|User name and surname: First and last name of the user that<br>you have on your system or received via the order form|user_name (string)|Up to 60 characters|
|✓|User address: The address of the user that you have on your<br>system or received via the order form|user_address (string)|Up to 400 characters|
|✓|User phone number: The phone number of the user that you<br>have on your system or received via the order form|user_phone (string)|Up to 20 characters|
|✓|User basket/order contents|user_basket (string)|JSON - Please check the<br>sample codes for structure|
||Display errors: If the value is 1, when wrong or incomplete<br>information is transmitted to the API, error message is<br>displayed on the page.|debug_on (int)|0 or 1<br>(Be sure to send 1 to detect<br>errors during the integration<br>and testing process)|
||Sync Mode: After sending a payment request, the response<br>in JSON format returns directly to the request result without<br>redirecting to the successful or unsuccessful page according<br>to the result of the transaction. In addition; Details of the<br>transaction are sent to the defined Notification URL address.<br>The values that the status field returned as a result of sync<br>mode can receive are “failed”, “wait_callback” and “success”.<br>**Note**: The Non3D authorization must be turned on in your<br>store for this operation.|<br>sync_mode(int)|0 or 1 (A request must be sent<br>to us in order for the relevant<br>authorization to be defined to<br>the store. If it is approved by<br>our units, the authorization<br>will be defined to the store.)|



Page **3** / **4** 

||**SYNC MODE RESPONSE**||
|---|---|---|
|**status**|**msg (description)**|**utoken, ctoken**<br>(If the card has been<br>stored)|
|failed|_“You have a process that is still in progress, you can try it again after it is_<br>_completed”._or a different error message.|-|
|wait_callback|Checking Payment, Wait for Notification.|✓|
|success|Successful Payment|✓|



5)The PAYTR system performs checks and verifications on the sent information and directs the customer to the merchant_ok_url or merchant_fail_url address given by the merchant according to the payment result. In case of redirecting to merchant_ok_url, no data is sent in the POST content.In case of redirecting to merchant_fail_url, the fail_message field in the POST content contains information about why the payment failed. This information is a formatted message that can be displayed directly to the customer. 

6)The result of the payment attempt is entered into the Notification URL (Callback URL) on the Merchant website, token, merchant order number and payment status, etc. The member workplace is notified by posting the information. 

This step concludes the part of the payment process which the user will interact with. **<u>HOWEVER</u>** <u>; the integration is not yet complete.</u> **STEP 2** must be completed in order to receive the payment result (success / failed) and to confirm / cancel the order. 

To complete the integration, please see the document inside **STEP 2** folder. 

Page **4** / **4** 

