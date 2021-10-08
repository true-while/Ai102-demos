# M08 DEMO #1

- Computer Vision

This code is provided for demo purposes only for course AI-102.

### Requirements
- Azure Subscription
- Python


## Computer Vision

1. Navigate to the integrated [tutorial](https://azure.microsoft.com/en-us/services/cognitive-services/computer-vision/)

1. Observe **object detection** capability with providing object position and description.

    ![obj](obj.png)

1. Observe **read and OCR** capability from handwritten and printed text.

    ![read](read.png)

1. Run the scripts from the `example` folder. Update `.env` file with your service region and key.

    ```INI
    ACCOUNT_REGION=your region
    ACCOUNT_KEY=your key
    ```

1. Observe **thumbnail generation** capability by running `thumb.py`.

    ![crop](crop.png)

1. Observe **landmarks detection** capability by running `landmarks.py`.

    ![landmarks](landmarks.png)