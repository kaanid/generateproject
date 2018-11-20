using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriftService
{
    public abstract class GenerateProject : IGenerateProject
    {
        public string Version {
            get;
            private set;
        }
        
        public GenerateProject()
        {
            Version = DateTime.Now.ToString("1.yyyy.M.dHH1"); ;
        }


        public abstract void Run();
    }
}
