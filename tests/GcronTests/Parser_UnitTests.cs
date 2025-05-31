using System.Collections.Generic;
using System.IO;
using Common.Configuration;

namespace GcronTests;

public class Parser_UnitTests
{
    [Fact]
    public void Parser_Parse_SimpleJob()
    {
        // Arrange
        var input = "1 2 3 4 5 echo Hello World!";
        using var parser = new Parser(new StringReader(input));
        var minutes = new Minute(1);
        var hours = new Hour(2);
        var dayOfMonth = new DayOfMonth(3);
        var month = Month.April;
        var dayOfWeek = DayOfWeek.Friday;
        var command = "echo Hello World!";
        HashSet<CronJob> expected = [new() { Minutes = minutes, Hours = hours, Days = dayOfMonth, Months = month, Weekdays = dayOfWeek, Command = command }];
        // Act
        var actual = parser.Parse();
        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parser_Parse_TwoJobs()
    {
        // Arrange
        var input = """
        1 2 3 4 5 echo Hello World!
        2 3 4 5 6 echo Cool!
        """;
        using var parser = new Parser(new StringReader(input));

        var minutes1 = new Minute(1);
        var hours1 = new Hour(2);
        var dayOfMonth1 = new DayOfMonth(3);
        var month1 = Month.April;
        var dayOfWeek1 = DayOfWeek.Friday;
        var command1 = "echo Hello World!";
        var job1 = new CronJob() { Minutes = minutes1, Hours = hours1, Days = dayOfMonth1, Months = month1, Weekdays = dayOfWeek1, Command = command1 };
        var minutes2 = new Minute(2);
        var hours2 = new Hour(3);
        var dayOfMonth2 = new DayOfMonth(4);
        var month2 = Month.May;
        var dayOfWeek2 = DayOfWeek.Saturday;
        var command2 = "echo Cool!";
        var job2 = new CronJob() { Minutes = minutes2, Hours = hours2, Days = dayOfMonth2, Months = month2, Weekdays = dayOfWeek2, Command = command2 };
        HashSet<CronJob> expected = [job1, job2];
        // Act
        var actual = parser.Parse();
        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parser_Parse_Job_Asterisk()
    {
        // Arrange
        var input = "* * * * * echo Hello World!";
        using var parser = new Parser(new StringReader(input));
        var minutes = Minute.All;
        var hours = Hour.All;
        var dayOfMonth = DayOfMonth.All;
        var month = Month.All;
        var dayOfWeek = DayOfWeek.All;
        var command = "echo Hello World!";
        HashSet<CronJob> expected = [new() { Minutes = minutes, Hours = hours, Days = dayOfMonth, Months = month, Weekdays = dayOfWeek, Command = command }];
        // Act
        var actual = parser.Parse();
        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parser_Parse_Job_Range()
    {
        // Arrange
        var input = "0-0 5-9 18-22 1-3 0-7 echo Hello World!";
        using var parser = new Parser(new StringReader(input));
        var minutes = new Minute(0);
        var hours = new Hour(5, 6, 7, 8, 9);
        var dayOfMonth = new DayOfMonth(18, 19, 20, 21, 22);
        var month = Month.January | Month.February | Month.March;
        var dayOfWeek = DayOfWeek.All;
        var command = "echo Hello World!";
        HashSet<CronJob> expected = [new() { Minutes = minutes, Hours = hours, Days = dayOfMonth, Months = month, Weekdays = dayOfWeek, Command = command }];
        // Act
        var actual = parser.Parse();
        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parser_Parse_Job_Commas()
    {
        // Arrange
        var input = "0 5,9 18,6 3 1 echo Hello World!";
        using var parser = new Parser(new StringReader(input));
        var minutes = new Minute(0);
        var hours = new Hour(5, 9);
        var dayOfMonth = new DayOfMonth(18, 6);
        var month = Month.March;
        var dayOfWeek = DayOfWeek.Monday;
        var command = "echo Hello World!";
        HashSet<CronJob> expected = [new() { Minutes = minutes, Hours = hours, Days = dayOfMonth, Months = month, Weekdays = dayOfWeek, Command = command }];
        // Act
        var actual = parser.Parse();
        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parser_Parse_AllAsterisks()
    {
        // Arrange
        var input = "* * * * * echo Hello World!";
        using var parser = new Parser(new StringReader(input));
        var minutes = Minute.All;
        var hours = Hour.All;
        var dayOfMonth = DayOfMonth.All;
        var month = Month.All;
        var dayOfWeek = DayOfWeek.All;
        var command = "echo Hello World!";
        HashSet<CronJob> expected = [new() { Minutes = minutes, Hours = hours, Days = dayOfMonth, Months = month, Weekdays = dayOfWeek, Command = command }];
        // Act
        var actual = parser.Parse();
        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parser_Parse_ExtraNewLine()
    {
        // Arrange
        var input = "* * * * * echo Hello World!\n";
        using var parser = new Parser(new StringReader(input));
        var minutes = Minute.All;
        var hours = Hour.All;
        var dayOfMonth = DayOfMonth.All;
        var month = Month.All;
        var dayOfWeek = DayOfWeek.All;
        var command = "echo Hello World!";
        HashSet<CronJob> expected = [new() { Minutes = minutes, Hours = hours, Days = dayOfMonth, Months = month, Weekdays = dayOfWeek, Command = command }];
        // Act
        var actual = parser.Parse();
        // Assert
        Assert.Equal(expected, actual);
    }

    static HashSet<CronJob> ParseString(string input)
    {
        using var parser = new Parser(new StringReader(input));
        return parser.Parse();
    }

    [Theory]
    // too big
    [InlineData("60 2 3 4 5 echo Hello World!")]
    // negative
    [InlineData("-60 2 3 4 5 echo Hello World!")]
    // not a digit
    [InlineData("6h0 2 3 4 5 echo Hello World!")]
    // overflow
    [InlineData("600 2 3 4 5 echo Hello World!")]
    public void Parser_Parse_InvalidMinutes(string input)
    {
        // Assert
        Assert.Throws<InvalidConfigurationException>(() => ParseString(input));
    }

    [Theory]
    // too big
    [InlineData("1 24 3 4 5 echo Hello World!")]
    // negative
    [InlineData("1 -2 3 4 5 echo Hello World!")]
    // not a digit
    [InlineData("1 2hu 3 4 5 echo Hello World!")]
    //overflow
    [InlineData("1 750 3 4 5 echo Hello World!")]
    public void Parser_Parse_InvalidHours(string input)
    {
        // Assert
        Assert.Throws<InvalidConfigurationException>(() => ParseString(input));
    }

    [Theory]
    // too big
    [InlineData("1 2 32 4 5 echo Hello World!")]
    // too small
    [InlineData("1 2 0 4 5 echo Hello World!")]
    // negative
    [InlineData("1 2 -3 4 5 echo Hello World!")]
    // not a digit
    [InlineData("1 2 3hy 4 5 echo Hello World!")]
    // overflow
    [InlineData("1 2 600 4 5 echo Hello World!")]
    public void Parser_Parse_InvalidDayOfMonth(string input)
    {
        // Assert
        Assert.Throws<InvalidConfigurationException>(() => ParseString(input));
    }

    [Theory]
    // too big
    [InlineData("1 2 3 13 5 echo Hello World!")]
    // too small
    [InlineData("1 2 3 0 5 echo Hello World!")]
    // negative
    [InlineData("1 2 3 -4 5 echo Hello World!")]
    // not a digit
    [InlineData("1 2 3 4hui 5 echo Hello World!")]
    // overflow 
    [InlineData("1 2 3 900 5 echo Hello World!")]
    public void Parser_Parse_InvalidMonth(string input)
    {
        // Assert
        Assert.Throws<InvalidConfigurationException>(() => ParseString(input));
    }

    [Theory]
    // too big
    [InlineData("1 2 3 4 8 echo Hello World!")]
    // negative
    [InlineData("1 2 3 4 -5 echo Hello World!")]
    // not a digit
    [InlineData("1 2 3 4 5hui echo Hello World!")]
    // overflow
    [InlineData("1 2 3 4 600 echo Hello World!")]
    public void Parser_Parse_InvalidDayOfWeek(string input)
    {
        // Assert
        Assert.Throws<InvalidConfigurationException>(() => ParseString(input));
    }

    [Theory]
    // negativerRange = range start is greater than end
    [InlineData("0-0 9-5 18-22 1-3 0-7 echo Hello World!")]
    // invalid first part
    [InlineData("0-0 abc-5 18-22 1-3 0-7 echo Hello World!")]
    // invalid second part
    [InlineData("0-0 9-cde 18-22 1-3 0-7 echo Hello World!")]
    // out of range
    [InlineData("0-60 9-12 18-22 1-3 0-7 echo Hello World!")]
    public void Parser_Parse_Job_InvalidRange(string input)
    {
        // Assert
        Assert.Throws<InvalidConfigurationException>(() => ParseString(input));
    }

    [Theory]
    // empty comma
    [InlineData("0, 9 18 1 1 echo Hello World!")]
    // too big number
    [InlineData("0,61 5 18 3 5 echo Hello World!")]
    // only comma
    [InlineData(", 9 18 1 0 echo Hello World!")]
    public void Parser_Parse_Job_InvalidCommas(string input)
    {
        // Assert
        Assert.Throws<InvalidConfigurationException>(() => ParseString(input));
    }

    [Theory]
    // missing command
    [InlineData("0 9 18 1 1")]
    // missing day of week
    [InlineData("0 5 18 3")]
    // missing month
    [InlineData("0 9 18")]
    // missing day of month
    [InlineData("0 9")]
    // missing hours
    [InlineData("0")]
    // second incomplete
    [InlineData("0 9 18 1 1 x\n5")]
    public void Parser_Parse_Job_InvalidRecord(string input)
    {
        // Assert
        Assert.Throws<InvalidConfigurationException>(() => ParseString(input));
    }
}
