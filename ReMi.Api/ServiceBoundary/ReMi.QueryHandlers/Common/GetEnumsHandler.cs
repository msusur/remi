using System.Collections.Generic;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Constants.ProductRequests;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Data;
using ReMi.DataEntities.Plugins;
using ReMi.DataEntities.ReleasePlan;
using ReMi.Queries.Common;

namespace ReMi.QueryHandlers.Common
{
    public class GetEnumsHandler : IHandleQuery<GetEnumsRequest, GetEnumsResponse>
    {
        public GetEnumsResponse Handle(GetEnumsRequest request)
        {
            return new GetEnumsResponse
            {
                Enums = new Dictionary<string, IEnumerable<EnumDescription>>
                {
                    { "RemovingReason", EnumDescriptionHelper.GetEnumDescriptions<RemovingReason, RemovingReasonDescription>() },
                    { "ReleaseType", EnumDescriptionHelper.GetEnumDescriptions<ReleaseType, ReleaseTypeDescription>() },
                    { "ReleaseTrack", EnumDescriptionHelper.GetEnumDescriptions<ReleaseTrack, ReleaseTrackDescription>() },
                    { "ReleaseTaskEnvironment", EnumDescriptionHelper.GetEnumDescriptions<ReleaseTaskEnvironment, ReleaseTaskEnvironmentDescription>() },
                    { "ReleaseTaskRisk", EnumDescriptionHelper.GetEnumDescriptions<ReleaseTaskRisk, ReleaseTaskRiskDescription>() },
                    { "ReleaseTaskType", EnumDescriptionHelper.GetEnumDescriptions<ReleaseTaskType, ReleaseTaskTypeDescription>() },
                    { "PluginType", EnumDescriptionHelper.GetEnumDescriptions<PluginType, PluginTypeDescription>() },
                    { "JobStage", EnumDescriptionHelper.GetEnumDescriptions<JobStage, CommonEnumDescription>() }
                }
            };
        }
    }
}
