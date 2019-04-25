using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HolidaysSpan
{
  public partial class Program
  {
    public static async Task Main(string[] args)
    {
      var p = new HolidayProcessorClassic();
      Console.ReadLine();
      Stopwatch sw = new Stopwatch();
      sw.Start();
      p.Run();
      sw.Stop();
      Console.WriteLine(sw.ElapsedMilliseconds);
      Console.ReadLine();
    }
  }
}