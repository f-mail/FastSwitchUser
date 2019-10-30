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
             
        public Sus()
        {
            InitializeComponent();
        }

        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);
            if (command == SWITCH_USER_COMMAND)
            {
                Win32Sessions.SwitchUser();
            }
        }       
    }
}

