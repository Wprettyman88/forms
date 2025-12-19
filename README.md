# WiseLink for Label Products
Label Quotes.  
Label Orders.  
Label Invoices.  

## Deployment
[Production](https://wiselabels20251125155258-btbvc6cwdwetbjdc.canadacentral-01.azurewebsites.net/)7e
[Development](https://wiselabels2025112515525-wiselabels-dev-f7bqa3fsamakdpcs.canadacentral-01.azurewebsites.net/)
[Demo Site Wrapper](http://azwbfdev/wrapper.html)

## Software Product Requirements

### User Access Requirements

1. **Resellers - No Authentication**: Automatic authentication in the background to a default web user for resellers. Landing page/popup for verification where resellers agree they are a reseller/distributor and provide their company name and contact name to be recorded with the quote.

2. **Resellers - WiseLink Account**: Automatic authentication in the background to the associated account/credentials linked to their WiseLink account.

3. **End Users - White Label**: Automatic authentication in the background to a default web user for use with reseller-branded white label websites. Records both the reseller and end user company and contact information.

### Outputs for the Quote

1. Quote Letter PDF download link
1. Automatic Email to Sales, Customer Service, etc. This email could have an attachment, or the quote data in another format in the email body.
1. Automatic Email to the Customer.

**See [discountlabels.com](https://discountlabels.com) for reference as this is what most of our customers are used to using now.


### Filtering Options on the Quote Form Page

Where possible use the "AllowQuickQuote" filterable field.
This Includes:
Dies
Substrates
Printing (Color Codes)
Finishing (Finishing Types)
Packing (Packing Procedures)

#### Finishing Field Filters based on Printing Field Choice
If a "Printing" option (aka Color Code) is chosen that includes the string "Flexo", then filter the options in Finshing to options that are Finishing Type "1", which is "Inline".

If a "Printing" option (aka Color Code) is chosen that includes the string "Digital", then filter the options in Finishing to options that are Finishing Type "2", which is "Offline".

See v1etaf__.etap_typ in Cerm SQL Database for values of "1" for "Inline" or "2" for "Offline"

### Color Code Sample
```json
{
    "Id": "DRL",
    "Descriptions": [
        {
            "ISOLanguageCode": "en-US",
            "Description": "Delam / Relam"
        },
        {
            "ISOLanguageCode": "en-GB",
            "Description": "Delam / Relam"
        }
    ],
    "AllowQuickQuote": false,
    "AllowRFQ": false,
    "Blocked": false
},
```
#### Existing Die Field Filters based on Printing Field Choice
If a "Printing" option (aka Color Code) is chosen that includes the string "Flexo", then filter the Die options down to Material Type "1" (Rotary Die)

If a "Printing" option (aka Color Code) is chosen that includes the string "Digital", then filter the Die options down to Material Type "2" (Flexible Die)

See stnspr__.materie_ in Cerm SQL Database for values of "1" for "Rotary Die" or "2" for "Flexible Die"


### ToDo:
1. Add contact information fields for resellers and end users during the quote/order/invoice process.
2. Store contact information with the quote/order/invoice records. 
3. Display contact information on generated documents (quotes, orders, invoices).
4. Printing needs to come from CERM.
5. Filter Materials.
6. Change Materials to use substrates API from CERM.
7. Auth via GP account number.



Parameter order
1. Description - working
2. Shape - working
3. Corners - working
4. Label Size - working
5. Material/Substrate - pull from API. No filter.
6. Printing - working.
7. Cutting Die - requires Printing.
8. Finishing


