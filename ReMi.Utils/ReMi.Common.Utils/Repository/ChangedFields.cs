using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Common.Logging;

namespace ReMi.Common.Utils.Repository
{
    public class ChangedFields<TEntity> : List<ChangedField>
    {
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();

        public void Add(string name, object originalValue, object newValue)
        {
            Add(new ChangedField(name, originalValue, newValue));
        }

        public bool IsFieldChanged<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            return null != GetChange(property);
        }

        public ChangedField GetChange<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            var name = GetPropertyName(property);

            return this.FirstOrDefault(o => o.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public override string ToString()
        {
            return string.Format("[{0}]", string.Join(", ", this.Select(o => o.ToString()).ToArray()));
        }

        public string GetPropertyName<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            try
            {
                if(property.Body as MemberExpression == null)
                    throw new ArgumentException("The lambda expression 'body' should point to a valid MemberExpression");

                var propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;
                if (propertyInfo == null)
                    throw new ArgumentException("The lambda expression 'property' should point to a valid Property");

                return propertyInfo.Name;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return string.Empty;
        }
    }
}
