

PAYTR STATUS INQUIRY SERVICE 

Status inquiry service is divided into two categories as Merchant Status Inquiry and Marketplace Status Inquiry. 

You can access the status of **<u>merchant</u>** transactions made. You can view the returns if there are any. 

- 1- A request is made to status inquiry service https://www.paytr.com/odeme/durum-sorgu along with the fields that must be sent. 

|**Compulsory **|**Explanation**|Field name / Type|
|---|---|---|
|✓|Merchant Number: Merchant number given to you by PayTR|merchant_id<br>(integer)|
|✓|Merchant Number: Merchant number given to you by PayTR|merchant_oid<br>(string)|
|✓|PayTR Token: It is the value that you will create to make sure that the<br>request comes from you and that the content has not changed (You<br>should look at the sample codes regarding the calculation)|paytr_token<br>(string)|



- 1- Your request will be returned in JSON format. 

   - a. If there is no error in the query, the status value is "success" and the information in the table below returns. 

   - b. If you have an error in the query, the status value returns an error. In this case, you should check the err_msg content for the error detail. 

|Explanation|Field name / Type|**Values**|
|---|---|---|
|Status: Result of query|status (string)|success or error|
|Payment Amount: Amount information for the<br>order.|payment_amount(string)|<sup>10,8</sup>|
|Payment Total: The amount paid by the customer for<br>the order.|payment_total(string)|10,8|
|Returns: If there is a refund in the order, the value is<br>returned.|returns(string)||
|Currency : Currency|Currency(string)|TL,$ etc.|
|err_no: error number.|err_no|004|
|err_msg: error message.|err_msg|Failed to find<br>successful payment<br>with merchant_oid|



You can access the status of **<u>marketplace</u>** transactions made. You can view the returns or submerchant payments if there are any. 

- 1- A request is made to status inquiry service https://www.paytr.com/odeme/durum-sorgu along with the fields that must be sent. 

|**Compulsory **|**Explanation**|Field name / Type|
|---|---|---|
|✓|Merchant Number: Merchant number given to you by PayTR|merchant_id<br>(integer)|
|✓|Merchant Number: Merchant number given to you by PayTR|merchant_oid<br>(string)|
|✓|PayTR Token: It is the value that you will create to make sure that the<br>request comes from you and that the content has not changed (You<br>should look at the sample codes regarding the calculation)|paytr_token<br>(string)|



- 2- Your request will be returned in JSON format. 

   - a. If there is no error in the query, the status value is "success" and the information in the table below returns. 

   - b. If you have an error in the query, the status value returns an error. In this case, you should check the err_msg content for the error detail. 

|Explanation|Field name / Type|**Values**|
|---|---|---|
|Status: Result of query|status (string)|success or error|
|Payment Amount: Amount information for the<br>order.|payment_amount(string)|<sup>10,8</sup>|
|Payment Total: The amount paid by the customer for<br>the order.|payment_total(string)|10,8|
|Returns: If there is a refund in the order, the value is<br>returned.|returns(string)||
|Currency : Currency|Currency(string)|TL,$ etc.|
|err_no: error number.|err_no|004|
|err_msg: error message.|err_msg|Failed to find<br>successful payment<br>with merchant_oid|
|submerchant_payments: Submerchant payments|submerchant_payments||



