# M05 DEMO #4

- Speech Integration


This code is provided for demo purposes only for course AI-102.

### Requirements
- Azure Subscription
- Node.js
- VS code

## Speech Integration

1. The code based on the hands-on example converted to Node.js and adopted to execute on RPi. The sample is working with a microphone and expects input from the microphone of any sentence and recognizes it as text and evaluates it with the LUIS model. Finally provide the requested response of the time or date depending on location. 

1. Use the import option to restore the LUIS model from file `clock.lu`.

1. Settings file .env should be updated based on your values.

    ```INI
    LU_APP_ID=your luis app id
    LU_PREDICTION_REGION=your cs region
    LU_PREDICTION_KEY=your cs key
    ```

1. Original code and installation instruction can be found in following [repo](https://github.com/true-while/AI-102-AIEngineer-Nodejs)

1. When the application is starting it is expecting input from the microphone in one of the following sentences.

    - What time is it?
    - What date is it now?
    - What time in London?

1. In the output you can observe JSON responses from LUIS. You can re-train the model to fix unrecognized intents or locations.

    ![result](result.png)

>The sample includes Speech recognition and Speech synthesizing code examples. 

