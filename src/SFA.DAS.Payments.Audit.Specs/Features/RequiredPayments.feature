Feature: PV2-4065 - Prevent duplicate recording of duplicate GSO payments

Scenario: Verify UX_RequiredPaymentEvent_LogicalDuplicates allows for reprocessing of GSO payments when a change of circumstance occurs

Given the requiredpayments service has received an earnings event for a GSO short course
And the audit service has recorded the original set of earnings which includes a milestone1 payment
And the provider has made a change or there has been a change of circumstance resulting in a new milestone 1 earnings being generated
When the requiredpayments service processes the new earnings
Then the audit service records the new earnings including the new milestone payment

Scenario: Verify UX_RequiredPaymentEvent_LogicalDuplicates does not allow for exact duplicate entries

Given the requiredpayments service has received an earnings event for a GSO short course
And the audit service has recorded the original set of earnings which includes a milestone1 payment
And the provider has submitted a duplicate milestone1 without different externalEarningsId
When the requiredpayments service processes the new earnings
Then the audit service should not record the new earnings