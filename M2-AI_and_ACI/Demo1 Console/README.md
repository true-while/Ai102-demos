# M01 DEMO #1

- Provisioning Azure Cognitive services
- Querying REST API request from console
- Querying from Python

This code is provided for demo purpose only for course AI-102.

### Requirements
- Azure Subscription
- Python 3.7 or higher
- VS code

## Provisioning Azure Cognitive services

1. Provision multi service from Azure portal. Follow instruction from [QuickStart](https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account?tabs=multiservice%2Clinux)

1. Grub `Key` and `Region` of provisioned service. 

1. Paste the keys in the text file for future use.


## Querying REST API request from console

1. Open Cognitive service [console](https://westus.dev.cognitive.microsoft.com/docs/services/)

1. Find by use search computer vision API v3.1 or highest. Chose **Describe image** service. 

1. The region should be the same as your service provisioned above.

1. Provide `Key` for subscription in header. 

1. Update body with link to your image.

>You can take any available image reference with out query parameters going after "?"

1. Analyze description result. Status code 200 correspond to successful recognition.

>If you get 500 status code try to use another image. Pay attention on HTTP/S and query parameters.


## Querying from Python

1. Open VS code and CD in the folder with py script.

1. Install requirements by command `pip install -r requrements.txt`

1. Update the `subscription_key` in the code with value you grub above.

1. Update the service region in variable `vision_base_url`

1. Execute script by command `py .\img.py` running from console of VS code. 

1. Observe resulted dialog and output in the console for details.

![Cow](./cow.png)