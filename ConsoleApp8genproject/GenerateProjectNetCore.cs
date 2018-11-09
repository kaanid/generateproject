using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp8genproject
{
    public class GenerateProjectNetCore : GenerateProject
    {
        private readonly ThriftServiceInfo _info;
        private readonly string _tempPath;
        public GenerateProjectNetCore(ThriftServiceInfo info)
        {
            _info = info;
            _tempPath= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "netcore");
        }

        public override void Run()
        {
            //$guid$
            //$dllname$
            //$dllpath$
            //$compilelist$

            var path = Path.Combine(_tempPath, "temp.csproj");

            var text = File.ReadAllText(path);
            text = text

                .Replace("$dllname$", _info.ThriftNamespaceName)
                .Replace("$version$", Version)
                .Replace("$description$", $"{_info.ThriftNamespaceName} netcore thrift service");

            var newPath = _info.ProjectDir;
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            var path2 = Path.Combine(newPath, $"{_info.ThriftNamespaceName}.csproj");
            File.WriteAllText(path2, text);

            //$serviceclassname$
            //$dllname$

            var textClient = File.ReadAllText(_tempPath + "\\ThriftClienttemp.cs");
            textClient = textClient
                .Replace("$serviceclassname$", _info.ThriftServiceClassName)
                .Replace("$dllname$", _info.ThriftNamespaceName);

            var newClientPath = Path.Combine(newPath, $"ThriftClient.cs");
            File.WriteAllText(newClientPath, textClient);

            //$servicename$
            //$host$
            //$port$

            var textJson = File.ReadAllText(_tempPath + "\\configs\\temp.json");
            textJson = textJson
                .Replace("$servicename$", _info.ServiceName)
                .Replace("$host$", _info.Host)
                .Replace("$port$", _info.Port.ToString());

            Directory.CreateDirectory(Path.Combine(newPath, "configs"));
            var newJsonPath = Path.Combine(newPath, "configs", $"{_info.ThriftNamespaceName}Service.json");
            File.WriteAllText(newJsonPath, textJson);
        }
    }
}
