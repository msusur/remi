using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.Common.Utils.Repository;

namespace ReMi.Plugin.Common.PluginsConfiguration
{
    public static class UpdateCollectionHelper
    {
        public static void UpdateCollection<TData, TBusiness>(
            IEnumerable<TData> dataCollection,
            IEnumerable<TBusiness> businessCollection,
            IRepository<TData> repository,
            IMappingEngine mapper,
            Func<TData, TBusiness, bool> comaparer,
            Action<TData> assingId)
        {
            var toUpdate = dataCollection.
                Where(x => businessCollection.Any(f => comaparer(x, f))).ToList();
            var toRemove = dataCollection.
                Where(x => businessCollection.All(f => !comaparer(x, f))).ToList();
            var toInsert = businessCollection.
                Where(x => dataCollection.All(f => !comaparer(f, x))).ToList();

            toUpdate.Each(x =>
            {
                var item = businessCollection.First(f => comaparer(x, f));
                mapper.Map(item, x);
                repository.Update(x);
            });
            toRemove.Each(repository.Delete);
            toInsert.Each(x =>
            {
                var newItem = mapper.Map<TBusiness, TData>(x);
                assingId(newItem);
                repository.Insert(newItem);
            });
        }
    }
}
