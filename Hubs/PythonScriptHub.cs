using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace awt_pj_ws23_24_mobile_streaming_1.Hubs;
public class PythonScriptHub : Hub
{
    public async Task ExecutePythonScript()
    {
        Console.WriteLine("executing script");
        // changes need to be made to this when run on a different machine, keep in mind windows uses backslash
        // Retrieve the Python path from the environment variables
        string pythonPath = GetPythonPath();
        Console.WriteLine(pythonPath);

        // Check if Python path is available
        if (string.IsNullOrEmpty(pythonPath))
        {
            await Clients.All.SendAsync("PythonScriptError", "Python path not found in environment variables");
            return;
        }
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

    private static string GetPythonPath(string requiredVersion = "", string maxVersion = "")
    {
        string[] possiblePythonLocations = new string[3] {
        @"HKLM\SOFTWARE\Python\PythonCore\",
        @"HKCU\SOFTWARE\Python\PythonCore\",
        @"HKLM\SOFTWARE\Wow6432Node\Python\PythonCore\"
    };

        //Version number, install path
        Dictionary<string, string> pythonLocations = new Dictionary<string, string>();
        Console.WriteLine("Getting python path");

        foreach (string possibleLocation in possiblePythonLocations)
        {
            string regKey = possibleLocation[..4],
                   actualPath = possibleLocation[5..];
            RegistryKey? theKey = OperatingSystem.IsWindows() ? (regKey == "HKLM" ? Registry.LocalMachine : Registry.CurrentUser) : null;
            RegistryKey? theValue = theKey?.OpenSubKey(actualPath);

            if (theValue != null)
            {
                foreach (var v in theValue.GetSubKeyNames())
                {
                    if (theValue.OpenSubKey(v) is RegistryKey productKey)
                    {
                        try
                        {
                            string? pythonExePath = productKey.OpenSubKey("InstallPath")?.GetValue("ExecutablePath")?.ToString();

                            if (pythonExePath != null && pythonExePath != "")
                            {
                                pythonLocations.Add(v.ToString(), pythonExePath);
                            }
                        }
                        catch
                        {
                            // Install path doesn't exist
                        }
                    }
                }
            }
        }

        if (pythonLocations.Count > 0)
        {
            System.Version desiredVersion = new(requiredVersion == "" ? "0.0.1" : requiredVersion);
            System.Version maxPVersion = new(maxVersion == "" ? "999.999.999" : maxVersion);

            string highestVersion = "", highestVersionPath = "";

            foreach (KeyValuePair<string, string> pVersion in pythonLocations)
            {
                //TODO; if on 64-bit machine, prefer the 64 bit version over 32 and vice versa
                int index = pVersion.Key.IndexOf("-"); //For x-32 and x-64 in version numbers
                string formattedVersion = index > 0 ? pVersion.Key.Substring(0, index) : pVersion.Key;

                System.Version thisVersion = new System.Version(formattedVersion);
                int comparison = desiredVersion.CompareTo(thisVersion),
                    maxComparison = maxPVersion.CompareTo(thisVersion);

                if (comparison <= 0)
                {
                    //Version is greater or equal
                    if (maxComparison >= 0)
                    {
                        desiredVersion = thisVersion;

                        highestVersion = pVersion.Key;
                        highestVersionPath = pVersion.Value;
                    }
                    //else
                    //    Console.WriteLine("Version is too high; " + maxComparison.ToString());
                }
                //else
                //    Console.WriteLine("Version (" + pVersion.Key + ") is not within the spectrum.");$
            }

            //Console.WriteLine(highestVersion);
            //Console.WriteLine(highestVersionPath);
            return highestVersionPath;
        }

        return "";
    }
}
