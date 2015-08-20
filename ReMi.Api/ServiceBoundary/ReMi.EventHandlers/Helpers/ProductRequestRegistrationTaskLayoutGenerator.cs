using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReMi.BusinessEntities.ProductRequests;

namespace ReMi.EventHandlers.Helpers
{
    public static class ProductRequestRegistrationTaskLayoutGenerator
    {
        public static string Get(ProductRequestRegistration registration, IEnumerable<ProductRequestTask> tasksIngroup, IEnumerable<Guid> changedTasks = null)
        {
            var changedTasksList = (changedTasks ?? Enumerable.Empty<Guid>()).ToList();

            const string headerTemplate = "<tr style='background-color: #ddd;'><th style='{4}'>{0}</th><th style='{4}'>{1}</th><th style='{4}'>{2}</th><th style='{4}'>{3}</th></tr>\n";

            const string rowTemplate = "<tr style='{4}'><td style='{5}'>{0}</td><td style='{5}'>{1}</td><td style='{5}'><small>{2}</small></td><td style='{5}'><small>{3}</small></td></tr>\n";

            const string commentRowTemplate = "<tr style='{1}'><td colspan='4' style='{2}'><small>Comment: {0}</small></td></tr>\n";

            var sb = new StringBuilder();
            sb.Append("<table style='width: 100%; border: 1px solid #ddd;' cellpadding='0' cellspacing='0'>");
            sb.AppendFormat(headerTemplate,
                "Question text",
                "Completed?",
                "Changed by",
                "Changed on",
                "padding: 3px;");

            int counter = 0;
            foreach (var task in tasksIngroup)
            {
                var registrationTask = registration.Tasks.FirstOrDefault(o => o.ProductRequestTaskId == task.ExternalId);
                if (registrationTask == null) continue;

                var style = "";
                if (changedTasksList.Contains(task.ExternalId))
                    style = "background-color: #fffacd;";
                else if (counter % 2 != 0)
                    style = "background-color: #eee;";

                sb.AppendFormat(rowTemplate,
                    task.Question,
                    registrationTask.IsCompleted ? "<span style='color:green'>Completed</span>" : string.Empty,
                    registrationTask.LastChangedBy,
                    registrationTask.LastChangedOn.HasValue
                        ? registrationTask.LastChangedOn.Value.ToLocalTime().ToLongDateString()
                        : string.Empty,
                    !string.IsNullOrWhiteSpace(style) ? style : "",
                    "border-collapse: collapse; padding: 5px;" +
                        (string.IsNullOrWhiteSpace(registrationTask.Comment) ? "border-bottom: 1px solid #ddd;" : ""));

                if (!string.IsNullOrWhiteSpace(registrationTask.Comment))
                {
                    sb.AppendFormat(commentRowTemplate,
                        registrationTask.Comment,
                        !string.IsNullOrWhiteSpace(style) ? style : "",
                        "padding: 10px 5px 5px 20px; border-bottom: 1px solid #ddd; border-collapse: collapse;");

                }

                counter++;
            }

            sb.Append("</table>");

            return sb.ToString();
        }
    }
}
