using AutoMapper;
using ReMi.Commands.ReleaseCalendar;
using BusinessReleaseWindow = ReMi.BusinessEntities.ReleaseCalendar.ReleaseWindow;

namespace ReMi.CommandHandlers.AutoMapper
{
    public class BasicReleaseWindowRequestToReleaseWindowConverter :
        ITypeConverter<BasicReleaseWindowCommand, BusinessReleaseWindow>
    {
        public BusinessReleaseWindow Convert(ResolutionContext context)
        {
            return Convert(context.SourceValue as BasicReleaseWindowCommand);
        }

        public BusinessReleaseWindow Convert(BasicReleaseWindowCommand source)
        {
            return source.ReleaseWindow;
        }
    }
}
