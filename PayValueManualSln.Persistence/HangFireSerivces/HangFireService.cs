using Hangfire;

namespace PayValueManualSln.Infrastructure.Persistence.HangFireSerivces
{
    public static class HangFireService
    {
        public static void InitializeService()
        {
            // Pusher Starts Here
            const string generateExternalPaymentCodeAsyncJobId = "482cbab9d555-423abfe6c2c5b541276c";
            RecurringJob.RemoveIfExists(generateExternalPaymentCodeAsyncJobId);

            //push assessment to repository
            const string sendAssessmentToRepositoryAsyncJobId = "482cbab9d555-423abfe6c2b5b5212764";
            RecurringJob.RemoveIfExists(sendAssessmentToRepositoryAsyncJobId);
            
            //Calculate Renewal Date
            const string CalculateRenewalDateAsyncJobId = "432cbbb9d455-423abfe6c2b5b1212794";
            RecurringJob.RemoveIfExists(CalculateRenewalDateAsyncJobId);
            
            //Renewal Assessment
            const string RenewalAssessmentAsyncJobId = "432cccc9d255-423abfe6c2b5b11232764";
            RecurringJob.RemoveIfExists(RenewalAssessmentAsyncJobId);

            //Push Pending Assessment To BPMS By Agency
            RecurringJob.AddOrUpdate<ServiceScheduler>(generateExternalPaymentCodeAsyncJobId, x => x.GenerateExternalPaymentCodeForPendingAssessmentAsync(), Cron.Hourly(2));
            RecurringJob.AddOrUpdate<ServiceScheduler>(sendAssessmentToRepositoryAsyncJobId, x => x.SendAssessmentToRepository(null), Cron.Hourly(1));
            RecurringJob.AddOrUpdate<ServiceScheduler>(CalculateRenewalDateAsyncJobId, x => x.CalculateRenewalDate(), Cron.Hourly(1));
            RecurringJob.AddOrUpdate<ServiceScheduler>(RenewalAssessmentAsyncJobId, x => x.RenewAssessment(), Cron.Hourly(1));

        }
    }
}
