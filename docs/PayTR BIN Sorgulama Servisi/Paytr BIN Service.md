

**PAYTR BIN SERVICE** 

You can send a BIN number to access the detailed information of the card using that service. 

- 1- A request is made to the bin service https://www.paytr.com/odeme/api/bin-detail along with BIN number of the card(The first 6 or 8 digits of the card number) and the fields that must be sent. 

|**Compulsory**|**Explanation**|**Variable / Type**|
|---|---|---|
|✓|Store Number: Store number given to you by PAYTR|merchant_id<br>(integer)|
|✓|The first 6 or 8 digits of the card number of the card to be questioned. Use 8<br>digits for maximum verification.|bin_number (string)|
|✓|paytr_token: This value is the one you need to create to make sure the<br>request is coming from you and that the values it contains have not changed.|<sup>paytr_token (string)</sup>|



2- Your response to this request is returned in JSON format. 

- a. If the BIN Number is not defined (For example, if it is a foreign card), the status value returns "failed". b. If the BIN number is defined, the status value returns as "success" with the information in the table below returns. 

- c. If you have an error in the request, the status value returns "error". In this case, you should check the "err_msg" for error details. 

When the status value is successful, the returned values are in the table below. 

|**Explanation**|**Variable / Type**|**Values**|
|---|---|---|
|Status: Result of the request (Response)|status (string)|success, error or failed|
|Card Type|cardType (string)<br>|credit / debit|
|Company Card: Whether the card is a company card or<br>not|businessCard (string)|y / n|
|Bank: The bank of the card|bank (string)|Example: Yapı Kredi|
|Program partners: (If the card is not included in a<br>program partnership, the value will be**none**. In this case,<br>installment payment can not be made with the card on<br>PayTR.)|brand (string)|Example: axess, bonus,<br>cardfinans, combo, world,<br>paraf, advantage,<br>maximum, saglamkart|
|Information on which scheme the card belongs to. (If it is<br>not known which scheme the card belongs to, answer<br>returns as OTHER.)|schema (string)|VISA, MASTERCARD,<br>AMEX, TROY, OTHER|
|Bank Code: Card bank code|bankCode (int)|Example: 0010|



