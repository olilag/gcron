using System;
using System.Collections.Generic;
using System.Linq;

using Common.Configuration;

namespace GcronTests;

public class CronJob_UnitTests
{
    [Fact]
    public void DayOfMonth_OneDay()
    {
        // Arrange
        byte day = 5;
        // Act
        var dayOfMonth = new DayOfMonth(day);
        // Assert
        byte[] expected = [day];
        Assert.Equal(expected, dayOfMonth);
    }

    [Fact]
    public void DayOfMonth_MultipleDays()
    {
        // Arrange
        byte day1 = 5;
        byte day2 = 9;
        byte day3 = 25;
        // Act
        var dayOfMonth = new DayOfMonth(day1, day2, day3);
        // Assert
        byte[] expected = [day1, day2, day3];
        Assert.Equal(expected, dayOfMonth);
    }

    [Fact]
    public void DayOfMonth_AllDays()
    {
        // Arrange
        byte[] expected = [.. Enumerable.Range(1, 31).Select(idx => (byte)idx)];
        // Act
        var dayOfMonth = new DayOfMonth(expected);
        // Assert
        Assert.Equal(expected, dayOfMonth);
    }

    [Fact]
    public void DayOfMonth_All()
    {
        // Arrange
        byte[] expected = [.. Enumerable.Range(1, 31).Select(idx => (byte)idx)];
        // Act
        var dayOfMonth = DayOfMonth.All;
        // Assert
        Assert.Equal(expected, dayOfMonth);
        Assert.Equal(new(expected), dayOfMonth);
    }

    [Fact]
    public void DayOfMonth_DayTooBig()
    {
        // Arrange
        byte day = 32;
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new DayOfMonth(day));
    }

    [Fact]
    public void DayOfMonth_DayTooSmall()
    {
        // Arrange
        byte day = 0;
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new DayOfMonth(day));
    }

    [Fact]
    public void Hour_OneHour()
    {
        // Arrange
        byte hour1 = 5;
        // Act
        var hour = new Hour(hour1);
        // Assert
        byte[] expected = [hour1];
        Assert.Equal(expected, hour);
    }

    [Fact]
    public void Hour_MultipleHours()
    {
        // Arrange
        byte hour1 = 5;
        byte hour2 = 9;
        byte hour3 = 23;
        // Act
        var hour = new Hour(hour1, hour2, hour3);
        // Assert
        byte[] expected = [hour1, hour2, hour3];
        Assert.Equal(expected, hour);
    }

    [Fact]
    public void Hour_AllHours()
    {
        // Arrange
        byte[] expected = [.. Enumerable.Range(0, 24).Select(idx => (byte)idx)];
        // Act
        var hour = new Hour(expected);
        // Assert
        Assert.Equal(expected, hour);
    }

    [Fact]
    public void Hour_All()
    {
        // Arrange
        byte[] expected = [.. Enumerable.Range(0, 24).Select(idx => (byte)idx)];
        // Act
        var hour = Hour.All;
        // Assert
        Assert.Equal(expected, hour);
        Assert.Equal(new(expected), hour);
    }

    [Fact]
    public void Hour_HourTooBig()
    {
        // Arrange
        byte hour = 24;
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new Hour(hour));
    }

    [Fact]
    public void Minute_OneMinute()
    {
        // Arrange
        byte minute1 = 5;
        // Act
        var minute = new Minute(minute1);
        // Assert
        byte[] expected = [minute1];
        Assert.Equal(expected, minute);
    }

    [Fact]
    public void Minute_MultipleMinutes()
    {
        // Arrange
        byte minute1 = 5;
        byte minute2 = 32;
        byte minute3 = 57;
        // Act
        var minute = new Minute(minute1, minute2, minute3);
        // Assert
        byte[] expected = [minute1, minute2, minute3];
        Assert.Equal(expected, minute);
    }

    [Fact]
    public void Minute_AllMinutes()
    {
        // Arrange
        byte[] expected = [.. Enumerable.Range(0, 60).Select(idx => (byte)idx)];
        // Act
        var minute = new Minute(expected);
        // Assert
        Assert.Equal(expected, minute);
    }

    [Fact]
    public void Minute_All()
    {
        // Arrange
        byte[] expected = [.. Enumerable.Range(0, 60).Select(idx => (byte)idx)];
        // Act
        var minute = Minute.All;
        // Assert
        Assert.Equal(expected, minute);
        Assert.Equal(new(expected), minute);
    }

    [Fact]
    public void Minute_MinuteTooBig()
    {
        // Arrange
        byte minute = 60;
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new Minute(minute));
    }

    [Theory]
    [InlineData(Common.Configuration.DayOfWeek.Monday, System.DayOfWeek.Monday, 0)]
    [InlineData(Common.Configuration.DayOfWeek.Monday, System.DayOfWeek.Tuesday, 6)]
    [InlineData(Common.Configuration.DayOfWeek.Monday, System.DayOfWeek.Sunday, 1)]
    [InlineData(Common.Configuration.DayOfWeek.Sunday, System.DayOfWeek.Sunday, 0)]
    [InlineData(Common.Configuration.DayOfWeek.Sunday, System.DayOfWeek.Saturday, 1)]
    [InlineData(Common.Configuration.DayOfWeek.All, System.DayOfWeek.Saturday, 0)]
    [InlineData(Common.Configuration.DayOfWeek.All, System.DayOfWeek.Sunday, 0)]
    [InlineData(Common.Configuration.DayOfWeek.Sunday | Common.Configuration.DayOfWeek.Wednesday, System.DayOfWeek.Saturday, 1)]
    [InlineData(Common.Configuration.DayOfWeek.Sunday | Common.Configuration.DayOfWeek.Wednesday, System.DayOfWeek.Tuesday, 1)]
    [InlineData(Common.Configuration.DayOfWeek.Sunday | Common.Configuration.DayOfWeek.Wednesday, System.DayOfWeek.Wednesday, 0)]
    [InlineData(Common.Configuration.DayOfWeek.Sunday | Common.Configuration.DayOfWeek.Wednesday, System.DayOfWeek.Friday, 2)]
    [InlineData(Common.Configuration.DayOfWeek.Sunday | Common.Configuration.DayOfWeek.Wednesday, System.DayOfWeek.Monday, 2)]
    public void DayOfWeek_DaysUntilNext(Common.Configuration.DayOfWeek dayOfWeek, System.DayOfWeek current, int expected)
    {
        // Act
        var actual = dayOfWeek.DaysUntilNext(current);
        // Assert
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> TestData()
    {
        yield return new object[] { new Minute(0, 1, 2, 3), "0-3" };
        yield return new object[] { new Minute(0, 1, 2, 3, 5, 6, 7), "0-3,5-7" };
        yield return new object[] { new Minute(1), "1" };
        yield return new object[] { new Minute(1, 3, 5), "1,3,5" };
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void EnumerableExtensions_MakeCompact(Minute minute, string expected)
    {
        // Act
        var actual = minute.MakeCompact();
        // Assert
        Assert.Equal(expected, actual);
    }
}
