using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriftService
{
    public class GenerateProjectNet45: GenerateProject
    {
        private readonly ThriftServiceInfo _info;
        private readonly string _tempPath;
        public GenerateProjectNet45(ThriftServiceInfo info,string tempDir)
        {
            _info = info;
            _tempPath= Path.Combine(tempDir, "net45");
        }

        public override void Run()
        {
            //$guid$
            //$dllname$
            //$dllpath$
            //$compilelist$

            var path = Path.Combine(_tempPath, "temp.csproj");

            StringBuilder sb = new StringBuilder();
            foreach (var s in _info.ThriftCSFileNameList)
            {
                sb.AppendFormat("    <Compile Include=\"{0}\" />\r\n", s);
            }

            var guid = Guid.NewGuid();

            var text = File.ReadAllText(path);
            text = text
                .Replace("$guid$", guid.ToString().ToUpper())
                .Replace("$dllname$", _info.ThriftNamespaceName)
                .Replace("$dllpath$", "..\\..\\dependency")
                .Replace("$compilelist$", sb.ToString().Trim());

            var newPath = _info.ProjectDir;
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            var path2 = Path.Combine(newPath, $"{_info.ThriftNamespaceName}Net45.csproj");
            File.WriteAllText(path2, text);


            //$id$
            //$version$
            //$title$
            //$authors$
            //$description$
            //$dllfilefullname$

            var textNuspec = File.ReadAllText(_tempPath + "\\temp.nuspec");
            textNuspec = textNuspec
                .Replace("$id$", _info.ThriftNamespaceName)
                .Replace("$version$", Version)
                .Replace("$title$", _info.ThriftNamespaceName)
                .Replace("$authors$", "fanews")
                .Replace("$description$", $"{_info.ThriftNamespaceName} Net45 thrift service")
                .Replace("$dllfilefullname$", _info.ThriftNamespaceName);

            var newNuspecPath = Path.Combine(newPath, $"{_info.ThriftNamespaceName}Net45.nuspec");
            File.WriteAllText(newNuspecPath, textNuspec);

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

            var textJson = File.ReadAllText(_tempPath + "\\Configs\\temp.json");
            textJson = textJson
                .Replace("$servicename$", _info.ServiceName)
                .Replace("$host$", _info.Host)
                .Replace("$port$", _info.Port.ToString());

            Directory.CreateDirectory(Path.Combine(newPath, "Configs"));
            var newJsonPath = Path.Combine(newPath, "Configs", $"{_info.ThriftNamespaceName}Service.json");
            File.WriteAllText(newJsonPath, textJson);

            //$dllname$
            //$guid$
            //$version$
            var testAssemblyInfo = File.ReadAllText(_tempPath + "\\Properties\\AssemblyInfo.cs");
            testAssemblyInfo = testAssemblyInfo
                .Replace("$dllname$",_info.ThriftNamespaceName)
                .Replace("$guid$", guid.ToString())
                .Replace("$version$", Version);

            Directory.CreateDirectory(Path.Combine(newPath, "Properties"));
            var newPropertiesPath = Path.Combine(newPath, "Properties", "AssemblyInfo.cs");
            File.WriteAllText(newPropertiesPath, testAssemblyInfo);
        }
    }
}
