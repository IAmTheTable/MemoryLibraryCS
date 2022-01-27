using System.Reflection;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using MemoryLibraryCS.Library;

MemoryManager manager = new("CPPTesting");
Console.WriteLine(manager.Read<int>(0x25F0CFFAF4));
manager.Write(0x9CE6AFF724, 0x69); // TODO: *FIX THE WRITE MEMORY* (I HATE CS)
Console.WriteLine(manager.Read<int>(0x25F0CFFAF4));