using NLog;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cim.Domain.Transfer
{
    public interface ITransfer
    {
        bool Transfer(string message, string routingKey);
    }

    public class DebugTransfer : ITransfer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public bool Transfer(string message, string routingKey)
        {
            logger.Debug($"message={message}, routingKey={routingKey}");
            return true;
        }
    }

    public class MqTransfer : ITransfer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region 초기화
        //<mq userName = "rabbituser" password="1234lge" virtualHost="/" hostname="127.0.0.1" hostnames="127.0.0.1" port="5672" exchange="mainMirror" />
        public bool IsOpen => connection != null && connection.IsOpen;

        public string UserName
        {
            get => factory.UserName;
            set => factory.UserName = value;
        }
        public string Password
        {
            get => factory.Password;
            set => factory.Password = value;
        }
        public string VirtualHost
        {
            get => factory.VirtualHost;
            set => factory.VirtualHost = value;
        }
        public string HostName
        {
            get => factory.HostName;
            set => factory.HostName = value;
        }
        public List<string> HostNames { get; set; }
        public int Port
        {
            get => factory.Port;
            set => factory.Port = value;
        }

        public string Exchange { get; set; } = "mainMirror";//Globals.Settings.MessageServer.MQ.Exchange

        private readonly ConnectionFactory factory;
        private bool IsCancel;
        private IModel channel;
        private IConnection connection;
        private IBasicProperties basicProperties;

        public MqTransfer()
        {
            factory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = false,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(1),
                RequestedConnectionTimeout = TimeSpan.FromMilliseconds(5000),
            };
        } 

        #endregion

        public void Dispose()
        {
            IsCancel = true;
            try
            {
                if (channel != null)
                {
                    channel.Dispose();
                    channel = null;
                }

                if (connection != null)
                {
                    try
                    {
                        connection.Dispose();
                    }
                    catch (Exception ex) 
                    {
                        logger.Error($"ex={ex}");
                    }
                    connection = null;
                }
            }
            catch(Exception ex)
            {
                logger.Error($"ex={ex}");
            }
        }

        /// <summary>
        /// message를 MQ에 전송
        /// </summary>
        /// <param name="message"></param>
        /// <param name="routingKey">바인딩 키 룰 : {설비id}.{ValueType}.{수집항목종류}</param>
        public bool Transfer(string message, string routingKey)
        {
            bool result = false;
            try
            {
                CheckConnect();

                byte[] body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(Exchange, routingKey, basicProperties, body);
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
            return result;
        }

        private void Connect()
        {
            if (IsOpen)
            {
                return;
            }

            try
            {
                logger.Debug("Start Connect");

                if (channel != null && channel.IsOpen)
                {
                    channel.Close();
                }

                if (connection != null && connection.IsOpen)
                {
                    connection.Close();
                }

                if (HostNames.Count > 1)
                {
                    connection = factory.CreateConnection(HostNames);
                }
                else
                {
                    connection = factory.CreateConnection();
                }

                channel = connection.CreateModel();

                basicProperties = channel.CreateBasicProperties();
                basicProperties.ContentType = "text/plain";
                basicProperties.DeliveryMode = 2; // rabbit 재시작해도 메시지 보존되도록 하기위한 설정
            }
            catch (Exception ex)
            {
                logger.Error($"ex={ex}");
            }
            logger.Debug("End Connect");
        }

        private void CheckConnect()
        {
            if (connection?.IsOpen != true || channel?.IsOpen != true)
            {
                while (true) // 재접속 코드 부분
                {
                    if (IsCancel)
                    {
                        return;
                    }

                    Connect();

                    if (channel.IsOpen)
                    {
                        break;
                    }

                    Thread.Sleep(100);
                }
            }
        }

    }
}
