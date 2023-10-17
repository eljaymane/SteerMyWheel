## SteerMyWheel

# Abstract
This project aims to auto-detect scripts running on remote hosts, and automatically sync them with any git repository. 

It is based on a "CronReading" mechanism to detect executions, and from their path extract the repository name.
All the gathered informations are stored in a graph database (Neo4j).

# [WIP] Workflows : 
The next version will handle workflows. Which means executing scheduled scripts via SSH in a chained fashion. Furthermore, this feature aims to replace all the scripts, because it is easier to maintain one app than a hundred. 

The mechanism is magically simple : 
Since every scripts performs the same tasks in a different order and for different data (request data via sql or sftp, process it, send it back somewhere or send a mail...)

The idea is that we'll use blocks that represent the tasks of a workflow.

## Table of contents

*[General info](#general-info)

*[Setup](#setup)

## General info
The project is written with C# and based on .Net 7. So it is cross-platform.
Before running, make sure to fill the global config.xml file.

## Setup

Build : 
````
$ dotnet restore
$ dotnet build

````

Run : 

````
$ dotnet run

````


