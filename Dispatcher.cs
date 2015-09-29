using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Newtonsoft.Json;

using TicketMachine.LocalNetwork;
using TicketMachine.Devices;

namespace TicketMachine
{

    class Dispatcher
    {
        /// <summary>
        /// Logged
        /// </summary>
        public static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // инстанс для паттерна Singleton
        private static Dispatcher _instance;

        // Локальный сетевой сервер, для взаимодействия с игрой
        private LocalNetworkServer _localNetServer;

        // Сервер сетевой политики (CrossDomain)
        private SocketPolicyServer policyServer;

        // Ticket контроллер
        private ITicketController _ticketController;


        #region Constructor

        public Dispatcher()
        {
        }

        public static Dispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Dispatcher();
                }
                return _instance;
            }
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Инициализация аппаратной части
        /// </summary>
        public bool InitHardware(TICKETCONTROLLER_TYPE controllerType, string port)
        {
            if (controllerType == TICKETCONTROLLER_TYPE.VIRTUAL)
                _ticketController = new VirtualTicketController();
            else
                _ticketController = new TicketController();

            _ticketController.ButtonsStateChanged += _ticketController_ButtonsStateChanged;
            _ticketController.CoinReceived += _ticketController_CoinReceived;
            _ticketController.StatusChanged += _ticketController_StatusChanged;

            return _ticketController.Init(port);
        }

        /// <summary>
        /// Освобождение аппаратной части
        /// </summary>
        public void ShutdownHardware()
        {
            _ticketController.ButtonsStateChanged -= _ticketController_ButtonsStateChanged;
            _ticketController.CoinReceived -= _ticketController_CoinReceived;
            _ticketController.StatusChanged -= _ticketController_StatusChanged;
            _ticketController.Dispose();
        }

        /// <summary>
        /// Запуск сервера локальной сети
        /// </summary>
        public void InitLocalNetwork()
        {
            // Запуск сервера сетевой политики (cross-domain)
            policyServer = new SocketPolicyServer(LocalNetwork.SocketPolicyServer.AllPolicy);
            policyServer.Start();

            _localNetServer = new LocalNetworkServer();
            _localNetServer.Start();

            _localNetServer.NetDataReceivedEvent += _localNetServer_NetDataReceivedEvent;
        }

        /// <summary>
        /// Остановка сервера локальной сети
        /// </summary>
        public void ShutdownLocalNetwork()
        {
            policyServer.Stop();

            _localNetServer.NetDataReceivedEvent -= _localNetServer_NetDataReceivedEvent;
            _localNetServer.Stop();
        }

        public void ProcessKeyboard(ConsoleKeyInfo Key)
        {
            OnKeyboardKeyPressed(new KeyboardEventArgs(Key.Key));
        }

        #endregion


        #region Event's handlers

        //---------------------------------------------//
        // Обработка данных принятых из локальной сети //
        //---------------------------------------------//
        void _localNetServer_NetDataReceivedEvent(object sender, NetDataEventArgs e)
        {
#if DEBUG
            Logger.Debug("Received message: " + e.Data);
#endif
            Command command = JsonConvert.DeserializeObject<Command>(e.Data);
            
            if (command.Key == CommnadKey.GET_TICKET)
            {
                _ticketController.GetTickets(Convert.ToInt32(command.FirstParameter));
            }
            else if (command.Key == CommnadKey.SWITCH_LIGHT)
            {
                _ticketController.SwitchLight(Convert.ToInt32(command.FirstParameter), Convert.ToBoolean(command.SecondParameter));
            }
            else if (command.Key == CommnadKey.CHANGE_LIGHTING)
            {
                _ticketController.ChangePWM(Convert.ToInt32(command.FirstParameter), Convert.ToInt32(command.SecondParameter));
            }
            else if (command.Key == CommnadKey.ENABLE_COINVALIDATOR)
            {
                _ticketController.EnableCoinValidator();
            }
            else if (command.Key == CommnadKey.DISABLE_COINVALIDATOR)
            {
                _ticketController.DisableCoinValidator();
            }
        }

        //-----------------------------------------//
        // Обработка событий тикетного контроллера //
        //-----------------------------------------//
        void _ticketController_StatusChanged(object sender, TicketMachineStatusChangedEventArgs e)
        {
            ServerResponse resp = new ServerResponse();
            resp.Key = ResponseKey.TICKET_MACHINE_STATUS_CHANGED;
            resp.Data = JsonConvert.SerializeObject(e.Status, Formatting.Indented);
            _localNetServer.SendMessage(JsonConvert.SerializeObject(resp, Formatting.Indented));
        }

        //----------------------------------//
        // Обработка события приема монетки //
        //----------------------------------//
        void _ticketController_CoinReceived(object sender, CoinReceivedEventArgs e)
        {
            ServerResponse resp = new ServerResponse();
            resp.Key = ResponseKey.COIN_RECEIVED;
            resp.Data = JsonConvert.SerializeObject(e.Value, Formatting.Indented);
            _localNetServer.SendMessage(JsonConvert.SerializeObject(resp, Formatting.Indented));
        }

        //-----------------------------------//
        // Обработка нажатия кнопок аппарата //
        //-----------------------------------//
        void _ticketController_ButtonsStateChanged(object sender, ButtonsStateChangedEventArgs e)
        {
            ServerResponse resp = new ServerResponse();
            resp.Key = ResponseKey.BUTTONS_STATES_CHANGED;
            resp.Data = JsonConvert.SerializeObject(e.States, Formatting.Indented);
            _localNetServer.SendMessage(JsonConvert.SerializeObject(resp, Formatting.Indented));
        }

        #endregion


        #region Events

        /// <summary>
        /// Событие нажатия клавиши клавиатуры
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyboardKeyPressed;
        private void OnKeyboardKeyPressed(KeyboardEventArgs e)
        {
            if (KeyboardKeyPressed != null)
                KeyboardKeyPressed(this, e);
        }

        #endregion
    }

    /// <summary>
    /// Аргументы события клавиатуры
    /// </summary>
    public class KeyboardEventArgs : EventArgs
    {
        public ConsoleKey Key { get; set; }

        public KeyboardEventArgs (ConsoleKey key)
        {
            Key = key;
        }
    }
}
