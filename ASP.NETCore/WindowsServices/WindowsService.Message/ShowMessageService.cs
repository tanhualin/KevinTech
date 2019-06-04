using NLog;
using System.ServiceProcess;
using System.Windows.Forms;

namespace WindowsService.Message
{
    public partial class ShowMessageService : ServiceBase
    {
        public static Logger logger = LogManager.GetLogger("f");

        public ShowMessageService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("开始启动服务！调用窗口！");
            System.Threading.Thread.Sleep(5000);
            ShowMessageBoxTimeout("发现InSight定制功能重要更新，3秒后关闭InSight，将自动更新。", "InSight定制功能服务提示", 5000);
        }

        protected override void OnStop()
        {
            logger.Info("停止服务！");
        }

        protected override void OnContinue()
        {
            logger.Info("继续服务！");
        }
        public void ShowMessageBoxTimeout(string text, string caption, int timeout)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(CloseState.CloseMessageBox),
                new CloseState(caption, timeout));
            //System.Windows.Forms.MessageBox.Show(text, caption);
            MessageBox.Show("要弹的信息。", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
    }
}
