using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace MemoryLibraryCS.Helpers
{
    internal static class Imports
    {
        internal enum FunctionType
        {
            Kernel32,
            User32
        }
        
        //public static Func<T> GetFunction<T>(FunctionType funcType, string functionName)
        //{
            
        //}
        

    }
}
