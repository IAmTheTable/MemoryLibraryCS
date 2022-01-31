using System.Reflection;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using MemoryLibraryCS.Library;

MemoryManager manager = new("CPPTesting");
Console.WriteLine(manager.Read<int>(new(0x7FF611C10000)));

manager.Write(new(0x7FF611C10000), 0x69);
Console.WriteLine(manager.Read<int>(new(0x7FF611C10000)));
manager.Write(new(0x7FF611C10000), 144);
Console.WriteLine(manager.Read<int>(new(0x7FF611C10000)));