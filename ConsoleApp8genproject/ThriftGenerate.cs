using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp8genproject
{
    public abstract class ThriftGenerate
    {
        protected readonly ThriftServiceInfo _info;

        public ThriftGenerate(string filePath,NetVersion netVersion)
        {
            _info=Init(filePath,netVersion);
        }

        public ThriftServiceInfo Init(string filePath, NetVersion netVersion)
        {
            var textThrift = File.ReadAllText(filePath);

            string dllName = GetRegexGroup(textThrift, "^namespace csharp ([A-z_.]+)");
            string thriftServiceClassName = GetRegexGroup(textThrift, "^service ([A-z_]+)");
            string serviceName = GetRegexGroup(textThrift, "^# servicename=([A-z_]+)");
            string host = GetRegexGroup(textThrift, "^# host=([0-9.]+)");
            string portStr = GetRegexGroup(textThrift, "^# port=(\\d+)");
            int.TryParse(portStr, out int port);

            var newPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "newNet45", DateTime.Now.Ticks.ToString());
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
                Port = port
            };
            return info;
        }

        public abstract void GenerateSource(bool isOpen=false);
        public abstract void GenerateProject();

        public void OpenFolder(string folderPath)
        {
            string dosCommand = $"explorer.exe {folderPath}";
            Util.CmdRun(dosCommand, _info.WorkDir);
        }

        private string GetRegexGroup(string text, string patten, int index = 1, RegexOptions regexOptions = RegexOptions.Multiline)
        {
            var m = Regex.Match(text, patten, regexOptions);
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
                return new GenerateProjectNet45(_info);
            return new GenerateProjectNetCore(_info);
        }

    }
}
