﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriftService
{
    public interface IGenerateProject
    {
        string Version { get; }
        void Run();
    }
}
