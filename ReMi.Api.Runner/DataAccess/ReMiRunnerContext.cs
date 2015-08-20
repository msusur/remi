using System.Data.Entity;

namespace ReMi.Api.Runner.DataAccess
{
    public class ReMiRunnerContext : DbContext
    {
        public ReMiRunnerContext()
            : base((string) Constants.ConnectionString)
        { }
        public ReMiRunnerContext(string connectionString)
            : base(connectionString)
        { }
    }
}
