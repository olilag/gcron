namespace Daemon;

class Program
{
    static void Main(string[] args)
    {
        var d = new Daemon();
        d.MainLoop();
    }
}
