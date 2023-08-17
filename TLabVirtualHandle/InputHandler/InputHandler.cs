using System;
using System.Runtime.InteropServices;

namespace TLabVirtualHandle.InputHandler
{
    class InputHandler
    {
        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern void mouse_event(
            int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void SetCursorPos(int X, int Y);

        [DllImport("USER32.DLL")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [Flags]
        public enum MouseEventFlags
        {
            LeftDown    = 0x00000002,
            LeftUp      = 0x00000004,
            MiddleDown  = 0x00000020,
            MiddleUp    = 0x00000040,
            Move        = 0x00000001,
            Absolute    = 0x00008000,
            RightDown   = 0x00000008,
            RightUp     = 0x00000010,
            Wheel       = 0x00000800
        };

        private IntPtr desktopWindow;
        private double anchorX;
        private double anchorY;
        private double vecX;
        private double vecY;
        private double mamount;
        private double ramount;
        private double rlimit;
        private double currentRotate;
        private bool registered;
        private const double PI = 3.14;
        private const double round = 360;
        private const double slow = 500;

        public bool Registered
        {
            get
            {
                return registered;
            }
            set
            {
                registered = false;
            }
        }

        public InputHandler()
        {
            desktopWindow = GetDesktopWindow();
            registered = false;
        }

        public void RegisterConfig(int anchorX, int anchorY,
            int handleX, int handleY, int mamount, int ramount, int rlimit)
        {
            this.anchorX = anchorX;
            this.anchorY = anchorY;
            this.vecX = handleX - anchorX;
            this.vecY = handleY - anchorY;
            this.mamount = mamount / round * 2 * PI / slow;
            this.ramount = ramount / round * 2 * PI / slow;
            this.rlimit = rlimit / round * 2 * PI;
            currentRotate = 0;
            registered = true;
        }

        public void KeyDown()
        {
            SetForegroundWindow(desktopWindow);
            SetCursorPos();
            mouse_event((int)MouseEventFlags.LeftDown, 0, 0, 0, 0);
        }

        private void SetCursorPos()
        {
            SetCursorPos(
                (int)(anchorX + vecX * Math.Cos(currentRotate) - vecY * Math.Sin(currentRotate)),
                (int)(anchorY + vecX * Math.Sin(currentRotate) + vecY * Math.Cos(currentRotate)));
        }

        public void KeyRotateRight()
        {
            currentRotate += mamount;
            if(currentRotate > rlimit)
            {
                currentRotate = rlimit;
            }
            SetCursorPos();
        }

        public void KeyRotateLeft()
        {
            currentRotate -= mamount;
            if(currentRotate < -rlimit)
            {
                currentRotate = -rlimit;
            }
            SetCursorPos();
        }

        public bool ReverseKeyRotate()
        {
            if(currentRotate > ramount)
            {
                currentRotate -= ramount;
            }
            else if(currentRotate < -ramount)
            {
                currentRotate += ramount;
            }
            else
            {
                currentRotate = 0;
                SetCursorPos();
                mouse_event((int)MouseEventFlags.LeftUp, 0, 0, 0, 0);
                return false;
            }
            SetCursorPos();
            return true;
        }
    }
}
