# Introduction
This is a collection of Azure Functions that provides a report of all outbound traffic for Network Security Groups (NSGs). This report is useful for anlayzing the impact when implementing more restrictive outbound network rules. Outbound traffic that is destined to known Azure datacenter subnets is labeled accordingly.

![Outbound IP report](/images/outbound-ip-report.png)

There are three functions in this solution:

## UpdateAzureIpRanges
This is a timer-triggered function that downloads the published Microsoft Azure IP ranges and stores them in a storage table (DatacenterIpRanges).

## FilterNsgFlowEvents
This is a blob-triggered function that parses Network Security Group flow logs (PT1H.json files). It filters on network flow events that match the default NSG outbound rule (DefaultRule_AllowInternetOutBound) and writes out flow tuple data (NSG ID, destination IP, destination port, protocol) to an Azure Queue message. Flow tuple ata is de-duplicated 

## UpdateNsgFlowStats
This is a queue-triggered function that processes de-duplicated flow tuples from FilterNsgFlowEvents. Each destination IP address is compared to a recent list of known Azure IP subnets. If the destination IP address falls within such a subnet, it is documented in the output report.

# Getting Started

## Set up the Azure Functions Infrastucture
1.  Create an Azure Functions App via: https://functions.azure.com
2.	In the functions app, navigate to Platform Features > Application Settings and find an app setting named: AzureWebJobsStorage. Identify the storage account name in the connection string
3.  Navigate to the storage account and create the following items: **NsgFlowStats** (table), **NetworkFlowProcessStatus** (table), **DatacenterIpRanges** (table), **nsgflows** (queue)

## Configure NSG Flow Log Monitoring
1.  Enable the Network Watcher feature on each NSG requiring analysis
2.  Enable "NetworkSecurityGroupFlowEvent" in the Diagnostics section for each NSG. Ensure the target storage account is the same as the one configured for Azure Functions

## Clone the source and configure integration with Functions
Clone the repository to your computer and upload it to your own repository of choice (Visual Studio Online, GitHub, Bitbucket, etc...). Navigate to your Function App and configure your source repository in Platform Features > Deployment Options. Shortly after this, three functions will be deployed to your Function App.

# Build and Test
Documentation for developing and debugging Functions locally can be found here: https://blogs.msdn.microsoft.com/appserviceteam/2017/03/16/publishing-a-net-class-library-as-a-function-app/