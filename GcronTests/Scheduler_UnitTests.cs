using System;
using System.IO;
using System.Linq;
using Common.Configuration;
using Daemon;

namespace GcronTests;

public class Scheduler_UnitTests
{
    #region GetNextExecution - Minute

    [Fact]
    public void Scheduler_GetNextExecution_Minute_Basic()
    {
        // Arrange
        var input = "1 * * * * echo";
        using var parser = new Parser(new StringReader(input));
        var job = parser.Parse().First();
        var startTime = new DateTime(2025, 1, 1, 0, 0, 0);
        // Act
        var nextExecution = Scheduler.GetNextExecution(job, startTime);
        // Assert
        Assert.Equal(new DateTime(2025, 1, 1, 0, 1, 0), nextExecution);
    }

    [Fact]
    public void Scheduler_GetNextExecution_Minute_Carry()
    {
        // Arrange
        var input = "1 * * * * echo";
        using var parser = new Parser(new StringReader(input));
        var job = parser.Parse().First();
        var startTime = new DateTime(2025, 1, 1, 0, 2, 0);
        // Act
        var nextExecution = Scheduler.GetNextExecution(job, startTime);
        // Assert
        Assert.Equal(new DateTime(2025, 1, 1, 1, 1, 0), nextExecution);
    }

    [Fact]
    public void Scheduler_GetNextExecution_Minute_More()
    {
        // Arrange
        var input = "5,15,36 * * * * echo";
        using var parser = new Parser(new StringReader(input));
        var job = parser.Parse().First();
        var startTime = new DateTime(2025, 1, 1, 0, 6, 0);
        // Act
        var nextExecution = Scheduler.GetNextExecution(job, startTime);
        // Assert
        Assert.Equal(new DateTime(2025, 1, 1, 0, 15, 0), nextExecution);
    }

    [Fact]
    public void Scheduler_GetNextExecution_Minute_More_Carry()
    {
        // Arrange
        var input = "5,15,36 * * * * echo";
        using var parser = new Parser(new StringReader(input));
        var job = parser.Parse().First();
        var startTime = new DateTime(2025, 1, 1, 0, 40, 0);
        // Act
        var nextExecution = Scheduler.GetNextExecution(job, startTime);
        // Assert
        Assert.Equal(new DateTime(2025, 1, 1, 1, 5, 0), nextExecution);
    }

    [Fact]
    public void Scheduler_GetNextExecution_Minute_SameAsStartTime()
    {
        // Arrange
        var input = "0 * * * * echo";
        using var parser = new Parser(new StringReader(input));
        var job = parser.Parse().First();
        var startTime = new DateTime(2025, 1, 1, 0, 0, 0);
        // Act
        var nextExecution = Scheduler.GetNextExecution(job, startTime);
        // Assert
        Assert.Equal(new DateTime(2025, 1, 1, 0, 0, 0), nextExecution);
    }

    #endregion

    #region GetNextExecution - Hour

    [Fact]
    public void Scheduler_GetNextExecution_Hour_Basic()
    {
        // Arrange
        var input = "* 1 * * * echo";
        using var parser = new Parser(new StringReader(input));
        var job = parser.Parse().First();
        var startTime = new DateTime(2025, 1, 1, 0, 0, 0);
        // Act
        var nextExecution = Scheduler.GetNextExecution(job, startTime);
        // Assert
        Assert.Equal(new DateTime(2025, 1, 1, 1, 0, 0), nextExecution);
    }

    [Fact]
    public void Scheduler_GetNextExecution_Hour_Carry()
    {
        // Arrange
        var input = "* 1 * * * echo";
        using var parser = new Parser(new StringReader(input));
        var job = parser.Parse().First();
        var startTime = new DateTime(2025, 1, 1, 2, 0, 0);
        // Act
        var nextExecution = Scheduler.GetNextExecution(job, startTime);
        // Assert
        Assert.Equal(new DateTime(2025, 1, 2, 1, 0, 0), nextExecution);
    }

    [Fact]
    public void Scheduler_GetNextExecution_Hour_More()
    {
        // Arrange
        var input = "* 1,6,16 * * * echo";
        using var parser = new Parser(new StringReader(input));
        var job = parser.Parse().First();
        var startTime = new DateTime(2025, 1, 1, 2, 0, 0);
        // Act
        var nextExecution = Scheduler.GetNextExecution(job, startTime);
        // Assert
        Assert.Equal(new DateTime(2025, 1, 1, 6, 0, 0), nextExecution);
    }

    [Fact]
    public void Scheduler_GetNextExecution_Hour_More_Carry()
    {
        // Arrange
        var input = "* 1,6,16 * * * echo";
        using var parser = new Parser(new StringReader(input));
        var job = parser.Parse().First();
        var startTime = new DateTime(2025, 1, 1, 17, 0, 0);
        // Act
        var nextExecution = Scheduler.GetNextExecution(job, startTime);
        // Assert
        Assert.Equal(new DateTime(2025, 1, 2, 1, 0, 0), nextExecution);
    }

    [Fact]
    public void Scheduler_GetNextExecution_Hour_SameAsStaringTime()
    {
        // Arrange
        var input = "* 6 * * * echo";
        using var parser = new Parser(new StringReader(input));
        var job = parser.Parse().First();
        var startTime = new DateTime(2025, 1, 1, 6, 0, 0);
        // Act
        var nextExecution = Scheduler.GetNextExecution(job, startTime);
        // Assert
        Assert.Equal(new DateTime(2025, 1, 1, 6, 0, 0), nextExecution);
    }

    #endregion

    #region GetNextExecution - Day

    [Fact]
    public void Scheduler_GetNextExecution_Day_Basic()
    {
        // Arrange
        var input = "* * 1 * * echo";
        using var parser = new Parser(new StringReader(input));
        var job = parser.Parse().First();
        var startTime = new DateTime(2025, 1, 1, 0, 0, 0);
        // Act
        var nextExecution = Scheduler.GetNextExecution(job, startTime);
        // Assert
        Assert.Equal(new DateTime(2025, 1, 1, 0, 0, 0), nextExecution);
    }

    #endregion
}
