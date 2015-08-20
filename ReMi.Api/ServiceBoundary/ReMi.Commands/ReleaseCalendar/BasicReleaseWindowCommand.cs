using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseCalendar
{
    public abstract class BasicReleaseWindowCommand : ICommand
    {
        public ReleaseWindow ReleaseWindow { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindow = {0}]", ReleaseWindow);
        }

        public CommandContext CommandContext { get; set; }
    }
}
