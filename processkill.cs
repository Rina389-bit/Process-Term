using System;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {   //writes to console 
        Console.WriteLine("Attempting to block CRASHPAD_HANDLER.EXE...");
    
        Task.Run(() => MonitorAndBlockProcess("CRASHPAD_HANDLER.EXE"));
        //loop to keep running 
        while (true) { }
    }

//declares a method that monitors the creation of processes with the name processName // can change 1 to a different number if needed
static void MonitorAndBlockProcess(string processName)
{
    try
    {
        string query = $"SELECT * FROM _InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name ='{processName}'";
        ManagementEventWatcher watcher = new ManagementEventWatcher(new WqlEventQuery(query)); //creats manage event watcher that willl listen for events based on WMI query
       
       //attaches event handler that gets triggered when process specified is correct
        watcher.EventArrived += (sender, e) =>
        {
            //extracts the targetinstance property which contains the information about new process
            var targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            var processId = Convert.ToInt32(targetInstance["ProcessId"]); //retrieves the process ID of the newly create process used to identify to terminate
            Console.WriteLine($"Detected and killing: {processName} (PID: {processId})");

                try
                {
                    // Kill the process when detected
                    Process.GetProcessById(processId).Kill();
                    Console.WriteLine($"{processName} killed successfully.");
                }
                catch (Exception ex) //if error, the exception is caught and an error message is printed
                {
                    Console.WriteLine($"Failed to kill {processName}: {ex.Message}");
                }
            };

            watcher.Start(); //starts manage event watcher to begin listening for process creation events
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting up process watcher: {ex.Message}");
        }
    }

}