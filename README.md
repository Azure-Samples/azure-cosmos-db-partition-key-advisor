
# Partition Key Advisor

Data analyzing tool for helping customers choose the best partition key for their Cosmos DB collections.

## Features

This project framework provides the following features:

* Schema discovery and cardinality assesment
* Display of storage distribution based on logical partition keys
* Display of distinctness of keys at a given second 
* Recommendation based on cardinality, distinctness/sec and storage distribution for write-heavy workloads

## Getting Started

Before you can run this sample, you must have the following prerequisites:

* Visual Studio
* Azure SDK for Visual Studio
* .NET Core 2.2. SDK

From Visual Studio, open the PartitionKeyAdvisor.sln file from the root directory.

In Visual Studio Build menu, select Build Solution (or Press F6).

You can substitute the endpoint and primary key in the appsettings.json with your Cosmos DB account values including the endpointUri and primary key information. 

You can now run and debug the application locally by pressing F5 in Visual Studio.

## Demo

This demo is included to show you how to use the project.

To run the demo using existing configurations, follow these steps:

1. Build project as stated above, and add your connection string/keys found in your Azure Account.
2. To test a collection: run it on a DatabaseID and CollectionID of your choosing.
3. For candidate partition keys, choose partition keys available within your collection: ex."make_id", "make_is_common", "make_country".
