using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Models
{
    public static class ArchiveRunInformationHelper
    {
        public static ArchiveRunInformation Merge(ArchiveRunInformation state, ArchiveRunInformation input)
        {
            if (state == null) return input;
            if (input == null) return state;

            // Example merge logic (adjust based on actual properties of ArchiveRunInformation)
            state.JobId = input.JobId;
            state.Status = input.Status ?? state.Status;
            state.InstanceId = input.InstanceId ?? state.InstanceId;

            return state;
        }
    }
}
