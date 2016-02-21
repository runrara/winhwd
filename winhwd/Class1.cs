using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace winhwd
{
    public class util
    {
        /// <summary>
        /// 指定されたクラス名、ウインドウタイトルと完全一致するウインドウハンドルを返却する。
        /// </summary>
        /// <param name="className">検索対象クラス名（nullの場合は検索条件外）</param>
        /// <param name="windowTitle">検索対象タイトル（nullの場合は検索条件外）</param>
        /// <returns>条件に一致するウインドウのハンドル</returns>
        public static IntPtr getTopLevelHwd(string className, string windowTitle)
        {
            string pClassName = String.IsNullOrEmpty(className) ? null : className;
            string pTitle = String.IsNullOrEmpty(windowTitle) ? null : windowTitle;

            return FindWindow(pClassName, pTitle);
        }

        /// <summary>
        /// 指定されたクラス名、ウインドウタイトルと完全一致するプロセスを返却する。
        /// </summary>
        /// <param name="className">検索対象クラス名（nullの場合は検索条件外）</param>
        /// <param name="windowTitle">検索対象タイトル（nullの場合は検索条件外）</param>
        /// <returns>条件に一致するプロセス（見つからない場合はnull）</returns>
        public static Process getTopLevelProcess(string className, string windowTitle)
        {
            IntPtr hwd = getTopLevelHwd(className, windowTitle);

            if (hwd == IntPtr.Zero)
            {
                return null;
            }

            int processId;
            GetWindowThreadProcessId(hwd, out processId);

            Process p = Process.GetProcessById(processId);

            return p;
        }

        // 指定したウィンドウの全ての子孫ウィンドウを取得し、リストに追加する
        public static List<Window> getAllChildWindows(Window parent, List<Window> dest)
        {
            dest.Add(parent);
            enumChildWindows(parent.hWnd).ToList().ForEach(x => getAllChildWindows(x, dest));
            return dest;
        }

        // 与えた親ウィンドウの直下にある子ウィンドウを列挙する（孫ウィンドウは見つけてくれない）
        public static IEnumerable<Window> enumChildWindows(IntPtr hParentWindow)
        {
            IntPtr hWnd = IntPtr.Zero;
            while ((hWnd = FindWindowEx(hParentWindow, hWnd, null, null)) != IntPtr.Zero) { yield return getWindow(hWnd); }
        }

        // ウィンドウハンドルを渡すと、ウィンドウテキスト（ラベルなど）、クラス、スタイルを取得してWindowsクラスに格納して返す
        public static Window getWindow(IntPtr hWnd)
        {
            int textLen = GetWindowTextLength(hWnd);
            string windowText = null;
            if (0 < textLen)
            {
                //ウィンドウのタイトルを取得する
                StringBuilder windowTextBuffer = new StringBuilder(textLen + 1);
                GetWindowText(hWnd, windowTextBuffer, windowTextBuffer.Capacity);
                windowText = windowTextBuffer.ToString();
            }

            //ウィンドウのクラス名を取得する
            StringBuilder classNameBuffer = new StringBuilder(256);
            GetClassName(hWnd, classNameBuffer, classNameBuffer.Capacity);

            // スタイルを取得する
            int style = GetWindowLong(hWnd, GWL_STYLE);
            return new Window() { hWnd = hWnd, Title = windowText, ClassName = classNameBuffer.ToString(), Style = style };
        }

        private static List<Window> lstTopWnd;
        public static List<WindowStruct> getAllHwnd()
        {
           lstTopWnd = new List<Window>();

            // 全てのトップレベルウインドウを取得
            EnumWindows(new EnumWindowsDelegate(EnumWindowCallBack), IntPtr.Zero);

            List<WindowStruct> lstWndStruct = new List<WindowStruct>();

            // 取得した全てのトップレベルウインドウに対して処理実施
            foreach (Window w in lstTopWnd)
            {
                List<Window> buf = new List<Window>();
                List<Window> childBuf = getAllChildWindows(w, buf);

                WindowStruct structBuf = new WindowStruct();
                structBuf.parent = w;

                if(childBuf.Count != 0)
                {
                    structBuf.child = childBuf;
                }

                lstWndStruct.Add(structBuf);
            }

            return lstWndStruct;
        }

        private static bool EnumWindowCallBack(IntPtr hWnd, IntPtr lparam)
        {
            lstTopWnd.Add(getWindow(hWnd));

            //次のウィンドウを検索
            return true;
        }










        private static int GWL_STYLE = -16;


        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hWnd, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);
        
    }

    public class Window
    {
        public string ClassName;
        public string Title;
        public IntPtr hWnd;
        public int Style;
    }

    public class WindowStruct
    {
        public Window parent;
        public List<Window> child;
    }
}
