using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsConsole.Message
{
    public class Interop
    {
        private int _Timeout;

        /// <summary>
        /// In millisecond
        /// </summary>
        public int Timeout
        {
            get
            {
                return _Timeout;
            }
        }

        private string _Caption;

        /// <summary>
        /// Caption of dialog
        /// </summary>
        public string Caption
        {
            get
            {
                return _Caption;
            }
        }

        public Interop(string caption, int timeout)
        {
            _Timeout = timeout;
            _Caption = caption;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        public const int WM_CLOSE = 0x10;

        public static void CloseMessageBox(object state)
        {
            Interop closeState = state as Interop;
            System.Threading.Thread.Sleep(closeState.Timeout);
            IntPtr hwnd_win = FindWindow(null, closeState.Caption);
            if (hwnd_win != IntPtr.Zero)
            {
                SendMessage(hwnd_win, WM_CLOSE, 0, 0);
            }
        }


    }
}
