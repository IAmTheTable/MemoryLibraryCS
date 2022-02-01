using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryLibraryCS.Library
{
    public class HookingManager
    {
        public Action OnProcessExit;

        private Process _processInstance;
        private string _targetProcess;
        private MemoryManager _manager;
        public HookingManager(string processName)
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
            _manager = new(_targetProcess);
        }

        public bool AddHook<ret_type, Args>(long address, Func<Args[], ret_type> func)
        {
            _manager.ParseFile(_processInstance.MainModule.FileName);
            return true;
        }
    }
}
