namespace ReMi.Queries.Plugins
{
    public class GetPluginsConfigurationResponse
    {
        public BusinessEntities.Plugins.Plugin Plugin { get; set; }

        public override string ToString()
        {
            return string.Format("[Plugin={0}]", Plugin);
        }
    }
}
