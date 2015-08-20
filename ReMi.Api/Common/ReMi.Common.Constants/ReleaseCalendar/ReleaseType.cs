using ReMi.Contracts.Enums;

namespace ReMi.Common.Constants.ReleaseCalendar
{
    public enum ReleaseType
    {
        [EnumDescription("Scheduled Release")]
        Scheduled = 1,
        [EnumDescription("Hotfix")]
        Hotfix,
        [EnumDescription("Request For Change")]
        ChangeRequest,
        [EnumDescription("PCI", Annotation = "IsMaintenance")]
        Pci,
        [EnumDescription("System Maintenance", Annotation = "IsMaintenance")]
        SystemMaintenance,
        [EnumDescription("Automated Release")]
        Automated,
        [EnumDescription("Corp IT", Annotation = "IsMaintenance")]
        CorpIT,
        [EnumDescription("3rd party", Annotation = "IsMaintenance")]
        ThirdParty
    }
}
