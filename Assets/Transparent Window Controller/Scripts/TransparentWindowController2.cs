using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;

namespace Desdinova
{
    public class TransparentWindowController2 : MonoBehaviour
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;

        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        public Color transparentColor = Color.black;

        public bool RemoveWindowBorder = false;


        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("Dwmapi.dll")]
        private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);



        // CAUTION:
        // To control enable or disable, use Start method instead of Awake.

        protected virtual void Start()
        {
            if (!Application.isEditor)
            {
                this.SetTransparent();

                if (RemoveWindowBorder)
                {
                    Desdinova.WindowsUtilities.RemoveWindowBorders();
                }
            }
        }

        public void SetTransparent()
        {
            if (!Application.isEditor)
            {

                IntPtr hwnd = GetActiveWindow();

                SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_LAYERED);

                MARGINS margins = new MARGINS() { cxLeftWidth = -1 };
                DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }
            else
            {
                Debug.Log("SetTransparent run only in the Windows build.");
            }
        }

        public void SetAlwaysOnTop(bool enable)
        {
            Desdinova.WindowsUtilities.SetAlwaysOnTop(enable);
        }

        public void ChangeMonitor()
        {
            Desdinova.WindowsUtilities.ChangeMonitor();
        }

        public static void ExitApplication()
        {
            Application.Quit();
        }
    }
}