using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;

namespace TicketMachine
{
    class GameLauncher
    {
        // Путь к исполняемому файлу игры
        private const string GAME_EXE_PATH = "C:\\AdrenalineGame\\Game\\Game.exe";
        // Интервал установки окна поверх всех
        private const int SET_FOREGROUND_INTERVAL = 5000;

        // Процесс игры
        private Process _gameProcess;
        // И нформация о процессе игры
        private ProcessStartInfo _gameProcessStartInfo;

        // Таймер установки окна поверх всех
        private Timer _setForegroundTimer;

        /// <summary>
        /// Конструктор
        /// </summary>
        public GameLauncher()
        {
            _gameProcessStartInfo = new ProcessStartInfo();
            _gameProcessStartInfo.FileName = GAME_EXE_PATH;
            _gameProcessStartInfo.UseShellExecute = true;

            _gameProcess = new Process();
            _gameProcess.StartInfo = _gameProcessStartInfo;

            _setForegroundTimer = new Timer();
            _setForegroundTimer.Interval = SET_FOREGROUND_INTERVAL;
            _setForegroundTimer.Elapsed += _setForegroundTimer_Elapsed;
        }

        /// <summary>
        /// Запуск процесса игры
        /// </summary>
        public void Start()
        {
            _gameProcess.Start();
            _setForegroundTimer.Start();
        }

        /// <summary>
        /// WINAPI функция установки окна поверх всех
        /// </summary>
        /// <param name="handle">Handle окна</param>
        /// <returns>Возвращаемый код</returns>
        [DllImport("user32.dll")]
        static extern int SetForegroundWindow(IntPtr handle);

        //----------------------------------------//
        // Таймер установки окна поверх остальных //
        //----------------------------------------//
        void _setForegroundTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SetForegroundWindow(_gameProcess.MainWindowHandle);
        }
    }
}
