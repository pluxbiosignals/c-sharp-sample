
// "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\MSBuild\\15.0\\Bin\\Roslyn\\csc.exe" -reference:plux.dll Program_biosignalsPluxPro+.cs -out:"Program.exe"
// Program.exe scan   Run app scanning for devices (save output on log file)
// Program.exe BTH58:8E:81:A2:41:A6 > "log.txt"   Run app connecting to specific device (save output on log file)




using System.Collections.Generic;
using System;
using System.Threading.Tasks;


public class MyDevice : PluxDotNet.SignalsDev
{
   public MyDevice(string path) : base(path) {}

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

         if (args.Length > 0) {
            if(String.Compare(args[0], "scan", true) == 0) {
               // Search for Plux devices in range and use the first found
               System.Console.WriteLine("Scanning for devices...");
               List<PluxDotNet.DevInfo> devs = PluxDotNet.BaseDev.FindDevices();
               if (devs.Count > 0) {
                  System.Console.WriteLine("Devices found:");
                  foreach (PluxDotNet.DevInfo devInfo in devs)
                     System.Console.WriteLine(" * {0} - {1}", devInfo.path, devInfo.description);
                  macAddr = devs[0].path;
               } else {
                  System.Console.WriteLine("No devices found.");
                  System.Environment.Exit(1);
               }
            } else {
               // Device MAC address:
               macAddr = args[0];
               //macAddr = "BTH58:8E:81:A2:41:A6";  // replace with your device MAC address
               //macAddr = "BLE58:8E:81:A2:41:A6";
               //macAddr = "COM48";
            }
         } else {
            //System.Console.WriteLine(" ERROR: NULL input parameter. \n Please define device MAC address as input parameter or 'scan' keyword to search for Plux devices in range");
         }

         if (!String.IsNullOrEmpty(macAddr)) {

            System.Console.WriteLine("Connecting to {0}...", macAddr);


            // Example without "using" keyword - need to call Dispose() at the end
            MyDevice dev = new MyDevice(macAddr);

            Dictionary<string, object> props = dev.GetProperties();
            System.Console.WriteLine("Device description: {0} - {1}", props["description"], props["path"]);


            dev.freq = 200;  // acquisition base frequency of 200 Hz
   
   
            if (props["description"].ToString() == "biosignalsplux") {
               // (connecting to biosignalsplux Device)


               // start acquisition (CH1-CH4 acquisition, 16-bits)
               //dev.Start(dev.freq, 0x0F, 16);


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

               PluxDotNet.Source src_spo2 = new PluxDotNet.Source();
               src_spo2.port = 5; // SPO2 source port
               src_spo2.chMask = 0x03; // One-pair of leds (Red + IR)


               List<PluxDotNet.Source> srcs = new List<PluxDotNet.Source>() { src_emg1, src_emg2, src_ecg, src_spo2 };


               int[] LED_param = {80, 40}; // LED values (0-255). Default values:
                                           //  * Head band -> Red = 80, IR = 40;
                                           //  * Arm band -> Red = 4, IR = 2;
               dev.SetParameter(0x09, 0x03, LED_param, 2); // update oximeter LED values (Red and IR)   

               dev.Start(dev.freq, srcs);  // start acquisition

               System.Console.WriteLine("Acquisition started. Press a key to stop.");

               dev.Loop(); // run message loop

               dev.Stop(); // stop acquisition
           
            } else {
               System.Console.WriteLine("Device is not a biosignalsplux: {0}", props["description"]);
            }

            dev.Dispose(); // disconnect from device
            
         } else {
            System.Console.WriteLine(" ERROR: NULL input parameter. \n Please define device MAC address as input parameter or 'scan' keyword to search for Plux devices in range");
         }
      }
      catch (PluxDotNet.Exception.PluxException e)
      {
         System.Console.WriteLine("Exception: {0}", e.Message);
         //System.Console.WriteLine("Exception: {0} - {1}:{2} ({3})", e.Message, e.file, e.line, e.function);
      }
   }
}
