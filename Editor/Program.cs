using System;
using System.IO;
using Common.Communication;

namespace Editor;

class Program
{
    static void Main(string[] args)
    {
        var client = new Client();
        using var conn = client.Connect();
        Console.WriteLine("Connected to daemon");
        conn.WriteString(Path.GetFullPath(args[0]));
    }
}
