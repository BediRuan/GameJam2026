using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Desdinova
{
    public class TransparentWindowController : MonoBehaviour
    {
        private IntPtr hwnd;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int LWA_COLORKEY = 0x00000001;
        private const int LWA_ALPHA = 0x00000002;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        public Color transparentColor = Color.black;


        [Range(0, 255)]
        public int WindowOpacity = 255;
        public bool RemoveWindowBorder = false;

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);




        void Start()
        {
            if (!Application.isEditor)
            {
                hwnd = GetActiveWindow();

                this.SetTransparentColor(transparentColor);

                if (this.RemoveWindowBorder)
                {
                    Desdinova.WindowsUtilities.RemoveWindowBorders();
                }
            }
        }


        public void SetTransparentColor(string hexTransparentColor)
        {
            if (!Application.isEditor)
            {
                if (ColorUtility.TryParseHtmlString(hexTransparentColor, out Color color))
                {
                    this.SetTransparentColor(color);
                }
                else
                {
                    Debug.LogError("Invalid hex color string: " + hexTransparentColor);
                }
            }
            else
            {
                Debug.Log("SetTransparentColor run only in the Windows build.");
            }
        }
        public void SetTransparentColor(Color transparentColor)
        {
            if (!Application.isEditor)
            {

                SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_LAYERED);

                uint colorKey = Desdinova.WindowsUtilities.ColorToColorRef(transparentColor);

                SetLayeredWindowAttributes(hwnd, colorKey, (byte)this.WindowOpacity, LWA_COLORKEY | LWA_ALPHA); // You might not need LWA_COLORKEY
            }
            else
            {
                Debug.Log("SetTransparentColor run only in the Windows build.");
            }
        }


        public void SetOpacity(int opacity)
        {
            if (!Application.isEditor)
            {
                this.WindowOpacity = Mathf.Clamp(opacity, 0, 255);
                uint colorKey = Desdinova.WindowsUtilities.ColorToColorRef(transparentColor);

                SetLayeredWindowAttributes(hwnd, colorKey, (byte)this.WindowOpacity, LWA_COLORKEY | LWA_ALPHA); // You might not need LWA_COLORKEY
            }
            else
            {
                Debug.Log("SetOpacity run only in the Windows build.");
            }
        }

        public void SetTransparent(bool enable)
        {
            if (!Application.isEditor)
            {
                if (enable)
                {
                    SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_LAYERED | WS_EX_TRANSPARENT);
                    this.SetOpacity(this.WindowOpacity);
                }
                else
                {
                    SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) & ~WS_EX_LAYERED);
                }
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