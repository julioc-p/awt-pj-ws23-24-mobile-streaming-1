// a stop analytics hub with a method to call stop meassurement from meassurement proces
using awt_pj_ss23_green_streaming_1.Model;
using Microsoft.AspNetCore.SignalR;
using System;
using System.IO;

namespace awt_pj_ss23_green_streaming_1.Hubs;

/// <summary>
/// Represents a hub for stopping analytics measurement process.
/// </summary>
public class AnalyticsHub : Hub
{
  /// <summary>
  /// Stops the measurement process.
  /// </summary>
  public void StopAnalitycs()
  {
    System.Console.WriteLine("Stoping Measurement Hub from StopAnalitycsHub---------------------------");
    MeasurementProcess.Stop();
  }

  // make a method to create a folder with a parameter name
  public void CreateNewFolder(string name)
  {

    Console.WriteLine("CreateNewFolder: " + name);
    string path =
            $"Measurements/data/{name}";           
        path = path.Replace(":", "");
        Directory.CreateDirectory(path);
  }
}
