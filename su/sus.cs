using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace su
{
    public partial class Sus : ServiceBase
    {
        private const int SWITCH_USER_COMMAND = 193;    

        enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WTS_SESSION_INFO
        {
            public int SessionId;
            public string pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto)]
        private static extern bool WTSEnumerateSessions(IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)]
            int Reserved,
            [MarshalAs(UnmanagedType.U4)]
            int Version,
            ref IntPtr ppSessionInfo,
            [MarshalAs(UnmanagedType.U4)]
            ref int pCount);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSDisconnectSession(IntPtr hServer, int sessionId, bool bWait);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto)]
        private static extern bool WTSConnectSession(UInt64 TargetSessionId, UInt64 SessionId, string pPassword, bool bWait);

        private static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
        
        public Sus()
        {
            InitializeComponent();
        }

        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);
            if (command == SWITCH_USER_COMMAND)
            {
                SwitchUser();
            }
        }

        private void SwitchUser()
        {
            IntPtr buffer = IntPtr.Zero;

            StreamWriter fw = new StreamWriter("E:\\fileFromService.txt");
            fw.Write(DateTime.Now.ToString() + " BeginSwitch");
            fw.Close();


            int count = 0;

            // получаем список сессий, в которых выполнен вход
            if (WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, ref buffer, ref count))
            {
                WTS_SESSION_INFO[] sessionInfo = new WTS_SESSION_INFO[count];

                // самая сложная часть:
                // аккуратно преобразовать неуправляемую память в управляемую
                for (int index = 0; index < count; index++)
                    sessionInfo[index] = (WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)((int)buffer +
                    (Marshal.SizeOf(new WTS_SESSION_INFO()) * index)), typeof(WTS_SESSION_INFO));

                int activeSessId = -1;
                int targetSessId = -1;

                // получаем Id активного, и неактивного сеанса
                // 0 пропускаем, там всегда "Services"
                for (int i = 1; i < count; i++)
                {
                    if (sessionInfo[i].State == WTS_CONNECTSTATE_CLASS.WTSDisconnected)
                        targetSessId = sessionInfo[i].SessionId;
                    else if (sessionInfo[i].State == WTS_CONNECTSTATE_CLASS.WTSActive)
                        activeSessId = sessionInfo[i].SessionId;
                }


                if ((activeSessId > 0) && (targetSessId > 0))
                {
                    // если есть неактивный сеанс, то переключаемся на него.
                    WTSConnectSession(Convert.ToUInt64(targetSessId), Convert.ToUInt64(activeSessId), "", false);
                    Thread.Sleep(1000);
                    if (activeSessId == 1)

                        System.Diagnostics.Process.Start(@"C:\Users\admin\AppData\Roaming\Realtime Soft\UltraMon\3.4.1\Profiles\Only 2 Monitor.umprofile");
                    else
                        System.Diagnostics.Process.Start(@"C:\Users\admin\AppData\Roaming\Realtime Soft\UltraMon\3.4.1\Profiles\Only_1_Monitor.umprofile");
                }
                else
                {
                    // если неактивных нет. просто отключаемся (переходим на экран выбора пользователя)
                    System.Diagnostics.Process.Start(@"C:\Users\admin\AppData\Roaming\Realtime Soft\UltraMon\3.4.1\Profiles\Only 2 Monitor.umprofile");
                    Thread.Sleep(500);

                    WTSDisconnectSession(WTS_CURRENT_SERVER_HANDLE, activeSessId, false);

                    
                }
            }

            // обязательно чистим память
            WTSFreeMemory(buffer);

            SendButtons2();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int PostMessage(IntPtr hwnd, int wMsg, uint wParam, uint lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, uint wParam, uint lParam);

        private static int br = 0xffff;

        const int BM_SETSTATE = 243;
        const int WM_LBUTTONDOWN = 513;
        const int WM_LBUTTONUP = 514;
        const int WM_KEYDOWN = 256;
        const int WM_CHAR = 258;
        const int WM_KEYUP = 257;
        const int WM_SETFOCUS = 7;
        const int WM_SYSCOMMAND = 274;
        const int SC_MINIMIZE = 32;
        const int WM_GETTEXT = 0x000D;
        const int WM_GETTEXTLENGTH = 0x000E;
        const int WM_SETTEXT = 0X000C;
        const int WM_SYSKEYDOWN = 0x0104;
        const int WM_SYSKEYUP = 0x0105;

        private void SendButtons()
        {
            Thread.Sleep(3000);
            SendMessage((IntPtr)br, (int)WM_SYSKEYDOWN, 0x09, 0);
        }


        [DllImport("user32.dll")]
        public static extern void keybd_event(IntPtr bVk, byte bScan, UInt32 dwFlags, IntPtr dwExtraInfo);

        public const UInt32 KEYEVENTF_EXTENDEDKEY = 1;
        public const UInt32 KEYEVENTF_KEYUP = 2;

        public static void SendButtons2()
        {
            Thread.Sleep(5000);
            keybd_event((IntPtr)13, 0, 0, IntPtr.Zero);
            StreamWriter fw = new StreamWriter("E:\\fileFromServiceLogonScreen.txt");
            fw.Write(DateTime.Now.ToString() + " BeginSwitch");
            fw.Close();

            //keybd_event(Keys.C, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }
    }
}

