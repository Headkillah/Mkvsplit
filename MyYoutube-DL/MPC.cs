using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace mkvsplit
{
    class MPC
    {
        /// <summary> объявляем тип делегата, который будет читать текст из указанного окна. </summary>
        public delegate bool WindowEnumDelegate(IntPtr hwnd, int lParam);

        /// <summary> объявляем использование метода из user32.dll, который:
        /// берёт процесс с дескриптором hwnd и перечисляя всего его окна выполняет для каждого делегат del </summary>
        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hwnd, WindowEnumDelegate del, int lParam);

        /// <summary>
        /// объявляем использование метода из user32.dll, который:
        /// берёт процесс с дескриптором hwnd и считывает
        /// </summary>
        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder bld, int size);

        
        public static string GetTimeFromMPC(string fileName)
        {
            Process app = null;

            var appslist = new List<Process>(Process.GetProcessesByName("mpc-hc64"));
            appslist.AddRange(Process.GetProcessesByName("mpc-hc"));

            foreach (var ap in appslist)
            {
                if (ap.MainWindowTitle != fileName) continue;

                app = ap;
                break;
            }

            if (app == null)
            {
                MessageBox.Show("Video player not found.");
                return "00:00:00";
            }

            var mainWindowHandle = app.MainWindowHandle;

            var list = new List<string>();

            EnumChildWindows(mainWindowHandle,
                                delegate(IntPtr hwnd, int param)
                                {
                                    var srtBuilder = new StringBuilder(256);

                                    GetWindowText(hwnd, srtBuilder,256);

                                    var text = srtBuilder.ToString();
                                    if (!string.IsNullOrEmpty(text)) list.Add(text);

                                    return true;
                                },
                                0);

            var l = "";

            for (var i = 0; i < list.Count; i++)
            {
                l += list[i] + "\n";
            }

            l = l.Split(' ')[0];

            return l;
        }
    }
}
