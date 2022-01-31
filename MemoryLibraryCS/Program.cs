using MemoryLibraryCS.Library;

MemoryManager manager = new("CPPTesting");

Console.WriteLine(manager.Read<int>(new(0x040A1B6F8A4)));
manager.Write(new(0x040A1B6F8A4), 0x69);
Console.WriteLine(manager.Read<int>(new(0x040A1B6F8A4)));