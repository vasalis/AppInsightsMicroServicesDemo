# Starting with

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


# SECRETS NEEDED
## PaaS

1. Four publishing profiles for Api gateway, Alerts microservice, Devices microservice, Back office microservice
    