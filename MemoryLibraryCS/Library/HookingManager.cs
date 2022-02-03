using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MemoryLibraryCS.Library
{
    public class HookingManager
    {
        public Action OnProcessExit;

        private Process _processInstance;
        private string _targetProcess;
        private MemoryManager _memoryManager;

        /// <summary>
        /// Construct a new hooking manager instance
        /// </summary>
        /// <param name="processName">The name of the process you would like to attach to.</param>
        /// <param name="memManager">(Optional) Instance of a memory manager to use, if not specified, one will be constructed.</param>
        /// <exception cref="Exception">Thrown when the process name is not found.</exception>
        public HookingManager(string processName, MemoryManager memManager = default)
        {
            // check if the process actually exists
            if (Process.GetProcessesByName(processName).Length < 1)
                throw new Exception("Process not found.");

            // set target process name for later use (if any)
            _targetProcess = processName;
            // set the process object
            _processInstance = Process.GetProcessesByName(_targetProcess)[0];
            // add the hook for users to use
            _processInstance.Exited += (_, _) =>
            {
                OnProcessExit.Invoke();
            };

            // instantiate the new memory manager instance
            _memoryManager = memManager ?? new(processName);
        }

        public void Call(long address)
        {
            Helpers.Imports.CreateThread(_processInstance.Handle, address);
        }

        /// <summary>
        /// DO NOT USE ( DOES NOT WORK ) ( WORK IN PROGRESS )
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="value"></param>
        public void Call<T>(long address, T value) where T : notnull
        {
            uint AllocationSize = 0;

            //Temp
            bool Flag = value is string;
            
            // Allocate size string, otherwise default
            if (value is string _val)
                AllocationSize = (uint)(_val.Length + 1);
            else
                AllocationSize = (uint)Marshal.SizeOf(value);

            // Allocate the memory in the target process
            var ValuePtr = _memoryManager.Allocate(AllocationSize);
            // Write the value into the allocated memory
            if (Flag)
            {
                for (int i = 0; i < AllocationSize - 1; i++)
                    _memoryManager.Write(ValuePtr + i, value.ToString()[i]);
                _memoryManager.Write(ValuePtr + AllocationSize, 0);
            }
            else
                _memoryManager.Write(ValuePtr, value);
            // Call the remote thread
            Helpers.Imports.CreateThread(_processInstance.Handle, address, ValuePtr);

            //_memoryManager.Free(ValuePtr);
        }

        /// <summary>
        /// DONT USE( WORK IN PROGRESS )( DOES NOT WORK )
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="value"></param>
        public void CallWithAllocate<T>(long address, T value) where T : notnull
        {
            // Create the allocation
            IntPtr HeapAllocation = Marshal.AllocHGlobal(typeof(T) == typeof(string) ? value.ToString().Length : Marshal.SizeOf(value));
            // copy the struct into the pointer
            Marshal.StructureToPtr(value, HeapAllocation, false);
            // Location in memory of the allocated region
            var MemoryLocation = _memoryManager.Allocate((uint)Marshal.SizeOf<T>());
            Console.WriteLine($"Allocated at {MemoryLocation}");
            // Write the memory into the other process
            _memoryManager.Write(MemoryLocation, HeapAllocation);
            // Free our memory
            Marshal.FreeHGlobal(HeapAllocation);
            // Create the thread with the pointer value of our allocated data
            Helpers.Imports.CreateThread(_processInstance.Handle, address, MemoryLocation);
            _memoryManager.Free(MemoryLocation);
        }
    }
}
