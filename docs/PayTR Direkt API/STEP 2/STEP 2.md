

# PAYTR DIREKT API INTEGRATION STEP 2 

Create the Callback URL to Receive Payment Results 

When a user makes a payment using the form displayed inside the Iframe ( **STEP 1** ), the PayTR system makes a request to the merchant’s Callback URL. PayTR system must receive a response for this request. Otherwise, the payment process will not be considered as completed and the merchant will not be paid. 

The Callback URL where the payment result notification will be sent by the PayTR system should be specified by the merchant and should be defined on the SETTINGS (AYARLAR) page on the Merchant Panel (MAĞAZA PANELİ). 

The result of each payment process (success or failed) will be sent separately to the Callback URL by PAYTR system. In response to this request, the merchant will have to approve or cancel the user’s order and respond by <u>simply displaying OK to inform the PayTR system.</u> 

## ***  POST REQUEST FIELDS AND VALUES sent to the Callback URL by the PayTR system:** 

|**Field name**|**Success**|**Failed**|**Description**|
|---|---|---|---|
|merchant_oid|✓|✓|Merchant order id: The unique order ID set for the transaction and sent in STEP<br>1.|
|status|✓|✓|The result of the payment (‘success’ or ‘failed’)|
|total_amount|✓|✓|Total amount collected from the user<br>(Multiplied by 100: e.g. 34.56 => 3456)<br>(Note: The amount collected may be more than the "payment_amount" value<br>you sent in**STEP 1**in cases such as installment payments, alternative payment<br>methods, etc.)|
|hash|✓|✓|The hash value generated to check the received values are intact for security<br>purposes (**See the sample codes for the calculation**)|
|failed_reason_code||✓|Sent if payment is not approved<br>(See codes and description on the table below)|
|failed_reason_msg||✓|Explains why the payment is not approved (Only in Turkish for now)<br>(See codes and description on the table below)|
|test_mode|✓|✓|Sent as 1 in test mode or while running a test in live mode.|
|payment_type|✓|✓|Indicates the method which the customer used to complete the payment.<br>'card' or 'eft'.|
|currency|✓||Indicates the currency of payment. 'TL', 'USD', 'EUR', 'GBP', 'RUB'|
|payment_amount|✓||The "payment_amount" value that is sent in**STEP 1**<br>(Multiplied by 100: e.g. 34.56 => 3456)|



## *** The RESPONSE that the Callback URL gives to the PayTR system should be plain text OK** 

Example (PHP): echo "OK"; exit; Example (.NET): Response.Write("OK"); return; 

## **<u>IMPORTANT WARNINGS:</u>** 

1. You should not restrict access to your Callback URL by any means such as session control, etc. This is vital for the PayTR system to reach the page. 

2. You should not display HTML or any other content before or after the “OK” response. 

3. Callback URL is not a page which users see during payment process, thus there will be no user SESSION at this page and no SESSION values can be used. PayTR system submits a POST which contains relevant information such as “merchant_oid”. 

4. For payments which the PayTR system does not receive an OK response from the Callback URL, the status will be displayed as "In Progress" (Devam Ediyor) on the Transactions page on the Merchant Panel. 

Sayfa **1** / **2** 

5. When the PayTR system can not connect to the Callback URL or does not receive the OK response from the Callback URL, PayTR system will try again after a minute. This may happen due to network issues, instant overloads on merchant stystems, etc. Thus, multiple notifications for the same payment transaction can be received on Callback URL. For this reason, in such cases, it is very important that recurring notifications should be handled correctly on Callback URL. Only the first notification should be taken into account to approve/cancel the order and the recurring ones should only be responded to by displaying OK. Recurring notifications should be checked based on “merhant_oid” value. 

**6.** It is crucial for security reasons to check that the hash value in POST is the same as the hash value that will be created using the related values in POST. This is necessary to ensure that the POST request comes from the PayTR system and the values do not change during transport. **Be warned that if you do not check hash value, you may face financial losses.** 

|**failed_reason_code**|<br>**failed_reason_msg**|**Description**|
|---|---|---|
|0|VARIOUS (READ DESCRIPTION)|Detailed error message on why the payment was<br>not approved (For example: Card limit / balance is<br>insufficient).|
|1|Authentication not performed. Please try again<br>and complete the process.|The customer did not enter the mobile number in<br>the authentication step.|
|2|Authentication failed. Please try again and enter<br>the correct password.|The customer did not enter the correct password for<br>authentication.|
|3|Not approved after the security checks.|The customer's transaction failed to pass security<br>checks.|
|6|The customer refused to pay and left the<br>checkout page.|The customer did not complete the transaction in<br>the processing time (request_exp_date value<br>defined in STEP 1) or the customer closed the<br>payment page and ended the transaction.|
|8|Installment payment cannot be made by this<br>card.|The installment payment method selected by the<br>customer is not allowed with the card used.|
|9|There is no authorization to process this card.|Your store does not have transaction authorization<br>for the card the customer is using.|
|10|3D Secure must be used for this transaction.|The customer must pay with 3D Secure for this type<br>of transaction.|
|11|Security alert. Check your trading customer.|There is fraud detection in the customer's<br>transaction. For your safety, check the customer's<br>transactions.|
|99|Operation failed: Technical integration error.|Response to return if there is a technical integration<br>error. (if debug_on value is 0)|



To verify that the Callback URL is created in accordance with the explanations given above, a test payment should be made. 

- If the status of test payment is displayed as "Successful" (Başarılı) on the Transactions (İşlemler) page on PayTR Merchant Panel (Mağaza Paneli), the PayTR integration is complete. 

- If the status of test payment is displayed as "In Progress" (Devam Ediyor), it means that the PayTR system has not received "OK" response from the Callback URL. Click on the "Detail" (Detay) link of the test payment on the Transactions page and check what response PayTR system receives from the Callback URL to debug. 

Sayfa **2** / **2** 

