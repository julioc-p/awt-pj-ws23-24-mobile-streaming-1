using Microsoft.AspNetCore.SignalR;
using awt-pj-ws23-24-mobile-streaming-1.Model;

namespace awt-pj-ws23-24-mobile-streaming-1.Hubs
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
        public async Task StartMeasurement()
        {
            MeasurementProcess.ProcessMeasurement = SendMeasurement;
            MeasurementProcess.Start();
            Console.WriteLine("Started Measurement");
            await MeasurementProcess.WaitForExitAsync();
        }

        /// <summary>
        /// Saves measurements collected by <c>MeasurementProcess</c>
        /// </summary>
        /// <returns></returns>
        public async Task SaveMeasurements()
        {
            await MeasurementProcess.SaveMeasurements();
            Console.WriteLine("Saved Measurment");
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
