﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriftService
{
    public class ThriftGenerateNet45:ThriftGenerate
    {
        public ThriftGenerateNet45(string filePath):base(filePath, NetVersion.Net45)
        {

        }

        public override void GenerateProject()
        {
            //C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe
            string _nugetPath = GetEXEFilePath("nuget.exe");

            string vsPath = GetMSBuildPath();

            string _msbuildPath = $"\"{vsPath}\"";

            SetCSFileList();

            MoveToProject();

            var exprotProject = GetGenerateProject();
            exprotProject.Run();

            Util.CopyDllToThriftDir(_resourcesDir);

            //编译项目
            //MSBuild  /t:Rebuild /p:Configuration=Release /fl  /flp:FileLogger,Microsoft.Build.Engine;logfile=Build.log;errorsonly;Encoding=UTF-8
            string message = Util.CmdRunAndReturn($"{_msbuildPath}  /t:Rebuild /p:Configuration=Release /fl  /flp:FileLogger,Microsoft.Build.Engine;logfile=Build.log;errorsonly;Encoding=UTF-8", _info.ProjectDir);
            //Console.WriteLine(message);
            Util.CheckCmdMessageThrewException(message);

            //pack
            //nuget pack
            //nuget pack Fanews.UserManage.ThriftNet45.nuspec
            message = Util.CmdRunAndReturn($"{_nugetPath} pack {_info.ThriftNamespaceName}Net45.nuspec", _info.ProjectDir);
            //Console.WriteLine(message);
            Util.CheckCmdMessageThrewException(message);


            if (_info.NugetPush)
            {
                //发布 push
                //nuget push Fanews.UserManage.ThriftN45.1.2018.11.9141.nupkg -source http://10.252.148.40/nuget -apikey fanews@2018ngt!@#$
                var nugetpackName = $"{_info.ThriftNamespaceName}Net45.{exprotProject.Version}.nupkg";
                message = Util.CmdRunAndReturn($"{_nugetPath} push {nugetpackName} -source http://nuget.hzfanews.fw/nuget -apikey {NugetApiKey}", _info.ProjectDir);
                //Console.WriteLine(message);
                Util.CheckCmdMessageThrewException(message);
            }

            OpenFolder(_info.ProjectDir);
        }

        public override void GenerateSource(bool isOpen = false)
        {
            string dosCommand = $"{_thriftPath} --gen csharp:async {_info.ThriftFile} ";
            string message = Util.CmdRunAndReturn(dosCommand, _info.WorkDir);
            //Console.WriteLine(message);
            Util.CheckCmdMessageThrewException(message);

            if (isOpen)
            {
                OpenSoureFolder();
            }
        }

        private IEnumerable<string> GetMSBUildFiles()
        {
            string path1 = "C:\\Program Files\\Microsoft Visual Studio\\";
            foreach (var s in System.IO.Directory.GetFiles(path1, "MSBuild.exe", SearchOption.AllDirectories))
                yield return s;

            string path2 = "C:\\Program Files (x86)\\Microsoft Visual Studio\\";
            foreach (var s in System.IO.Directory.GetFiles(path2, "MSBuild.exe", SearchOption.AllDirectories))
                yield return s;
        }

        private string GetMSBuildPath()
        {
            if (!GetMSBUildFiles().Any())
                throw new NotSupportedException("MSBuild.exe not found");

            string[] versions = { "2022","2019", "2017" };
            string file1 = string.Empty;
            foreach (var file in GetMSBUildFiles())
            {
                if(file1==string.Empty)
                    file1 = file;

                foreach (var version in versions)
                {
                    if (file.Contains(version) && file.EndsWith("Bin\\MSBuild.exe"))
                    {
                        return file;
                    }
                }
            }
            return file1;
        }
    }
}
