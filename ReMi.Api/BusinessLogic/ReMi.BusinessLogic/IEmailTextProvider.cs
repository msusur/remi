using System.Collections.Generic;

namespace ReMi.BusinessLogic
{
    public interface IEmailTextProvider
    {
        IDictionary<string, string> EmailTemplates { get; }
        string GetText(string key, IEnumerable<KeyValuePair<string, object>> placeholders);
    }
}
