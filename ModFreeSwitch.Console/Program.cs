using System.Threading;
using System.Threading.Tasks;
using ModFreeSwitch.Commands;
using ModFreeSwitch.Common;
using ModFreeSwitch.Events;
using ModFreeSwitch.Handlers.inbound;
using ModFreeSwitch.Handlers.outbound;
using NLog;
using NLog.Config;
using System;

namespace ModFreeSwitch.Console
{
    internal class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            const string address = "10.0.75.2";
            const string password = "ClueCon";
            const int port = 8021;
            const int ServerPort = 10000;
            try
            {
                //LogManager.Configuration = new XmlLoggingConfiguration(System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\NLog.config");
                var client = new OutboundSession(address,
                    port,
                    password);
                client.ConnectAsync().ConfigureAwait(false);

                Thread.Sleep(1000);

                _logger.Info("Connected and Authenticated {0}",
                    client.CanSend());
                var @event = "plain CHANNEL_HANGUP CHANNEL_HANGP_COMPLETE";
                var subscribed = client.SubscribeAsync(@event).ConfigureAwait(true);

                System.Console.WriteLine("Api Response {0}",
                    subscribed.GetAwaiter().GetResult());

                _logger.Warn("Api Response {0}",
                    subscribed.GetAwaiter().GetResult());

                var commandString = "sofia profile external gwlist up";
                var response = client.SendApiAsync(new ApiCommand(commandString)).ConfigureAwait(false);
                System.Console.WriteLine("hello test");
                _logger.Warn("Api Response {0}",
                    response.GetAwaiter().GetResult().ReplyText);

                var inboundServer = new InboundServer(ServerPort,
                    new DefaultInboundSession());
                inboundServer.StartAsync().Wait(500);
                var callCommand = "{ignore_early_media=false,originate_timeout=120}sofia/gateway/smsghlocalsip/233247063817 &socket(192.168.74.1:10000 async full)";

                client.SendBgApiAsync(new BgApiCommand("originate",
                    callCommand)).Wait(500);

                System.Console.ReadKey();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                System.Console.ReadLine();
                return ;
            }
        }
    }

    public class DefaultInboundSession : InboundSession
    {
        private const string AudioFile = "https://s3.amazonaws.com/plivocloud/Trumpet.mp3";
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        protected override Task HandleEvents(EslEvent @event,
            EslEventType eventType)
        {
            _logger.Debug(@event);
            return Task.CompletedTask;
        }

        protected override Task PreHandleAsync() { return Task.CompletedTask; }

        protected override async Task HandleAsync() { await PlayAsync(AudioFile); }
    }
}