using System.Text;
using System.Timers;
using awt_pj_ss23_green_streaming_1.Libs;

namespace awt_pj_ss23_green_streaming_1.Model;

public class MeasurementProcess
{
    // delay between measurements in ms
    private const int MEASUREMENT_RATE = 1000;
    public static uint NumOfMeasurements { get; set; } = 20;
    private static Measurement Measurement { get; set; }
    private static List<Measurement> Measurements { get; set; }
    private static System.Diagnostics.Process? Process { get; set; }
    private static System.Timers.Timer? Timer { get; set; }
    public static SendMeasurement? ProcessMeasurement { get; set; }
    public delegate Task SendMeasurement(Measurement measurement);
    public static DateTime StartTime { get; set; }
    private static bool _videoPlaying;
    private static CancellationTokenSource? cts;
    public static bool VideoPlaying
    {
        get => _videoPlaying;
        set
        {
            _videoPlaying = value;
            Measurement.VideoPlaying = value;
        }
    }

    static MeasurementProcess()
    {
        Measurement = new Measurement();
        Measurements = new List<Measurement>();
    }

    /// <summary>
    /// Sets up the MeasurementProcess and calls Start method of underlying process.
    /// Returns <c>true</c> and begins asynchronous reading of stdout if a new process has been started.
    /// Returns <c>false</c> if not.
    /// </summary>
    /// <returns></returns>
    public static bool Start(bool fixedNumber, object? obj)
    {
        SetupMeasurementProcess(fixedNumber, obj); // Assigns Process on macOS or Timer on Windows
        if (System.OperatingSystem.IsMacOS())
        {
            bool? started = Process?.Start();
            if (started is true)
            {
                StartTime = DateTime.Now;
                Process?.BeginOutputReadLine();
                return true;
            }
            return false;
        }
        else if (System.OperatingSystem.IsWindows())
        {
            try
            {
                if (Timer is null)
                {
                    return false;
                }
                StartTime = DateTime.Now;
                Timer?.Start();
                return true;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }
            return false;
        }
        throw new NotImplementedException("Implementation missing for the current OS.");
    }

    public static async Task<bool> StartAsync(bool fixedNumber, object? obj)
    {
        SetupMeasurementProcess(fixedNumber, obj); // Assigns Process on macOS or Timer on Windows
        if (System.OperatingSystem.IsMacOS())
        {
            bool? started = Process?.Start();
            if (started is true)
            {
                StartTime = DateTime.Now;
                Process?.BeginOutputReadLine();
                return true;
            }
            return false;
        }
        else if (System.OperatingSystem.IsWindows())
        {
            try
            {
                if (Timer is null)
                {
                    return false;
                }
                StartTime = DateTime.Now;
                Timer?.Start();
                await Task.Delay(100); // Allow time for asynchronous setup to complete
                return true;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
                return false;
            }
        }
        throw new NotImplementedException("Implementation missing for the current OS.");
    }


    public static void Stop()
    {
        // print some debug info
        System.Console.WriteLine("Stopping Measurement Process");
        // Add code to stop the measurement process (stop timers, clear resources, etc.)
        if (System.OperatingSystem.IsMacOS())
        {
            Process?.Kill(); // Terminate the powermetrics process on macOS
        }
        else if (System.OperatingSystem.IsWindows())
        {
            cts.Cancel();
        }
        cts.Dispose();
        ClearMeasurements();
    }

    public static async Task WaitForExitAsync()
    {
        if (Process is System.Diagnostics.Process p)
        {
            await p.WaitForExitAsync();
            return;
        }
        if (Timer is System.Timers.Timer t)
        {
            // check periodically if timer is complete
            while (t.Enabled)
            {
                await Task.Delay(1000);
            }
        }
        return;
    }

    /// <summary>
    /// Saves Measurements to file in Measurements directory.
    /// </summary>
    /// <returns></returns>
    public static async Task SaveMeasurements()
    {
        string path =
            $"Measurements/data/measurement_{MeasurementProcess.StartTime.ToString("s")}.txt";           
        path = path.Replace(":", "");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using (FileStream fs = File.Create(path))
        {
            StringBuilder sb = new StringBuilder(Measurements.Count * 101);
            sb.Append("Timestamp;VideoPlaying;TotalPower\n");
            foreach (var measurement in Measurements)
            {
                sb.Append(measurement.Time.ToString("s"))
                    .Append(';')
                    .Append(measurement.VideoPlaying)
                    .Append(';')
                    .Append(measurement.TotalPower)
                    .Append('\n');
            }
            byte[] text = new System.Text.UTF8Encoding(true).GetBytes(sb.ToString());
            await fs.WriteAsync(text, 0, text.Length);
        }
    }

    private static void SetupMeasurementProcess(bool fixedOps, object? obj)
    {
        if (System.OperatingSystem.IsMacOS())
        {
            SetupMeasurementProcessMacOs(fixedOps);
        }
        else if (System.OperatingSystem.IsWindows())
        { // TODO AMD
            SetupMeasurementProcessWindows(fixedOps, obj);
        }
        else
        {
            throw new NotImplementedException("Implementation missing for the current OS.");
        }
    }

    public static void ClearMeasurements()
    {
        Measurements = new List<Measurement>();
    }

    #region macOS
    /// <summary>
    /// Creates a new powermetrics process and assigns it to class variable Process
    /// </summary>
    /// <returns></returns>
    [System.Runtime.Versioning.SupportedOSPlatform("macos")]
    private static void SetupMeasurementProcessMacOs(bool fixedOps)
    {
        var procStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "sudo",
            Arguments =
                $"powermetrics -i {MEASUREMENT_RATE} -n {NumOfMeasurements} -f text --samplers cpu_power",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        var proc = new System.Diagnostics.Process { StartInfo = procStartInfo };
        proc.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(
            MacOsOutputHandler
        );
        Process = proc;
    }

    /// <summary>
    /// Parses output of powermetrics process.
    /// </summary>
    /// <param name="sendingProcess"></param>
    /// <param name="outLine"></param>
    [System.Runtime.Versioning.SupportedOSPlatform("macos")]
    private static async void MacOsOutputHandler(
        object sendingProcess,
        System.Diagnostics.DataReceivedEventArgs outLine
    )
    {
        // Collect the command output
        if (!String.IsNullOrEmpty(outLine.Data))
        {
            Measurement.CollectMeasurementMacOs(outLine.Data);
        }
        else
        {
            // no data do nothing
        }
        if (Measurement.IsComplete)
        {
            Measurement.Time = DateTime.Now;
            Measurement.VideoPlaying = VideoPlaying;
            Measurements.Add(Measurement);
            if (ProcessMeasurement is null)
            {
                return;
            }
            await ProcessMeasurement(Measurement);
            // create new Measurement so old one does not get overwritten
            Measurement = new Measurement();
        }
    }
    #endregion macOS


    // set cts method
    public static void set(CancellationTokenSource cts)
    {
        MeasurementProcess.cts = cts;
    }

    // get cts method
    public static CancellationTokenSource get()
    {
        return MeasurementProcess.cts;
    }
    #region windows
    /// <summary>
    /// Initializes the IntelPowerGadget Library and creates a Timer that reads its measurements
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void SetupMeasurementProcessWindows(bool fixedOps, object? obj) { 
       CancellationToken token = default(CancellationToken);
       if (!(obj is null))
        {
            token = (CancellationToken)obj;
        }
   
  
        var cpu = new System.Management.ManagementObjectSearcher("select * from Win32_Processor")
            .Get()
            .Cast<System.Management.ManagementObject>()
            .First();
        if (((string)cpu["Name"]).StartsWith("Intel"))
        {
            if (!IntelPowerGadget.LibIsInitialized)
            {
                // Initialize Intel Power Gadget Library
                if (!IntelPowerGadget.IntelEnergyLibInitialize())
                {
                    throw new ApplicationException("Could not initialize Intel Power Gadget.");
                }
                IntelPowerGadget.LibIsInitialized = true;
            }
            IntelPowerGadget.ReadSample();
            int numMsrs = 0;
            int numNodes = 0;
            if (
                !IntelPowerGadget.GetNumNodes(ref numNodes)
                || !IntelPowerGadget.GetNumMsrs(ref numMsrs)
            )
            {
                throw new ApplicationException(
                    "Could not get number of CPU Packages or number of supported MSRs."
                );
            }
            IntelPowerGadget.NumNodes = numNodes;
            IntelPowerGadget.NumMsrs = numMsrs;

            // find msrs corresponding to platform power
            var msrsForPower = new List<int>();
            for (int iMsr = 0; iMsr < numMsrs; iMsr++)
            {
                var szName = new System.Text.StringBuilder(16);
                if (!IntelPowerGadget.GetMsrName(iMsr, szName))
                {
                    throw new ApplicationException($"Could not get MSR Name for MSR number {iMsr}");
                }
                int pFuncId = -1;
                if (!IntelPowerGadget.GetMsrFunc(iMsr, ref pFuncId))
                {
                    throw new ApplicationException(
                        $"Could not get MSR function for MSR number {iMsr}"
                    );
                }
                // msr function 1 corresponds to power and P stands for platform, G would be graphics, I == processor, D = DRAM
                if (pFuncId == 1 && szName.ToString() == "P")
                {
                    msrsForPower.Add(iMsr);
                }
            }
            // Create Timer to periodically get measurements
            var timer = new System.Timers.Timer(MEASUREMENT_RATE);
            timer.Elapsed += new ElapsedEventHandler(
                (sender, e) => WindowsMeasurementHandler(sender, e, msrsForPower)
            );
            uint iterations = NumOfMeasurements;
            Console.WriteLine("Trying with cancellation token");
            if (iterations > 0)
            {
                timer.Elapsed += (s, e) =>
                {
                    if (iterations <= 0 || (!(obj is null) && token.IsCancellationRequested))
                    {
                        timer.Stop();
                    }

                    if (fixedOps) {
                        iterations--;
                    }
                    Console.WriteLine("Trying with cancellation token");
                };
            }
            Timer = timer;
        }
        else
        {
            throw new NotImplementedException($"CPU vendor is currently not supported");
        }
    }

   

    private static async void WindowsMeasurementHandler(
        Object? source,
        ElapsedEventArgs e,
        IEnumerable<int> msrsForPower
    )
    {
    
        // reads measurement, throws exception if fail
        if (!IntelPowerGadget.ReadSample())
        {
            throw new ApplicationException("Could not read sample.");
        }

        // for each msr and each node
        var measurements = new List<double>(IntelPowerGadget.NumNodes * msrsForPower.Count());

        // collects measurements
        for (int iNode = 0; iNode < IntelPowerGadget.NumNodes; iNode++)
        {
            foreach (var iMsr in msrsForPower)
            {
                int nResult = -1;
                var pResult = new double[8];
                if (!IntelPowerGadget.GetPowerData(iNode, iMsr, pResult, ref nResult))
                {
                    throw new ApplicationException(
                        $"Could not get power data on node {iNode} and msr {iMsr}."
                    );
                }
                measurements.Add(pResult[0]);
            }
        }
        Measurement.CollectMeasurementWindowsIntel(measurements);

        // sends Measurement through MeasurementHub to front end
        if (Measurement.IsComplete)
        {
            Measurement.Time = DateTime.Now;
            Measurements.Add(Measurement);
            if (ProcessMeasurement is null)
            {
                return;
            }
            await ProcessMeasurement(Measurement);
            // create new Measurement so old one does not get overwritten
            Measurement = new Measurement();
        }
    }
    #endregion windows
}
