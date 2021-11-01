# M10 DEMO #2

- Face identification 

This code is provided for demo purposes only for course AI-102.

### Requirements
- Azure Subscription
- Visual Studio 2019 or higher
- NetFramework 4.6
- [Windows Presentation Foundation](https://visualstudio.microsoft.com/vs/features/wpf/)

## Image Classification 

>Note: The code of demo is improved version of face tutorial from [repo](https://github.com/Azure-Samples/Cognitive-Face-CSharp-sample)

1. From Azure portal build Cognitive Face API service with Standard Size. Copy API Key and Endpoint values.

1. Open solution file `FaceIdentifier.sln` in VS 2019.

1.  Update/Create `appsettings.json` file with values copied above.

  ```json
   {
      "subscriptionKey": "",
      "faceEndpoint": ""
   }
  ```

1. Build and run project. The project includes 5-persons face library located in the project folder.
Click on the button **Load Faces** to load and train library. 

   >Note: You can modify face library based on the context of your photos by adding or removing faces in the project folder. The images must be located in folder `FaceLib` in own folder with person's name. eg. `FaceLib\Alex`  Make sure you mark image file as `Build action: content` and `Always copy to output`.


1. Loading and train process can takes about 2 min. It is important to have the library trained. Standard tier of the Face API operate faster then free. If the during the raining process you get an error you should start over by closing application.

   ![load](load-bar.png)

1. Click button 'Brows' to load photo. Source code include G7 photos for detection. After 10-15 secondes the faces on the images will have a red border.

   ![analyzed](analyzed.png)

1. To observer results of analyzing pont the mouse cursor to the red rectangle of the face. The details should be updated in the bottom status bar and tooltip

   ![results](results.png)

1. Alternatively you can demonstrate Face verification algorithm working under the hood of the face recognition on the following [page](https://azure.microsoft.com/en-us/services/cognitive-services/face/)
