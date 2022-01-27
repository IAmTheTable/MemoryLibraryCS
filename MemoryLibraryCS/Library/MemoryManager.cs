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
            // set target process name for later use (if any)
            _targetProcess = processName;

            // check if the process actually exists
            if (Process.GetProcessesByName(processName).Length < 0)
                throw new Exception("Process not found.");

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

        public T Read<T>(long memoryAddress) where T : struct
        {
            Helpers.Imports.ReadMemory(_processHandle, memoryAddress, out T Value);
            return Value;
        }

        public void Write<T>(long memoryAddress, T value)
        {
            bool res1 = Helpers.Imports.VirtualProtectEx(_processHandle, memoryAddress, Marshal.SizeOf<T>(), (uint)Helpers.Imports.PAGE_CONSTANT.PAGE_READWRITE, out UIntPtr old);

            Helpers.Imports.WriteMemory(_processHandle, memoryAddress, value);

            bool res3 = Helpers.Imports.VirtualProtectEx(_processHandle, memoryAddress, Marshal.SizeOf<T>(), old.ToUInt32(), out old);
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
