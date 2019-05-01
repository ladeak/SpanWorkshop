using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace HolidaysSpan
{
  public partial class Program
  {
    public static async Task Main(string[] args)
    {
      BenchmarkRunner.Run<BenchmarkFile>();
      BenchmarkRunner.Run<BenchmarkStream>();

      var p = new HolidayProcessor();
      Stopwatch sw = new Stopwatch();
      sw.Start();
      await p.Run();
      sw.Stop();
      Console.WriteLine(sw.ElapsedMilliseconds);
    }
  }
}