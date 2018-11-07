using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp8genproject
{
    class Program
    {
        static void Main(string[] args)
        {
            var activefile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fanews.TableSync.thrift");
            var textThrift = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fanews.TableSync.thrift"));
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

            //生成thrift文件
            //string genPath = $"gen-netcore\\{string.Join("\\", dllName.Split('.'))}";
            string genPath = $"gen-csharp\\{string.Join("\\", dllName.Split('.'))}";
            genPath = Path.Combine(newPath, genPath);
            //string dosCommand = $"thrift.exe --gen netcore {activefile} && explorer.exe gen-netcore\\{string.Join("\\", dllName.Split('.'))}";
            string dosCommand = $"thrift.exe --gen csharp:async {activefile} ";
            //csharp:async
            string message =Util.CmdRunAndReturn(dosCommand, newPath);
            Console.WriteLine(message);

            string[] thriftCsPathList = Directory.GetFiles(genPath, "*.cs", SearchOption.AllDirectories);
            string[] csFileList=thriftCsPathList.Select(m => Path.GetFileName(m)).ToArray();

            //生成项目
            string projectPath =Path.Combine(newPath,"Project");
            Directory.Move(genPath, projectPath);

            var exprotProject = new ExportNet45Project(projectPath, dllName, thriftServiceClassName, csFileList, port,host,serviceName);
            exprotProject.Run();

            //编译项目
            //MSBuild  /t:Rebuild /p:Configuration=Release /fl  /flp:FileLogger,Microsoft.Build.Engine;logfile=Build.log;errorsonly;Encoding=UTF-8
            message = Util.CmdRunAndReturn("MSBuild  /t:Rebuild /p:Configuration=Release /fl  /flp:FileLogger,Microsoft.Build.Engine;logfile=Build.log;errorsonly;Encoding=UTF-8", projectPath);
            Console.WriteLine(message);

            //nuget pack
            //nuget pack Fanews.UserManage.ThriftNet45.nuspec

            //Util.RunProgram("C:\\Windows\\System32\\nuget.exe", $" pack {dllName}Net45.nuspec", projectPath);
            //message = Util.CmdRunAndReturn($"nuget.exe pack {dllName}Net45.nuspec -Version 1.0.0 -properties Configuration=Release;package=1.0.0 -OutputDirectory C:\\", projectPath);
            message = Util.CmdRunAndReturn($"nuget.exe pack {dllName}Net45.nuspec", projectPath);
            Console.WriteLine(message);


            //发布 push
            //nuget push puckersa.sdfsf -source -apikey 

            Console.Read();

        }

        private static string GetRegexGroup(string text,string patten,int index=1, RegexOptions regexOptions= RegexOptions.Multiline)
        {
            var m = Regex.Match(text, patten, regexOptions);
            if(m.Success && m.Groups.Count > index)
            {
                return m.Groups[index].Value;
            }
            return null;
        }
    }

    public class ExportNet45Project
    {
        private readonly string _dllName;
        private readonly string _thriftServiceClassName;
        private readonly string[] _csList;
        private readonly string _port;
        private readonly string _host;
        private readonly string _serviceName;
        private readonly string _projectPath;
        public ExportNet45Project(string projectPath,string dllname,string thriftServiceClassName,string[] csList,int port,string host="127.0.0.1",string serviceName="")
        {
            _dllName = dllname;
            _thriftServiceClassName = thriftServiceClassName;
            _csList = csList;
            _port = port.ToString();
            _host = host;
            _serviceName = string.IsNullOrWhiteSpace(serviceName) ? dllname.Replace(".", "").Replace("Fanews", "") : serviceName;
            _projectPath = projectPath;
        }


        public void Run()
        {
            var tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "net45");

            //$guid$
            //$dllname$
            //$dllpath$
            //$compilelist$

            var path = Path.Combine(tempPath, "temp.csproj");
            
            StringBuilder sb = new StringBuilder();
            foreach (var s in _csList)
            {
                sb.AppendFormat("    <Compile Include=\"{0}\" />\r\n", s);
            }

            var text = File.ReadAllText(path);
            text = text
                .Replace("$guid$", Guid.NewGuid().ToString().ToUpper())
                .Replace("$dllname$", _dllName)
                .Replace("$dllpath$", "..\\..\\dependency")
                .Replace("$compilelist$", sb.ToString().Trim());

            //var newPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "newNet45", DateTime.Now.Ticks.ToString());
            var newPath = _projectPath;
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            var path2 = Path.Combine(newPath, $"{_dllName}Net45.csproj");
            File.WriteAllText(path2, text);


            //$id$
            //$version$
            //$title$
            //$authors$
            //$description$
            //$dllfilefullname$

            var textNuspec = File.ReadAllText(tempPath + "\\temp.nuspec");
            textNuspec = textNuspec
                .Replace("$id$", _dllName)
                .Replace("$version$", DateTime.Now.ToString("1.yyyy.MM.ddHH1"))
                .Replace("$title$", _dllName)
                .Replace("$authors$", "fanews")
                .Replace("$description$", $"{_dllName} Net45 thrift service")
                .Replace("$dllfilefullname$", _dllName);

            var newNuspecPath = Path.Combine(newPath, $"{_dllName}Net45.nuspec");
            File.WriteAllText(newNuspecPath, textNuspec);


            //$serviceclassname$
            //$dllname$

            var textClient = File.ReadAllText(tempPath + "\\ThriftClienttemp.cs");
            textClient = textClient
                .Replace("$serviceclassname$", _thriftServiceClassName)
                .Replace("$dllname$", _dllName);

            var newClientPath = Path.Combine(newPath, $"ThriftClient.cs");
            File.WriteAllText(newClientPath, textClient);

            //$servicename$
            //$host$
            //$port$

            var textJson = File.ReadAllText(tempPath + "\\configs\\temp.json");
            textJson = textJson
                .Replace("$servicename$", _serviceName)
                .Replace("$host$", _host)
                .Replace("$port$", _port);

            Directory.CreateDirectory(Path.Combine(newPath, "configs"));
            var newJsonPath = Path.Combine(newPath, "configs", $"{_dllName}Service.json");
            File.WriteAllText(newJsonPath, textJson);
        }


    }
}
