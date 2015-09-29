using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketMachine.Devices
{
    interface ITicketController
    {
        #region Public methods

        /// <summary>
        /// Особождение ресурсов
        /// </summary>
        void Dispose();

        /// <summary>
        /// Инициализирует работу с ticket-контроллером на указанном порте
        /// </summary>
        /// <param name="port">Порт на который подключено устройство</param>
        /// <returns>Возвращает результат инициализации устройства</returns>
        bool Init(string port);

        /// <summary>
        /// Включить монетоприемник
        /// </summary>
        void EnableCoinValidator();

        /// <summary>
        /// Выключить монетоприемник
        /// </summary>
        void DisableCoinValidator();

        /// <summary>
        /// Перезагрузка контроллера
        /// </summary>
        void ResetController();

        /// <summary>
        /// Включить/выключить лампочку
        /// </summary>
        /// <param name="lightNumber">Номер лампочки</param>
        /// <param name="state">Состояние лампочки: true - включить</param>
        void SwitchLight(int lightNumber, bool state);

        /// <summary>
        /// Управление ШИМ
        /// </summary>
        /// <param name="chanel">Канал шим: 1 или 2</param>
        /// <param name="value">Напряжение на выходе</param>
        void ChangePWM(int chanel, int value);

        /// <summary>
        /// Выдать тикеты
        /// </summary>
        /// <param name="count">Количество тикетов</param>
        void GetTickets(int count);

        /// <summary>
        /// Версия прошивки контроллера
        /// </summary>
        void GetControllerVersion();

        #endregion

        #region Events

        /// <summary>
        /// Событие смены состояния контроллера
        /// </summary>
        event EventHandler<TicketMachineStatusChangedEventArgs> StatusChanged;

        /// <summary>
        /// Событие - изменилось состояние кнопок
        /// </summary>
        event EventHandler<ButtonsStateChangedEventArgs> ButtonsStateChanged;

        /// <summary>
        /// Событие - прием монеты
        /// </summary>
        event EventHandler<CoinReceivedEventArgs> CoinReceived;

        #endregion
    }
}
