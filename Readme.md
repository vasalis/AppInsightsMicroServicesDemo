# Microservices Distributed Tracing with Application Insights

This repo show cases:

1. Application Insights Distributed Tracing for Microservices and Out-of-the-box dependency tracking for Application Insights.
2. Microservices deployment options for Azure (PaaS, AKS, Container Apps).
3. Infrastructure As Code (IaC) that deploys the solution on these different "deployment targets".

The project is parted of:

1. 3 Microservices (Devices, Alerts and BackOffice) with an Application Gateway (Ocelot) as the ingress.
2. Each microservice has a different persistent layer (CosmosDb, Azure SQL and Azure Table Storage).
3. Message Bus for inter-microservice communication is Azure Service Bus.

The idea is to:

1. Deploy all 4 components on different Azure Services that can host containers (App Services, Azure Functions, AKS, Container Apps).
2. Simulate traffic, using JMeter tests, and
3. See Application Insights Application Map which shows how a request is being propagated and processed (aka distributed tracing).

**There is a demo of how this works, from my session on ITProd Dev 2020 :point_right: [here](https://www.youtube.com/watch?v=gYndFGtNGe0)**.

While the demo is back on 2020, the main principles still apply. 
What is updated are the deployment options for the Microservices, with the recent add of Azure Container Apps.

## Starting with

You will need two Service Principals, one for running all workflows and one for running the AKS Cluster with Role Based Access Control RBAC. These two can be the same one.

step 1:
 az ad sp create-for-rbac -n "GitHubdeploySP" --sdk-auth --role contributor

 - This will create a SP with Contributor access to the default/active subscription - this way it has access to great a new Resource Group as needed for the Setup_Infrastructure to run. Alternativelly you could create first a Resource Group and then an SP with access rights only to that Resource Group -> In this case make sure you set RESOURCE_GROUP_NAME on Setup_Infrastructure env variable to the name of the Resource Group you created.

 Create the following SECRETS on GitHub:

 1. AZURE_CREDENTIALS: the result from step 1, entire JSON
 2. AKS_SERVICE_PRINCIPAL_ID: ClientId value from step 1
 3. AKS_SERVICE_PRINCIPAL_SECRET: clientSecret value from step 1

 step 2:
 Run Setup_Infrastructure.yml (after changing RESOURCE_GROUP_NAME and AZURE_REGION to your desired naming)
-> This should do the following:
1. Create the PaaS infrstructure - all config values should be passed to Web Apps and Azure Functions
2. Create AKS Infrastructure
3. Create GitHub Secrets to be used for the AKS CI/CD Pipelines


## SECRETS NEEDED
### PaaS

1. Four publishing profiles for Api gateway, Alerts microservice, Devices microservice, Back office microservice
    
