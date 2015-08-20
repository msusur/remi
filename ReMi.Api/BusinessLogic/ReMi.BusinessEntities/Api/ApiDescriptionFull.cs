using System;

namespace ReMi.BusinessEntities.Api
{
    public class ApiDescriptionFull : ApiDescription
    {
        public String Group { get; set; }
        public String DescriptionShort { get; set; }
        public string Roles { get; set; }

        public override string ToString()
        {
            return String.Format("[Url={0}, InputFormat={1}, OutputFormat={2}, Description={3}, Method={4}, Group={5}, DescriptionShort={6}, Roles={7}]",
                Url, InputFormat, OutputFormat, Description, Method, Group, DescriptionShort, Roles);
        }
    }
}
