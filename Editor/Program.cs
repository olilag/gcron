using System;
using System.IO;
using Common.Communication;

namespace Editor;

class Program
{
    static void Main(string[] args)
    {
        // TODO: launch editor ($EDITOR env var) or nano on Linux, idk what on Windows
        // create config file somewhere before editing if does not exist
        // validate the configuration -> notify daemon only if valid
        var client = new Client();
        using var conn = client.Connect();
        Console.WriteLine("Connected to daemon");
        conn.WriteString(Path.GetFullPath(args[0]));
    }
}
