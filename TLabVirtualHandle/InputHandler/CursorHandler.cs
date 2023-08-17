using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TLabVirtualHandle.InputHandler
{
    class CursorHandler
    {
        private bool mouseVisible;
        private const uint OCR_NORMAL = 32512;
        private const uint SPI_SETCURSORS = 0x57;   // カーソルをリセット

        public bool MouseVisible
        {
            get
            {
                return mouseVisible;
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursorFromFile(string lpFileName);
        [DllImport("user32.dll")]
        public static extern bool SetSystemCursor(IntPtr hcur, uint id);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(int hInstance, uint lpCursorName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(
            UInt32 uiAction, UInt32 uiParam, String pvParam, UInt32 fWinIni);

        public CursorHandler()
        {
            mouseVisible = true;
        }

        public void ShowCursor(bool appExit)
        {
            mouseVisible = !mouseVisible;
            if (mouseVisible == false)
            {
                SetSystemCursor(
                    LoadCursorFromFile(Path.GetFullPath(@".\Resources\blank.cur")), OCR_NORMAL);
            }
            else
            {
                SystemParametersInfo(SPI_SETCURSORS, 0, null, 0);
            }

            // アプリケーション終了時はカーソルを表示
            if(appExit == true)
            {
                SystemParametersInfo(SPI_SETCURSORS, 0, null, 0);
            }
        }
    }
}
