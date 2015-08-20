using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.Metrics;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.Queries.ReleaseExecution;

namespace ReMi.QueryHandlers.Metrics
{
    public class GetDeploymentJobMeasurementsByProductHandler : IHandleQuery<GetDeploymentJobMeasurementsByProductRequest, GetDeploymentJobMeasurementsByProductResponse>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IReleaseDeploymentMeasurementGateway> ReleaseDeploymentJobMeasurementGatewayFactory { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public GetDeploymentJobMeasurementsByProductResponse Handle(GetDeploymentJobMeasurementsByProductRequest request)
        {
            List<ReleaseWindow> releaseWindows;
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                releaseWindows = gateway.GetAllByProduct(request.Product).ToList();
            }

            var result = new List<Measurement>();
            using (var gateway = ReleaseDeploymentJobMeasurementGatewayFactory())
            {
                var allMeasurements = new Dictionary<Guid, List<JobMeasurement>>();
                foreach (var releaseWindow in releaseWindows)
                {
                    allMeasurements[releaseWindow.ExternalId] = gateway.GetDeploymentMeasurements(releaseWindow.ExternalId).ToList();
                }
                var stepNames = allMeasurements.SelectMany(o => o.Value).Select(o => o.StepName).Distinct().ToList();

                foreach (var releaseWindow in releaseWindows)
                {
                    var actionTimes = new List<MeasurementTime>();
                    var releaseMeasurements = allMeasurements[releaseWindow.ExternalId];
                    foreach (var stepName in stepNames)
                    {
                        var stepMeasurement = releaseMeasurements.FirstOrDefault(o => o.StepName == stepName);
                        actionTimes.Add(stepMeasurement != null
                            ? MappingEngine.Map<JobMeasurement, MeasurementTime>(stepMeasurement)
                            : new MeasurementTime { Name = stepName, Value = 0 });
                    }

                    if (actionTimes.Any(o => o.Value != 0))
                        result.Add(new Measurement
                        {
                            ReleaseWindow = releaseWindow,
                            Metrics = actionTimes.ToList()
                        });
                }
            }

            return new GetDeploymentJobMeasurementsByProductResponse
            {
                Measurements = result
            };
        }

        private IEnumerable<MeasurementTime> ConvertStepsToActionTimes(IEnumerable<JobMeasurement> stepMeasurements)
        {
            return MappingEngine.Map<IEnumerable<JobMeasurement>, IEnumerable<MeasurementTime>>(stepMeasurements);
        }
    }
}
