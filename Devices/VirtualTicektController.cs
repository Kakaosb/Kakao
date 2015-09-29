using System;
using System.Collections.Generic;

namespace TicketMachine.Devices
{
    class VirtualTicketController : ITicketController, IDisposable
    {
        // Флаг: Освобожден ли уже объект
        private bool isDisposed = false;

        #region Constructors/Destructors

        public VirtualTicketController()
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
                Dispatcher.Instance.KeyboardKeyPressed -= Instance_KeyboardKeyPressed;
            }

            // Free any unmanaged objects here.
            //
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
            Dispatcher.Instance.KeyboardKeyPressed += Instance_KeyboardKeyPressed;

            OnTicketMachineStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.READY, "Виртуальный ticket-контроллер готов к работе"));

            return true;
        }

        /// <summary>
        /// Включить монетоприемник
        /// </summary>
        void ITicketController.EnableCoinValidator()
        {

        }

        /// <summary>
        /// Выключить монетоприемник
        /// </summary>
        void ITicketController.DisableCoinValidator()
        {

        }

        /// <summary>
        /// Перезагрузка контроллера
        /// </summary>
        void ITicketController.ResetController()
        {

        }

        /// <summary>
        /// Включить/выключить лампочку
        /// </summary>
        /// <param name="lightNumber">Номер лампочки</param>
        /// <param name="state">Состояние лампочки: true - включить</param>
        void ITicketController.SwitchLight(int lightNumber, bool state)
        {

        }

        /// <summary>
        /// Управление ШИМ
        /// </summary>
        /// <param name="chanel">Канал шим: 1 или 2</param>
        /// <param name="value">Напряжение на выходе</param>
        void ITicketController.ChangePWM(int chanel, int value)
        {

        }

        /// <summary>
        /// Выдать тикеты
        /// </summary>
        /// <param name="count">Количество тикетов</param>
        void ITicketController.GetTickets(int count)
        {
            System.Threading.Thread.Sleep(1000);
            OnTicketMachineStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.TICKETS_GIVED, "Выданно тикетов: " + count));
        }

        /// <summary>
        /// Версия прошивки контроллера
        /// </summary>
        void ITicketController.GetControllerVersion()
        {
            OnTicketMachineStatusChanged(new TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS.TRACE, "Виртуальный ticket-контроллер"));
        }

        #endregion

        #region Private methods

        //-------------------------------------//
        // Обработка нажатий клавиш клавиатуры //
        //-------------------------------------//
        void Instance_KeyboardKeyPressed(object sender, KeyboardEventArgs e)
        {
            bool[] states = new bool[7];
            switch (e.Key)
            {
                case ConsoleKey.D1:
                    states[0] = true;
                    states[1] = false;
                    states[2] = false;
                    states[3] = false;
                    states[4] = false;
                    states[5] = false;
                    states[6] = false;
                    OnButtonsStateChanged(new ButtonsStateChangedEventArgs(states));
                    break;
                case ConsoleKey.D2:
                    states[0] = false;
                    states[1] = true;
                    states[2] = false;
                    states[3] = false;
                    states[4] = false;
                    states[5] = false;
                    states[6] = false;
                    OnButtonsStateChanged(new ButtonsStateChangedEventArgs(states));
                    break;
                case ConsoleKey.D3:
                    states[0] = false;
                    states[1] = false;
                    states[2] = true;
                    states[3] = false;
                    states[4] = false;
                    states[5] = false;
                    states[6] = false;
                    OnButtonsStateChanged(new ButtonsStateChangedEventArgs(states));
                    break;
                case ConsoleKey.D4:
                    states[0] = false;
                    states[1] = false;
                    states[2] = false;
                    states[3] = true;
                    states[4] = false;
                    states[5] = false;
                    states[6] = false;
                    OnButtonsStateChanged(new ButtonsStateChangedEventArgs(states));
                    break;
                case ConsoleKey.D5:
                    states[0] = false;
                    states[1] = false;
                    states[2] = false;
                    states[3] = false;
                    states[4] = true;
                    states[5] = false;
                    states[6] = false;
                    OnButtonsStateChanged(new ButtonsStateChangedEventArgs(states));
                    break;
                case ConsoleKey.D6:
                    states[0] = false;
                    states[1] = false;
                    states[2] = false;
                    states[3] = false;
                    states[4] = false;
                    states[5] = true;
                    states[6] = false;
                    OnButtonsStateChanged(new ButtonsStateChangedEventArgs(states));
                    break;
                case ConsoleKey.D7:
                    states[0] = false;
                    states[1] = false;
                    states[2] = false;
                    states[3] = false;
                    states[4] = false;
                    states[5] = false;
                    states[6] = true;
                    OnButtonsStateChanged(new ButtonsStateChangedEventArgs(states));
                    break;
            }

            states[0] = false;
            states[1] = false;
            states[2] = false;
            states[3] = false;
            states[4] = false;
            states[5] = false;
            states[6] = false;
            OnButtonsStateChanged(new ButtonsStateChangedEventArgs(states));
        }

        #endregion

        #region Events

        /// <summary>
        /// Событие смены состояния контроллера
        /// </summary>
        public event EventHandler<TicketMachineStatusChangedEventArgs> StatusChanged;
        private void OnTicketMachineStatusChanged(TicketMachineStatusChangedEventArgs e)
        {
            if (StatusChanged != null)
                StatusChanged(this, e);
        }

        /// <summary>
        /// Событие - изменилось состояние кнопок
        /// </summary>
        public event EventHandler<ButtonsStateChangedEventArgs> ButtonsStateChanged;
        private void OnButtonsStateChanged(ButtonsStateChangedEventArgs e)
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
