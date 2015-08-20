using System.Text;

namespace ReMi.Plugin.Common.Serialization
{
    public class JsonFormatter
    {
        private const string IndentString = "  ";
        public static string Format(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
                return jsonText;

            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < jsonText.Length; i++)
            {
                var ch = jsonText[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            indent++;
                            for (int idx = 0; idx < indent; idx++)
                                sb.Append(IndentString);
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            indent--;
                            for (int idx = 0; idx < indent; idx++)
                                sb.Append(IndentString);
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && jsonText[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            for (int idx = 0; idx < indent; idx++)
                                sb.Append(IndentString);
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
