﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp8genproject
{
    public class ThriftGenerateNet45:ThriftGenerate
    {
        public ThriftGenerateNet45(string filePath):base(filePath, NetVersion.Net45)
        {

        }

        public override void GenerateProject()
        {
            SetCSFileList();

            MoveToProject();

            var exprotProject = GetGenerateProject();
            exprotProject.Run();

            //编译项目
            //MSBuild  /t:Rebuild /p:Configuration=Release /fl  /flp:FileLogger,Microsoft.Build.Engine;logfile=Build.log;errorsonly;Encoding=UTF-8
            string message = Util.CmdRunAndReturn("MSBuild  /t:Rebuild /p:Configuration=Release /fl  /flp:FileLogger,Microsoft.Build.Engine;logfile=Build.log;errorsonly;Encoding=UTF-8", _info.ProjectDir);
            Console.WriteLine(message);

            //pack
            //nuget pack
            //nuget pack Fanews.UserManage.ThriftNet45.nuspec
            message = Util.CmdRunAndReturn($"nuget.exe pack {_info.ThriftNamespaceName}Net45.nuspec", _info.ProjectDir);
            Console.WriteLine(message);


            //发布 push
            //nuget push Fanews.UserManage.ThriftN45.1.2018.11.9141.nupkg -source http://10.252.148.40/nuget -apikey fanews@2018ngt!@#$
            var nugetpackName = $"{_info.ThriftNamespaceName}Net45.{exprotProject.Version}.nupkg";
            message = Util.CmdRunAndReturn($"nuget push {nugetpackName} -source http://10.252.148.40/nuget -apikey fanews@2018ngt!@#$", _info.ThriftSourceFileDir);
            Console.WriteLine(message);

            OpenFolder(_info.ThriftSourceFileDir);
        }

        public override void GenerateSource(bool isOpen = false)
        {
            string dosCommand = $"thrift.exe --gen csharp:async {_info.ThriftFile} ";
            string message = Util.CmdRunAndReturn(dosCommand, _info.WorkDir);
            Console.WriteLine(message);
        }
    }
}