using MovablePython;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace suc2
{
    internal class SUApplicationContext : ApplicationContext
    {
        private Hotkey hk;
        private Hotkey hkLogoff;
        private Hotkey hkKodi;
        private Form form;
        private const int SWITCH_USER_COMMAND = 193;
        internal SUApplicationContext()
        {
            // только создаем форму, она все равно нужна
            // чтобы слушать хоткеи
            form = new Form();

            // создаем и регистрируем глобайльный хоткей
            hk = new Hotkey(Keys.L, false, true, true, false); /////////// <- hotkey

            hk.Pressed += delegate { ActionProcess(); };
            if (hk.GetCanRegister(form))
                hk.Register(form);

            // хоткей для выхода из системы
            hkLogoff = new Hotkey(Keys.E, false, true, true, false); /////////// <- hotkey
            hkLogoff.Pressed += delegate { UserLogoff(); };
            if (hkLogoff.GetCanRegister(form))
                hkLogoff.Register(form);
            
            // хоткей для выхода из системы
            hkKodi = new Hotkey(Keys.K, false, true, true, false); /////////// <- hotkey
            hkKodi.Pressed += delegate { LaunchKodi(); };
            if (hkKodi.GetCanRegister(form))
                hkKodi.Register(form);

            // Вешаем событие на выход
            Application.ApplicationExit += Application_ApplicationExit;

            // отслеживаем событие входа в сессию
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

            //переключаем мониторы
            Thread.Sleep(3000);
            SwitchMonitor(false);

        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Thread.Sleep(2000);
            if (e.Reason == SessionSwitchReason.SessionLogon)
                SwitchMonitor(false);
            if (e.Reason == SessionSwitchReason.ConsoleConnect)
                SwitchMonitor(false);
        }

        private void SendSwitchCommand()
        {
            // Описываем нашу службу
            ServiceController sc = new ServiceController("Sus");
            try
            {
                // посылаем ей команду
                sc.ExecuteCommand(SWITCH_USER_COMMAND);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ActionProcess()
        {
            SwitchMonitor(true);
            Thread.Sleep(6000);
            SendSwitchCommand();
        }

        private const string cmdMonitorMain = @"E:\admin\Documents\Программы\Fast User Switch\script_switch_monitor_main.bat";
        private const string cmdMonitorTV = @"E:\admin\Documents\Программы\Fast User Switch\script_switch_monitor_tv.bat";


        private void SwitchMonitor(bool isMain = true)
        {
            Thread.Sleep(500);

            if (isMain)
            {
                System.Diagnostics.Process.Start(cmdMonitorMain);
            }
            else
            {
                System.Diagnostics.Process.Start(cmdMonitorTV);
            }
        }

        //user logoff
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        private void UserLogoff()
        {
            SwitchMonitor();
            Thread.Sleep(4000);
            ExitWindowsEx(0, 0);
        }


        private readonly string KodiProcess = "kodi";

        private void KillKodi()
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName(KodiProcess))
                {
                    proc.Kill();
                }
            }
            catch
            {

            }
        }

        private void LaunchKodi()
        {
            KillKodi();

            Thread.Sleep(2000);

            ProcessStartInfo info = new ProcessStartInfo(@"C:\Program Files\Kodi\kodi.exe");
            info.UseShellExecute = true;
            info.Verb = "runas";
            Process.Start(info);
        }
    

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            // при выходе разрегистрируем хоткей 
            if (hk.Registered)
                hk.Unregister();
            if (hkLogoff.Registered)
                hkLogoff.Unregister();
            if (hkKodi.Registered)
                hkKodi.Unregister();
        }
    }
}
