using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace HolidaysSpan
{
  public class HolidayProcessorClassicBenchmark
  {
    private Dictionary<DayOfWeek, int> _days = new Dictionary<DayOfWeek, int>();
    private readonly string _file;
    private readonly Stream _stream;

    public HolidayProcessorClassicBenchmark(string file)
    {
      _file = file;
    }

    public HolidayProcessorClassicBenchmark(Stream stream)
    {
      _stream = stream;
    }

    public string Run()
    {
      _days = new Dictionary<DayOfWeek, int>();
      if (_file != null)
        ProcessFile();
      else
        ProcessStream(_stream);
      return PrintResult();
    }

    private void ProcessFile()
    {
      using (var file = File.Open(_file, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        ProcessStream(file);
      }
    }

    private void ProcessStream(Stream stream)
    {
      using (var reader = new StreamReader(stream))
      {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
          ProcessLine(line);
        }
      }
    }

    private void ProcessLine(string line)
    {
      var parts = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
      DateTime startDate = ParseDate(parts[parts.Length - 2]);
      DateTime endDate = ParseDate(parts[parts.Length - 1]);
      ProcessDates(startDate, endDate);
    }

    private DateTime ParseDate(string date)
    {
      return DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    private void ProcessDates(DateTime startDate, DateTime endDate)
    {
      while (startDate.Date <= endDate.Date)
      {
        if (startDate.Date.DayOfWeek != DayOfWeek.Saturday && startDate.Date.DayOfWeek != DayOfWeek.Sunday)
          if (!_days.TryAdd(startDate.Date.DayOfWeek, 1))
            _days[startDate.Date.DayOfWeek]++;
        startDate = startDate.AddDays(1);
      }
    }

    private string PrintResult()
    {
      string result = string.Empty;
      foreach (var entry in _days)
      {
        result += $"{entry.Key}: {entry.Value}" + Environment.NewLine;
      }
      return result;
    }
  }
}
