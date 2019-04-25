using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace HolidaysSpan
{
  public class HolidayProcessorClassic
  {
    private Dictionary<DayOfWeek, int> _days = new Dictionary<DayOfWeek, int>();

    public void Run()
    {
      ProcessFile();
      PrintResult();
    }

    private void ProcessFile()
    {
      using (var file = File.Open("holidays.csv", FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        using (var reader = new StreamReader(file))
        {
          string line;
          while ((line = reader.ReadLine()) != null)
          {
            ProcessLine(line);
          }
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
        if (startDate.Date.DayOfWeek != DayOfWeek.Saturday || startDate.Date.DayOfWeek != DayOfWeek.Sunday)
          if (!_days.TryAdd(startDate.Date.DayOfWeek, 1))
            _days[startDate.Date.DayOfWeek]++;
        startDate = startDate.AddDays(1);
      }
    }

    private void PrintResult()
    {
      foreach (var entry in _days)
      {
        Console.WriteLine($"{entry.Key}: {entry.Value}");
      }
    }
  }
}
