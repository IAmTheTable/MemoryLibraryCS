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

    public enum AllocationType
    {
        /// <summary>
        /// Allocates memory charges (from the overall size of memory and the paging files on disk) for the specified reserved memory pages. The function also guarantees that when the caller later initially accesses the memory, the contents will be zero. Actual physical pages are not allocated unless/until the virtual addresses are actually accessed.
        /// To reserve and commit pages in one step, call VirtualAllocEx with MEM_COMMIT | MEM_RESERVE.
        /// Attempting to commit a specific address range by specifying MEM_COMMIT without MEM_RESERVE and a non-NULL lpAddress fails unless the entire range has already been reserved. The resulting error code is ERROR_INVALID_ADDRESS.
        /// An attempt to commit a page that is already committed does not cause the function to fail. This means that you can commit pages without first determining the current commitment state of each page.
        /// If lpAddress specifies an address within an enclave, flAllocationType must be MEM_COMMIT.
        /// </summary>
        MEM_COMMIT = 0x00001000,
        /// <summary>
        /// Reserves a range of the process's virtual address space without allocating any actual physical storage in memory or in the paging file on disk.
        /// You commit reserved pages by calling VirtualAllocEx again with MEM_COMMIT. To reserve and commit pages in one step, call VirtualAllocEx with MEM_COMMIT | MEM_RESERVE.
        /// Other memory allocation functions, such as malloc and LocalAlloc, cannot use reserved memory until it has been released.
        /// </summary>
        MEM_RESERVE = 0x00002000,
        /// <summary>
        /// Indicates that data in the memory range specified by lpAddress and dwSize is no longer of interest. The pages should not be read from or written to the paging file. However, the memory block will be used again later, so it should not be decommitted. This value cannot be used with any other value.
        /// Using this value does not guarantee that the range operated on with MEM_RESET will contain zeros. If you want the range to contain zeros, decommit the memory and then recommit it.
        /// When you use MEM_RESET, the VirtualAllocEx function ignores the value of fProtect. However, you must still set fProtect to a valid protection value, such as PAGE_NOACCESS.
        /// VirtualAllocEx returns an error if you use MEM_RESET and the range of memory is mapped to a file. A shared view is only acceptable if it is mapped to a paging file.
        /// </summary>
        MEM_RESET = 0x00080000,
        /// <summary>
        /// MEM_RESET_UNDO should only be called on an address range to which MEM_RESET was successfully applied earlier. It indicates that the data in the specified memory range specified by lpAddress and dwSize is of interest to the caller and attempts to reverse the effects of MEM_RESET. If the function succeeds, that means all data in the specified address range is intact. If the function fails, at least some of the data in the address range has been replaced with zeroes.
        /// This value cannot be used with any other value. If MEM_RESET_UNDO is called on an address range which was not MEM_RESET earlier, the behavior is undefined. When you specify MEM_RESET, the VirtualAllocEx function ignores the value of flProtect. However, you must still set flProtect to a valid protection value, such as PAGE_NOACCESS.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003 and Windows XP:  The MEM_RESET_UNDO flag is not supported until Windows 8 and Windows Server 2012.
        /// </summary>
        MEM_RESET_UNDO = 0x1000000,
        /// <summary>
        /// Allocates memory using large page support.
        /// The size and alignment must be a multiple of the large-page minimum. To obtain this value, use the GetLargePageMinimum function.
        /// If you specify this value, you must also specify MEM_RESERVE and MEM_COMMIT.
        /// </summary>
        MEM_LARGE_PAGES = 0x20000000,
        /// <summary>
        /// Reserves an address range that can be used to map Address Windowing Extensions (AWE) pages.
        /// This value must be used with MEM_RESERVE and no other values.
        /// </summary>
        MEM_PHYSICAL = 0x00400000,
        /// <summary>
        /// Allocates memory at the highest possible address. This can be slower than regular allocations, especially when there are many allocations.
        /// </summary>
        MEM_TOP_DOWN = 0x00100000
    }
    public enum FreeType
    {
        MEM_DECOMMIT = 0x00004000,
        MEM_RELEASE = 0x00008000,
        MEM_COALESCE_PLACEHOLDERS = 0x00000001,
        MEM_PRESERVE_PLACEHOLDER = 0x00000002
    }

    internal static class Imports
    {        /*HANDLE CreateRemoteThreadEx(
          [in]            HANDLE                       hProcess,
          [in, optional]  LPSECURITY_ATTRIBUTES        lpThreadAttributes,
          [in]            SIZE_T                       dwStackSize,
          [in]            LPTHREAD_START_ROUTINE       lpStartAddress,
          [in, optional]  LPVOID                       lpParameter,
          [in]            DWORD                        dwCreationFlags,
          [in, optional]  LPPROC_THREAD_ATTRIBUTE_LIST lpAttributeList,
          [out, optional] LPDWORD                      lpThreadId
        );
         */

        [DllImport("Kernel32.dll", EntryPoint = "CreateRemoteThreadEx")]
        private static extern IntPtr CreateRemoteThread(IntPtr handle, int attr, uint stackSize, long startAddress, object param, uint creationFlags, int attrList, out uint threadId);
        public static IntPtr CreateThread(IntPtr handle, long address)
        {
            return CreateRemoteThread(handle, 0, 0, address, null, 0, 0, out uint _);
        }
        public static IntPtr CreateThread(IntPtr handle, long address, dynamic variable)
        {
            return CreateRemoteThread(handle, 0, 0, address, variable, 0, 0, out uint _);
        }


        [DllImport("Kernel32.dll", EntryPoint = "VirtualFreeEx")]
        public static extern bool VirtualFreeEx(IntPtr handle, IntPtr address, uint size, uint freeType);


        [DllImport("Kernel32.dll", EntryPoint = "OpenProcess")]
        public static extern IntPtr GetProcessHandle(uint access, bool inheritHandle, int processId);

        [DllImport("Kernel32.dll", EntryPoint = "GetLastError")]
        public static extern uint GetLastError();
        [DllImport("Kernel32.dll", EntryPoint = "VirtualAllocEx")]
        public static extern long VirtualAllocEx(IntPtr handle, IntPtr address, uint size, uint allocFlags, uint protectionFlags);

        [DllImport("Kernel32.dll", EntryPoint = "ReadProcessMemory")]
        private static extern bool ReadProcessMemory(IntPtr handle, IntPtr address, out IntPtr value, int sz, out int bytesRead);
        public static bool ReadMemory<T>(IntPtr handle, IntPtr address, out T value, out int bytesRead)
        {
            bool Result = ReadProcessMemory(handle, address, out IntPtr ResultValue, Unsafe.SizeOf<T>(), out bytesRead);
            value = (T)Convert.ChangeType(ResultValue.ToInt32(), typeof(T));
            return Result;
        }

        [DllImport("Kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr handle, string funcName);
        [DllImport("Kernel32.dll")]
        public static extern IntPtr GetModuleHandleA(string moduleName);
        [DllImport("Kernel32.dll")]
        public static extern bool GetModuleHandleExW(int flags, string moduleName, out IntPtr handle);




        [DllImport("Kernel32.dll")]
        private unsafe static extern bool WriteProcessMemory(IntPtr handle, IntPtr address, byte[] value, int sz, out int bytes);

        public unsafe static bool WriteMemory<T>(IntPtr handle, IntPtr address, T _value, out int bytesRead) where T : notnull
        {
            /* some really bad reflection method of passing T as a value.
             * Its necessary, because this will only work at runtime.
             */

            var GetBytes = typeof(BitConverter).GetMethod("GetBytes", new[]
            {
                // return long if type is IntPtr, otherwise return actual type
                (typeof(T) == typeof(IntPtr)) ? typeof(long) : typeof(T)
            }); //

            object Value = null;

            if (_value is IntPtr _Value)
                Value = _Value.ToInt64();
            else
                Value = _value;

            byte[] result = (byte[])GetBytes.Invoke(null, new[] { Value });

            return WriteProcessMemory(handle, address, result, Unsafe.SizeOf<T>(), out bytesRead);
        }

        [DllImport("Kernel32.dll", EntryPoint = "VirtualProtectEx")]
        public unsafe static extern bool VirtualProtectEx(IntPtr handle, IntPtr address, uint size, uint newProtect, out uint oldProtect);
    }
}