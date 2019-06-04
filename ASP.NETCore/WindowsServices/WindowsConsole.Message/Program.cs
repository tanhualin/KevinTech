using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsConsole.Message
{
    class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        public const int WM_CLOSE = 0x10;

        static void Main(string[] args)
        {
            ShowMessageBoxTimeout("发现InSight定制功能重要更新，3秒后关闭InSight，将自动更新。", "InSight定制功能服务提示", 3000);
        }

        public static void ShowMessageBoxTimeout(string text, string caption, int timeout)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(Interop.CloseMessageBox),
                new Interop(caption, timeout));
            //System.Windows.Forms.MessageBox.Show(text, caption);
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
    }
}
