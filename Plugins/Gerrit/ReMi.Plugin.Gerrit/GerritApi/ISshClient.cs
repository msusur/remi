using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMi.Plugin.Gerrit.GerritApi
{
    public interface ISshClient : IDisposable
    {
        void Connect();
        string ExecuteCommand(string commandText);
    }
}
