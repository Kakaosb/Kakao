using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Timers;


namespace TicketMachine.Devices
{
    class TicketController : ITicketController, IDisposable
    {
        // Таймаут таймера опроса устройства
        private const int POLL_DEVICE_TIMEOUT = 20;

        // Максимальное количество ошибок CRC
        private const int MAX_CRC_ERROR_COUNT = 50;

        // Флаг: Освобожден ли уже объект
        private bool isDisposed = false;

        // Список команд для протокола
        private enum CONTROLLER_COMNADS
        {
            GET_VERSION = 0xB9,
            GET_TICKETS = 0xB8,
            CHANGE_PWM = 0xB7,
            CHANGE_COINVALIDATOR_STATUS = 0xB6,
            CHANGE_LIGHT_STATE = 0xB5,
            RESET_COINS = 0xB6,
            POLL = 0xB0,
            RESET = 0x00
        }

        // Последовательный порт RS-232
        private SerialPort _comPort;

        // Данные, поступившие из порта
        private byte[] _responseData;

        // Команда для отправки контроллеру
        private CONTROLLER_COMNADS _commandToSend;

        // Первый параметр передаваемый вместе с контроллером
        private byte _firstParamToSend;

        // Второй параметр передаваемый вместе с контроллером
        private byte _secondParamToSend;

        // Таймер опроса устройства
        private Timer _pollDeviceTimer;

        // Счетчик устройство не отвечает
        private int _iDeviceNotResponse = 0;

        // Счетчик ошибок CRC
        private int _iCRCErrorCounter = 0;

        // Счетчик монет
        private int _iAccumulatedCoin = 0;

        // Предыдущее количество выданных тикетов
        private int _lastGivingTicketsCount = 0;

        // Последнее состояние кнопок
        private byte _lastButtonsState = 0x00;

        // Флаг: установил клиент комманду
        private bool _isClientSetCommand = false;

        // Флаг: запрошена ли версия прошивки контроллера
        private bool _isControllerVersionRequested = false;


        #region Constructors/Destructors

        /// <summary>
        /// Конструктор
        /// </summary>
        public TicketController()
        {
        }


        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        /// <param name="disposing">Освободить управляемые ресурсы</param>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.

            if (_comPort.IsOpen)
                _comPort.Close();

            isDisposed = true;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Инициализирует работу с призовой плате на указанном порте
        /// </summary>
        /// <param name="port">Порт на который подключено устройство</param>
        /// <returns>Возвращает результат инициализации устройства</returns>
        bool ITicketController.Init(string port)
        {
            bool result = false;

            // инициализация последовательного порта RS-232
            _comPort = new SerialPort();
            _comPort.PortName = port;
            _comPort.BaudRate = 115200;
            _comPort.DataBits = 8;
            _comPort.StopBits = StopBits.One;
            _comPort.Parity = Parity.None;
            _comPort.DataReceived += _comPort_DataReceived;

            _responseData = new byte[8];

            try
            {
                _comPort.Open();
                result = true;

                _pollDeviceTimer = new Timer(POLL_DEVICE_TIMEOUT);
                _pollDeviceTimer.Elapsed += _pollDeviceTimer_Elapsed;
                _pollDeviceTimer.Start();

                _commandToSend = CONTROLLER_COMNADS.POLL;
            }
            catch (UnauthorizedAccessException eX)
            {
                OnStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.INIT, "Не удается открыть порт " + port + " " + eX.Message));
                Dispatcher.Logger.Error("Не удается открыть порт " + port + " " + eX.Message);
            }
            catch (ArgumentOutOfRangeException eX)
            {
                OnStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.INIT, "Не удается открыть порт " + port + " " + eX.Message));
                Dispatcher.Logger.Error("Не удается открыть порт " + port + " " + eX.Message);
            }
            catch (ArithmeticException eX)
            {
                OnStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.INIT, "Не удается открыть порт " + port + " " + eX.Message));
                Dispatcher.Logger.Error("Не удается открыть порт " + port + " " + eX.Message);
            }
            catch (InvalidOperationException eX)
            {
                OnStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.INIT, "Не удается открыть порт " + port + " " + eX.Message));
                Dispatcher.Logger.Error("Не удается открыть порт " + port + " " + eX.Message);
            }

            return result;
        }

        /// <summary>
        /// Включить монетоприемник
        /// </summary>
        void ITicketController.EnableCoinValidator()
        {
            _isClientSetCommand = true;

            _commandToSend = CONTROLLER_COMNADS.CHANGE_COINVALIDATOR_STATUS;
            _firstParamToSend = 0x00;
            _secondParamToSend = 0x01;

#if DEBUG
            Dispatcher.Logger.Debug("Включить прием монет");
#endif
        }

        /// <summary>
        /// Выключить монетоприемник
        /// </summary>
        void ITicketController.DisableCoinValidator()
        {
            _isClientSetCommand = true;

            _commandToSend = CONTROLLER_COMNADS.CHANGE_COINVALIDATOR_STATUS;
            _firstParamToSend = 0x00;
            _secondParamToSend = 0x00;

#if DEBUG
            Dispatcher.Logger.Debug("Выключить прием монет");
#endif
        }

        /// <summary>
        /// Перезагрузка контроллера
        /// </summary>
        void ITicketController.ResetController()
        {
            _isClientSetCommand = true;

            _commandToSend = CONTROLLER_COMNADS.RESET;
            _firstParamToSend = 0x00;
            _secondParamToSend = 0x00;
        }

        /// <summary>
        /// Включить/выключить лампочку
        /// </summary>
        /// <param name="lightNumber">Номер лампочки</param>
        /// <param name="state">Состояние лампочки: true - включить</param>
        void ITicketController.SwitchLight(int lightNumber, bool state)
        {
            _isClientSetCommand = true;

            _commandToSend = CONTROLLER_COMNADS.CHANGE_LIGHT_STATE;

#warning КОСТЫЛЬ ДЛЯ СТАРОЙ ПРОШИВКИ
            _firstParamToSend = Convert.ToByte(lightNumber+1);
            if (state)
                _secondParamToSend = 0x01;
            else
                _secondParamToSend = 0x00;
        }

        /// <summary>
        /// Управление ШИМ
        /// </summary>
        /// <param name="chanel">Канал шим: 1 или 2</param>
        /// <param name="value">Напряжение на выходе</param>
        void ITicketController.ChangePWM(int chanel, int value)
        {
            _isClientSetCommand = true;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Выдать тикеты
        /// </summary>
        /// <param name="count">Количество тикетов</param>
        void ITicketController.GetTickets(int count)
        {
#if DEBUG
            Dispatcher.Logger.Debug("Выдать тикеты " + count + " шт");
#endif
            _isClientSetCommand = true;

            Int16 tmp = Convert.ToInt16(count);
            byte[] buff = BitConverter.GetBytes(tmp);
            _commandToSend = CONTROLLER_COMNADS.GET_TICKETS;
            _firstParamToSend = buff[0];
            _secondParamToSend = buff[1];
        }

        /// <summary>
        /// Версия прошивки контроллера
        /// </summary>
        void ITicketController.GetControllerVersion()
        {
            _isClientSetCommand = true;
            _isControllerVersionRequested = true;

            _commandToSend = CONTROLLER_COMNADS.GET_VERSION;
            _firstParamToSend = 0x00;
            _secondParamToSend = 0x00;
        }

        #endregion

        #region Private methods

        //-----------------------------//
        // Отправка команды устройству //
        //-----------------------------//
        private void _sendCommand()
        {
            byte[] commandBuffer = { (byte)_commandToSend, _firstParamToSend, _secondParamToSend };
            byte[] crc = CRC16.ComputeCRC16(commandBuffer);
            byte[] mes = CRC16.CRC16Pack(commandBuffer, crc);
            _comPort.Write(mes, 0, mes.Length);

            _isClientSetCommand = false;
        }

        //------------------------------//
        // Обработка поступивших данных //
        //------------------------------//
        void _processReceivedData()
        {
            // Разбор версии прошивки
            if (_isControllerVersionRequested)
            {
                _isControllerVersionRequested = false;
                Dispatcher.Logger.Trace("Версия прошивки: " + System.Text.ASCIIEncoding.ASCII.GetString(_responseData));
                OnStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.TRACE, "Версия прошивки: " + System.Text.ASCIIEncoding.ASCII.GetString(_responseData)));

                _commandToSend = CONTROLLER_COMNADS.POLL;
                _firstParamToSend = 0x00;
                _secondParamToSend = 0x00;

                _pollDeviceTimer.Start();
                return;
            }

            BitArray bits = new BitArray(_responseData);

            // Разбор кнопок
            if (_responseData[4] != _lastButtonsState)
            {
                _lastButtonsState = _responseData[4];
                List<bool> buff = new List<bool>();

                for (int i = 32; i <= 39; i++)
                {
                    buff.Add(bits[i]);
                }
                OnButtonStateChanged(new ButtonsStateChangedEventArgs(buff.ToArray()));
            }

            // Принята монетка
            if (_responseData[1] != 0x00)
            {
                _iAccumulatedCoin = Convert.ToInt32(_responseData[1]);
                if (!_isClientSetCommand)
                {
                    _commandToSend = CONTROLLER_COMNADS.RESET_COINS;
                    _firstParamToSend = 0x00;
                    _secondParamToSend = 0x02;
                }
            }
            // Контроллер ждет команду
            else if (_responseData[5] == 0x07)
            {
                // Счетчик монетоприемника сброшен
                if (_responseData[1] == 0 && _iAccumulatedCoin != 0)
                {
                    Dispatcher.Logger.Trace("Приняты монеты: " + _iAccumulatedCoin);
                    OnCoinReceived(new CoinReceivedEventArgs(_iAccumulatedCoin));
                    _iAccumulatedCoin = 0;
                }

                if (!_isClientSetCommand)
                {
                    _commandToSend = CONTROLLER_COMNADS.POLL;
                    _firstParamToSend = 0x00;
                    _secondParamToSend = 0x00;
                }
            }
            // Монетоприемник выключен
            else if (_responseData[5] == 0x78)
            {
                Dispatcher.Logger.Trace("Монетоприемник выключен");
                if (!_isClientSetCommand)
                {
                    _commandToSend = CONTROLLER_COMNADS.POLL;
                    _firstParamToSend = 0x01;
                    _secondParamToSend = 0x00;
                }
            }
            // Монетоприемник включен
            else if (_responseData[5] == 0x79)
            {
                Dispatcher.Logger.Trace("Монетоприемник включен");
                if (!_isClientSetCommand)
                {
                    _commandToSend = CONTROLLER_COMNADS.POLL;
                    _firstParamToSend = 0x01;
                    _secondParamToSend = 0x00;
                }
            }
            // Выдаются тикеты
            else if (_responseData[5] == 0x7A)
            {
                byte[] tmp = new byte[2];
                tmp[0] = _responseData[2];
                tmp[1] = _responseData[3];
                OnStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.TICKETS_GIVING, BitConverter.ToInt16(tmp, 0).ToString()));
                _commandToSend = CONTROLLER_COMNADS.POLL;
                _firstParamToSend = 0x00;
                _secondParamToSend = 0x00;
                Dispatcher.Logger.Trace("Тикеты выдаются, осталось выдать " + BitConverter.ToInt16(tmp, 0) + " тикетов");
            }
            // Тикеты выданы
            else if (_responseData[5] == 0x7B)
            {
                OnStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.TICKETS_GIVED, "Тикеты выданы"));
                _commandToSend = CONTROLLER_COMNADS.POLL;
                _firstParamToSend = 0x01;
                _secondParamToSend = 0x00;
                Dispatcher.Logger.Trace("Тикеты выданы");
            }
            // Тикеты закончились
            else if (_responseData[5] == 0x7C)
            {
                byte[] tmp = new byte[2];
                tmp[0] = _responseData[2];
                tmp[1] = _responseData[3];
                if (_lastGivingTicketsCount != BitConverter.ToInt16(tmp, 0))
                {
                    _lastGivingTicketsCount = BitConverter.ToInt16(tmp, 0);
                    OnStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.TICKETS_OUTOFRANGE, _lastGivingTicketsCount.ToString()));
                    _commandToSend = CONTROLLER_COMNADS.POLL;
                    _firstParamToSend = 0x01;
                    _secondParamToSend = 0x00;
                    Dispatcher.Logger.Trace("Тикеты закончились, осталось выдать " + _lastGivingTicketsCount + " тикетов");
                }
            }

            _pollDeviceTimer.Start();
        }

        #endregion

        #region Event handlers

        //---------------------------------------------------------------//
        // Обработчик события поступления данных в последовательный порт //
        //---------------------------------------------------------------//
        void _comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_comPort.BytesToRead <= 0)
                return;

            _comPort.Read(_responseData, 0, _responseData.Length);

            byte[] received_data = new byte[6];
            byte[] received_crc = new byte[2];
            Array.Copy(_responseData, received_data, 6);
            Array.Copy(_responseData, 6, received_crc, 0, 2);
            byte[] computed_crc = CRC16.ComputeCRC16(received_data);

            if (received_crc[0] != computed_crc[0] || received_crc[1] != computed_crc[1])
            {
                _iCRCErrorCounter++;
                if (_iCRCErrorCounter > MAX_CRC_ERROR_COUNT)
                {
                    _pollDeviceTimer.Stop();
                    OnStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.ERROR, "Некоректный CRC"));
                    Dispatcher.Logger.Error("Некоректный CRC");
                }
            }
            else
            {
                _iCRCErrorCounter = 0;
                _iDeviceNotResponse = 0;
                _pollDeviceTimer.Stop();
                _processReceivedData();
            }
        }

        //---------------------------------------//
        // Обработчик таймера - опрос устройства //
        //---------------------------------------//
        void _pollDeviceTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _iDeviceNotResponse++;
            if (_iDeviceNotResponse > 10)
            {
                OnStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.ERROR, "Устрйоство не отвечает"));
                Dispatcher.Logger.Error("Устрйоство не отвечает");
            }
            else
                _sendCommand();
        }

        #endregion

        #region Events

        /// <summary>
        /// Событие смены состояния контроллера
        /// </summary>
        public event EventHandler<TicketMachineStatusChangedEventArgs> StatusChanged;
        private void OnStatusChanged(TicketMachineStatusChangedEventArgs e)
        {
            if (StatusChanged != null)
                StatusChanged(this, e);
        }

        /// <summary>
        /// Событие - изменилось состояние кнопок
        /// </summary>
        public event EventHandler<ButtonsStateChangedEventArgs> ButtonsStateChanged;
        private void OnButtonStateChanged(ButtonsStateChangedEventArgs e)
        {
            if (ButtonsStateChanged != null)
                ButtonsStateChanged(this, e);
        }

        /// <summary>
        /// Событие - прием монеты
        /// </summary>
        public event EventHandler<CoinReceivedEventArgs> CoinReceived;
        private void OnCoinReceived(CoinReceivedEventArgs e)
        {
            if (CoinReceived != null)
                CoinReceived(this, e);
        }

        #endregion
    }
}
