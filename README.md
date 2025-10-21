# Payments V2 Audit

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/DCT/_apis/build/status/GitHub/Service%20Fabric/SkillsFundingAgency.das-payments-v2-audit?branchName=main)](https://dev.azure.com/sfa-gov-uk/DCT/_apis/build/status/GitHub/Service%20Fabric/SkillsFundingAgency.das-payments-v2-audit?branchName=main)
[![Jira Project](https://img.shields.io/badge/Jira-Project-blue)](https://skillsfundingagency.atlassian.net/secure/RapidBoard.jspa?rapidView=782&projectKey=PV2)
[![Confluence Project](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/3700621400/Provider+and+Employer+Payments+Payments+BAU)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)


The Payments V2 Audit ServiceFabric application provides functionality for recording information about the activity of applications within the Payments V2 system

More information here: https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/400130049/4.+Payments+v2+-+Components+DAS+Space

## How It Works

This repository contains services that record information about the following Payments V2 applications:

* Data Locks
* Earning Events
* Funding Source
* Required Payments

Records are created in the Payments V2 Audit SQL Server database that logs the activity from the respective applications, using the information published in event messages, which are processed in batches.

## 🚀 Installation

### Pre-Requisites

Setup instructions can be found at the following link, which will help you set up your environment and access the correct repositories: [https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/950927878/Development+Environment+-+Payments+V2+DAS+Space](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/4948754681)

Select the configuration for the Audit application

## 🔗 External Dependencies

N/A

## Technologies

* .NetCore 2.1/3.1/6
* Azure SQL Server
* Azure Functions
* Azure Service Bus
* ServiceFabric

## 🐛 Known Issues

N/A
