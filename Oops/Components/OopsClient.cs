using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Oops.DataModels;

namespace Oops.Components
{

    public class OopsClient
    {
        MqttFactory factory;
        MqttClient mqttClient;
        string topic;

        public async Task Start(string host, string topic)
        {
            factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient() as MqttClient;
            this.topic = topic ?? "Undefined";

            var options = new MqttClientOptionsBuilder()
                .WithClientId(Environment.MachineName + "." + topic)
                .WithTcpServer(host)
                .Build();

            //重連
            mqttClient.UseDisconnectedHandler(async e =>
            {
            //Console.WriteLine("### disconnected from server ###");
            await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None);
                }
                catch
                {
                //Console.WriteLine("### reconnecting failed ###");
            }
            });

            //開始連線
            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None);
                // LogManager.GetLogger(typeof(OopsClient)).InfoFormat("連線 {0}/{1}",
                //     host, topic);
            }
            catch
            {
                // LogManager.GetLogger(typeof(OopsClient)).WarnFormat(
                //     "連線失敗 {0}/{1}", host, topic);
            }
        }

        public void Push(string pushMessage)
        {
            if (mqttClient != null)
            {
                try
                {
                    Error error = new Error();
                    error.HostName = "測試";
                    error.Message = pushMessage;
                    mqttClient.PublishAsync(this.topic, System.Text.Json.JsonSerializer.Serialize(error));
                }
                catch { }
            }
        }

        public void Stop()
        {
            if (mqttClient == null)
            {
                
                return;
            }
            mqttClient.Dispose();
        }
    }
}