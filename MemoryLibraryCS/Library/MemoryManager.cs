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
        }

        public void ParsePE(string fileLocation)
        {
            if (File.Exists(fileLocation))
            {
                var FileData = File.ReadAllBytes(fileLocation).ToList();
                const int BaseHeader = 0x3c;

                var loc = BitConverter.ToInt32(FileData.ToArray(), BaseHeader);
                var name = Encoding.ASCII.GetString(FileData.Take(new Range(new(loc), new(loc + 4))).ToArray());
                // Base offset of the optional headers
                var PEBase = BitConverter.ToInt16(FileData.ToArray(), loc + 24); // 24 is the offset of the PE magic number
                if(PEBase == 0x20B)
                {
                    // PE32+
                    var ImageBase = BitConverter.ToInt64(FileData.ToArray(), PEBase + 24);
                    Console.WriteLine($"Image base is: {ImageBase.ToString("X")}");
                }
                else if (PEBase == 0x10B)
                {
                    // PE32
                }
            }
        }

        public T Read<T>(long memoryAddress)
        {
            ParsePE(_process.MainModule.FileName);
            var ProcessHandle = _process.Handle;
            var ModuleBase = _process.MainModule.BaseAddress;
            return (T)Convert.ChangeType((ProcessHandle.ToInt64() - ModuleBase.ToInt64()) + memoryAddress, typeof(T));
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
