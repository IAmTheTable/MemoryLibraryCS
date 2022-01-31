# MemoryLibraryCS
A simple and easy to use memory manipulation library.

I wrote this library because of 2 reasons,
1) Im bored and have nothing better to do.
2) I love C# and C++, I'm implementing C++ features into C# the best I can (with my current knowledge set).

# Information
This library will allow you to externally edit the memory of other applications.
Please do note: This library is also not 100% complete and I'm sure has bugs reguarding use.
Please report these bugs!

# How to use
```cs
// First include the namespace.
using MemoryLibraryCS.Library;
// Instantiate a new memory manager instance related to the process we would like to manage
MemoryManager Manager = new("CPPTesting"); // Dotnet 6.0 Standard as of 1/31/2022
// Read an integer at the address
var Value = Manager.Read<int>(new(0xDEADBEEF));
// Print out our value in console.
Console.WriteLine($"Value of 0xDEADBEEF -> {Value}");
// Overwrite our original value with the value of 144(DEC), 0x90(HEX)
Manager.Write<int>(new(0xDEADBEEF), 0x90);
// Reread the integer at the address
Value = Manager.Read<int>(new(0xDEADBEEF));
// Print the value out again
Console.WriteLine($"New value of 0xDEADBEEF -> {Value}");
```
# Full Example
```cs
using System.Reflection;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using MemoryLibraryCS.Library;

MemoryManager manager = new("CPPTesting");

Console.WriteLine(manager.Read<int>(new(0x040A1B6F8A4)));
manager.Write(new(0x040A1B6F8A4), 0x69);
Console.WriteLine(manager.Read<int>(new(0x040A1B6F8A4)));
```
# Thanks
Thank you for taking your time checking out my repo, it really means a lot to me!
