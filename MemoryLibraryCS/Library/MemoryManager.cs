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

        private Process _process;
        private string? _targetProcess;
        private ulong _imageBase;
        private IntPtr _processHandle;
        public MemoryManager(string processName)
        {
            // check if the process actually exists
            if (Process.GetProcessesByName(processName).Length < 0)
                throw new Exception("Process not found.");

            // set target process name for later use (if any)
            _targetProcess = processName;

            // set our process
            _process = Process.GetProcessesByName(processName)[0];
            // add an exit hook for users to use within their program.(args are useless)
            _process.Exited += (_, _) =>
            {
                OnProcessExit.Invoke();
            };

            // set the image base
            _imageBase = GetImageBase(_process.MainModule?.FileName);
            _processHandle = Helpers.Imports.GetProcessHandle((uint)Helpers.Imports.PROCESS_RIGHTS.PROCESS_ALL_ACCESS, false, _process.Id);
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

        public T Read<T>(IntPtr memoryAddress) where T : struct
        {
            if (!Helpers.Imports.ReadMemory(_processHandle, memoryAddress, out T Value, out int bytesRead))
                throw new Exception("Failed to read memory region.");

            return Value;
        }

        
        public unsafe void Write<T>(IntPtr memoryAddress, T value) where T : struct
        {
            // try and unprotect the memory address
            if (!Helpers.Imports.VirtualProtectEx(_processHandle, memoryAddress, (uint)Marshal.SizeOf<T>(), (int)Helpers.Imports.PAGE_CONSTANT.PAGE_EXECUTE_READWRITE, out uint old))
                throw new Exception("Failed to unprotect memory region.");

            if (!Helpers.Imports.WriteMemory(_processHandle, memoryAddress, value, out int bytesWrote))
                throw new Exception("Failed to write memory.");

            if (!Helpers.Imports.VirtualProtectEx(_processHandle, memoryAddress, (uint)Marshal.SizeOf<T>(), old, out uint _))
                throw new Exception("Failed to restore memory region.");
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
