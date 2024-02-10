using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Threading.Tasks;

namespace awt_pj_ss23_green_streaming_1.Hubs;
public class PythonScriptHub : Hub
{
    public async Task ExecutePythonScript()
    {
        Console.WriteLine("executing script");
        // changes need to be made to this when run on a different machine, keep in mind windows uses backslash
        string pythonPath = @"C:\Users\perez\AppData\Local\Programs\Python\Python310\python.exe";
        string scriptPath = @".\PythonScripts\analysis_script.py";

        // Create a ProcessStartInfo object
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = scriptPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Create a new process and set the StartInfo property
        using (Process process = new Process { StartInfo = startInfo })
        {
            // Start the process
            process.Start();

            // Read the output and error streams (optional)
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            Console.WriteLine(output);

            // Wait for the process to exit
            process.WaitForExit();

            // Invoke a client method to send the output to the connected clients
            await Clients.All.SendAsync("PythonScriptOutput", output);

            // Log error if any
            if (!string.IsNullOrEmpty(error))
            {
                await Clients.All.SendAsync("PythonScriptError", error);
            }
        }
    }
}
