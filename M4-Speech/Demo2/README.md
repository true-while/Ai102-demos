# M04 DEMO #2

- Speech to Text
- Custom Speech

This code is provided for demo purposes only for course AI-102.

### Requirements
- Azure Subscription

## Text to Speech

1. Follow the [link](https://azure.microsoft.com/en-us/services/cognitive-services/speech-to-text//) to observe speech to text capabilities by providing sample input from your microphone and observe the results of speech recognition. 

1. You also can upload the file `yesterday.wav` to observe recognition from the file.


## Speech Studio 

1. Build new if it does not exist cognitive multi-service in Azure portal.
 
1. Open speech portal [https://speech.microsoft.com/portal](https://speech.microsoft.com/portal) and sign in with your azure subscription account.

1. Navigate to 'Real-time Speech-to-text' tab to demonstrate how your speech will be recognized from the microphone. You also can use "Auto detect" for the language you use.

1. Next you can play with Pronunciation Assistant and train your pronunciation to be accrued to provided examples.

1. Next you can build a custom model by switching the 'Custom Speech'.
    - Build your project and upload speech dataset from local files `audio-and-trans1.zip` and `audio-and-trans1.zip` 
    - Train custom model on the one of the dataset.
    - Then test model by using another dataset and observe the result.

![test-custom](./test-custom-speech.png)

