using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HolidaysSpan
{
  public partial class Program
  {
    public static async Task Main(string[] args)
    {
      var p = new HolidayProcessor();
      Stopwatch sw = new Stopwatch();
      sw.Start();
      await p.Run();
      sw.Stop();
      Console.WriteLine(sw.ElapsedMilliseconds);
    }
  }
}