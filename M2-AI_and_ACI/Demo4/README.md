# M02 DEMO #4

- Azure Container Instance
- Run vision service in local container

This code is provided for demo purpose only for course AI-102.

### Requirements
- Azure Subscription
- Docker Desktop
- VS code

## Azure Container Instance

1. Complete [Tutorial](https://docs.microsoft.com/en-us/azure/machine-learning/tutorial-automated-ml-forecast) and publish the finished model to Azure Container Instance.

1. Grab reference to the deployed service and open python script `bike-forecast.py`

1. Replace values of `endpoint` & `key` with values of your model deployed on ACI. 

1. Run the script and get output with forecast:

![forecast](bikes.png)


## Run vision service in local container

1. Make sure you install and run Docker Desktop.

1. Execute command from file `command.azcli` in VS code terminal or CMD.

1. Navigate to [http://localhost:5000](http://localhost:5000) and- observe the home page.

1. Following output should came from console where you run the command:

![console](./screen/container-local.png)

1. Navigate to the swagger page located on the address [http://localhost:5000/swagger](http://localhost:5000/swagger) and chose operation **Read**

1. Chose file for OCR read from the local folder eg. `tabs-vs-spaces.png`

1. Click on `execute` button to get the result of reading as output JSON.

![Result](./screen/container-result.png)