using ReMi.BusinessEntities.BusinessRules;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.BusinessRules
{
    [Command("Save rule", CommandGroup.BusinessRules)]
    public class SaveRuleCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public BusinessRuleDescription Rule { get; set; }

        public override string ToString()
        {
            return string.Format("[CommandContext={0}, Rule={1}]",
                CommandContext, Rule);
        }
    }
}
