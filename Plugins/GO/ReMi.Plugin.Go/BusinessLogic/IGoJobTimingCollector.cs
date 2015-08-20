using ReMi.Plugin.Go.Entities;

namespace ReMi.Plugin.Go.BusinessLogic
{
    public interface IGoJobTimingCollector
    {
        StepTiming GetPipelineTiming(string pipeline);
    }
}
