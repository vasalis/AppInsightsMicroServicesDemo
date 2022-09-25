#---------------------------------------- Config Part -----------------------------------------------------------#
# Resource Group and Azure Region variables
$resourceGroupName = $env:RESOURCE_GROUP_NAME
$azureRegion = $env:AZURE_REGION
# Use this to have unique name on every component - you can modify differently if needed.
$deploymentPrefix = $resourceGroupName.substring(0,4)
Write-Host "(Got from ENV): RG: " $resourceGroupName " location: "  $azureRegion 
Write-Host "Environment Azure CL: " az --version

# Cosmos Related variables
$storageAccountName = ($deploymentPrefix + 'storage').ToLower()
$cosmosDbAccount = ($deploymentPrefix + 'cosmosdb').ToLower()
$cosmosDbName = 'DevicesDatabase'
$cosmosDbContainerName = 'Devices'
$cosmosDbPartitionKey = '/category'

# Azure SQL Database variables
$SQLServername = ($deploymentPrefix + 'sqlsrv').ToLower()
$SQLDbName = 'Alerts'
$SQLlogin="AdminLogin"
$SQLPassword="TODO-ChangeThisAsap123"

# Service Bus Related variables
$serviceBusNameSpace = 'aksNpaas-service-bus'
$serviceBusMessageBusTopicNamePaaS = 'paasmessagebus'
$serviceBusMessageBusTopicNameAks = 'aksmessagebus'

# Functions related variables
$functionDevicesName = $deploymentPrefix + 'paasdevices'
$functionDevicesAiName ='paas-devicesai'
$functionBackOfficeName = $deploymentPrefix + 'paasboffice'
$functionBackOfficeAiName ='paas-backofficeai'

# Azure Container Registry variables
$acrName = $deploymentPrefix + 'acr'

# Web Apps Related Variables
$apiGwServicePlanName = 'apigwsp'
$alertsServicePlanName = 'alertssp'
$apiGwWebAppName = $deploymentPrefix + 'apigw'
$apiGwAiName = 'paas-apigwai'
$alertsWebAppName = $deploymentPrefix + 'alerts'
$alertsAiName = 'paas-alertsai'

#---------------------------------------- Execution Part -----------------------------------------------------------#

# Create the resource group
Write-Host 'About to create resourse group: ' $resourceGroupName -ForegroundColor Green
az group create -l $azureRegion -n $resourceGroupName

# Create the Service Bus
Write-Host 'About to create Service Bus: ' $serviceBusNameSpace ', ' $serviceBusMessageBusTopicNamePaaS ', ' $serviceBusMessageBusTopicNameAks  -ForegroundColor Green
az servicebus namespace create --resource-group $resourceGroupName --name $serviceBusNameSpace --location $azureRegion
az servicebus topic create --resource-group $resourceGroupName --namespace-name $serviceBusNameSpace --name $serviceBusMessageBusTopicNamePaaS
az servicebus topic subscription create --resource-group $resourceGroupName --namespace-name $serviceBusNameSpace --topic-name $serviceBusMessageBusTopicNamePaaS --name 'all'
az servicebus topic create --resource-group $resourceGroupName --namespace-name $serviceBusNameSpace --name $serviceBusMessageBusTopicNameAks
az servicebus topic subscription create --resource-group $resourceGroupName --namespace-name $serviceBusNameSpace --topic-name $serviceBusMessageBusTopicNameAks --name 'all'
$serviceBusConnectionString = az servicebus namespace authorization-rule keys list --resource-group $resourceGroupName --namespace-name $serviceBusNameSpace --name RootManageSharedAccessKey --query primaryConnectionString --output tsv

# Create the Cosmos Db
Write-Host 'About to create cosmos db: ' $cosmosDbAccount -ForegroundColor Green
az cosmosdb create --name $cosmosDbAccount --resource-group $resourceGroupName

# Get Cosmos keys and pass them as Application variables
$cosmosPrimaryKey = az cosmosdb keys list --name $cosmosDbAccount --resource-group $resourceGroupName --type keys --query 'primaryMasterKey'
# create connection string
$cosmosConString = "AccountEndpoint=https://"+$cosmosDbAccount+".documents.azure.com:443/;AccountKey="+$cosmosPrimaryKey

# Create the Azure SQL Database
az sql server create --name $SQLServername --resource-group $resourceGroupName --location $azureRegion --admin-user $SQLlogin --admin-password $SQLPassword
# Setup the db
az sql db create --resource-group $resourceGroupName --server $SQLServername --name $SQLDbName --edition GeneralPurpose --family Gen5 --capacity 2 --zone-redundant false

# Create the storage account to be used for Functions
Write-Host 'About to create storage: ' $storageAccountName -ForegroundColor Green
az storage account create -n $storageAccountName -g $resourceGroupName -l $azureRegion --kind StorageV2

# Create Azure Storage Table
az storage table create --name "BackOfficeLogs" --account-name $storageAccountName
$tblConnectionString = az storage account show-connection-string -g $resourceGroupName -n $storageAccountName --output tsv

# Create Azure Container Registry
Write-Host 'About to create Azure Container Registry: ' $acrName
az acr create -n $acrName -g $resourceGroupName --sku Standard --admin-enabled true
$acrUserName = $(az acr credential show -n $acrName --query username).replace('"','')
$acrPassword = $(az acr credential show -n $acrName --query passwords[0].value).replace('"','')

# Create Application Insights for Web Apps and Functions
az extension add --name application-insights

# Create Application Insights for Web Apps
Write-Host 'About to create AI for apiwgw: ' $apiGwAiName -ForegroundColor Green
az monitor app-insights component create -a $apiGwAiName -l $azureRegion -g $resourceGroupName
$apiGwAiKey = az monitor app-insights component show --app $apiGwAiName -g $resourceGroupName --query 'instrumentationKey'
Write-Host 'Got app insights key for Api GW: ' $apiGwAiKey -ForegroundColor Yellow

Write-Host 'About to create AI for alerts: ' $alertsAiName -ForegroundColor Green
az monitor app-insights component create -a $alertsAiName -l $azureRegion -g $resourceGroupName
$alertsAiKey = az monitor app-insights component show --app $alertsAiName -g $resourceGroupName --query 'instrumentationKey'
Write-Host 'Got app insights key for Api GW: ' $alertsAiKey -ForegroundColor Yellow

# Create Application Insights for Azure Functions
Write-Host 'About to create AI for devices: ' $functionDevicesAiName -ForegroundColor Green
az monitor app-insights component create -a $functionDevicesAiName -l $azureRegion -g $resourceGroupName
$functionDevicesAiKey = az monitor app-insights component show --app $functionDevicesAiName -g $resourceGroupName --query 'instrumentationKey'
Write-Host 'Got app insights key for Devices: ' $functionDevicesAiKey -ForegroundColor Yellow

Write-Host 'About to create AI for back office: ' $functionBackOfficeAiName -ForegroundColor Green
az monitor app-insights component create -a $functionBackOfficeAiName -l $azureRegion -g $resourceGroupName
$functionBackOfficeAiKey = az monitor app-insights component show --app $functionBackOfficeAiName -g $resourceGroupName --query 'instrumentationKey'
Write-Host 'Got app insights key for Back Office: ' $functionBackOfficeAiKey -ForegroundColor Yellow

# Create new Azure Functions with .NetCore, Application Insights
# Linux is not working for consumption plan: https://github.com/Azure/azure-cli/pull/12817
# Hosted agents use Azure CLI 2.3.1
# az functionapp create -c $azureRegion -n $functionsName --os-type Linux -g $resourceGroupName --runtime dotnet -s $storageAccountName --app-insights $appInsightsName --app-insights-key $appInsightsKey

# Create Azure Function for Devices and set Config values
Write-Host 'About to create Devices function: ' $functionDevicesName -ForegroundColor Green
az functionapp create -c $azureRegion -n $functionDevicesName --os-type Linux -g $resourceGroupName --runtime dotnet -s $storageAccountName --app-insights $functionDevicesAiName --app-insights-key $functionDevicesAiKey

# Setup environment variables for Azure Function
az functionapp config appsettings set --name $functionDevicesName --resource-group $resourceGroupName --settings "CosmosConnectionString=$cosmosConString"
az functionapp config appsettings set --name $functionDevicesName --resource-group $resourceGroupName --settings "CosmosDbName=$cosmosDbName"
az functionapp config appsettings set --name $functionDevicesName --resource-group $resourceGroupName --settings "CosmosDbContainerName=$cosmosDbContainerName"
az functionapp config appsettings set --name $functionDevicesName --resource-group $resourceGroupName --settings "CosmosDbPartitionKey=$cosmosDbPartitionKey"
az functionapp config appsettings set --name $functionDevicesName --resource-group $resourceGroupName --settings "ServiceBusConnectionString=$serviceBusConnectionString"
az functionapp config appsettings set --name $functionDevicesName --resource-group $resourceGroupName --settings "ServiceBusTopicName=$serviceBusMessageBusTopicNamePaaS"

# Create Azure Function for Back Office
Write-Host 'About to create Back Office function: ' $functionBackOfficeName -ForegroundColor Green
az functionapp create -c $azureRegion -n $functionBackOfficeName --os-type Linux -g $resourceGroupName --runtime dotnet -s $storageAccountName --app-insights $functionBackOfficeAiName --app-insights-key $functionBackOfficeAiKey
az functionapp config appsettings set --name $functionBackOfficeName --resource-group $resourceGroupName --settings "ServiceBusConnectionString=$serviceBusConnectionString"
az functionapp config appsettings set --name $functionBackOfficeName --resource-group $resourceGroupName --settings "ServiceBusTopicName=$serviceBusMessageBusTopicNamePaaS"
az functionapp config appsettings set --name $functionBackOfficeName --resource-group $resourceGroupName --settings "StorageConnectionString=$tblConnectionString"

# Create Web Apps for Api Gateway and Alerts
Write-Host 'About to create App Service Plans: ' $apiGwServicePlanName ',  ' $alertsServicePlanName -ForegroundColor Green
az appservice plan create -g $resourceGroupName -n $apiGwServicePlanName --is-linux --sku S1
az appservice plan create -g $resourceGroupName -n $alertsServicePlanName --is-linux --sku S1

Write-Host 'About to create Web Apps: ' $apiGwWebAppName ',  ' $alertsWebAppName -ForegroundColor Green
az webapp create -g $resourceGroupName -p $apiGwServicePlanName -n $apiGwWebAppName -i $acrName".azurecr.io/docker-image:tag"
az webapp create -g $resourceGroupName -p $alertsServicePlanName -n $alertsWebAppName -i $acrName".azurecr.io/docker-image:tag"

Write-Host 'About to set AI Keys for Web Apps: ' $apiGwWebAppName ',  ' $alertsWebAppName -ForegroundColor Green
az webapp config appsettings set --name $apiGwWebAppName --resource-group $resourceGroupName --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$apiGwAiKey"
az webapp config appsettings set --name $alertsWebAppName --resource-group $resourceGroupName --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$alertsAiKey"

Write-Host 'About to set ASPNETCORE_ENVIRONMENT for ApiGW: ' $apiGwWebAppName -ForegroundColor Green
az webapp config appsettings set --name $apiGwWebAppName --resource-group $resourceGroupName --settings "ASPNETCORE_ENVIRONMENT=ProductionOnAzure"

# Set Firewall rule for SQL Db
$webAppIP = az webapp config hostname get-external-ip --webapp-name $alertsWebAppName --resource-group $resourceGroupName --output tsv
az sql server firewall-rule create --resource-group $resourceGroupName --server $SQLServername -n AllowYourIp --start-ip-address $webAppIP --end-ip-address $webAppIP

# Setup connection string to Alerts Web App
$SQLConnectionString = az sql db show-connection-string -s $SQLServername -n $SQLDbName -c ado.net
$SQLConnectionString = $SQLConnectionString.replace("<username>", $SQLlogin).replace("<password>", $SQLPassword)
az webapp config appsettings set --name $alertsWebAppName --resource-group $resourceGroupName --settings "SQLServer__ConnectionString=$SQLConnectionString"

# Set Environment Variables for next step in order to set GitHub Secrets needed for CI/CD pipelines
# Connection Strings, Cosmos and Service Bus - this needed for AKS Infra, as for PaaS it is passed on Web app and Functions configuration variables


Write-Output "::set-output name=TMP_SERVICEBUS_CON::$serviceBusConnectionString"
Write-Output "::set-output name=TMP_COSMOS_CON::$cosmosConString"
Write-Output "::set-output name=TMP_COSMOS_DB_NAME::$cosmosDbName"
Write-Output "::set-output name=TMP_COSMOS_CONTAINER_NAME::$cosmosDbContainerName"
Write-Output "::set-output name=TMP_COSMOS_PARTITION_KEY::$cosmosDbPartitionKey"
Write-Output "::set-output name=TMP_SQL_CONNECTION_STRING::$SQLConnectionString"
Write-Output "::set-output name=TMP_AZURETABLE_CONNECTION_STRING::$tblConnectionString"
$acrName = $acrName + ".azurecr.io"
Write-Output "::set-output name=TMP_ACR_NAME::$acrName"
Write-Output "::set-output name=TMP_ACR_USER_NAME::$acrUserName"
Write-Output "::set-output name=TMP_ACR_PASSWORD::$acrPassword"

# testing
# Write-Output $acrName ", " $acrUserName ", " $acrPassword