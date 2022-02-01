using System;
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
            var result = manager.Allocate(0x9247274, 4u, Helpers.AllocationType.MEM_COMMIT, Helpers.PAGE_CONSTANT.PAGE_EXECUTE_READWRITE);
            Console.WriteLine($"Result: {result}");
            manager.Write<int>(0x9247274, 69);
            Console.WriteLine($"Value: {manager.Read<int>(0x9247274)}");
        }
    }
}
