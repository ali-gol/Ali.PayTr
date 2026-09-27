

PAYTR CARD STORAGE INTEGRATION (CAPI) 

# **SAVING THE USER'S CARD – DURING THE PAYMENT (CAPI PAYMENT)** 

By using this service, you can create a user and a card belonging to the user registered at PAYTR during the payment. The process to be followed should be as follows. 

- **1-** Create your payment page as specified in the Direct API Document. 

- **2-** Add a checkbox in the step where the credit card information is entered, where the user can choose what he wants to register. 

- **3-** If user chooses to save card information, add necessary information to POST content: 

   - **a.** If a card is registered in the system for the first time in the name of the user, only the “store_card” parameter is sent in the POST content. 

   - **b.** If the user has a card previously defined in the system and wants to save a new card, the parameters “utoken” and “store_card” in POST content should be sent together. 

- **4-** In the notification (Notification URL) as a result of the payment, record the following values sent for card storage in your relevant tables and keep them ready for the next transaction. 

**Card retention information returned in addition to payment information as a result of the payment transaction** 

|**Explanation**|**Variable / Type**|
|---|---|
|User Token: Token created by PAYTR that is specific to the user on your site. You must match<br>this token with the user who traded on your system.|utoken (string)|



# **GETTING THE USER'S REGISTERED CARD LIST (CAPI LIST)** 

- **1-** In order to list the cards registered in PAYTR to the user when a user starts the payment process, make a request to <u>https://www.paytr.com/odeme/capi/list with the following parameters.</u> 

|**Compulsory**|**Explanation**|**Variable / Type**|
|---|---|---|
|✓|Store Number: Store number given to you by PAYTR|merchant_id (integer)|
|✓|User Token: User specific token notified to you by PAYTR system in post-payment<br>payment notification.|utoken (string)|
|✓|PayTR Token: It is the value that you will create to make sure that the request<br>comes from you and that the content has not changed (You should look at the<br>sample codes regarding the calculation)|paytr_token (string)|



- **2-** The values in the table below will return to JSON format. When no match is found with the information you sent, the answer is returned as empty JSON. 

|**Explanation**|**Variable / Type**|**Possible / Sample Values**|
|---|---|---|
|Status: Returns an error in the event of an error, not an operation when<br>successful|<br>status (string)|error|
|Error Message: If the request is unsuccessful, the error reason is<br>returned in err_msg|err_msg (string)|Example: Connection error<br>occurred|
|Card Token: The token that identifies the user's registered card|ctoken (string)||
|Last 4: Last 4 digits of the registered card|last_4 (string)||
|Month: Month information of the card's expiration date|month (string)|Örnek: 05|
|Year: Year information of the card's expiration date|year (string)|Örnek: 28|
|Bank: The bank of the card|c_bank (string)|Example: Yapı Kredi|
|Name Surname: Name surname entered by the user during card<br>registration|c_name||



Page **1** / **4** 

|Card Program Partnership Name|c_brand (string)|Example: maximum,<br>bonus, world vb.|
|---|---|---|
|Card Type: Credit or debit card / prepaid card|c_type (string)|credit or debit|
|Company Card: Information whether the card is a company card|businessCard (string)|y / n|
|Card Scheme. If it is not known which scheme the card belongs to,<br>answer returns as OTHER.|schema (string)|VISA, MASTERCARD,<br>AMEX, TROY, etc.|



- **3-** List the registered cards that the user can choose by getting the returning card information. 

- **4-** Start the payment using the ctoken information of the selected registered card and the utoken information of the user (If the require_cvv value is 1 for the selected card, you must provide the user with a field to enter a CVV and send the CVV in the payment request). 

# **DELETING THE USER CARD (CAPI DELETE)** 

- **1-** To delete a card from a user's registered cards, make a request by sending the following parameters to <u>https://www.paytr.com/odeme/capi/delete</u> 

|**Compulsory**|**Explanation**|**Variable / Type**|
|---|---|---|
|✓|Store Number: Store number given to you by PAYTR|merchant_id (integer)|
|✓|PayTR Token: It is the value that you will create to make sure that the request<br>comes from you and that the content has not changed (You should look at the<br>sample codes regarding the calculation)|paytr_token (string)|
|✓|User Token: User specific token notified to you by PAYTR system in post-payment<br>payment notification.|utoken (string)|
|✓|Card Token: The token that identifies the user's registered card.|ctoken (string)|



**2-** The values in the table below will return to JSON format. You can inform your user according to the response. 

|**Explanation**|**Variable / Type**|**Possible / Sample Values**|
|---|---|---|
|Status: Indicates that the card deletion request made was<br>successful or failed.|status (string)|success or error|
|Error Message: If the request is unsuccessful, the error reason is<br>returned in err_msg|err_msg (string)|Example: No card or<br>previously deleted|



# **RECURRING PAYMENT WITH REGISTERED CARD** 

Using this service, you can pay for your user with a card available in PAY for recurring payment. 

1-)Create the payment request block wtih specified values. The payment process will be formed as a result of the request that you will send to the service with the registered card information through the structure that you will create yourself. 

2-)For this reason, there is no need to create a form to interact with the user.Transactions will take place as Non3D (Non Secure).Your user will october take any additional actions or any information will not be requested from him during the process.(In order to use it, your store must have Non3D permissions). 

Page **2** / **4** 

3-)From the CAPI LIST service, you need to access the ctoken data by using the utoken data belonging to the user whose name you want to pay for.After that, utoken, ctoken and the following table with the specified values https://www.paytr.com/odeme you can request payment by **POST** method to the address. 

|**Mandator**|**y Description**|**Field name / type**|**Limitations & Notes**|
|---|---|---|---|
|✓|Merchant ID: Your Merchant ID (Mağaza no) provided by<br>PayTR|merchant_id (integer)||
|✓|Paytr_token: It is used to ensure that the request comes<br>from you and the content did not change|paytr_token (string)|Please check the sample codes<br>for calculation|
|✓|User ip: User IP received during the request<br>(Important: Make sure you send the external IP address<br>when you run tests on your local machine)|user_ip (string)|Up to 39 characters (ipv4)|
|✓|Merchant order id: The unique order id you set for the<br>transaction.<br>(Note: Order number is posted back within callback<br>notification - on STEP 2)|merchant_oid (string)|<sup>Up to 64 characters,</sup><br>Alpha numeric|
|✓|User email address: The email address which;<br>a) the user registered with on your system<br>b) or you received via the order form|email (string)|Up to 100 characters|
|✓|Payment type|payment_type(string)|('card', 'card_points')|
|✓|Payment amount: The total amount of the order.|payment_amount<br>(double), decimal (.)<br>and two digits after<br>the point.|For example: 100.99 or 150 or<br>1500.35|
|✓|Installment count|installment_count(int)|<sup>0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11,</sup><br>12|
||Card type (For installment transactions)<br>Currency|card_type(string)<br>currency(string)|advantage, axess, combo,<br>bonus, cardfinans, maximum,<br>paraf, world, saglamkart<br>TL(or TRY), EUR, USD (TL is<br>assumed if not sent)|
||Language to be used on payment process|client_lang(string)|**tr**for Turkish or**en**for English<br>(tr is assumed if not sent)|
||When the merchant is in live mode, it can be sent as 1 to run<br>a test|test_mode|0 or 1|
||For Non3D transactions send this value as 1|non_3d|0 or 1|
||If you need to test failed Non3D transaction send 1 (non_3d<br>and test_mode values must be both 1)|non3d_test_failed|0or1|
|✓|Card holder name|cc_owner(string)|Up to 50 characters|
|✓|Card number|card_number(string)|Up to 16 characters|
|✓|Card expiry date (Month)|expiry_month(string)|1, 2, 3, .. , 11, 12|
|✓|Card expiry date (Year)|expiry_year(string)|18, 19, 20,…|
|✓|Card security code|cvv(string)|Up to 4 characters|
|✓|The page the user will be redirected to after successful<br>payment (e.g. Order status / my orders page)<br>(Warning: the payment may not have been approved yet<br>when the user reaches this page)|merchant_ok_url|Up to 400 characters|



Page **3** / **4** 

|✓|The page that the user will be redirected to if something<br>unexpected occurs|merchant_fail_url|Up to 400 characters|
|---|---|---|---|
|✓|User name and surname: First and last name of the user that<br>you have on your system or received via the order form|user_name (string)|Up to 60 characters|
|✓|User address: The address of the user that you have on your<br>system or received via the order form|user_address (string)|Up to 400 characters|
|✓|User phone number: The phone number of the user that you<br>have on your system or received via the order form|user_phone (string)|Up to 20 characters|
|✓|User basket/order contents|user_basket (string)|JSON - Please check the<br>sample codes for structure|
|✓|User Token: User specific token notified to you by PAYTR<br>system in post-payment payment notification.|utoken(string)||
|✓|Card Token: The token that identifies the user's registered<br>card|ctoken(string)||
||Display errors: If the value is 1, when wrong or incomplete<br>information is transmitted to the API, error message is<br>displayed on the page.|debug_on (int)|0 or 1<br>(Be sure to send 1 to detect<br>errors during the integration<br>and testing process)|
||Recurring: After sending a payment request, the response in<br>JSON format returns directly to the request result without<br>redirecting to the successful or unsuccessful page according<br>to the result of the transaction. In addition; Details of the<br>transaction are sent to the defined Notification URL address.<br>The values that the status field returned as a result of<br>recurring can receive are “failed”, “wait_callback” and<br>“success”.<br>Note: The Non3D authorization must be turned on in your<br>store for this operation.|recurring (int)|0 or 1<br>(A request must be sent to us<br>in order for the relevant<br>authorization to be defined to<br>the store. If it is approved by<br>our units, the authorization<br>will be defined to the store.)|



## **RECURRING RESPONSE** 

|**status**|**msg (description)**|**try_again**|
|---|---|---|
|failed|“The card was closed by the bank. Do not send transactions again with this<br>card.” or a different error message.|false|
|failed|You have an ongoing transaction, you can try again after it is completed.|true|
|wait_callback|Checking Payment, Wait for Notification.|-|
|success|Successful Payment|-|



Page **4** / **4** 

