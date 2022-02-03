using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace MemoryLibraryCS.Library
{
    public class MemoryManager : IDisposable
    {
        public Action OnProcessExit;
        public readonly string ProcessName;
        public readonly PEReader FileReader;

        private Process _process;
        private string? _targetProcess;
        private ulong _imageBase;
        private IntPtr _processHandle;
        public MemoryManager(string processName)
        {
            // check if the process actually exists
            if (Process.GetProcessesByName(processName).Length < 1)
                throw new Exception("Process not found.");

            // set target process name for later use (if any)
            _targetProcess = processName;
            ProcessName = processName;

            // set our process
            _process = Process.GetProcessesByName(processName)[0];
            // add an exit hook for users to use within their program.(args are useless)
            _process.Exited += (_, _) =>
            {
                OnProcessExit.Invoke();
            };

            // set the image base
            _imageBase = GetImageBase(_process.MainModule?.FileName);
            _processHandle = Helpers.Imports.GetProcessHandle((uint)Helpers.PROCESS_RIGHTS.PROCESS_ALL_ACCESS, false, _process.Id);
            FileReader = new PEReader(new MemoryStream(File.ReadAllBytes(_process.MainModule?.FileName)));
        }

        public static long GetFunctionAddress(long handle, string name) => Helpers.Imports.GetProcAddress((IntPtr)handle, name).ToInt64();
        public long GetFunctionAddress(string name) => Helpers.Imports.GetProcAddress(_processHandle, name).ToInt64();

        public static long GetModuleHandle(string name) => Helpers.Imports.GetModuleHandleA(name).ToInt64();
        public static long GetModuleHandleEx(string name)
        {
            if (!Helpers.Imports.GetModuleHandleExW(0, name, out IntPtr handle))
                throw new MemoryException("Could not get module handle.");
            return handle.ToInt64();
        }

        public PEHeader ParseFile(string? fileLocation)
        {
            // check if the file location exists.
            if (fileLocation == null)
                throw new Exception("File location does not exist.");

            if (File.Exists(fileLocation))
            {
                var FileData = File.ReadAllBytes(_process.MainModule?.FileName);
                PEHeaders h = new(new MemoryStream(FileData));
                return h.PEHeader;
            }
            else
                throw new Exception("File does not exist.");

        }
        public ulong GetImageBase(string? fileLocation)
        {
            // check if the file location exists.
            if (fileLocation == null)
                throw new Exception("File location does not exist.");

            if (File.Exists(fileLocation))
            {
                var FileData = File.ReadAllBytes(fileLocation);
                PEHeaders h = new(new MemoryStream(FileData));
                return h.PEHeader?.ImageBase ?? 0x0;
            }
            else
                throw new Exception("File does not exist.");
        }

        public T Read<T>(long memoryAddress) where T : struct
        {
            if (!Helpers.Imports.ReadMemory(_processHandle, (IntPtr)memoryAddress, out T Value, out int bytesRead))
                throw new Exception("Failed to read memory region.");

            return Value;
        }


        public unsafe void Write<T>(long memoryAddress, T value) where T : notnull
        {
            // try and unprotect the memory address
            if (!Helpers.Imports.VirtualProtectEx(_processHandle, (IntPtr)memoryAddress, (uint)Marshal.SizeOf<T>(), (int)Helpers.PAGE_CONSTANT.PAGE_EXECUTE_READWRITE, out uint old))
                throw new Exception("Failed to unprotect memory region.");

            if (!Helpers.Imports.WriteMemory(_processHandle, (IntPtr)memoryAddress, value, out int bytesWrote))
                throw new Exception("Failed to write memory.");

            if (!Helpers.Imports.VirtualProtectEx(_processHandle, (IntPtr)memoryAddress, (uint)Marshal.SizeOf<T>(), old, out uint _))
                throw new Exception("Failed to restore memory region.");
        }

        /// <summary>
        /// Modify a memory region with protection levels
        /// </summary>
        /// <param name="memoryAddress">Start address of the memory region to modify.</param>
        /// <param name="regionSize">The amount of bytes to modify.</param>
        /// <param name="pageAccess">The page access flags to use.</param>
        /// <param name="oldPageAccess">The old pageAccess value.</param>
        /// <returns>True if the modification was succesful.</returns>
        /// <exception cref="MemoryException">Failure to modify memory region</exception>
        /// <example>if(!Manager.Unprotect(0xDEADBEEF, 4, PageAccess.READ_WRITE, out uint oldPermissions) Console.WriteLine("Modification was unsuccessful");</example>
        public unsafe bool Unprotect(long memoryAddress, uint regionSize, uint pageAccess, out uint oldPageAccess)
        {
            if (!Helpers.Imports.VirtualProtectEx(_processHandle, (IntPtr)memoryAddress, regionSize, pageAccess, out oldPageAccess))
                throw new MemoryException("Failed to unprotect memory region.");

            return true;
        }

        /// <summary>
        /// Allocate memory at a specified point
        /// </summary>
        /// <param name="amountBytes">The amount of bytes to allocate.</param>
        /// <param name="allocationFlags" cref="Helpers.AllocationType">The type of allocation.</param>
        /// <param name="allocationProtection">The protection flags of the new memory region.</param>
        /// <returns>The base address of the newly allocated region.</returns>
        public unsafe long Allocate(uint amountBytes, Helpers.AllocationType allocationFlags = Helpers.AllocationType.MEM_COMMIT | Helpers.AllocationType.MEM_RESERVE, Helpers.PAGE_CONSTANT allocationProtection = Helpers.PAGE_CONSTANT.PAGE_EXECUTE_READWRITE)
        {
            var Result = Helpers.Imports.VirtualAllocEx(_processHandle, IntPtr.Zero, amountBytes, (uint)allocationFlags, (uint)allocationProtection);
            if (Result == null)
                throw new MemoryException("Failed to allocate memory");
            return Result;
        }

        public bool Free(long address)
        {
            if (!Helpers.Imports.VirtualFreeEx(_processHandle, (IntPtr)address, 0, (uint)Helpers.FreeType.MEM_RELEASE))
                throw new MemoryException("Failed to free memory.");
            return true;
        }


        public void Dispose()
        {
            _targetProcess = null;
            _process.Dispose();
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
