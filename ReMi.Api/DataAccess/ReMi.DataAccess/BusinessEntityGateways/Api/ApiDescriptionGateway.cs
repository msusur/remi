using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Api;
using ReMi.Common.Utils.Repository;
using ReMi.DataEntities.Api;

namespace ReMi.DataAccess.BusinessEntityGateways.Api
{
    public class ApiDescriptionGateway : BaseGateway, IApiDescriptionGateway
    {
        public IRepository<Description> DescriptionRepository { get; set; }

        public override void OnDisposing()
        {
            DescriptionRepository.Dispose();
            base.OnDisposing();
        }
        
        public List<ApiDescription> GetApiDescriptions()
        {
            var descriptions = DescriptionRepository.Entities.Select(x => new ApiDescription
            {
                Description = x.DescriptionText,
                Url = x.Url,
                Method = x.HttpMethod
            }).ToList();

            return descriptions;
        }

        public void RemoveApiDescriptions(List<ApiDescription> descriptions)
        {
            var entities =
                DescriptionRepository.Entities.ToList()
                    .Where(x => descriptions.Any(d => d.Method == x.HttpMethod && d.Url == x.Url));

            foreach (var desc in entities)
            {
                DescriptionRepository.Delete(desc);
            }
        }

        public void CreateApiDescriptions(List<ApiDescription> descriptions)
        {
            var entities = descriptions.Select(x => new Description
            {
                HttpMethod = x.Method,
                Url = x.Url
            });

            DescriptionRepository.Insert(entities);
        }

        public void UpdateApiDescription(ApiDescription description)
        {
            var entity =
                DescriptionRepository.GetSatisfiedBy(
                    x => x.HttpMethod == description.Method && x.Url == description.Url);

            entity.DescriptionText = description.Description;

            DescriptionRepository.Update(entity);
        }
    }
}
