using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketMachine
{
    class Program
    {
        static void Main(string[] args)
        {
            TICKETCONTROLLER_TYPE _ticketControllerType;
            string _comPortName;

            Dispatcher.Logger.Trace("Adrenaline Dispatcher started");

            Dispatcher.Instance.InitLocalNetwork();

            // Обработка параметров командной строки
            if (args.Length > 0)
            {
                try
                {
                    _ticketControllerType = (TICKETCONTROLLER_TYPE)Enum.Parse(typeof(TICKETCONTROLLER_TYPE), args[0]);
                }
                catch (Exception eX)
                {
                    Dispatcher.Logger.Error("Ошибка параметра \"Тип контроллера\": " + eX.Message);
                    _ticketControllerType = TICKETCONTROLLER_TYPE.CONTROLLER;
                }

                try
                {
                    _comPortName = args[1];
                }
                catch (Exception eX)
                {
                    Dispatcher.Logger.Error("Ошибка параметра \"Имя com-порта\": " + eX.Message);
                    _comPortName = "COM1";
                }
            }
            else
            {
                _ticketControllerType = TICKETCONTROLLER_TYPE.CONTROLLER;
                _comPortName = "COM1";
            }

            if (_ticketControllerType == TICKETCONTROLLER_TYPE.CONTROLLER)
                Dispatcher.Logger.Trace("Инициализация тикетного контроллера на порте: " + _comPortName);
            else
                Dispatcher.Logger.Trace("Инициализация виртуального тикетного контроллера");

            Dispatcher.Instance.InitHardware(_ticketControllerType, _comPortName);

            while (true)
            {
                ConsoleKeyInfo KeyInfo = Console.ReadKey();
                if (KeyInfo.Key == ConsoleKey.Escape)
                    break;
                else
                    Dispatcher.Instance.ProcessKeyboard(KeyInfo);
            }

            Dispatcher.Instance.ShutdownHardware();
            Dispatcher.Instance.ShutdownLocalNetwork();
        }
    }
}
