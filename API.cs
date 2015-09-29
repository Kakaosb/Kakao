using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketMachine
{
    #region Enums

    public enum TICKETMACHINE_STATUS
    {
        READY,
        TRACE,
        WARNING,
        ERROR,
        INIT,
        TICKETS_GIVING,
        TICKETS_GIVED,
        TICKETS_OUTOFRANGE
    }

    /// <summary>
    /// Перечисление типов контроллеров
    /// </summary>
    public enum TICKETCONTROLLER_TYPE
    {
        CONTROLLER,
        VIRTUAL
    }

    #endregion

    #region Event Arguments

    /// <summary>
    /// Аргументы события состояния кнопок
    /// </summary>
    public class ButtonsStateChangedEventArgs : EventArgs
    {
        public ButtonsState States { get; set; }

        public ButtonsStateChangedEventArgs(bool[] states)
        {
            States = new ButtonsState(states);
        }
    }

    /// <summary>
    /// Аргументы события состояния призового контроллера
    /// </summary>
    public class TicketMachineStatusChangedEventArgs : EventArgs
    {
        public TicketMachineStatus Status { get; set; }

        public TicketMachineStatusChangedEventArgs(TICKETMACHINE_STATUS status, string message)
        {
            Status = new TicketMachineStatus(status, message);
        }
    }

    /// <summary>
    /// Аргументы события приема монетки
    /// </summary>
    public class CoinReceivedEventArgs : EventArgs
    {
        public CoinsReceived Value { get; set; }

        public CoinReceivedEventArgs(int value)
        {
            Value = new CoinsReceived(value);
        }
    }

    #endregion

    /// <summary>
    /// Состояния кнопок
    /// </summary>
    public class ButtonsState
    {
        public bool[] States { get; set; }

        public ButtonsState(bool[] states)
        {
            States = states;
        }
    }

    /// <summary>
    /// Состояние контроллера
    /// </summary>
    public class TicketMachineStatus
    {
        public TICKETMACHINE_STATUS Status { get; set; }
        public string Message { get; set; }

        public TicketMachineStatus(TICKETMACHINE_STATUS status, string message)
        {
            Status = status;
            Message = message;
        }
    }

    /// <summary>
    /// Принятые монетки
    /// </summary>
    public class CoinsReceived
    {
        public int Value { get; set; }
        public CoinsReceived(int value)
        {
            Value = value;
        }
    }

    #region API

    // Команды для сервера
    public enum CommnadKey
    {
        GET_TICKET,
        SWITCH_LIGHT,
        CHANGE_LIGHTING,
        ENABLE_COINVALIDATOR,
        DISABLE_COINVALIDATOR
    }

    // Ответы для клиента
    public enum ResponseKey
    {
        TICKET_MACHINE_STATUS_CHANGED,
        BUTTONS_STATES_CHANGED,
        COIN_RECEIVED
    }

    // Команда для аппаратной части
    public class Command
    {
        public CommnadKey Key { get; set; }
        public string FirstParameter { get; set; }
        public string SecondParameter { get; set; }
    }

    // Ответ клиенту
    public class ServerResponse
    {
        public ResponseKey Key { get; set; }
        public string Data { get; set; }
    }

    #endregion
}
