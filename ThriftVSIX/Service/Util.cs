using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ThriftService
{
    public class Util
    {
        public static string ExtensionToolVersion {
            get {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        public static void RunProgram(string programName, string cmd, string currentPath = "")
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = programName;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.WorkingDirectory = currentPath;
            proc.Start();
            if (cmd.Length != 0)
            {
                proc.StandardInput.WriteLine(cmd);
            }
            proc.Close();
        }

        public static void CmdRun(string dosCommand,string currentPath="")
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C " + dosCommand; //设定参数，其中的“/C”表示执行完命令后马上退出
                startInfo.UseShellExecute = false; //不使用系统外壳程序启动
                startInfo.RedirectStandardInput = false; //不重定向输入
                startInfo.RedirectStandardOutput = true; //重定向输出
                startInfo.CreateNoWindow = true; //不创建窗口
                startInfo.WorkingDirectory = currentPath;

                process.StartInfo = startInfo;
                process.Start();
            }
        }

        // 当找不到文件或者拒绝访问时出现的Win32错误码
        const int ERROR_FILE_NOT_FOUND = 2;
        const int ERROR_ACCESS_DENIED = 5;


        /// <summary>
        /// 
        /// </summary>
        public static string CmdRunAndReturn(string dosCommand, string currentPath = "")
        {
            Process process = new Process();
            try
            {
                process.StartInfo.UseShellExecute = false;   //是否使用操作系统shell启动 
                process.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值 (不显示程序窗口)
                process.StartInfo.RedirectStandardInput = true;  // 接受来自调用程序的输入信息 
                process.StartInfo.RedirectStandardOutput = true;  // 由调用程序获取输出信息
                process.StartInfo.RedirectStandardError = true;  //重定向标准错误输出
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.WorkingDirectory = currentPath;
                process.Start();                         // 启动程序

                process.ErrorDataReceived += Process_ErrorDataReceived;
                
                process.StandardInput.WriteLine(dosCommand); //向cmd窗口发送输入信息
                process.StandardInput.AutoFlush = true;
                // 前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
                process.StandardInput.WriteLine("exit");

                StringBuilder sb = new StringBuilder();
                StreamReader reader = process.StandardOutput;//获取exe处理之后的输出信息
                string curLine = string.Empty; //获取错误信息到error

                do
                {
                    curLine = reader.ReadLine();
                    if (!string.IsNullOrEmpty(curLine))
                    {
                        sb.Append(curLine);
                    }
                }
                while (!reader.EndOfStream);
                reader.Close(); //close进程

                string err = process.StandardError.ReadToEnd();

                process.WaitForExit(1000);  //等待程序执行完退出进程
                process.Close();

                if (!string.IsNullOrWhiteSpace(err))
                    return $"Error::"+err;

                return sb.ToString().Substring(sb.ToString().IndexOf('>')+1);

            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == ERROR_FILE_NOT_FOUND)
                {
                    Console.WriteLine(e.Message + ". 检查文件路径.");
                    return e.Message + ". 检查文件路径.";
                }
                else if (e.NativeErrorCode == ERROR_ACCESS_DENIED)
                {
                    Console.WriteLine(e.Message + ". 你没有权限操作文件.");
                    return e.Message + ". 你没有权限操作文件.";
                }
                return e.Message;
            }
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        public static string GetExpansionToolPath()
        {
            var plus = Directory.GetDirectories($"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Microsoft\\VisualStudio\\", "Resources", SearchOption.AllDirectories);
            if (plus.Length == 0)
                throw new ArgumentNullException("未安装插件[0]");

            foreach(var p in plus)
            {
                if (!p.Contains("Extensions"))
                    continue;

                //var p2= Path.Combine(p, "..");
                var p2 = p.Replace("\\Resources","");

                //ThriftVSIX
                var files=Directory.GetFiles(p2, "ThriftVSIX.dll", SearchOption.AllDirectories);
                if (files.Length == 0)
                    continue;

                // 暂不考虑version
                //Util.ExtensionToolVersion

                return p2;
            }

            throw new ArgumentNullException("未安装插件[1]");
            //return plus[0];
        }

        public static string GetExpansionToolResourcesPath()
        {
            var toolPath = GetExpansionToolPath();

            //var resourcesPath = Path.Combine(toolPath, $"{Util.ExtensionToolVersion}\\Resources");
            var resourcesPath = Path.Combine(toolPath, "Resources");

            if (!Directory.Exists(resourcesPath))
                throw new ArgumentNullException($"资源不存在 version:{Util.ExtensionToolVersion} path:{resourcesPath}");

            return resourcesPath;
        }

        public static void CopyDllToThriftDir(string resourcesPath)
        {
            string newPath = $"{Path.GetTempPath()}thrift\\dependency\\";
            if (Directory.Exists(newPath))
            {
                return;
            }

            var fList = Directory.GetDirectories(resourcesPath + "\\dependency\\", "*", SearchOption.AllDirectories);
            foreach (var f in fList)
            {
                var str = Path.GetFileName(f);
                string str2 = Path.Combine(newPath, str);
                Directory.CreateDirectory(str2);

                var tList = Directory.GetFiles(f);
                foreach (var t in tList)
                {
                    File.Copy(t, Path.Combine(str2, Path.GetFileName(t)));
                }
            }
        }

        public static void CheckCmdMessageThrewException(string message)
        {
            if (message.StartsWith("Error::"))
                throw new Exception($"CMD Exception {message}");
        }
    }
}
