# Introduction
This is a collection of Azure Functions that provides a report of all outbound traffic for Network Security Groups (NSGs) that matches the default rule "DefaultRule_AllowInternetOutBound". This report is useful for anlayzing the impact when implementing more restrictive outbound network rules.

# Getting Started

## Set up the Azure Functions Infrastucture
1.  Create an Azure Functions App via: https://functions.azure.com
2.	In the functions app, navigate to Platform Features > Application Settings and find an app setting named: AzureWebJobsStorage. Identify the storage account name in the connection string
3.  Navigate to the storage account and create the following items: **NsgFlowStats** (table), **NetworkFlowProcessStatus** (table), **DatacenterIpRanges** (table), **nsgflows** (queue)

## Configure NSG Flow Log Monitoring
1.  Enable the Network Watcher feature on each NSG requiring analysis
2.  Enable "NetworkSecurityGroupFlowEvent" in the Diagnostics section for each NSG. Ensure the target storage account is the same as the one configured for Azure Functions

## Clone the source and configure integration with Functions
Clone the repository to your computer and upload it to your own repository. 

# Build and Test
TODO: Describe and show how to build your code and run the tests. 