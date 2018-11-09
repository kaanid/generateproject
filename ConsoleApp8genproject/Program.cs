using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp8genproject
{
    class Program
    {
        static void Main(string[] args)
        {
            var thriftList= Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.thrift");
            for(var i=0;i<thriftList.Length;i++)
            {
                Console.WriteLine($"{i},{Path.GetFileName(thriftList[i])}");
            }

            Console.Read();

            foreach (var activefile in thriftList)
            {
                //ThriftGenerate thriftGenerate = new ThriftGenerateNet45(activefile);
                //thriftGenerate.GenerateProject();

                ThriftGenerate thriftGenerate = new ThriftGenerateNetCore(activefile);
                thriftGenerate.GenerateProject();
                

                Thread.Sleep(1000);
            }

            Console.WriteLine("success");
            Console.ReadLine();
            Console.WriteLine("success");
            Console.ReadLine();

        }
    }

  
}
