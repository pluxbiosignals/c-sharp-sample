


// "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\MSBuild\\15.0\\Bin\\Roslyn\\csc.exe" -reference:plux.dll Program_SimpleExample.cs -out:"Program.exe"
// Program.exe > "log.txt"  (Run app and save output on log file)




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
         string macAddr = "BTH00:07:80:F9:DC:D5";  // replace with your device MAC address
         //string macAddr = "COM48";


         System.Console.WriteLine("Connecting to {0}...", macAddr);


         // Example without "using" keyword - need to call Dispose() at the end
         MyDevice dev = new MyDevice(macAddr);
         Dictionary<string, object> props = dev.GetProperties();
         System.Console.WriteLine("Device description: {0} - {1}", props["description"], props["path"]);


         dev.freq = 1000;  // acquisition base frequency of 1000 Hz


         // Setting sensor sources individually...

         PluxDotNet.Source src_acc = new PluxDotNet.Source();
         src_acc.port = 11; // ACC source port
         src_acc.chMask = 0x07;  // bitmask for channels 1,2,3
         // src_acc.nBits kept at default value of 16


         PluxDotNet.Source src_spo2 = new PluxDotNet.Source();
         src_spo2.port = 9; // SPO2 source port
         src_spo2.chMask = 0x0F; // 2-Pair of Leds

         int[] LED_param = {80, 40};
         dev.SetParameter(0x09,0x03, LED_param, 2); // update oximeter LED values (Red and IR)


         List<PluxDotNet.Source> srcs = new List<PluxDotNet.Source>() { src_acc, src_spo2 };


         dev.Start(dev.freq, srcs);  // start acquisition
         

         System.Console.WriteLine("Acquisition started. Press a key to stop.");

         dev.Loop(); // run message loop

         dev.Stop(); // stop acquisition
        
         dev.Dispose(); // disconnect from device

      }
      catch (PluxDotNet.Exception.PluxException e)
      {
         System.Console.WriteLine("Exception: {0}", e.Message);
         //System.Console.WriteLine("Exception: {0} - {1}:{2} ({3})", e.Message, e.file, e.line, e.function);
      }
   }
}
