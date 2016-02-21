using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using winhwd;

namespace controlStreamEngine
{
    class Program
    {
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;

        static int Main(string[] args)
        {
            RecStatus status = RecStatus.stopped;

            // StreamEngineのハンドルを取得
            IntPtr streamEngineHwd = util.getTopLevelHwd(null, "AVerMedia Stream Engine");

            // ----------------録画開始ボタンを探す----------------
            Window recButton = null;

            // StreamEngine内の全ての要素のウインドウハンドル取得
            List<Window> lstWindow = util.getAllChildWindows(util.getWindow(streamEngineHwd), new List<Window>());


            foreach(Window w in lstWindow)
            {
                if ("開始".Equals(w.Title))
                {
                    status = RecStatus.stopped;
                    recButton = w;
                } else if("停止".Equals(w.Title))
                {
                    status = RecStatus.started;
                    recButton = w;
                }
            }

            // 取得できたかを判定
            if (recButton == null)
            {
                return 9;
            }

            


            // ----------------録画開始処理----------------
            if ("start".Equals(args[0]) && status == RecStatus.stopped)
            {
                System.Threading.Thread.Sleep(4400);
                SendMessage(recButton.hWnd, WM_LBUTTONDOWN, 0, 0);
                SendMessage(recButton.hWnd, WM_LBUTTONUP, 0, 0);

            } else if ("stop".Equals(args[0]) && status == RecStatus.started)
            {
                // ----------------録画ファイル保存先・リネームすべきファイル名等取得----------------
                string recDir = Environment.GetEnvironmentVariable("IKALOG_MP4_DESTDIR").Replace("\"", "");
                string srcFilePath = getLatestFilePath(recDir);
                string destName = Environment.GetEnvironmentVariable("IKALOG_MP4_DESTNAME").Replace("\"", "").Replace(".mp4", ".ts");

                Console.WriteLine("src:" + srcFilePath);
                Console.WriteLine("dest:" + destName);

                System.Threading.Thread.Sleep(10600);
                SendMessage(recButton.hWnd, WM_LBUTTONDOWN, 0, 0);
                SendMessage(recButton.hWnd, WM_LBUTTONUP, 0, 0);

                System.Threading.Thread.Sleep(15000);
                File.Move(srcFilePath, destName);
            }

            return 0;            
        }

        private static string getLatestFilePath(string serchFolder)
        {
            return Directory.GetFiles(serchFolder, "*.ts").OrderBy(f => File.GetLastWriteTime(f)).Last<string>();            
        }











        enum RecStatus
        {
            started,
            stopped
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);
    }

    
}
