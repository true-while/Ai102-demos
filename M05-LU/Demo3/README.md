# M05 DEMO #3

- LU integration.

This code is provided for demo purposes only for course AI-102.

### Requirements
- Azure Subscription
- .Net core 3.1
- VS Code or VS 2019

## LU Integration.

1. Build new LUIS model by importing file `PictureBotLuisModel.lu`.

1. Build and run Azure bot. The source code available on [repo](https://github.com/true-while/ai100-demo-bot). 

1. Configuration file `appsettings.json` should be updated with your values before publishing to the Azure Bot instance. 

1. Following sentences can be tested and detected as intent with % provided in output from bot:

    - Hello; What is up; Hey; Howdy.
    - Search for dog pictures; Find flower pic.

![bot](bot-intents.png)

