

**PAYTR TRANSACTION DETAIL SERVICE** 

You can access the details of the sales and return transactions made within the date duration (3 days maximum) using that service. 

- 1- A request is made to transaction detail service <u>https://www.paytr.com/rapor/islem-dokumu</u> along with BIN number of the card(The first 6 digits of the card number) and the fields that must be sent. 

|**Compulsory**|**Explanation**|**Field name / Type**|
|---|---|---|
|✓|Merchant Number: Merchant number given to you by PayTR|merchant_id<br>(integer)|
|✓|Start Date: Date Format:**2021-01-01 00:00:00 (YYYY-MM-DD**<br>**hh:mm:ss)**|start_date (string)|
|✓|End Date: Date Format:**2021-01-01 23:59:59 (YYYY-MM-DD**<br>**hh:mm:ss)**|end_date (string)|
|✓|Demo Data: It is used to simulate the data returned from the<br>service. The data are not real, it can only be used for testing.|dummy (1 veya 0)|
|✓|paytr_token: This value is the one you need to create to make<br>sure the request is coming from you and that the values it<br>contains have not changed.|paytr_token<br>(string)|



- 2- Your response to this request is returned in JSON format. 

   - a. If there is no transaction within the given date duration, the status value returns **failed** . 

   - b. If there is a transaction within the given date duration the status value returns as 

      - " **success** " with the information in the table below returns. 

   - c. If there is any mistake in the request, the status value returns " **error** ". In this case, you should check the "err_msg" for error details. 

When the status value is “successful", the returned values are in the table below. Same values are returned in Sales and Returns transactions. 

|**Explanation**|**Field name / Type**|**Values**|
|---|---|---|
|Transaction Type: The type of the transaction.|islem_tipi(string)|S (Sales) or I (Return)|
|Net Amount: The amount after deduction.|net_tutar (string)|e.g. 9.76|
|Deduction Amount: The amount deducted for the<br>transaction.|kesinti_tutari (string)|e.g. 0.24|
|Deduction Rate: The deduction rate for the<br>transaction.|kesinti_orani (string)|e.g. 2.35|
|Transaction Amount: The amount of the transaction<br>made.|islem_tutari (string)|e.g. 10.00|
|Payment Amount: It is returned if there is a<br>payment above the transaction amount.|odeme_tutari (string)|<sup>e.g. 10.00</sup>|
|Transaction Date: The transaction was made.|islem_tarihi (strgin)|e.g. 13.01.2021|
|Currency: Transaction currency.|para_birimi (string)|TL, USD, EUR, GBP, RUB|
|Installment : If the payment is made in installments,<br>the number of installments will return.|taksit (string)|0,2,3,4,5,6,7,8,9,10,11,12|
|Card Brand: The brand of the card used in the<br>transaction.|kart_marka (string)|e.g. WORD, BONUS, etc.|



|Card No: The masked credit card number that is|kart_no (string)|e.g. 455359***6747|
|---|---|---|
|processed.|||
|Order Number: Order number of the transaction.|siparis_no (strgin)|e.g. ABC123|
|Payment Type: The type of payment.|odeme_tipi (strgin)|KART(Card) or  EFT|



