using MovablePython;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace suc
{
    internal class SUApplicationContext: ApplicationContext
    {
        private Hotkey hk;
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

            // Вешаем событие на выход
            Application.ApplicationExit += Application_ApplicationExit;
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
            if (Win32Sessions.IsHasNonActiveSession())
            {
                SendSwitchCommand();
            }
            else
            {
                ShowSwitchUserScreen();
            }
        }

        private void ShowSwitchUserScreen()
        {
            int delay = 350;
            Thread.Sleep(delay*2);
            SendKeys.SendWait("^{ESC}");
            Thread.Sleep(delay*3);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(delay);
            SendKeys.SendWait("{DOWN}");
            Thread.Sleep(delay);
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(delay);
            SendKeys.SendWait("{DOWN}");
            Thread.Sleep(delay);
            SendKeys.SendWait("{DOWN}");
            Thread.Sleep(delay);
            SendKeys.SendWait("{DOWN}");
            Thread.Sleep(delay);
            SendKeys.SendWait("{ENTER}");            
        }



        void Application_ApplicationExit(object sender, EventArgs e)
        {
            // при выходе разрегистрируем хоткей 
            if (hk.Registered)
                hk.Unregister();
        }
    }
}
