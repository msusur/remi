using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ReMi.Common.Utils.Repository;

namespace ReMi.DataAccess.Helpers
{
    public class DatabaseAdapter : IDatabaseAdapter
    {
        public IUnitOfWork UnitOfWork { get; set; }

        public List<List<string>> RunStoredProcedure(string query, int columns, IDictionary<string,object> parameters)
        {
            using (var conn = (SqlConnection)UnitOfWork.DbContext.Database.Connection)
            using (var command = new SqlCommand(query))
            {
                conn.Open();
                command.CommandType = CommandType.StoredProcedure;
                command.Connection = conn;
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }

                using (var reader = command.ExecuteReader())
                {
                    var result = new List<List<string>>();
                    while (reader.Read())
                    {
                        result.Add(new List<string>());
                        for (var i = 0; i < columns; i++)
                        {
                            if (reader.GetFieldType(i) == typeof (DateTime))
                            {
                                result[result.Count - 1].Add(reader.GetDateTime(i).ToUniversalTime().ToString("yyyy-MM-dd HH:mm"));
                            }
                            else
                            {
                                result[result.Count - 1].Add(reader.GetValue(i).ToString());
                            }
                        }
                    }
                    conn.Close();
                    return result;
                }
            }
        }

        

        public void Dispose()
        {
            UnitOfWork.Dispose();
        }
    }
}
