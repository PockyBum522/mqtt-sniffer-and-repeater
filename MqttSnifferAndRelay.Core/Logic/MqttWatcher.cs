using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MQTTnet;
using MQTTnet.Client;
using MqttSnifferAndRelay.Core.Models;

namespace MqttSnifferAndRelay.Core.Logic;

public class MqttWatcher 
{
    private readonly Dispatcher _uiThreadDispatcher;

    string DisplayBoardOutTopic => "allenst/kit/rotovap-controller/peripheral/out/raw/displayboard/rawpackets";
    string DisplayBoardInTopic => "allenst/kit/rotovap-controller/peripheral/in/displayboard/rawpackets";    
    
    string MotorBoardOutTopic => "allenst/kit/rotovap-controller/peripheral/out/raw/motorboard/rawpackets";
    string MotorBoardInTopic => "allenst/kit/rotovap-controller/peripheral/in/motorboard/rawpackets";
    
    string DebugTopic => "allenst/kit/rotovap-controller/peripheral/out/debug";
    
    public Queue<string> DisplayBoardOutMessages = new();
    public Queue<string> MotorBoardOutMessages = new();
    public Queue<string> DisplayBoardInMessages = new();
    public Queue<string> MotorBoardInMessages = new();
    public Queue<string> DebugTopicMessages = new();
    public Queue<string> EverythingCombinedMessages = new();
    public Queue<string> ApplicationStatusLog = new();

    public bool RealTimeUpdatesEnabled = true;
    public bool RepeatFromMotorBoardToDisplayBoard;
    public bool RepeatFromDisplayBoardToMotorBoard;
    
    private IMqttClient _mqttClient;

    public MqttWatcher(Dispatcher uiThreadDispatcher)
    {
        _uiThreadDispatcher = uiThreadDispatcher;
    }
    
    public async Task StartMqttListener()
    {
        var mqttFactory = new MqttFactory();

        _mqttClient = mqttFactory.CreateMqttClient();
        
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithClientId("cs4ha_client")
            .WithTcpServer("192.168.1.25", 1883)
            .WithCredentials(SECRETS.MQTT_USERNAME, SECRETS.MQTT_PASSWORD)
            .Build();

        _mqttClient.DisconnectedAsync += async e =>
        {
            if (e.ClientWasConnected)
            {
                // Use the current options as the new options.
                await _mqttClient.ConnectAsync(_mqttClient.Options);
            }
        };
        
        // Setup message handling before connecting 
        _mqttClient.ApplicationMessageReceivedAsync += HandleIncomingMessage;

        await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var mqttSubscribeOptions = 
            mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => { f.WithTopic(DisplayBoardOutTopic); })
                .WithTopicFilter(f => { f.WithTopic(DisplayBoardInTopic); })
                .WithTopicFilter(f => { f.WithTopic(MotorBoardOutTopic); })
                .WithTopicFilter(f => { f.WithTopic(MotorBoardInTopic); })
                .WithTopicFilter(f => { f.WithTopic(DebugTopic); })
                .Build();

        await _mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
        
        // Pause forever to wait for incoming messages
        while (true){ await Task.Delay(9999); }
     
        // ReSharper disable once FunctionNeverReturns because it's not supposed to
    }

    public async Task PublishAsync(string topic, string payload, bool retainFlag = false, int qos = 2) => 
        await _mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
            .WithRetainFlag(retainFlag)
            .Build());
    
    private Task HandleIncomingMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        var timeStamp = DateTimeOffset.Now.ToString("HH:mm:ss.ff");

        var rawPayload = e.ApplicationMessage.Payload;
        var asciiPayload = System.Text.Encoding.ASCII.GetString(rawPayload);

        var topic = e.ApplicationMessage.Topic;

        _uiThreadDispatcher.Invoke(() =>
        {
            ApplicationStatusLog.Enqueue($"New MQTT message on topic: [{topic}] with payload: [{asciiPayload}]");
        });

        if (!RealTimeUpdatesEnabled) return Task.CompletedTask;
        
        if (topic.Equals(DebugTopic, StringComparison.InvariantCultureIgnoreCase))
        {
            _uiThreadDispatcher.Invoke(() =>
            {
                DebugTopicMessages.Enqueue($"{timeStamp}: [{asciiPayload}]");
            }); 
        }
        
        if (topic.Equals(DisplayBoardOutTopic, StringComparison.InvariantCultureIgnoreCase))
        {
            _uiThreadDispatcher.Invoke(() =>
            {
                DisplayBoardOutMessages.Enqueue($"{timeStamp}: [{asciiPayload}]");
                EverythingCombinedMessages.Enqueue($"{timeStamp} [display/out]: [{asciiPayload}]");
                
                if (RepeatFromDisplayBoardToMotorBoard)
                    PublishAsync(MotorBoardInTopic, asciiPayload);
            }); 
        }
        
        if (topic.Equals(DisplayBoardInTopic, StringComparison.InvariantCultureIgnoreCase))
        {
            _uiThreadDispatcher.Invoke(() =>
            {
                DisplayBoardInMessages.Enqueue($"{timeStamp}: [{asciiPayload}]");
                EverythingCombinedMessages.Enqueue($"{timeStamp} [display/in]: [{asciiPayload}]");
            }); 
        }
        
        if (topic.Equals(MotorBoardOutTopic, StringComparison.InvariantCultureIgnoreCase))
        {
            _uiThreadDispatcher.Invoke(() =>
            {
                MotorBoardOutMessages.Enqueue($"{timeStamp}: [{asciiPayload}]");
                EverythingCombinedMessages.Enqueue($"{timeStamp} [motor/out]: [{asciiPayload}]");

                if (RepeatFromMotorBoardToDisplayBoard)
                    PublishAsync(DisplayBoardInTopic, asciiPayload);
            }); 
        }
        
        if (topic.Equals(MotorBoardInTopic, StringComparison.InvariantCultureIgnoreCase))
        {
            _uiThreadDispatcher.Invoke(() =>
            {
                MotorBoardInMessages.Enqueue($"{timeStamp}: [{asciiPayload}]");
                EverythingCombinedMessages.Enqueue($"{timeStamp} [motor/in]: [{asciiPayload}]");
            }); 
        }

        return Task.CompletedTask;
    }
}