using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using awt_pj_ws23_24_mobile_streaming_1.Libs;
using System.Text;

namespace awt_pj_ws23_24_mobile_streaming_1.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }


    public void OnGet()
    {
        /*
        if (IntelPowerGadget.IntelEnergyLibInitialize())
        {
            System.Console.WriteLine("Intel Power Gadget driver initialized!");
            int numMsrs = 0;
            int numNodes = 0;
            if (IntelPowerGadget.ReadSample())
            {
                System.Console.WriteLine("Sample read!");
            }
            if (IntelPowerGadget.GetNumNodes(ref numNodes))
            {
                System.Console.WriteLine($"Number of CPU Packages on the system: {numNodes}");
                IntelPowerGadget.NumNodes = numNodes;
            }
            if (IntelPowerGadget.GetNumMsrs(ref numMsrs))
            {
                System.Console.WriteLine($"Number of supported MSRs for bulk reading and logging: {numMsrs}");
            }
            for (int i = 0; i < numMsrs; i++)
            {
                var sb = new StringBuilder(256);
                int pFuncId = -1;
                if (IntelPowerGadget.GetMsrName(iMsr: i, sb) && IntelPowerGadget.GetMsrFunc(iMsr: i, ref pFuncId))
                {
                    System.Console.WriteLine($"MSR number {i}, MSR Name {sb.ToString()}, function {pFuncId}");
                }
                // get power data
            }
            if (IntelPowerGadget.IsGTAvailable())
            {
                System.Console.WriteLine("Intel graphics are available");
                int gtFreq = -1;
                if (IntelPowerGadget.GetGTFrequency(ref gtFreq))
                {
                    System.Console.WriteLine($"Graphics frequency: {gtFreq} MHz");
                }
            }
            else
            {
                System.Console.WriteLine("Intel graphics are unavailable");
            }
            long pSysTime = 0;
            if (IntelPowerGadget.GetSysTime(ref pSysTime))
            {
                long timeInSeconds = pSysTime >> 32;
                long timeInNanoSeconds = (pSysTime << 32) >> 32;
                System.Console.WriteLine($"This should be a 1: {(1 << 32) >> 32}");
                System.Console.WriteLine($"System time as of last ReadSample call: {timeInSeconds} s or {timeInNanoSeconds} ns");
                System.Console.WriteLine($"Equivalent to {timeInSeconds / 60} min or {timeInSeconds / 3600} h");
                System.Console.WriteLine($"Equivalent to {timeInNanoSeconds / 60000} min or {timeInSeconds / 3600000} h");
            }
            double pOffset = -1;
            if (IntelPowerGadget.GetTimeInterval(ref pOffset))
            {
                System.Console.WriteLine($"Time between the last two calls to ReadSample(): {pOffset} s");
            }
            for (int iNode = 0; iNode < IntelPowerGadget.NumNodes; iNode++)
            {
                if (iNode > 10)
                {
                    break;
                }
                int freqInMHz = -1;
                if (IntelPowerGadget.GetIAFrequency(iNode, ref freqInMHz))
                {
                    System.Console.WriteLine($"The frequency is {freqInMHz} MHz.");
                }
                double tdp = -1;
                if (IntelPowerGadget.GetTDP(iNode, ref tdp))
                {
                    System.Console.WriteLine($"The package TDP is {tdp} W");
                }
                int degreeC = -1;
                int maxDegreeC = -1;
                if (IntelPowerGadget.GetTemperature(iNode, ref degreeC) && IntelPowerGadget.GetMaxTemperature(iNode, ref maxDegreeC))
                {
                    System.Console.WriteLine($"The temperature target is {degreeC} C, the max temperature is {maxDegreeC}");
                }
                double pBaseFrequency = -1;
                if (IntelPowerGadget.GetBaseFrequency(iNode, ref pBaseFrequency))
                {
                    System.Console.WriteLine($"The processor frequency of the package is {pBaseFrequency}");
                }
            }
            IntelPowerGadget.ReadSample();
            double[] pResult = new double[16];
            int nResult = -1;
            var sb = new StringBuilder(16);
            IntelPowerGadget.GetPowerData(0, 1, pResult, ref nResult);
            for (int i = 0; i < nResult && i < 16; i++)
            {
                sb.Append(pResult[i]).Append(' ');
            }
            System.Console.WriteLine($"{sb.ToString()}");
            sb.Clear();
            IntelPowerGadget.GetPowerData(0, 2, pResult, ref nResult);
            for (int i = 0; i < nResult && i < 16; i++)
            {
                sb.Append(pResult[i]).Append(' ');
            }
            System.Console.WriteLine($"{sb.ToString()}");
            sb.Clear();

            Thread.Sleep(2000);
            IntelPowerGadget.ReadSample();
            IntelPowerGadget.GetPowerData(0, 1, pResult, ref nResult);
            for (int i = 0; i < nResult && i < 16; i++)
            {
                sb.Append(pResult[i]).Append(' ');
            }
            System.Console.WriteLine($"{sb.ToString()}");
            sb.Clear();
            IntelPowerGadget.GetPowerData(0, 2, pResult, ref nResult);
            for (int i = 0; i < nResult && i < 16; i++)
            {
                sb.Append(pResult[i]).Append(' ');
            }
            System.Console.WriteLine($"{sb.ToString()}");
            sb.Clear();

            Thread.Sleep(2000);
            IntelPowerGadget.ReadSample();
            Thread.Sleep(2000);
            IntelPowerGadget.ReadSample();

        }
        else
        {
            System.Console.WriteLine("false");
        }
        */
    }
}
