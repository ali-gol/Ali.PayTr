

**PAYTR INSTALLMENT SERVICE** 

You can check the installment rate of your PayTR merchant using installment service. 

- 1- A POST request is made to the installment rate service https://www.paytr.com/odeme/taksit-oranlari along with the fields that must be sent. 

|**Compulsory**|**Explanation**|**Field name / Type**|
|---|---|---|
|✓|Merchant Number: Merchant number given to you by PayTR|merchant_id<br>(integer)|
|✓|Request ID: With a maximum of 32 characters.|request_id<br>(string)|
|✓|paytr_token: This value is the one you need to create to make sure<br>the request is coming from you and that the values it contains have<br>not changed.|paytr_token<br>(string)|



- 2- Your response to this request is returned in JSON format. 

   - a. If there is not a mistake in the request status value returns as **success** with the information in the table below returns. 

   - b. If there is any mistake in the request, the status value returns **error** In this case, you should check the **err_msg** for error details. 

When the status value is **success** , the returned values are in the table below. 

|**Explanation**|**Field name / Type**|**Values**|
|---|---|---|
|Status: Request result.|status (string)|success or error|
|Query ID: The value you send during the request..|request_id (string)|-|
|Installment limit for non commercial cards : It is the<br>maximum numbers of installments you can offer for<br>customers who use non commerical cards.|max_inst_non_bus<br>(string)|2,3,4,5,6,7,8,9,10,11,12|



