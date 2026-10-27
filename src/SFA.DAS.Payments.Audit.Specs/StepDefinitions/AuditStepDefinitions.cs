using Microsoft.EntityFrameworkCore;
using Reqnroll;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using UUIDNext;

namespace SFA.DAS.Payments.Audit.Specs.StepDefinitions
{
    [Binding]
    public class AuditStepDefinitions
    {
        private readonly ScenarioContext scenarioContext;
        private readonly MessagingContext messagingContext;
        private TestSession testSession;
        private Guid existingEarningEventId;
        private Guid newEarningEventId;
        public AuditStepDefinitions(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            testSession = new TestSession();
            existingEarningEventId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            newEarningEventId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            
            Console.WriteLine($"UKPRN : {testSession.Provider.Ukprn}, ULN: {testSession.Learner.Uln}");
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            await testSession.DataContext.ClearRequiredPaymentTestData(testSession);
        }

        [Given("the requiredpayments service has received an earnings event for a GSO short course")]
        [Given("the provider has made a change or there has been a change of circumstance resulting in a new milestone 1 earnings being generated")]
        public void BlankStep()
        {
        }

        [Given("the audit service has recorded the original set of earnings which includes a milestone1 payment")]
        public async Task TheAuditServiceHasRecordedTheOriginalSetOfEarnings()
        {
            var requiredPayment = CreateCalculatedRequiredLevyAmount();
            await testSession.Pv2MessageContext.Send(requiredPayment, EndpointLocation.RequiredPayments);

            await testSession.WaitForIt(async () =>
            {
                var foundPreviousMilestonePayment = await testSession.DataContext.RequiredPaymentEvent.AnyAsync(p =>
                    p.LearnerUln == testSession.Learner.Uln &&
                    p.Ukprn == testSession.Provider.Ukprn &&
                    p.JobId == testSession.JobId &&
                    p.EarningEventId == existingEarningEventId &&
                    p.ExternalEarningsId == existingEarningEventId
                );
                return foundPreviousMilestonePayment;
            }, "Expected original milestone1 payment to have been added");

            await testSession.WaitForIt(async () =>
            {
                var eventCount = await testSession.DataContext.RequiredPaymentEvent.CountAsync(p =>
                    p.LearnerUln == testSession.Learner.Uln &&
                    p.Ukprn == testSession.Provider.Ukprn &&
                    p.JobId == testSession.JobId &&
                    p.EarningEventId == existingEarningEventId &&
                    p.ExternalEarningsId == existingEarningEventId
                );
                return eventCount == 1;
            }, "Unexpected count of original milestone1 payment");

        }

        [Given("the provider has submitted a duplicate milestone1 without different externalEarningsId")]
        public async Task TheProviderHasSubmittedADuplicateMilestone1()
        {
            var requiredPayment = CreateCalculatedRequiredLevyAmount();
            await testSession.Pv2MessageContext.Send(requiredPayment, EndpointLocation.RequiredPayments);

            await testSession.WaitForIt(async () =>
            {
                var foundPreviousMilestonePayment = await testSession.DataContext.RequiredPaymentEvent.AnyAsync(p =>
                    p.LearnerUln == testSession.Learner.Uln &&
                    p.Ukprn == testSession.Provider.Ukprn &&
                    p.JobId == testSession.JobId &&
                    p.EarningEventId == existingEarningEventId &&
                    p.ExternalEarningsId == existingEarningEventId
                );
                return foundPreviousMilestonePayment;
            }, "Expected original milestone1 payment to have been added");

            await testSession.WaitForIt(async () =>
            {
                var eventCount = await testSession.DataContext.RequiredPaymentEvent.CountAsync(p =>
                    p.LearnerUln == testSession.Learner.Uln &&
                    p.Ukprn == testSession.Provider.Ukprn &&
                    p.JobId == testSession.JobId &&
                    p.EarningEventId == existingEarningEventId &&
                    p.ExternalEarningsId == existingEarningEventId
                );
                return eventCount == 1;
            }, "Unexpected count of original milestone1 payment");
        }

        [When("the requiredpayments service processes the new earnings")]
        public async Task WhenRequiredPaymentsServiceProcessesTheNewEarnings()
        {
            var requiredPayment = CreateCalculatedRequiredLevyAmount();
            requiredPayment.EarningEventId = newEarningEventId;
            requiredPayment.ExternalEarningsId = newEarningEventId;
            await testSession.Pv2MessageContext.Send(requiredPayment, EndpointLocation.RequiredPayments);
        }

        [Then("the audit service records the new earnings including the new milestone payment")]
        public async Task TheAuditServiceRecordsTheNewEarningsIncludingTheMilestonePayment()
        {
            await testSession.WaitForIt(async () =>
            {
                var foundNewMilestonePayment = await testSession.DataContext.RequiredPaymentEvent.AnyAsync(p =>
                        p.LearnerUln == testSession.Learner.Uln &&
                        p.Ukprn == testSession.Provider.Ukprn &&
                        p.JobId == testSession.JobId &&
                        p.EarningEventId == newEarningEventId &&
                        p.ExternalEarningsId == newEarningEventId
                );
                return foundNewMilestonePayment;
            }, "Expected new milestone1 payment to have been added");

            await testSession.WaitForIt(async () =>
            {
                var eventCount = await testSession.DataContext.RequiredPaymentEvent.CountAsync(p =>
                    p.LearnerUln == testSession.Learner.Uln &&
                    p.Ukprn == testSession.Provider.Ukprn &&
                    p.JobId == testSession.JobId &&
                    p.EarningEventId == newEarningEventId &&
                    p.ExternalEarningsId == newEarningEventId
                );
                return eventCount == 1;
            }, "Unexpected count of new milestone1 payment");
        }

        [Then("the audit service should not record the new earnings")]
        public async Task TheAuditServiceShouldNotRecordTheNewEarnings()
        {
            await testSession.WaitForIt(async () =>
            {
                var eventCount = await testSession.DataContext.RequiredPaymentEvent.CountAsync(p =>
                    p.LearnerUln == testSession.Learner.Uln &&
                    p.Ukprn == testSession.Provider.Ukprn &&
                    p.JobId == testSession.JobId &&
                    p.EarningEventId == existingEarningEventId &&
                    p.ExternalEarningsId == existingEarningEventId
                );
                return eventCount == 1;
            }, "Unexpected count of new milestone1 payment");
        }

        private CalculatedRequiredLevyAmount CreateCalculatedRequiredLevyAmount()
        {
            return new CalculatedRequiredLevyAmount
            {
                JobId = testSession.JobId,
                EarningEventId = existingEarningEventId,
                ExternalEarningsId = existingEarningEventId,
                Ukprn = testSession.Learner.Ukprn,
                EventTime = DateTimeOffset.UtcNow,
                EventId = existingEarningEventId,
                Learner = new Learner
                {
                    ReferenceNumber = "LR-12345",
                    Uln = testSession.Learner.Uln
                },
                LearningAim = new LearningAim
                {
                    Reference = "LA-54321",
                    ProgrammeType = 25,
                    StandardCode = 30,
                    CourseCode = "ZPROG001",
                    FrameworkCode = 445,
                    PathwayCode = 1,
                    FundingLineType = "16-18 Apprenticeship Levy Funding",
                    SequenceNumber = 1L,
                    StartDate = new DateTime(2024, 8, 1),
                    LearningType = LearningType.Apprenticeship
                },
                IlrSubmissionDateTime = new DateTime(2026, 6, 15),
                IlrFileName = "ILR-10012345-2526-20260615-123456.xml",
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 2526,
                    Period = 10
                },
                ClawbackSourcePaymentEventId = existingEarningEventId,
                PriceEpisodeIdentifier = "25-26-1-445-30-1-01/08/2024",
                AmountDue = 500.00m,
                DeliveryPeriod = 10,
                AccountId = 999000111,
                TransferSenderAccountId = 999000222,
                ContractType = ContractType.Act1,
                StartDate = new DateTime(2024, 8, 1),
                PlannedEndDate = new DateTime(2026, 7, 31),
                ActualEndDate = new DateTime(2026, 7, 20),
                CompletionStatus = 2,
                CompletionAmount = 1000.00m,
                InstalmentAmount = 250.00m,
                NumberOfInstalments = 12,
                LearningStartDate = new DateTime(2024, 8, 1),
                ApprenticeshipId = 400L,
                ApprenticeshipPriceEpisodeId = 800L,
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
                ReportingAimFundingLineType = "16-18 Apprenticeship Levy Funding",
                LearningAimSequenceNumber = 1L,
                SfaContributionPercentage = 0.95m,
                OnProgrammeEarningType = OnProgrammeEarningType.Milestone1,
                AgeAtStartOfLearning = 19,
                CourseType = CourseType.Apprenticeship,
                Priority = 1,
                AgreementId = "AG-998877",
                AgreedOnDate = new DateTime(2024, 7, 25),
                FundingPlatformType = FundingPlatformType.DigitalApprenticeshipService
            };
        }
    }
}
