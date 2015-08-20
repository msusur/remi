using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.Common.Constants;
using ReMi.Common.Constants.ProductRequests;
using ReMi.Common.Utils.Enums;
using ReMi.DataEntities.ProductRequests;

namespace ReMi.DataAccess.AutoMapper
{
    public class TasksToProductRequestRegistrationStatusResolver : ValueResolver<IEnumerable<ProductRequestRegistrationTask>, string>
    {
        protected override string ResolveCore(IEnumerable<ProductRequestRegistrationTask> source)
        {
            if (source == null) return EnumDescriptionHelper.GetDescription(ProductRequestRegistrationStatus.New);

            var list = source.ToList();

            if (list.All(o => !o.IsCompleted)) return EnumDescriptionHelper.GetDescription(ProductRequestRegistrationStatus.New);

            if (list.All(o => o.IsCompleted)) return EnumDescriptionHelper.GetDescription(ProductRequestRegistrationStatus.Completed);

            return EnumDescriptionHelper.GetDescription(ProductRequestRegistrationStatus.InProgress);
        }
    }
}
