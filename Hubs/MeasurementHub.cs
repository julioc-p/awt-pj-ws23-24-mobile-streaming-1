using Microsoft.AspNetCore.SignalR;
using awt_pj_ws23_24_mobile_streaming_1.Model;

namespace awt_pj_ws23_24_mobile_streaming_1.Hubs
{
    public class MeasurementHub : Hub
    {
        public MeasurementHub() { }

        /// <summary>
        /// Sends <c>TotalPower</c> and time difference between <c>StartTime</c> of
        /// <c>MeasurementProcess</c> and <c>Time</c> of <paramref name="measurement"/>
        /// to all clients.
        /// </summary>
        /// <param name="measurement"></param>
        /// <returns></returns>
        public async Task SendMeasurement(Measurement measurement)
        {
            var timeDiff = (measurement.Time - MeasurementProcess.StartTime).TotalSeconds;
            await Clients.All.SendAsync("ReceiveMeasurement", measurement.TotalPower, timeDiff);
        }

        /// <summary>
        /// Starts <c>MeasurementProcess</c> but blocks <c>MeasurementHub</c> until
        /// <c>MeasurementProcess</c> is finished.
        /// </summary>
        /// <returns></returns>
        public async Task StartMeasurementFixedOps()
        {

            MeasurementProcess.ProcessMeasurement = SendMeasurement;
            MeasurementProcess.Start(true, null);
            await MeasurementProcess.WaitForExitAsync();
        }

        /// <summary>
        /// Starts <c>MeasurementProcess</c> but blocks <c>MeasurementHub</c> until
        /// <c>MeasurementProcess</c> is finished.
        /// </summary>
        /// <returns></returns>
        public async Task StartMeasurementUntilEnd()
        {   
            MeasurementProcess measurementProcess = new MeasurementProcess();
            CancellationTokenSource cts = new CancellationTokenSource();
            MeasurementProcess.ProcessMeasurement = SendMeasurement;
            MeasurementProcess.set(cts);
            //MeasurementProcess.HubContext = hubc;
            MeasurementProcess.Start(false, cts.Token);
            await MeasurementProcess.WaitForExitAsync();
        }

        /// <summary>
        /// Saves measurements collected by <c>MeasurementProcess</c>
        /// </summary>
        /// <returns></returns>
        public async Task SaveMeasurementsInFolder(string folderName, string currentSettings)
        {
            await MeasurementProcess.SaveMeasurements(folderName, currentSettings);
        }

        /// <summary>
        /// Saves measurements collected by <c>MeasurementProcess</c>
        /// </summary>
        /// <returns></returns>
        public async Task SaveMeasurements()
        {
            await MeasurementProcess.SaveMeasurements();

        }

        public void SaveNumberOfMeasurements(uint numOfMeasurements)
        {
            MeasurementProcess.NumOfMeasurements = numOfMeasurements;
            System.Console.WriteLine(
                $"Adjusted Number of Measurements to {MeasurementProcess.NumOfMeasurements}"
            );
        }

        public void ClearMeasurements()
        {
            MeasurementProcess.ClearMeasurements();
        }
    }
}
