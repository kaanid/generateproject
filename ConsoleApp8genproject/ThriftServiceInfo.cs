using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp8genproject
{
    public class ThriftServiceInfo
    {
        public string WorkDir { set; get; }
        public string ThriftFile { set; get; }
        public string ThriftNamespaceName { set; get; }
        public string ThriftServiceClassName { set; get; }
        public string ThriftSourceFileDir {
            get {
                if (NetVersion == NetVersion.Net45)
                {
                    return $"{WorkDir}\\gen-csharp\\{string.Join("\\", ThriftNamespaceName.Split('.'))}";
                }
                else
                {
                    return $"{WorkDir}\\gen-netcore\\{string.Join("\\", ThriftNamespaceName.Split('.'))}";
                }
            }
        }

        private string serviceName;
        public string ServiceName {
            set {
                serviceName = value;
            }
            get {
                return serviceName ?? ThriftNamespaceName.Replace(".", "").Replace("Fanews", "");
            }
        }
        public string Host { set; get; } = "127.0.0.1";
        public int Port { set; get; } = 9090;

        public NetVersion NetVersion { set; get; }

        public string ProjectDir
        {
            get {
                return $"{WorkDir}\\Project\\";
            }
        }

        public string[] ThriftCSFileNameList { set; get; }
    }

    public enum NetVersion
    {
        Net45=0,
        Netcore=1,
    }
}
