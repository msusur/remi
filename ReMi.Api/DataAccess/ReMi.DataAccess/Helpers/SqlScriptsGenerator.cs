using System.Text;
using ReMi.Common.Constants;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils.Enums;
using ReMi.DataEntities.Reports;

namespace ReMi.DataAccess.Helpers
{
    public static class SqlScriptsGenerator
    {
        private const string CommandPermissionsQuery = @"IF NOT EXISTS (SELECT * FROM Api.Commands WHERE Name = '{0}')
                BEGIN
                    INSERT INTO Api.Commands ([Name], [Group], [Description], [IsBackground]) VALUES('{0}', '{1}', '{2}', {3})
                END
                IF NOT EXISTS (select * from Auth.CommandPermissions As Cp where Cp.CommandId =
                    (select CommandId from Api.Commands As C where C.Name = '{0}'))
                BEGIN
                    INSERT INTO Auth.CommandPermissions SELECT R.Id, C.CommandId FROM Api.Commands AS C CROSS JOIN Auth.Roles AS R WHERE C.Name = '{0}' AND R.{5} {6} IN ({4})                
                END";
        private const string QueryPermissionsQuery = @"IF NOT EXISTS (SELECT * FROM Api.Queries WHERE Name = '{0}')
                BEGIN
                    INSERT INTO Api.Queries ([Name], [Group], [Description]) VALUES('{0}', '{1}', '{2}')
                END
                IF NOT EXISTS (select * from Auth.QueryPermissions As Qp where Qp.QueryId =
                    (select QueryId from Api.Queries As Q where Q.Name = '{0}'))
                BEGIN
                    INSERT INTO Auth.QueryPermissions SELECT R.Id, Q.QueryId FROM Api.Queries AS Q CROSS JOIN Auth.Roles AS R WHERE Q.Name = '{0}' AND R.{5} IN ({4})                
                END";

        public static String AddCommandPermissions(IEnumerable<CqrsPermissions> permissions)
        {
            var commandDescriptions =
                CommandCollector.Collect()
                    .Join(permissions, c => c.Name, p => p.Name,
                        (c, p) =>
                            new
                            {
                                Command = c,
                                Permission = p.RoleIds.IsNullOrEmpty() ? null : p.RoleIds,
                                PermissionNames = p.RoleNames.IsNullOrEmpty() ? null : p.RoleNames
                            })
                    .ToList();

            var sql = "";
            commandDescriptions.ForEach(
                c => sql += " " +
                    String.Format(CommandPermissionsQuery,
                        c.Command.Name,
                        c.Command.Group,
                        c.Command.Description,
                        c.Command.IsBackground ? 1 : 0,
                        c.Permission == null
                            ? IsAllRoles(c.PermissionNames)
                                ? "'NotAuthenticated', 'BasicUser'"
                                : string.Format("'{0}'", String.Join("', '", c.PermissionNames))
                            : String.Join(", ", c.Permission),
                        c.Permission == null ? "Name" : "Id",
                        IsAllRoles(c.PermissionNames) ? "NOT" : "")
            );

            return sql;
        }

        private static bool IsAllRoles(IEnumerable<string> roleNames)
        {
            if (roleNames == null) return false;

            var roles = roleNames.ToList();

            return roles.Count() == 1 && roles.First() == "*";
        }

        public static String AddQueriePermissions(IEnumerable<CqrsPermissions> permissions)
        {
            var queryDescriptions =
                QueryCollector.Collect()
                    .Join(permissions, q => q.Name, p => p.Name,
                        (q, p) =>
                            new
                            {
                                Query = q,
                                Permission = p.RoleIds.IsNullOrEmpty() ? null : p.RoleIds,
                                PermissionNames = p.RoleNames.IsNullOrEmpty() ? null : p.RoleNames
                            })
                    .ToList();

            var sql = "";
            queryDescriptions.ForEach(
                q => sql += " " + String.Format(QueryPermissionsQuery, q.Query.Name, q.Query.Group, q.Query.Description, q.Query.IsStatic ? 1 : 0,
                    q.Permission == null ? string.Format("'{0}'", String.Join("', '", q.PermissionNames)) : String.Join(", ", q.Permission),
                    q.Permission == null ? "Name" : "Id",
                    q.Query.Namespace));
            return sql;
        }

        public static String DeleteQueriesWithPermissions(IEnumerable<string> names, Boolean force = false)
        {
            var sql = "";
            const String deleteQuery = "DELETE FROM Api.Queries WHERE Name=";

            if (force)
            {
                names.ToList().ForEach(
                    q => sql += " " + String.Format(@"{1}'{0}'" + Environment.NewLine, q, deleteQuery));

                return sql;
            }

            var queryDescriptions =
                QueryCollector.Collect().Where(o => names.Any(x => x.Equals(o.Name))).ToList();


            queryDescriptions.ForEach(
                q => sql += " " + String.Format(@"{1}'{0}'" + Environment.NewLine, q.Name, deleteQuery));

            return sql;
        }


        public static String DeleteCommandsWithPermissions(IEnumerable<string> names, Boolean force = false)
        {
            var sql = "";
            const String deleteCommand = "DELETE FROM Api.Commands WHERE Name=";

            if (force)
            {
                names.ToList().ForEach(
                    c => sql += " " + String.Format(@"{1}'{0}'" + Environment.NewLine, c, deleteCommand));

                return sql;
            }

            var commandDescriptions =
                CommandCollector.Collect().Where(o => names.Any(x => x.Equals(o.Name))).ToList();


            commandDescriptions.ForEach(
                cmd => sql += " " + String.Format(@"{1}'{0}'" + Environment.NewLine, cmd.Name, deleteCommand));

            return sql;
        }

        public static String DropDefaultConstraint(string table, string column)
        {
            return string.Format(@"
                DECLARE @name sysname

                SELECT @name = dc.name
                FROM sys.columns c
                JOIN sys.default_constraints dc ON dc.object_id = c.default_object_id
                WHERE c.object_id = OBJECT_ID('{0}')
                AND c.name = '{1}'

                IF @name IS NOT NULL
                    EXECUTE ('ALTER TABLE {0} DROP CONSTRAINT ' + @name)
                ", table, column);
        }

        public static String CreateDefaultConstraint(string table, string column, string defaultValue = "")
        {
            return string.Format(@"ALTER TABLE {0} ADD DEFAULT ('{2}') FOR {1}", table, column, defaultValue);
        }

        public static String FillEnum<T, TEnum>(string table)
            where T : EnumDescription, new()
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            var enumDescs = EnumDescriptionHelper.GetEnumDescriptions<TEnum, T>();
            const string script = @"IF NOT EXISTS (select * from {0} where Id = {1})
                BEGIN
                    INSERT INTO {0} (Id, Name, [Description]) Values ({1}, '{2}', '{3}')
                END";

            var builder = new StringBuilder();
            foreach (var enumDesc in enumDescs)
            {
                builder.AppendFormat(script, table, enumDesc.Id, enumDesc.Name, enumDesc.Description).AppendLine();
            }

            return builder.ToString();
        }

        public static String FillEnumOrdered<T, TEnum>(string table)
            where T : OrderedEnumDescription, new()
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            var enumDescs = EnumDescriptionHelper.GetOrderedEnumDescriptions<TEnum, T>();
            const string script = @"IF NOT EXISTS (select * from {0} where Id = {1})
                BEGIN
                    INSERT INTO {0} (Id, Name, [Description], [Order]) Values ({1}, '{2}', '{3}', {4})
                END";

            var builder = new StringBuilder();
            foreach (var enumDesc in enumDescs)
            {
                builder.AppendFormat(script, table, enumDesc.Id, enumDesc.Name, enumDesc.Description, enumDesc.Order).AppendLine();
            }

            return builder.ToString();
        }

        public static String CreateBusinessRule(BusinessRuleGroup group, RuleConstants ruleConstants,
            string script, string[] testData, string accountTestData = "")
        {
            if (ruleConstants.Parameters != null && ruleConstants.Parameters.Count != testData.Length)
                throw new Exception("Parameters and Test data count are not equal");

            var builder = new StringBuilder();

            builder.AppendFormat(
              @"IF NOT EXISTS (select * from BR.BusinessRuleGroup WHERE Id = {0})
                BEGIN
                    INSERT INTO BR.BusinessRuleGroup (Id, Name, [Description]) Values ({0}, '{1}', '{2}')
                END",
                (int)group, group, EnumDescriptionHelper.GetDescription(group));
            builder.AppendLine();

            builder.AppendFormat(
              @"IF NOT EXISTS (select * from BR.BusinessRule WHERE ExternalId = '{0}')
                BEGIN
                    INSERT INTO BR.BusinessRule (ExternalId, [Group], Name, [Description], Script)
                        VALUES ('{0}', {1}, '{2}', '{3}', '{4}')
                END
                ELSE
                BEGIN
                    UPDATE BR.BusinessRule SET Script = '{4}'
                        WHERE ExternalId = '{0}'
                END
                DECLARE @ruleId int, @parameterId int
                SET @ruleId = (SELECT P.BusinessRuleId FROM BR.BusinessRule P WHERE P.ExternalId = '{0}')",
                ruleConstants.ExternalId, (int)group, ruleConstants.Name, ruleConstants.Description, script);
            builder.AppendLine();

            if (!string.IsNullOrEmpty(accountTestData))
            {
                builder.AppendFormat(
                    @"IF NOT EXISTS(SELECT * FROM BR.BusinessRuleAccountTestData WHERE BusinessRuleAccountTestDataId = @ruleId)
                    BEGIN
                        INSERT INTO BR.BusinessRuleAccountTestData (BusinessRuleAccountTestDataId, ExternalId, JsonData)
                            VALUES (@ruleId, NEWID(), '{0}')
                    END
                    ELSE
                    BEGIN
                        UPDATE BR.BusinessRuleAccountTestData SET JsonData = '{0}'
                            WHERE BusinessRuleAccountTestDataId = @ruleId
                    END",
                    accountTestData);
                builder.AppendLine();
            }

            if (ruleConstants.Parameters != null)
            {
                foreach (var parmeter in ruleConstants.Parameters.Zip(testData, (x, y) => new { Parameter = x, TestData = y }))
                {
                    builder.AppendFormat(
                      @"IF NOT EXISTS (select * from BR.BusinessRuleParameter WHERE ExternalId = '{0}')
                    BEGIN
                        INSERT INTO BR.BusinessRuleParameter (ExternalId, BusinessRuleId, Name, Type)
                            VALUES ('{0}', @ruleId, '{1}', '{2}')
                    END
                    SET @parameterId = (SELECT P.BusinessRuleParameterId FROM BR.BusinessRuleParameter P WHERE P.ExternalId = '{0}')",
                        parmeter.Parameter.ExternalId, parmeter.Parameter.Name, parmeter.Parameter.Type);
                    builder.AppendLine();
                    builder.AppendFormat(
                      @"IF NOT EXISTS(SELECT * FROM BR.BusinessRuleTestData WHERE BusinessRuleTestDataId = @parameterId)
                    BEGIN
                        INSERT INTO BR.BusinessRuleTestData (BusinessRuleTestDataId, ExternalId, JsonData)
                            VALUES (@parameterId, NEWID(), '{0}')
                    END
                    ELSE
                    BEGIN
                        UPDATE BR.BusinessRuleTestData SET JsonData = '{0}'
                            WHERE BusinessRuleTestDataId = @parameterId
                    END",
                        parmeter.TestData);
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }

        public static String DeleteBusinessRule(BusinessRuleGroup group, Guid ruleId)
        {
            return string.Format(
              @"IF ((SELECT COUNT(*) FROM BR.BusinessRule WHERE [Group] = {0}) = 1)
                BEGIN
                    DELETE FROM BR.BusinessRuleGroup WHERE Id = {0}
                END
                ELSE
                BEGIN
                    DELETE FROM BR.BusinessRule WHERE ExternalId = '{1}'
                END",
                (int)group, ruleId);
        }

        public static IEnumerable<String> CreateProcedure(ReportDescription description,
            String procedureBody)
        {
            var originalColumnOrders = description.ReportColumns.GroupBy(x => x.Order).Where(g => g.Count() == 1).ToList();
            if (originalColumnOrders.Count < description.ReportColumns.Count)
            {
                throw new ApplicationException("Duplicated order of report column");
            }

            if (description.ReportColumns.Any(c => String.IsNullOrEmpty(c.Name)))
            {
                throw new ApplicationException("Empty column name is not allowed");
            }

            if (String.IsNullOrEmpty(description.Name) || String.IsNullOrEmpty(description.ProcedureName))
            {
                throw new ApplicationException("Incorrect description: empty name or creator procedure");
            }

            var builder =
                new StringBuilder(String.Format("create proc Report.{0} {1}", description.ProcedureName,
                    String.Join(",",
                        description.ReportParameters.Select(
                            s =>
                                String.Format("@{0} {1}", s.Name,
                                    new[] { "report.packages", "product" }.Contains(s.Type.ToLower()) ? String.Format("{0} readonly", s.Type) : s.Type)))));
            builder.AppendLine(@"
                as
            ");
            builder.AppendLine(procedureBody);
            yield return builder.ToString();
            builder.Clear();

            builder.AppendLine(String.Format(@"
                declare @schema varchar(64)
                set @schema = (select TABLE_SCHEMA from INFORMATION_SCHEMA.TABLES
                    where TABLE_NAME = 'ReportDescriptions')
                declare @reportId int
                if(@schema = 'Report')
                    begin
                        insert into Report.ReportDescriptions ([Name], [ProcedureName])
                        values ('{0}', '{1}')
                    end
                else
                    begin
                        insert into ReMi.ReportDescriptions ([Name], [ProcedureName])
                        values ('{0}', '{1}')
                    end

                set @reportId = @@identity               
                
            ", description.Name, description.ProcedureName));

            foreach (var column in description.ReportColumns)
            {
                builder.AppendLine(String.Format(@"
                    if(@schema = 'Report')
                        begin
                            insert into Report.ReportColumns ([Name], [ReportDescriptionId], [Order])
                            values ('{0}', @reportId, {1})
                        end
                    else
                        begin
                            insert into ReMi.ReportColumns ([Name], [ReportDescriptionId], [Order])
                            values ('{0}', @reportId, {1})
                        end
                    
                ", column.Name, column.Order));
            }

            foreach (var parameter in description.ReportParameters)
            {
                builder.AppendLine(String.Format(@"
                    if(@schema = 'Report')
                        begin
                            insert into Report.ReportParameters ([Name], [ReportDescriptionId], [Description], [Type])
                            values ('{0}', @reportId, '{1}', '{2}')
                        end
                    else
                        begin
                            insert into ReMi.ReportParameters ([Name], [ReportDescriptionId], [Description], [Type])
                            values ('{0}', @reportId, '{1}', '{2}')
                        end

                ", parameter.Name, parameter.Description, parameter.Type));
            }

            yield return builder.ToString();
        }

        public static IEnumerable<String> RemoveProcedure(String proc)
        {
            if (String.IsNullOrEmpty(proc))
            {
                throw new ApplicationException("Incorrect description: empty creator procedure");
            }

            yield return String.Format("IF (OBJECT_ID(N'Report.{0}') IS NOT NULL) drop proc Report.{0} ", proc);

            yield return String.Format(@"
                declare @schema varchar(64)
                set @schema = (select TABLE_SCHEMA from INFORMATION_SCHEMA.TABLES
                    where TABLE_NAME = 'ReportDescriptions')
                if(@schema = 'Report')
                    begin
                       delete from Report.ReportDescriptions where ProcedureName = '{0}'
                    end
                else
                    begin
                       delete from ReMi.ReportDescriptions where ProcedureName = '{0}'         
                    end
                ", proc);
        }
    }
}
