using System;

namespace ReMi.BusinessEntities.Api
{
    public class ApiDescription
    {
        public string Name { get; set; }
        public String Url { get; set; }
        public String InputFormat { get; set; }
        public String OutputFormat { get; set; }
        public String Description { get; set; }
        public String Method { get; set; }

        public override string ToString()
        {
            return String.Format("[Name={0}, Url={1}, InputFormat={2}, OutputFormat={3}, Description={4}, Method={5}]", 
                Name, Url, InputFormat, OutputFormat, Description, Method);
        }
    }
}
