using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriftVSIX
{
    public interface IGenerateProject
    {
        string Version { get; }
        void Run();
    }
}
