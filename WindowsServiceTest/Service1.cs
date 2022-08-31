using System.ServiceProcess;
using System.Timers;
using System.IO;

namespace WindowsServiceTest
{
    public partial class Service1 : ServiceBase
    {
        FileSystemWatcher FileSystemWatcher;
        private int timerEventRunning1 = 0;
        public Service1()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            EU2012.Common.Trace.SetLogFile(@"D:\Homework\WindowsService_FileWatcher\WindowsServiceTest\bin\Debug\", "WindowsServerWatcher");
            EU2012.Common.Trace.DebugWrite("OnStart", "windows server start!");
            
            timer1.Interval = 2000; //代表時間間隔，單位為毫秒
            timer1.Start(); //啟動timer
        
            FileSystemWatcher = new FileSystemWatcher(@"D:\upload_file")
            {
                EnableRaisingEvents = true,//取得或設定是否應該在處理序終止時引發 Exited 事件。
                IncludeSubdirectories = true//取得或設定數值，表示是否應該監視指定路徑內的子目錄。
            };
            FileSystemWatcher.Created += DirectoryChanged;//創建檔案
            FileSystemWatcher.Deleted += DirectoryChanged;//刪除檔案
            FileSystemWatcher.Changed += DirectoryChanged;
            FileSystemWatcher.Renamed += DirectoryChanged;//檔案更名
        }
        private void DirectoryChanged(object sender, FileSystemEventArgs e)
        {
            string message = e.ChangeType +"_" + e.FullPath;
            EU2012.Common.Trace.DebugWrite("FileWatcher", message);
        }
        protected override void OnStop()
        {
            EU2012.Common.Trace.DebugWrite("OnStop", "windows stop!");
        }
        private void timer1_Elapsed(object sender, ElapsedEventArgs e)//Elapsed代表，timer設定的時間到後要做什麼事情
        {
            EU2012.Common.Trace.DebugWrite("timer1_Elapsed", "Timer監控upload_file中");
            if (System.Threading.Interlocked.CompareExchange(ref timerEventRunning1, 1, 0) == 0)//timer的預設
            {
                string path = @"D:\upload_file";//監控資料夾路徑
                string[] files = Directory.GetFiles(path, "*.ok");//監控檔案類型

                if (files.Length != 0)
                {
                    EU2012.Common.Trace.DebugWrite("Timer", "新增.ok檔");
                    timer1.Stop();
                }
                else
                {
                    EU2012.Common.Trace.DebugWrite("Timer", "無.ok檔");
                }

                System.Threading.Interlocked.Decrement(ref timerEventRunning1);
            }
        }
    }
}
