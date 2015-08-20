using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using ReMi.Contracts.Plugins.Data.Authentication;
using ReMi.Contracts.Plugins.Services.Authentication;
using ReMi.Plugin.Ldap.DataAccess.Gateways;

namespace ReMi.Plugin.Ldap
{
    public class LdapRequest : IAuthenticationService
    {
        public Func<IGlobalConfigurationGateway> ConfigurationGatewayFactory { get; set; } 

        public Account GetAccount(string userName, string password)
        {
            return ReadAccount(userName, password);
        }

        public List<Account> Search(object criteria)
        {
            return criteria is string ? SearchByCriteria((string)criteria) : null;
        }

        private List<Account> SearchByCriteria(string criteria)
        {
            string ldapPath, userName, password, searchCriteriaFilter;
            using (var gateway = ConfigurationGatewayFactory())
            {
                var configuration = gateway.GetGlobalConfiguration();
                ldapPath = configuration.LdapPath;
                userName = configuration.UserName;
                password = configuration.Password;
                searchCriteriaFilter = configuration.SearchCriteria;
            }

            using (
                var entry = new DirectoryEntry(ldapPath, userName, password))
            {
                var searcher = new DirectorySearcher(entry)
                {
                    Filter =
                        string.Format(searchCriteriaFilter, criteria),
                };

                var accounts = new List<Account>();

                try
                {
                    var results = searcher.FindAll();
                    foreach (SearchResult result in results)
                    {
                        if (result != null && result.Properties != null && result.Properties.PropertyNames != null)
                        {
                            var account = ConvertToAccount(result);

                            Console.WriteLine("Account properties:");
                            foreach (string propertyName in result.Properties.PropertyNames)
                            {
                                Console.WriteLine("{0}={1}", propertyName, result.Properties[propertyName][0]);
                            }

                            accounts.Add(account);
                        }
                    }
                }
                catch (COMException ex)
                {
                    Console.WriteLine("Error occured {0}", ex.Message);

                    if (ex.ErrorCode == -2147023570)
                    {
                        Console.WriteLine("Login or password is incorrect");
                    }
                }

                return accounts;
            }
        }

        private Account ReadAccount(string user, string password)
        {
            string ldapPath;
            using (var gateway = ConfigurationGatewayFactory())
            {
                var configuration = gateway.GetGlobalConfiguration();
                ldapPath = configuration.LdapPath;
            }

            using (var entry = new DirectoryEntry(ldapPath, user, password))
            {
                var userName = GetNameFromEmail(user);
                var searcher = new DirectorySearcher(entry)
                {
                    Filter = "(&(objectClass=Person)(|(cn=" + userName + ")(sAMAccountName=" + userName + ")))"
                };

                #region Decrease fields for loading
                //searcher.PropertiesToLoad.Add("name");
                //searcher.PropertiesToLoad.Add("displayname");
                //searcher.PropertiesToLoad.Add("lastlogontimestamp");
                //searcher.PropertiesToLoad.Add("mail");
                //searcher.PropertiesToLoad.Add("userprincipalname");
                //searcher.PropertiesToLoad.Add("pwdlastset");
                //searcher.PropertiesToLoad.Add("objectguid");
                //searcher.PropertiesToLoad.Add("whenchanged");
                //searcher.PropertiesToLoad.Add("memberOf");
                #endregion

                try
                {
                    var result = searcher.FindOne();

                    if (result != null && result.Properties != null && result.Properties.PropertyNames != null)
                    {
                        var account = ConvertToAccount(result);

                        Console.WriteLine("Account properties:");
                        foreach (string propertyName in result.Properties.PropertyNames)
                        {
                            Console.WriteLine("{0}={1}", propertyName, result.Properties[propertyName][0]);
                        }

                        return account;
                    }
                }
                catch (COMException ex)
                {
                    Console.WriteLine("Error occured {0}", ex.Message);

                    if (ex.ErrorCode == -2147023570)
                    {
                        Console.WriteLine("Login or password is incorrect");
                    }
                }
            }

            return null;
        }

        private T GetProperty<T>(string propertyName, ResultPropertyCollection properties, IPropertyTypeConverter<T> converter = null)
        {
            var values = properties[propertyName];
            if (values.Count == 0)
                return default(T);

            if (converter != null)
                return converter.Convert(values[0]);

            return (T)values[0];
        }

        private string[] ReadGroups(SearchResult result)
        {
            int propertyCount = result.Properties["memberOf"].Count;
            var groupNames = new string[propertyCount];

            for (int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
            {
                var dn = (string)result.Properties["memberOf"][propertyCounter];

                int equalsIndex = dn.IndexOf("=", 1, StringComparison.InvariantCultureIgnoreCase);
                if (-1 == equalsIndex)
                    return null;

                int commaIndex = dn.IndexOf(",", 1, StringComparison.InvariantCultureIgnoreCase);

                groupNames[propertyCounter] = dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1);
            }

            return groupNames;
        }

        private string GetNameFromEmail(string mail)
        {
            if (string.IsNullOrWhiteSpace(mail))
                return mail;

            var parts = mail.Split('@');
            if (parts.Length > 0)
                return parts[0];

            return string.Empty;
        }

        private Account ConvertToAccount(SearchResult searchResult)
        {
            return new Account
            {
                Name = GetProperty<string>("name", searchResult.Properties),
                DisplayName = GetProperty<string>("displayname", searchResult.Properties),
                LastLogonTime = GetProperty<long>("lastlogontimestamp", searchResult.Properties),
                Mail = GetProperty<string>("userprincipalname", searchResult.Properties),
                PasswordLastSet = GetProperty<long>("pwdlastset", searchResult.Properties),
                AccountId = GetProperty("objectguid", searchResult.Properties, new ByteToGuidConverter()),
                WhenChanged = GetProperty<DateTime>("whenchanged", searchResult.Properties),

                Groups = ReadGroups(searchResult)
            };
        }
    }
}
