using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PSTimeTracker;

public static class Program
{
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern bool FreeConsole();

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;

    [STAThread]
    public static int Main(string[] args)
    {
        var app = new App();
        FreeConsole();
        ShowWindow(GetConsoleWindow(), SW_HIDE);
        return app.Run();

        //if (args != null && args.Length > 0)
        //{
        //    // TODO: Add your code to run in command line mode
        //    Console.WriteLine("Hello world. ");
        //    Console.ReadLine();
        //    return 0;
        //}
        //else
        //{
        //    FreeConsole();
        //    var app = new App();
        //    return app.Run();
        //}
    }
}
