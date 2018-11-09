using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriftVSIX
{
    public class ThriftGenerateNet45:ThriftGenerate
    {
        private readonly string _nugetPath;
        private readonly string _msbuildPath;

        public ThriftGenerateNet45(string filePath):base(filePath, NetVersion.Net45)
        {

            //_nugetPath = "nuget.exe";
            //_msbuildPath = "msbuild.exe";

            _nugetPath =GetEXEFilePath("nuget.exe");
            _msbuildPath = "\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Professional\\MSBuild\\15.0\\Bin\\MSBuild.exe\"";

        }

        public override void GenerateProject()
        {
            SetCSFileList();

            MoveToProject();

            var exprotProject = GetGenerateProject();
            exprotProject.Run();

            Util.CopyDllToThriftDir(_resourcesDir);

            //编译项目
            //MSBuild  /t:Rebuild /p:Configuration=Release /fl  /flp:FileLogger,Microsoft.Build.Engine;logfile=Build.log;errorsonly;Encoding=UTF-8
            string message = Util.CmdRunAndReturn($"{_msbuildPath}  /t:Rebuild /p:Configuration=Release /fl  /flp:FileLogger,Microsoft.Build.Engine;logfile=Build.log;errorsonly;Encoding=UTF-8", _info.ProjectDir);
            Console.WriteLine(message);

            //pack
            //nuget pack
            //nuget pack Fanews.UserManage.ThriftNet45.nuspec
            message = Util.CmdRunAndReturn($"{_nugetPath} pack {_info.ThriftNamespaceName}Net45.nuspec", _info.ProjectDir);
            Console.WriteLine(message);


            //发布 push
            //nuget push Fanews.UserManage.ThriftN45.1.2018.11.9141.nupkg -source http://10.252.148.40/nuget -apikey fanews@2018ngt!@#$
            var nugetpackName = $"{_info.ThriftNamespaceName}Net45.{exprotProject.Version}.nupkg";
            message = Util.CmdRunAndReturn($"{_nugetPath}nuget push {nugetpackName} -source http://10.252.148.40/nuget -apikey fanews@2018ngt!@#$", _info.ProjectDir);
            Console.WriteLine(message);

            OpenFolder(_info.ProjectDir);
        }

        public override void GenerateSource(bool isOpen = false)
        {
            string dosCommand = $"{_thriftPath} --gen csharp:async {_info.ThriftFile} ";
            string message = Util.CmdRunAndReturn(dosCommand, _info.WorkDir);
            Console.WriteLine(message);
        }
    }
}
