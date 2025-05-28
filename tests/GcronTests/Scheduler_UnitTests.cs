using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Configuration;
using Daemon;

namespace GcronTests;

public class Scheduler_UnitTests
{
    public static IEnumerable<object[]> TestData()
    {
        yield return new object[] { "1 * * * * echo", new DateTime(2025, 1, 1, 0, 0, 0), new DateTime(2025, 1, 1, 0, 1, 0) };
        yield return new object[] { "1 * * * * echo", new DateTime(2025, 1, 1, 0, 2, 0), new DateTime(2025, 1, 1, 1, 1, 0) };
        yield return new object[] { "5,15,36 * * * * echo", new DateTime(2025, 1, 1, 0, 6, 0), new DateTime(2025, 1, 1, 0, 15, 0) };
        yield return new object[] { "5,15,36 * * * * echo", new DateTime(2025, 1, 1, 0, 40, 0), new DateTime(2025, 1, 1, 1, 5, 0) };
        yield return new object[] { "0 * * * * echo", new DateTime(2025, 1, 1, 0, 0, 0), new DateTime(2025, 1, 1, 0, 0, 0) };
        yield return new object[] { "* 1 * * * echo", new DateTime(2025, 1, 1, 0, 0, 0), new DateTime(2025, 1, 1, 1, 0, 0) };
        yield return new object[] { "* 1 * * * echo", new DateTime(2025, 1, 1, 2, 0, 0), new DateTime(2025, 1, 2, 1, 0, 0) };
        yield return new object[] { "* 1,6,16 * * * echo", new DateTime(2025, 1, 1, 2, 0, 0), new DateTime(2025, 1, 1, 6, 0, 0) };
        yield return new object[] { "* 1,6,16 * * * echo", new DateTime(2025, 1, 1, 17, 0, 0), new DateTime(2025, 1, 2, 1, 0, 0) };
        yield return new object[] { "* 6 * * * echo", new DateTime(2025, 1, 1, 6, 0, 0), new DateTime(2025, 1, 1, 6, 0, 0) };
        yield return new object[] { "1 1,6,16 * * * echo", new DateTime(2025, 1, 1, 23, 2, 0), new DateTime(2025, 1, 2, 1, 1, 0) };
        yield return new object[] { "* * 2 * * echo", new DateTime(2025, 1, 1, 0, 0, 0), new DateTime(2025, 1, 2, 0, 0, 0) };
        yield return new object[] { "* * 2 * * echo", new DateTime(2025, 1, 3, 0, 0, 0), new DateTime(2025, 2, 2, 0, 0, 0) };
        yield return new object[] { "* * 31 * * echo", new DateTime(2025, 2, 1, 0, 0, 0), new DateTime(2025, 3, 31, 0, 0, 0) };
        yield return new object[] { "* * 30 * * echo", new DateTime(2025, 12, 31, 0, 0, 0), new DateTime(2026, 1, 30, 0, 0, 0) };
        yield return new object[] { "* 22 30 * * echo", new DateTime(2025, 11, 30, 23, 0, 0), new DateTime(2025, 12, 30, 22, 0, 0) };
        yield return new object[] { "* * 29 * * echo", new DateTime(2025, 1, 30, 0, 0, 0), new DateTime(2025, 3, 29, 0, 0, 0) };
        yield return new object[] { "* * 2,29 * * echo", new DateTime(2025, 1, 30, 0, 0, 0), new DateTime(2025, 2, 2, 0, 0, 0) };
        yield return new object[] { "* * 29 * * echo", new DateTime(2024, 1, 30, 0, 0, 0), new DateTime(2024, 2, 29, 0, 0, 0) };
        yield return new object[] { "* * 29,30 * * echo", new DateTime(2025, 1, 30, 0, 0, 0), new DateTime(2025, 1, 30, 0, 0, 0) };
        yield return new object[] { "* * * 1 * echo", new DateTime(2025, 1, 30, 0, 0, 0), new DateTime(2025, 1, 30, 0, 0, 0) };
        yield return new object[] { "* * * 1 * echo", new DateTime(2025, 2, 1, 0, 0, 0), new DateTime(2026, 1, 1, 0, 0, 0) };
        yield return new object[] { "* * 29 2 * echo", new DateTime(2025, 2, 1, 0, 0, 0), new DateTime(2028, 2, 29, 0, 0, 0) };
        yield return new object[] { "* * * * * echo", new DateTime(2025, 1, 1, 23, 59, 0), new DateTime(2025, 1, 1, 23, 59, 0) };
        yield return new object[] { "* * * * 1 echo", new DateTime(2025, 1, 1, 0, 0, 0), new DateTime(2025, 1, 6, 0, 0, 0) };
        yield return new object[] { "* * * * 3 echo", new DateTime(2025, 1, 1, 0, 0, 0), new DateTime(2025, 1, 1, 0, 0, 0) };
        yield return new object[] { "0 * * * 3 echo", new DateTime(2025, 1, 1, 23, 59, 0), new DateTime(2025, 1, 8, 0, 0, 0) };
        yield return new object[] { "0 * * 2 3 echo", new DateTime(2025, 1, 1, 0, 0, 0), new DateTime(2025, 1, 1, 0, 0, 0) };
        yield return new object[] { "0 * * 1 5 echo", new DateTime(2025, 1, 1, 0, 0, 0), new DateTime(2025, 1, 1, 0, 0, 0) };
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void Scheduler_GetNextExecution(string jobConfig, DateTime startTime, DateTime expected)
    {
        // Arrange
        using var parser = new Parser(new StringReader(jobConfig));
        var job = parser.Parse().First();
        // Act
        var nextExecution = Scheduler.GetNextExecution(job, startTime);
        // Assert
        Assert.Equal(expected, nextExecution);
    }
}
