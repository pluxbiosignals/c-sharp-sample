


// ================== Create Project =====================
// https://travis.media/how-to-run-csharp-in-vscode/
// =============== Build and Run Program =================
// "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\MSBuild\\15.0\\Bin\\Roslyn\\csc.exe" -reference:64-bit/plux.dll Program.cs -out:"Program.exe"
// Program.exe > "log.txt"  (Run app and save output on log file)
// Program.exe "00:07:80:D8:AA:AD"




using System.Collections.Generic;
using System;
using System.Threading.Tasks;


public class MyDevice : PluxDotNet.SignalsDev
{
    public MyDevice(string path) : base(path) { }

    public override bool OnRawFrame(int nSeq, int[] data)
    {

        if (nSeq % freq == 0)
        {  // here once per second
           // print one frame
            System.Console.Write("[" + DateTime.Now + "]");
            System.Console.Write(" {0} -", nSeq);

            foreach (int val in data)
                System.Console.Write(" {0}", val);

            System.Console.WriteLine();

            if (System.Console.KeyAvailable)
                return true;   // if a key was pressed, exit loop
        }

        return false;
    }

    public override bool OnEvent(PluxDotNet.Event.Event evt)
    {
        PluxDotNet.Event.Battery evtBat = evt as PluxDotNet.Event.Battery;
        if (evtBat != null)
        {
            System.Console.WriteLine("Battery event - voltage: {0} V ; charge remaining: {1} %",
               evtBat.voltage, evtBat.percentage);
            return false;
        }

        PluxDotNet.Event.SignalGood evtSigGood = evt as PluxDotNet.Event.SignalGood;
        if (evtSigGood != null)
        {
            System.Console.WriteLine("Signal state event - port: {0} ; is good: {1}",
               evtSigGood.port, evtSigGood.isGood);
            return false;
        }

        System.Console.WriteLine(evt);
        return false;
    }
    /*
    // only needed if timeouts are used
    public override bool OnTimeout()
    {
       return true;
    }

    // only needed if interrupts are used
    public override bool OnInterrupt(object param)
    {
       return true;
    }
    */
    public int freq;
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Device MAC address
            string macAddr = null;

            if (args.Length > 0)
            {
                if (String.Compare(args[0], "scan", true) == 0)
                {
                    // Search for Plux devices in range and use the first found
                    System.Console.WriteLine("Scanning for devices...");
                    List<PluxDotNet.DevInfo> devs = PluxDotNet.BaseDev.FindDevices();
                    foreach (PluxDotNet.DevInfo devInfo in devs)
                        System.Console.WriteLine(" * {0} - {1}", devInfo.path, devInfo.description);
                    macAddr = devs[0].path;
                }
                else
                {
                    // Device MAC address:
                    macAddr = args[0];
                    //macAddr = "BTH88:6B:0F:4D:F6:D0";  // replace with your device MAC address
                    //macAddr = "BLE88:6B:0F:4D:F6:D0";
                    //macAddr = "COM48";
                }
            }
            else
            {
                System.Console.WriteLine(" ERROR: NULL input parameter. \n Please define device MAC address as input parameter or 'scan' keyword to search for Plux devices in range");
            }


            if (!String.IsNullOrEmpty(macAddr))
            {

                System.Console.WriteLine("Connecting to {0}...", macAddr);


                // Example without "using" keyword - need to call Dispose() at the end
                MyDevice dev = new MyDevice(macAddr);
                //MyDevice dev = new MyDevice_mem(macAddr);
                Dictionary<string, object> props = dev.GetProperties();
                System.Console.WriteLine("Device description: {0} - {1}", props["description"], props["path"]);

                dev.freq = 1000;  // acquisition base frequency of 1000 Hz


                if (props["description"].ToString() == "biosignalsplux")
                {
                    // (connecting to biosignalsplux Device)

                    // start acquisition (CH1-CH8 acquisition, 16-bits)
                    dev.Start(dev.freq, 0xFF, 16);

                    /*
                    // Setting sensor sources individually...

                    PluxDotNet.Source src_emg1 = new PluxDotNet.Source();
                    src_emg1.port = 1; // EMG source port
                    // src_emg1.freqDivisor kept at default value of 1
                    // src_emg1.chMask kept at default value of 1 (channel 1 only)
                    // src_emg1.nBits kept at default value of 16

                    PluxDotNet.Source src_emg2 = new PluxDotNet.Source();
                    src_emg2.port = 2; // EMG source port
                    // src_emg2.freqDivisor kept at default value of 1
                    // src_emg2.chMask kept at default value of 1 (channel 1 only)
                    // src_emg2.nBits kept at default value of 16

                    PluxDotNet.Source src_ecg = new PluxDotNet.Source();
                    src_ecg.port = 3; // ECG source port
                    // src_ecg.freqDivisor kept at default value of 1
                    // src_ecg.chMask kept at default value of 1 (channel 1 only)
                    // src_ecg.nBits kept at default value of 16

                    // ...

                    List<PluxDotNet.Source> srcs = new List<PluxDotNet.Source>() { src_emg, src_ecg };

                    dev.Start(dev.freq, srcs);  // start acquisition
                    */

                }
                else if (props["description"].ToString() == "MuscleBAN BE Plux")
                {
                    // (connecting to MuscleBAN Device)

                    PluxDotNet.Source src_emg = new PluxDotNet.Source();
                    src_emg.port = 1; // EMG source port
                    src_emg.freqDivisor = 100;   // divide 1000 Hz by 100 to send EMG envelope at 10 Hz
                                                 // src_emg.chMask kept at default value of 1 (channel 1 only)
                                                 // src_emg.nBits kept at default value of 16

                    PluxDotNet.Source src_acc = new PluxDotNet.Source();
                    src_acc.port = 2; // ACC source port
                                      // src_acc.freqDivisor kept at default value of 1
                    src_acc.chMask = 0x07;  // bitmask for channels 1,2,3
                                            // src_acc.nBits kept at default value of 16

                    List<PluxDotNet.Source> srcs = new List<PluxDotNet.Source>() { src_emg, src_acc };

                    dev.Start(dev.freq, srcs);  // start acquisition

                }
                else if (props["description"].ToString() == "OpenBANPlux")
                {
                    // (connecting to OpenBAN Device)

                    PluxDotNet.Source src_mic = new PluxDotNet.Source();
                    src_mic.port = 1; // MIC source port
                                      // src_mic.freqDivisor kept at default value of 1
                                      // src_mic.chMask kept at default value of 1 (channel 1 only)
                                      // src_mic.nBits kept at default value of 16

                    PluxDotNet.Source src_emg = new PluxDotNet.Source();
                    src_emg.port = 2; // EMG source port
                                      // src_emg.freqDivisor kept at default value of 1
                                      // src_emg.chMask kept at default value of 1 (channel 1 only)
                                      // src_emg.nBits kept at default value of 16

                    PluxDotNet.Source src_acc = new PluxDotNet.Source();
                    src_acc.port = 11; // ACC source port
                                       // src_acc.freqDivisor kept at default value of 1
                    src_acc.chMask = 0x07;  // bitmask for channels 1,2,3
                                            // src_acc.nBits kept at default value of 16

                    List<PluxDotNet.Source> srcs = new List<PluxDotNet.Source>() { src_mic, src_emg, src_acc };

                    dev.Start(dev.freq, srcs);  // start acquisition

                }
                else if (props["description"].ToString() == "BITalino")
                {
                    dev.Start(dev.freq, 0x3F, 10);
                }
                else
                {
                    // (...)
                }

                System.Console.WriteLine("Acquisition started. Press a key to stop.");

                dev.Loop(); // run message loop

                dev.Stop(); // stop acquisition

                dev.Dispose(); // disconnect from device
            }
        }
        catch (PluxDotNet.Exception.PluxException e)
        {
            // System.Console.WriteLine("Exception: {0} - {1}:{2} ({3})", e.Message, e.file, e.line, e.function);
        }
    }
}
