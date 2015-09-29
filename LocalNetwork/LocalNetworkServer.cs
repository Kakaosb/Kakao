using System;
using System.Collections.Generic;
using Lidgren.Network;
using System.Threading;

namespace TicketMachine.LocalNetwork
{
    class LocalNetworkServer
    {
        // Сервер
        private NetServer _server;

        // Конфигурация сервера
        private NetPeerConfiguration _serverConfig;

        // Принятые данные
        private NetIncomingMessage _receivedData;

        /// <summary>
        /// Конструктор
        /// </summary>
        public LocalNetworkServer()
        {
            // needed for RegisterReceivedCallback, TODO replace with AsyncOperationM...
            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            _serverConfig = new NetPeerConfiguration("AdrenalineGame");
            _serverConfig.Port = 45045;
            _serverConfig.MaximumConnections = 10;
            _serverConfig.ConnectionTimeout = 10;
            _serverConfig.PingInterval = 3;
        }

        /// <summary>
        /// Старт сервера
        /// </summary>
        public void Start()
        {
            _server = new NetServer(_serverConfig);
            _server.RegisterReceivedCallback(new System.Threading.SendOrPostCallback(ReceiveDataCallback));
            _server.Start();
        }

        /// <summary>
        /// Остановка сервера
        /// </summary>
        public void Stop()
        {
            if (_server != null)
                _server.Shutdown("Shutdown Server");
        }

        /// <summary>
        /// Отправка данных клиенту
        /// </summary>
        /// <param name="data"></param>
        public void SendMessage(string data)
        {
            NetOutgoingMessage message = _server.CreateMessage();
            message.Write(data);
            _server.SendMessage(message, _server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
        }

        //--------------------------//
        // Обработка приняты данных //
        //--------------------------//
        private void ReceiveDataCallback(object connection)
        {
            _receivedData = ((NetServer)connection).ReadMessage();
            if (_receivedData == null)
                return;

            switch(_receivedData.MessageType)
            {
                    // Состояние клиента изменилось
                case NetIncomingMessageType.StatusChanged:
                    {
                        NetConnectionStatus status = (NetConnectionStatus)_receivedData.ReadByte();
                        string reason = _receivedData.ReadString();

                        if (status == NetConnectionStatus.Connected)
                            Dispatcher.Logger.Trace("Подключился клиент IP: " + _receivedData.SenderEndpoint.Address + " Port: " + _receivedData.SenderEndpoint.Port);
                        else if (status == NetConnectionStatus.Disconnected)
                            Dispatcher.Logger.Trace("Отключился клиент IP: " + _receivedData.SenderEndpoint.Address + " Port: " + _receivedData.SenderEndpoint.Port);
                        else if (status == NetConnectionStatus.RespondedConnect)
                            Dispatcher.Logger.Trace("Подключается клиент IP: " + _receivedData.SenderEndpoint.Address + " Port: " + _receivedData.SenderEndpoint.Port);
                        else
                            Dispatcher.Logger.Trace("Клиент IP " + _receivedData.SenderEndpoint.Address + " изменил свое состояние на: " + status + " (" + reason + ")");
                    }
                    break;
                    // Приняты данные от клиента
                case NetIncomingMessageType.Data:
                    {
                        string data = _receivedData.ReadString();
                        OnNetDataReceived(new NetDataEventArgs(data));
                    }
                    break;
                    // Обработка сетевых ошибок
                case NetIncomingMessageType.ErrorMessage:
                    Dispatcher.Logger.Error(_receivedData.ReadString());
                    break;
                    // Обработка отладочной информации
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.VerboseDebugMessage:
                    Dispatcher.Logger.Debug(_receivedData.ReadString());
                    break;
                    // Обработка предупреждений
                case NetIncomingMessageType.WarningMessage:
                    Dispatcher.Logger.Warn(_receivedData.ReadString());
                    break;
                default:
                    break;
            }
        }

        #region Events

        /// <summary>
        /// Событие принятия сообщения из сети
        /// </summary>
        public event EventHandler<NetDataEventArgs> NetDataReceivedEvent;
        private void OnNetDataReceived(NetDataEventArgs e)
        {
            EventHandler<NetDataEventArgs> handler = NetDataReceivedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
    }

    /// <summary>
    /// Аргументы события получения данных
    /// </summary>
    public class NetDataEventArgs : EventArgs
    {
        public String Data { get; set; }
        public DateTime TimeStamp { get; set; }

        public NetDataEventArgs(string data)
        {
            Data = data; TimeStamp = DateTime.Now;
        }
    }
}
