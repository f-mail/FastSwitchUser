using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace suc
{
    class Win32Sessions
    {
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

        public static bool IsHasNonActiveSession()
        {
            IntPtr buffer = IntPtr.Zero;

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
                    // если есть неактивный сеанс
                    // обязательно чистим память
                    WTSFreeMemory(buffer);
                    return true;
                }
                else
                {
                    // если неактивных нет. просто отключаемся (переходим на экран выбора пользователя)
                    // обязательно чистим память
                    WTSFreeMemory(buffer);
                    return false;
                }
            }
            // обязательно чистим память
            WTSFreeMemory(buffer);
            return false;
        }      

        public static void SwitchUser()
        {
            IntPtr buffer = IntPtr.Zero;          


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
                }
                else
                {
                    // если неактивных нет. просто отключаемся (переходим на экран выбора пользователя)
                    WTSDisconnectSession(WTS_CURRENT_SERVER_HANDLE, activeSessId, false);
                }
            }

            // обязательно чистим память
            WTSFreeMemory(buffer);            
        }        
    }

}

