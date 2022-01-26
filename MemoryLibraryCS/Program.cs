using System.Reflection;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using MemoryLibraryCS.Library;

MemoryManager manager = new("CPPTesting");
Console.WriteLine(manager.Read<int>(0x7FF793700000));