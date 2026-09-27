

# **PAYTR REFUND SERVICE INTEGRATION** 

## **IMPORTANT WARNING: Incorrect integration may cause incorrect returns and therefore you may experience financial loss. Please be very careful during integration! You can contact us for your questions.** 

- **1-** For the order you want to return, send the order number and the refund amount to the other information specified in the table below https://www.paytr.com/odeme/iade via POST. 

|**Compulsory**|**Explanation**|**Variable / Type**|
|---|---|---|
|✓|Store Number: Store number given to you by PAYTR|merchant_id (integer)|
|✓|Order Number: The order number of the transaction you want to return.|merchant_oid (string)|
|✓|Refund Amount: The amount you wish to return for the relevant order. (Only a<br>period (.) should be sent as delimiter. E.g.: 10.25)|return_amount (integer)|
|✓|PayTR Token: It is the value that you will create to make sure that the request<br>comes from you and that the content has not changed (You should look at the<br>sample codes regarding the calculation)|paytr_token (string)|
||Reference No: In case of transmission, it returns from**Status Query**service.|Up to 64 characters,<br>**Alpha numeric**|



- **2-** Your request will be returned in JSON format. 

   - a. If there is no transaction for the order number you have requested, the status value returns failed. 

   - b. If there is a transaction for the order number you have requested, the status value returns success and the information in the table below returns. 

   - c. If you have an error in the query, the status value returns an error. In this case, you should check the err_msg content for the error detail. 

|**Explanation**|**Values**|
|---|---|
|If the return request is successful, success returns.|Status (success /<br>failed)|
|Returns 1 if the return request is for testing.|is_test|
|Order number for which a return request was made.|merchant_oid|
|Amount made for a refund.|return_amount|
|Reference number, if submitted.|reference_no|



Page **1** / **2** 

