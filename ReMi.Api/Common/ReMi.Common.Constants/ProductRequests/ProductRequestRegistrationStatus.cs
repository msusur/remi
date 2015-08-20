using ReMi.Contracts.Enums;

namespace ReMi.Common.Constants.ProductRequests
{
    public enum ProductRequestRegistrationStatus
    {
        [EnumDescription("New")]
        New = 1,
        [EnumDescription("In progress")]
        InProgress,
        [EnumDescription("Completed")]
        Completed
    }
}
