using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMi.Common.WebApi.Notifications
{
    public interface INotificationFilterByRole: INotificationFilter
    {
        Guid RoleId { get; set; }
    }
}
