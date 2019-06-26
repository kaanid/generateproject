using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ThriftService
{
    public abstract class ThriftGenerate
    {
        protected readonly ThriftServiceInfo _info;
        protected readonly string _thriftPath;
        protected readonly string _resourcesDir;
        protected const string NugetApiKey= "fanews@2018nuget";
        private const string PatternNamespace = "^namespace csharp ([A-z0-9_.]+)";

        public ThriftGenerate(string filePath,NetVersion netVersion)
        {
            _resourcesDir = Util.GetExpansionToolResourcesPath();

            //_thriftPath = "thrift.exe";
            _thriftPath = GetEXEFilePath("thrift.exe");
            
            _info =Init(filePath,netVersion);

        }

        public ThriftServiceInfo Init(string filePath, NetVersion netVersion)
        {
            var textThrift = File.ReadAllText(filePath);

            string dllName = string.Empty;
            if (NetVersion.Net45 == netVersion)
            {
                dllName = GetRegexGroup(textThrift, PatternNamespace);
            }else
            {
                dllName = GetRegexGroup(textThrift, PatternNamespace);
            }

            if(string.IsNullOrWhiteSpace(dllName))
            {
                throw new ArgumentNullException($"namespace {NetVersion.Net45} is null");
            }

            string thriftServiceClassName = GetRegexGroup(textThrift, "^service ([A-z_]+)");
            string serviceName = GetRegexGroup(textThrift, "^# servicename=([A-z_]+)");
            string host = GetRegexGroup(textThrift, "^# host=([0-9.]+)");
            string portStr = GetRegexGroup(textThrift, "^# port=(\\d+)");
            int.TryParse(portStr, out int port);
            bool isPush = false;
            var strPush = GetRegexGroup(textThrift, "^# nugetpush=(TRUE|true)");
            if (strPush!=null)
                isPush = true;
            

            string newPath = $"{Path.GetTempPath()}thrift\\{DateTime.Now.Ticks}";
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            var info = new ThriftServiceInfo()
            {
                WorkDir= newPath,
                ThriftFile = filePath,
                ThriftNamespaceName = dllName,
                ThriftServiceClassName = thriftServiceClassName,
                NetVersion = netVersion,
                ServiceName = serviceName,
                Host = host,
                Port = port,
                NugetPush=isPush
            };
            return info;
        }

        public abstract void GenerateSource(bool isOpen = false);
        public abstract void GenerateProject();

        protected void OpenSoureFolder()
        {
            OpenFolder(_info.ThriftSourceFileDir);
        }

        public void OpenFolder(string folderPath)
        {
            string dosCommand = $"explorer.exe {folderPath}";
            Util.CmdRun(dosCommand, _info.WorkDir);
        }

        private string GetRegexGroup(string text, string pattern, int index = 1, RegexOptions regexOptions = RegexOptions.Multiline)
        {
            var m = Regex.Match(text, pattern, regexOptions);
            if (m.Success && m.Groups.Count > index)
            {
                return m.Groups[index].Value;
            }
            return null;
        }

        protected void SetCSFileList()
        {
            if (!Directory.Exists(_info.ThriftSourceFileDir))
            {
                GenerateSource();
            }

            string[] thriftCsPathList = Directory.GetFiles(_info.ThriftSourceFileDir, "*.cs", SearchOption.AllDirectories);
            string[] csFileList = thriftCsPathList.Select(m => Path.GetFileName(m)).ToArray();
            _info.ThriftCSFileNameList = csFileList;
        }

        protected void MoveToProject()
        {
            Directory.Move(_info.ThriftSourceFileDir, _info.ProjectDir);
        }

        protected IGenerateProject GetGenerateProject()
        {
            if (_info.NetVersion == NetVersion.Net45)
                return new GenerateProjectNet45(_info, _resourcesDir);
            return new GenerateProjectNetCore(_info, _resourcesDir);
        }

        protected string GetEXEFilePath(string fileName)
        {
            string[] thriftPaths = Directory.GetFiles(_resourcesDir, fileName, SearchOption.AllDirectories);
            if (thriftPaths == null || thriftPaths.Length == 0)
            {
                //thriftPaths = new string[] { "F:\\Tools\\utils\\thrift.exe" };
                throw new ArgumentException($"{fileName} 未找到");
            }
            return thriftPaths[0];
        }
    }
}
