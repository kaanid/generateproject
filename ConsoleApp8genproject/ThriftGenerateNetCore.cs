using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp8genproject
{
    public class ThriftGenerateNetCore : ThriftGenerate
    {
        public ThriftGenerateNetCore(string filePath):base(filePath, NetVersion.Netcore)
        {

        }

        public override void GenerateProject()
        {
            SetCSFileList();

            MoveToProject();

            var exprotProject = GetGenerateProject();
            exprotProject.Run();

            //还原nuget
            //dotnet restore -s http://10.252.148.40/nuget -s https://api.nuget.org/v3/index.json
            string message = Util.CmdRunAndReturn("dotnet restore -s http://10.252.148.40/nuget -s https://api.nuget.org/v3/index.json", _info.ProjectDir);
            Console.WriteLine(message);

            //编译项目
            // dotnet build -c Release
            message = Util.CmdRunAndReturn("dotnet build -c Release", _info.ProjectDir);
            Console.WriteLine(message);

            //pack
            //dotnet pack -c Release
            message = Util.CmdRunAndReturn("dotnet pack -c Release", _info.ProjectDir);
            Console.WriteLine(message);

            //发布 push
            //dotnet nuget push Fanews.UserManage.Thrift.1.2018.11.9141.nupkg -s http://10.252.148.40/nuget -k fanews@2018ngt!@#$
            var nugetpackDir = Path.Combine(_info.ProjectDir, "bin\\Release");
            var nugetpackName = $"{_info.ThriftNamespaceName}.{exprotProject.Version}.nupkg";
            message = Util.CmdRunAndReturn($"dotnet nuget push {nugetpackName} -s http://10.252.148.40/nuget -k fanews@2018ngt!@#$", nugetpackDir);
            Console.WriteLine(message);

            //打开文件夹
            OpenFolder(nugetpackDir);
        }

        public override void GenerateSource(bool isOpen = false)
        {
            string dosCommand = $"thrift.exe --gen netcore {_info.ThriftFile} ";
            string message = Util.CmdRunAndReturn(dosCommand, _info.WorkDir);
            Console.WriteLine(message);
        }
    }
}
