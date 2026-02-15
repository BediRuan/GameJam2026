using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


namespace Desdinova
{
    public static class WindowsUtilities
    {

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;

        private static int currentMonitorIndex = -1;

        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        public static uint ColorToColorRef(Color color)
        {
            Color32 c = color;
            return (uint)(c.r | (c.g << 8) | (c.b << 16));
        }

        public static void SetAlwaysOnTop(bool enable)
        {
            if (!Application.isEditor)
            {
                IntPtr hwnd = GetActiveWindow();

                if (enable)
                    SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);
                else
                    SetWindowPos(hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);
            }
            else
            {
                Debug.Log("SetAlwaysOnTop run only in the Windows build.");
            }
        }


        public static int ChangeMonitor()
        {
            if (!Application.isEditor)
            {
                List<DisplayInfo> allDisplays = new();
                Screen.GetDisplayLayout(allDisplays);
                var displayCount = allDisplays.Count;
                if (displayCount < 2)
                {
                    return 0;
                }

                currentMonitorIndex = (currentMonitorIndex + 1) % displayCount;
                var displayInfo = allDisplays[currentMonitorIndex];

                Screen.MoveMainWindowTo(displayInfo, Vector2Int.zero);
                var currentResolution = Screen.currentResolution;
                int newWidth = currentResolution.width;
                int newHeight = currentResolution.height;

                IntPtr hWnd = GetActiveWindow();

                Screen.SetResolution(newWidth, newHeight, FullScreenMode.FullScreenWindow);

                SetWindowPos(hWnd, IntPtr.Zero, newWidth * currentMonitorIndex, 0, newWidth, newHeight, SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW);

                return currentMonitorIndex;
            }
            else
            {
                Debug.Log("ChangeMonitor run only in the Windows build.");
                return 0;
            }
        }

    

        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_MINIMIZE = 0x20000000;
        private const int WS_MAXIMIZE = 0x01000000;
        private const int WS_SYSMENU = 0x00080000;
        private const uint SWP_FRAMECHANGED = 0x0020;


        public static void RemoveWindowBorders()
        {
            if (!Application.isEditor)
            {
                IntPtr hwnd = GetActiveWindow();

                int style = GetWindowLong(hwnd, GWL_STYLE);
                style &= ~WS_CAPTION;
                style &= ~WS_THICKFRAME;
                style &= ~WS_MINIMIZE;
                style &= ~WS_MAXIMIZE;
                style &= ~WS_SYSMENU;

                SetWindowLong(hwnd, GWL_STYLE, style);

                SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOSIZE | SWP_NOMOVE | SWP_NOZORDER | SWP_SHOWWINDOW);

            }
            else
            {
                Debug.Log("RemoveWindowBorders run only in the Windows build.");
            }
        }
    }
}
