﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoryLibraryCS.Library;

namespace MemoryLibraryCS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MemoryManager manager = new("CPPTesting");
            HookingManager hman = new("CPPTesting");
            hman.Call(0x7FF626B0116D);
        }
    }
}
