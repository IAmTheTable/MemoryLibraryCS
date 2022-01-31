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
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace MemoryLibraryCS.Helpers
{
    internal static class Imports
    {
        public enum PAGE_CONSTANT : int
        {
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_TARGETS_INVALID = 0x40000000,
            PAGE_TARGETS_NO_UPDATE = 0x40000000,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        public enum PROCESS_RIGHTS
        {
            PROCESS_CREATE_PROCESS = 0x0080,
            PROCESS_CREATE_THREAD = 0x0002,
            PROCESS_DUP_HANDLE = 0x0040,
            PROCESS_QUERY_INFORMATION = 0x0400,
            PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
            PROCESS_SET_INFORMATION = 0x0200,
            PROCESS_SET_QUOTA = 0x0100,
            PROCESS_SUSPEND_RESUME = 0x0800,
            PROCESS_TERMINATE = 0x0001,
            PROCESS_VM_OPERATION = 0x0008,
            PROCESS_VM_READ = 0x0010,
            PROCESS_VM_WRITE = 0x0020,
            SYNCHRONIZE = 0x00100000,
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFFF
        }

        [DllImport("Kernel32.dll", EntryPoint = "OpenProcess")]
        public static extern IntPtr GetProcessHandle(uint access, bool inheritHandle, int ProcessId);

        [DllImport("Kernel32.dll", EntryPoint = "GetLastError")]
        public static extern uint GetLastError();


        [DllImport("Kernel32.dll", EntryPoint = "ReadProcessMemory")]
        private static extern bool Read_Memory(IntPtr handle, IntPtr Address, out IntPtr value, int sz, out int BytesRead);
        public static bool ReadMemory<T>(IntPtr handle, IntPtr Address, out T value)
        {
            bool Result = Read_Memory(handle, Address, out IntPtr ResultValue, Unsafe.SizeOf<T>(), out int _BytesRead);
            value = (T)Convert.ChangeType(ResultValue.ToInt32(), typeof(T));
            return Result;
        }

        [DllImport("Kernel32.dll")]
        private unsafe static extern bool WriteProcessMemory(IntPtr handle, IntPtr Address, byte[] value, int sz, out IntPtr bytes);

        public unsafe static bool WriteMemory<T>(IntPtr handle, IntPtr Address, T _value) where T : struct
        {
            /* some really bad reflection method of passing T as a value.
             * Its necessary, because this will only work at runtime.
             */

            byte[] result = (byte[])typeof(BitConverter).GetMethod("GetBytes", new[] { typeof(T) }).Invoke(null, new[] { (object)_value });

            return WriteProcessMemory(handle, Address, result, Unsafe.SizeOf<T>(), out IntPtr bytes);
        }

        [DllImport("Kernel32.dll", EntryPoint = "VirtualProtectEx")]
        public unsafe static extern bool VirtualProtectEx(IntPtr handle, IntPtr Address, uint size, uint NewProtect, out uint OldProtect);
    }
}