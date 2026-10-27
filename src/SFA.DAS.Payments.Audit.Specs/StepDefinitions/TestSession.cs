using Bogus;
using NUnit.Framework;
using SFA.DAS.Payments.Audit.Specs.Data;
using SFA.DAS.Payments.Audit.Specs.Models;

namespace SFA.DAS.Payments.Audit.Specs.StepDefinitions
{
    public class TestSession
    {
        public string SessionId { get; } = Guid.NewGuid().ToString();
        private readonly Random random = new(Guid.NewGuid().GetHashCode());
        public Faker<Course> CourseFaker { get; } = new();
        public Provider Provider { get; }
        public Learner  Learner { get; }
        public TestSessionDataContext DataContext { get; }
        public TimeSpan TimeToWait => TimeSpan.FromSeconds(30);
        public TimeSpan TimeToPause => TimeSpan.FromSeconds(2);
        public long JobId { get; set; }
        public MessagingContext Pv2MessageContext { get; }
        public MessagingContext DASMessageContext { get; }


        public TestSession()
        {
            CourseFaker
                .RuleFor(course => course.AimSeqNumber, faker => faker.Random.Short(1))
                .RuleFor(course => course.FrameworkCode, faker => faker.Random.Short(1))
                .RuleFor(course => course.FundingLineType, faker => faker.Name.JobDescriptor() ?? "FundingLine")
                .RuleFor(course => course.LearnAimRef, "ZPROG001")
                .RuleFor(course => course.LearningPlannedEndDate, DateTime.Today.AddMonths(12))
                .RuleFor(course => course.LearningStartDate, DateTime.Today)
                .RuleFor(course => course.PathwayCode, faker => faker.Random.Short(1))
                .RuleFor(course => course.ProgrammeType, faker => faker.Random.Short(1))
                .RuleFor(course => course.StandardCode, faker => faker.Random.Int(1))
                .RuleFor(course => course.AgreedPrice, 15000);

            var cnn = TestRunBindings.Config["ConnectionStrings:PaymentsConnectionString"];
            DataContext = new TestSessionDataContext(cnn);
            Provider = GenerateProvider();
            Learner = GenerateLearner(Provider.Ukprn);
            JobId = GenerateId();
            Pv2MessageContext = new MessagingContext(TestRunBindings.PV2Endpoint);
            DASMessageContext = new MessagingContext(TestRunBindings.DASEndpoint);
        }

        public long GenerateId(int maxValue = 1000000)
        {
            var id = random.Next(maxValue);
            //TODO: make sure that the id isn't already in use.
            return id;
        }

        public Learner GenerateLearner(long ukprn, long uln = 0)
        {
            return new Learner
            {
                Ukprn = ukprn,
                Uln = uln != 0 ? uln :GenerateId(),
                LearnRefNumber = GenerateId().ToString(),
                Course = CourseFaker.Generate(1).FirstOrDefault(),
                LearnerIdentifier = Guid.NewGuid()
            };
        }

        public Provider GenerateProvider()
        {
            return new ProviderService(DataContext, TestRunBindings.Config["AppGuid"]).GetProvider();
        }

        


        public async Task WaitForIt(Func<Task<bool>> lookForIt, string failText)
        {
            var endTime = DateTime.Now.Add(TimeToWait);
            var lastRun = false;

            while (DateTime.Now < endTime || lastRun)
            {
                if (await lookForIt())
                {
                    if (lastRun) return;
                    lastRun = true;
                }
                else
                {
                    if (lastRun) break;
                }

                await Task.Delay(TimeToPause);
            }
            Assert.Fail($"{failText}  Time: {DateTime.Now:G}.  Ukprn: {Provider.Ukprn}. Job Id: {JobId}");
        }

        public async Task WaitForIt(Func<bool> lookForIt, string failText)
        {
            var endTime = DateTime.Now.Add(TimeToWait);
            var lastRun = false;

            while (DateTime.Now < endTime || lastRun)
            {
                if (lookForIt())
                {
                    if (lastRun) return;
                    lastRun = true;
                }
                else
                {
                    if (lastRun) break;
                }

                await Task.Delay(TimeToPause);
            }
            Assert.Fail($"{failText}  Time: {DateTime.Now:G}.  Ukprn: {Provider.Ukprn}. Job Id: {JobId}");
        }
    }
}

