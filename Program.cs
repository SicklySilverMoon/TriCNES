// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using System.Windows.Forms;

using Avalonia;
using System;

namespace TriCNES
{
    // internal static class Program
    // {
    //     /// <summary>
    //     /// The main entry point for the application.
    //     /// </summary>
    //     [STAThread]
    //     static void Main()
    //     {
    //         Application.EnableVisualStyles();
    //         Application.SetCompatibleTextRenderingDefault(false);
    //         Application.Run(new TriCNESGUI());
    //     }
    // }
    
    sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
