using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using MqttSnifferAndRelay.Core;
using MqttSnifferAndRelay.Core.Logic;

namespace MqttSnifferAndRelay.UI.WindowResources.MainWindow;

/// <summary>
/// The ViewModel for MainWindow
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _applicationStatusLog = "";
    [ObservableProperty] private string _debugTopicText = "";
    [ObservableProperty] private string _motorBoardInText = "";
    [ObservableProperty] private string _motorBoardOutText = "";
    [ObservableProperty] private string _displayBoardInText = "";
    [ObservableProperty] private string _displayBoardOutText = "";
    [ObservableProperty] private string _everythingCombinedText = "";

    public bool RealTimeUpdatesEnabled
    {
        get => _mqttWatcher.RealTimeUpdatesEnabled;
        set => _mqttWatcher.RealTimeUpdatesEnabled = value;
    }

    public bool RepeatFromMotorBoardToDisplayBoard
    {
        get => _mqttWatcher.RepeatFromMotorBoardToDisplayBoard;
        set => _mqttWatcher.RepeatFromMotorBoardToDisplayBoard = value;
    }
    
    public bool RepeatFromDisplayBoardToMotorBoard
    {
        get => _mqttWatcher.RepeatFromDisplayBoardToMotorBoard;
        set => _mqttWatcher.RepeatFromDisplayBoardToMotorBoard = value;
    }
    
    private readonly ILogger _logger;

    private readonly MqttWatcher _mqttWatcher;
    //private readonly ISettingsApplicationLocal _settingsApplicationLocal;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    // /// <param name="settingsApplicationLocal">ISettingsApplicationLocal proxy object from Config.net that was set up in DIContainerBuilder.cs</param>
    public MainWindowViewModel(ILogger logger, MqttWatcher mqttWatcher) //, ISettingsApplicationLocal settingsApplicationLocal)
    {
        _logger = logger;
        _mqttWatcher = mqttWatcher;
        // _settingsApplicationLocal = settingsApplicationLocal;

        //TextRunningAsUsernameMessage = $"Running As DomainUser: {Environment.UserDomainName} | User: {Environment.UserName}";
    }

    [RelayCommand]
    private void StartMainSetupProcess()
    {
        _logger.Information("Running {ThisName}", System.Reflection.MethodBase.GetCurrentMethod()?.Name);
    }
    
    [RelayCommand]
    private void ClearAll()
    {
        ApplicationStatusLog = "";
        DebugTopicText = "";
        MotorBoardInText = "";
        MotorBoardOutText = "";
        DisplayBoardInText = "";
        DisplayBoardOutText = "";
        EverythingCombinedText = "";
        
        _mqttWatcher.DisplayBoardOutMessages.Clear();
        _mqttWatcher.MotorBoardOutMessages.Clear();
        _mqttWatcher.DisplayBoardInMessages.Clear();
        _mqttWatcher.MotorBoardInMessages.Clear();
        _mqttWatcher.DebugTopicMessages.Clear();
        _mqttWatcher.EverythingCombinedMessages.Clear();
        _mqttWatcher.ApplicationStatusLog.Clear();
    }
    
    [RelayCommand]
    private void MainWindowOnClosing()
    {
        _logger.Information("Running {ThisName}, this is just an example message to show how to use MVVM behaviors from the XAML",
            System.Reflection.MethodBase.GetCurrentMethod()?.Name);
    }
    
    /// <summary>
    /// Called from the XAML when window is loaded
    /// </summary>
    public async Task OnWindowLoaded()
    {
        _mqttWatcher.StartMqttListener();
    
        await Task.Delay(2000);
    
        while (true)
        {
            try
            {
                UpdateApplicationStatusLog();
                UpdateDebugTopicMessages();
                UpdateDisplayBoardTopicMessages();
                UpdateMotorBoardTopicMessages();
                UpdateEverythingCombined();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        
    
            await Task.Delay(50);
        }
    }

    private void UpdateEverythingCombined()
    {
        if (_mqttWatcher.EverythingCombinedMessages.Count > 0)
        {
            EverythingCombinedText += _mqttWatcher.EverythingCombinedMessages.Dequeue() + Environment.NewLine;
        }
    }

    private void UpdateDisplayBoardTopicMessages()
    {
        if (_mqttWatcher.DisplayBoardInMessages.Count > 0)
        {
            DisplayBoardInText += _mqttWatcher.DisplayBoardInMessages.Dequeue() + Environment.NewLine;
        }
        
        if (_mqttWatcher.DisplayBoardOutMessages.Count > 0)
        {
            DisplayBoardOutText += _mqttWatcher.DisplayBoardOutMessages.Dequeue() + Environment.NewLine;
        }
    }
    
    private void UpdateMotorBoardTopicMessages()
    {
        if (_mqttWatcher.MotorBoardInMessages.Count > 0)
        {
            MotorBoardInText += _mqttWatcher.MotorBoardInMessages.Dequeue() + Environment.NewLine;
        }
        
        if (_mqttWatcher.MotorBoardOutMessages.Count > 0)
        {
            MotorBoardOutText += _mqttWatcher.MotorBoardOutMessages.Dequeue() + Environment.NewLine;
        }
    }

    private void UpdateDebugTopicMessages()
    {
        if (_mqttWatcher.DebugTopicMessages.Count > 0)
        {
            DebugTopicText += _mqttWatcher.DebugTopicMessages.Dequeue() + Environment.NewLine;
        }
    }

    private void UpdateApplicationStatusLog()
    {
        if (_mqttWatcher.ApplicationStatusLog.Count > 0)
        {
            ApplicationStatusLog += _mqttWatcher.ApplicationStatusLog.Dequeue() + Environment.NewLine;
        }
    }

    /// <summary>
    /// This is so we can just hide the window if we're running in Notification Tray Icon app mode
    /// </summary>
    /// <param name="sender">The main window</param>
    /// <param name="e">Cancel Event Args from the event</param>
    public void OnWindowClosing(object? sender, CancelEventArgs e) 
    {
        _logger.Information("Running {ThisName}, this is just an example message to show how to use MVVM behaviors from the XAML",
            System.Reflection.MethodBase.GetCurrentMethod()?.Name);

        if (!ApplicationInformation.RunAsTrayIconApplication) return;
        
        e.Cancel = true;

        if (sender is null) return;
        
        ((Window)sender).Hide();
    }
}