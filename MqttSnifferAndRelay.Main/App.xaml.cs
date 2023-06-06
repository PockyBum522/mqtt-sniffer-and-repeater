using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using MqttSnifferAndRelay.Core;
using MqttSnifferAndRelay.Core.Logic.Application;
using MqttSnifferAndRelay.UI.Interfaces;
using MqttSnifferAndRelay.UI.WindowResources.MainWindow;

namespace MqttSnifferAndRelay.Main;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
[SupportedOSPlatform("Windows7.0")]
public partial class App
{
    private readonly DiContainerBuilder _mainBuilder = new ();
    private ILifetimeScope? _scope;
    private MainWindow? _mainWindow;

    /// <summary>
    /// Overridden OnStartup, this is our composition root and has the most basic work going on to start the app
    /// </summary>
    /// <param name="e">Startup event args</param>
    [SupportedOSPlatform("Windows7.0")]
    protected override void OnStartup(StartupEventArgs e)
    {
        var dependencyContainer = _mainBuilder.GetBuiltContainer();
            
        _scope = dependencyContainer.BeginLifetimeScope();
            
        var exceptionHandler = _scope.Resolve<ExceptionHandler>(); 
            
        exceptionHandler.SetupExceptionHandlingEvents();

#pragma warning disable CS0162 
        
        ApplicationInformation.RunAsTrayIconApplication = true;
        
        ApplicationInformation.ApplicationFriendlyName = "Mqtt Sniffer and Relay"; 
        
        if (ApplicationInformation.RunAsTrayIconApplication)
        {
            // Start TrayIcon
            var unused = _scope.Resolve<ITrayIcon>();
        }
#pragma warning restore CS0162
    }
}