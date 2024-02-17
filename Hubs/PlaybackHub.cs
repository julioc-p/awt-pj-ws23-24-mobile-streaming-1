using awt_pj_ws23_24_mobile_streaming_1.Model;
using Microsoft.AspNetCore.SignalR;

namespace awt_pj_ws23_24_mobile_streaming_1.Hubs;

public class PlaybackHub : Hub
{
    /// <summary>
    /// Sets playback of <c>MeasurementProcess</c> to true.
    /// </summary>
    public void StartPlayback()
    {
        System.Console.WriteLine("Playback started");
        MeasurementProcess.VideoPlaying = true;
    }

    /// <summary>
    /// Sets playback of <c>MeasurementProcess</c> to false.
    /// </summary>
    public void StopPlayback()
    {
        System.Console.WriteLine("Playback stopped");
        MeasurementProcess.VideoPlaying = false;
    }
}
