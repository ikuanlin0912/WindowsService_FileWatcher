using System;
using System.Diagnostics;
using System.Threading;

namespace EU2012.Common {
    /// <summary>
    /// Summary description for Debug.
    /// </summary>
    public class Trace {
        /// <summary>
        /// 
        /// </summary>
        public Trace() {
            //
            // TODO: Add constructor logic here
            //

        }

        /// <summary>
        /// LogFilePath
        /// </summary>
        static private string LogFilePath = "";
        private static Mutex mut;
        private static DateTime errorDateTime = System.DateTime.Now.AddDays(-1);

        /// <summary>
        /// 設定 Log 檔名與路徑
        /// </summary>
        /// <param name="File_Path"></param>
        /// <param name="ApName"></param>
        static public void SetLogFile(string File_Path, string ApName) {

            try {
                mut = new Mutex(false, ApName);
                LogFilePath = File_Path + ApName;
            } catch (Exception ee) {
                string none = ee.Source;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Class_Name">類別名稱</param>
        /// <param name="Message">內文</param>
        static public void DebugWrite(string Class_Name, string Message) {
            // ***Release Mode 時不顯示
            // [DEBUG] 進出function: 以方便了解程式執行流程
            // [DEBUG] function parameters和return value
            // [DEBUG] 程式流程重點: 如重要判斷 
            try {
                mut.WaitOne(5*1000,true);

                // 設定 Track
                SetTrackFile();

                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " [" + Class_Name.PadRight(10) + "][DEBUG] " + Message);

                // 清除 Track
                ClearTrack();
            } catch (Exception ee) {
                WriteEventLog(ee);
            } finally {
                try {
                    mut.ReleaseMutex();
                } catch { }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Class_Name"></param>
        /// <param name="Message"></param>
        static public void InfoWrite(string Class_Name, string Message) {
            // [INFO] IO access 前後: 包括FTP, HTTP, SMTP, socket, MSMQ, 讀寫檔, 下SQL指令...等各種io, 以紀錄access所花時間
            // [INFO]  IO access的資料: 如所下的sql指令, ftp/http/smtp/socket/msmq/讀寫檔...等, 從哪裡到哪裡(source & destination)
            // [INFO] IO access的結果: 如成功/失敗, SQL query 回幾筆
            mut.WaitOne(5 * 1000, true);
            try {
                // 設定 Track
                SetTrackFile();
                System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " [" + Class_Name.PadRight(10) + "][ INFO] " + Message);
                // 清除 Track
                ClearTrack();
            } catch (Exception ee) {
                WriteEventLog(ee); 
            } finally {
                try {
                    mut.ReleaseMutex();
                } catch { }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Class_Name"></param>
        /// <param name="Message"></param>
        static public void WarnWrite(string Class_Name, string Message) {
            // [WARN] 特殊事件, 但還是屬於正常執行範圍內

            mut.WaitOne(5 * 1000, true);
            try {
                // 設定 Track
                SetTrackFile();
                string strError = DateTime.Now.ToString("HH:mm:ss.fff") + " [" + Class_Name.PadRight(10) + "][ WARN] " + Message;
                System.Diagnostics.Trace.WriteLine(strError);

                // 清除 Track
                ClearTrack();

                // 將錯誤訊息 寄至 AdminEmail
                //EU.Common.Bill_INI bill_ini = new EU.Common.Bill_INI();
                //localhost.EUMailClass eu = new localhost.EUMailClass();
                //eu.Url = bill_ini.g_HTTP_AP + "/" + "eumailclass.asmx";
                //eu.Timeout = -1;
                //eu.SendToAdmin("測試用 :\n" + strError);
            } catch (Exception ee) {
                WriteEventLog(ee);
            } finally {
                try {
                    mut.ReleaseMutex();
                } catch { }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Class_Name"></param>
        /// <param name="Message"></param>
        static public void ErrorWrite(string Class_Name, string Message) {
            // [ERROR] 系統執行異常狀況
            string strError = "";
            mut.WaitOne(5*1000,true);
            try {
                strError = DateTime.Now.ToString("HH:mm:ss.fff") +
                           " [" + Class_Name.PadRight(10) + "][ERROR] " +
                           Message ;

                // 設定 Track
                SetTrackFile();

                System.Diagnostics.Trace.WriteLine(strError);
                strError = strError.Replace("\n", "<br>");
                strError += "<br>來源應用程式:" + AppDomain.CurrentDomain.FriendlyName;
                // 清除 Track
                ClearTrack();
            } catch (Exception ee) {
                WriteEventLog(ee);
            } finally {
                try {
                    mut.ReleaseMutex();
                } catch { }
            }

        }


        static public void WriteEventLog(Exception ee) {
            try {
                if (errorDateTime.AddMinutes(30) < System.DateTime.Now) {
                    errorDateTime = System.DateTime.Now;
                    //EventLog.WriteEntry("Billhunter", "來源應用程式:" + AppDomain.CurrentDomain.FriendlyName + " " + ee.ToString(), EventLogEntryType.Error);
                }
            } catch{}
        }
        /// <summary>
        /// 
        /// </summary>
        static public void SetTrackFile() {
            try {
                //**************************************************
                // 設定 Trace 增加 File 輸出
                //************************************************** 
                string path = LogFilePath + "." + DateTime.Now.ToString("yyyy-MM-dd") + ".log";

                System.IO.StreamWriter write = new System.IO.StreamWriter(path, true, System.Text.Encoding.UTF8);
                System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(write, "log"));

                //**************************************************
                // 設定 Trace 增加 Console 輸出
                //************************************************** 
                //System.Diagnostics.Trace.Listeners.Add(
                //	new System.Diagnostics.TextWriterTraceListener(Console.Out,"console"));

                System.Diagnostics.Trace.AutoFlush = true;
                System.Diagnostics.Debug.AutoFlush = true;

            } catch (Exception ee) {
                string none = ee.Source;
                //Console.WriteLine(ee.ToString());
                //using(System.IO.StreamWriter write = new System.IO.StreamWriter(@"c:\commom.log",true,System.Text.Encoding.GetEncoding("big5")))
                //{
                //	write.WriteLine("SetTrackFile:" + ee.ToString());
                //	write.Close();
                //}
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static public void ClearTrack() {
            try {
                //**************************************************
                // 清除 Trace 
                //************************************************** 
                System.Diagnostics.Trace.Listeners["log"].Flush();
                System.Diagnostics.Trace.Listeners["log"].Close();
                System.Diagnostics.Trace.Listeners.Remove("log");
            } catch (Exception ee) {
                string none = ee.Source;
            }
        }
    }
}
