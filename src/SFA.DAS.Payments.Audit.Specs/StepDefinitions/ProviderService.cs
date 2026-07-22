using SFA.DAS.Payments.Audit.Specs.Data;
using SFA.DAS.Payments.Audit.Specs.Models;

namespace SFA.DAS.Payments.Audit.Specs.StepDefinitions
{
    public class ProviderService
    {
        private readonly TestSessionDataContext dataContext;
        private string appGuid;

        public ProviderService(TestSessionDataContext dataContext, string appGuid )
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.appGuid = appGuid;
        }

        public Provider GetProvider()
        {

            Provider provider = null;
            using (var mutex = new Mutex(false, $"Global\\{{{appGuid}}}"))
            {
                if (mutex.WaitOne(TimeSpan.FromMinutes(1)))
                {
                    provider = dataContext.LeastRecentlyUsed();
                    provider.Use();
                    dataContext.SaveChanges();

                    mutex.ReleaseMutex();
                }
                else
                {
                    throw new ApplicationException("Unable to obtain a Ukprn due to a locked Mutex");
                }
            }

            return provider;
        }
    }
}