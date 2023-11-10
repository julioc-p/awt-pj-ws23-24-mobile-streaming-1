using System.Runtime.InteropServices;
using System.Text;

namespace awt_pj_ss23_green_streaming_1.Libs;

public class IntelPowerGadget
{
    public static int NumNodes { get; set; } = 0;
    public static int NumMsrs { get; set; } = 0;
    public static bool LibIsInitialized { get; set; } = false;

    /// <summary>
    /// Initializes the library and connects to the driver.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool IntelEnergyLibInitialize();

    /// <summary>
    /// Reads sample data from the driver for all the supported MSRs.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool ReadSample();


    /// <summary>
    /// Returns the number of CPU packages on the system.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetNumNodes(ref int nNodes);

    /// <summary>
    /// Returns the number of supported MSRs for bulk reading and logging.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetNumMsrs(ref int nMsrs);

    /// <summary>
    /// Returns in szName the name of the MSR specified by iMsr.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetMsrName(int iMsr, StringBuilder szName);

    /// <summary>
    /// Returns in pFuncID the function of the MSR specified by MSR ID in iMsr.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetMsrFunc(int iMsr, ref int pFuncID);

    /// <summary>
    /// Returns the data collected by the most recent call to ReadSample(). 
    /// The returned data is for the data on the package specified by iNode, from the MSR specified by iMSR. 
    /// The data is returned in pResult, and the number of double results returned in pResult is returned in nResult.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetPowerData(int iNode, int iMSR, double[] pResult, ref int nResult);

    /// <summary>
    /// Returns true if Intel® graphics is available and false if Intel® graphics is unavailable (i.e if it’s not present or disabled).
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool IsGTAvailable();

    /// <summary>
    /// Returns the current GT frequency in MHz. The data is returned in freq.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetGTFrequency(ref int freq);

    /// <summary>
    /// Returns the system time as of the last call to ReadSample(). 
    /// The data returned in pSysTime is structured as follows: 
    /// pSysTime[63:32] = time in seconds ; pSysTime[31:0] = time in nanoseconds
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetSysTime(ref long pSysTime);

    /// <summary>
    /// Returns in pOffset the time (in seconds) that has elapsed between the two most recent calls to ReadSample().
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetTimeInterval(ref double pOffset);

    /// <summary>
    /// Reads the processor frequency MSR on the package specified by iNode, and returns the frequency (in MHz) in freqInMHz.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetIAFrequency(int iNode, ref int freqInMHz);

    /// <summary>
    /// Reads the package power info MSR on the package specified by iNode, and returns the TDP (in Watts) in TDP. 
    /// It is recommended that Package Power Limit is used instead of TDP whenever possible, 
    /// as it is a more accurate upper bound to the package power than TDP.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetTDP(int iNode, ref double TDP);

    /// <summary>
    /// Reads the temperature target MSR on the package specified by iNode, and returns the maximum temperature (in degrees Celsius) in degreeC.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetMaxTemperature(int iNode, ref int degreeC);

    /// <summary>
    /// Reads the temperature MSR on the package specified by iNode, and returns the current temperature (in degrees) Celsius in degreeC.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetTemperature(int iNode, ref int degreeC);

    /// <summary>
    /// Returns in pBaseFrequency the advertised processor frequency for the package specified by iNode.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool GetBaseFrequency(int iNode, ref double pBaseFrequency);

    /// <summary>
    /// Starts saving the data collected by ReadSample() until StopLog() is called.Data will be written to the file specified by szFileName.
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll", CharSet = CharSet.Unicode)]
    public static extern bool StartLog([MarshalAsAttribute(UnmanagedType.LPWStr)] StringBuilder szFileName);

    /// <summary>
    /// Stops saving data and writes all saved data to the file specified by the call to StartLog().
    /// </summary>
    [DllImport("Libs/EnergyLib64.dll")]
    public static extern bool StopLog();

    // MSR Function | Data                  | Returns
    // -----------------------------------------------------------------------------------------------------------------------------------------
    // 0            | Frequency             | Frequency of sample (in MHz)
    // 1            | Power                 | Average Power (in Watts), Cumulative Energy (in Joules), Cumulative Energy (in milliWatt-Hours)
    // 2            | Temperature           | Temperature (in Deg. Celsius) Proc Hot (‘0’ if false and ‘1’ if true)
    // 3            | Package Power Limit   | Package Power Limit (in Watts)
}