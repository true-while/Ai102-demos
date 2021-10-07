# M02 DEMO #2

- Provisioning Azure KeyVault
- Securing connectivity to Azure Cognitive Services
- Retrieving secrets from KeyVault

This code is provided for demo purposes only for course AI-102.

### Requirements
- Azure Subscription
- .NET core 3.1
- VS code or VS 2019

## Provisioning Azure KeyVaul

1. Provision keyvault from Azure portal. Follow instruction from [QuickStart](https://docs.microsoft.com/en-us/azure/key-vault/secrets/quick-create-portal)

1. Create Service principal from Azure portal. Follow instruction from [QuickStart](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#register-an-application-with-azure-ad-and-create-a-service-principal)

1. Grab `Subscription ID`, `AppID` and `Key` from Service principal. 

1. From `Access policy` of Keyvault add service principal with `get` and `list` permission to secret of keyvault as explained in the [tutorial](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#configure-access-policies-on-resources). 



## Retrieving secrets from KeyVault

1. Open VS code or project file in VS 2019.

1. Update `appsettings.json` with your values collected above:

    ```json
        "AZURE_CLIENT_ID": "your service account app id",
        "AZURE_CLIENT_SECRET": "your service account secret",
        "AZURE_TENANT_ID": "your tenant id",
        "KeyVaultName": "short name of your keyvault",
        "secretName": "key"
    ```

1. Run the application by command `dotnet run`

1. Observe the value on the console and compare it with the value of the secret KEY1 (`hello world` in my case).

![Console](./screen.png)