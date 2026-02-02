using Nice3point.Revit.Toolkit.External;
#if (isApplicationAddin)
using Nice3point.Revit.AddIn._1.Commands;
#endif
#if (addinLogging && !useDi)
using Serilog;
using Serilog.Events;
#endif

namespace Nice3point.Revit.AddIn._1;

/// <summary>
///     Application entry point
/// </summary>
[UsedImplicitly]
#if (isApplicationAddin)
public class Application : ExternalApplication
#else
public class Application : ExternalDBApplication
#endif
{
    public override void OnStartup()
    {
#if (useDi)
        Host.Start();
#endif
#if (addinLogging && !useDi)
        CreateLogger();
#endif
#if (isApplicationAddin)
        CreateRibbon();
#endif
    }
#if (diHosting || (!useDi && addinLogging))

    public override void OnShutdown()
    {
#if (diHosting)
        Host.Stop();
#elseif (addinLogging)
        Log.CloseAndFlush();
#endif
    }
#endif
#if (isApplicationAddin)

    private void CreateRibbon()
    {
        var panel = Application.CreatePanel("Commands", "Nice3point.Revit.AddIn.1");

        panel.AddPushButton<StartupCommand>("Execute")
            .SetImage("/Nice3point.Revit.AddIn.1;component/Resources/Icons/RibbonIcon16.png")
            .SetLargeImage("/Nice3point.Revit.AddIn.1;component/Resources/Icons/RibbonIcon32.png");
    }
#endif
#if (addinLogging && !useDi)

    private static void CreateLogger()
    {
        const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug(LogEventLevel.Debug, outputTemplate)
            .MinimumLevel.Debug()
            .CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var exception = (Exception)args.ExceptionObject;
            Log.Fatal(exception, "Domain unhandled exception");
        };
    }
#endif
}