using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TLabVirtualHandle
{
    public partial class Form1 : Form
    {
        private bool keepAlive;
        private bool pressed;
        private int rightKey;
        private int leftKey;
        private InputHandler.InputHandler inputHandler;
        private InputHandler.CursorHandler cursorHandler;
        private TypeConverter converter;

        private delegate void updateCoordUI(int x, int y);
        private delegate void updateCursorVisible(string visible);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        [DllImport("User32.dll")]
        static extern bool GetCursorPos(out POINT lppoint);

        // 透明な画像をカーソルとして読み込むことでカーソルを視覚的に見えなくする

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
            keepAlive = true;
            inputHandler = new InputHandler.InputHandler();
            cursorHandler = new InputHandler.CursorHandler();
            converter = TypeDescriptor.GetConverter(typeof(Keys));
            Thread mainLoop = new Thread(new ThreadStart(MainLoop));
            mainLoop.IsBackground = true;
            mainLoop.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            int anchorCoordXParse;
            int anchorCoordYParse;
            try
            {
                anchorCoordXParse = Int32.Parse(AnchorXCoord.Text);
                anchorCoordYParse = Int32.Parse(AnchorYCoord.Text);
            }
            catch (FormatException exception)
            {
                // 文字列の変換に失敗した
                MessageBox.Show(
                    string.Format("整数を文字型で入力してください{0}", exception.Message));
                return;
            }

            int handleCoordXParse;
            int handleCoordYParse;
            try
            {
                handleCoordXParse = Int32.Parse(HandleXCoord.Text);
                handleCoordYParse = Int32.Parse(HandleYCoord.Text);
            }
            catch (FormatException exception)
            {
                // 文字列の変換に失敗した
                MessageBox.Show(
                    string.Format("整数を文字型で入力してください{0}", exception.Message));
                return;
            }

            int mamount;
            try
            {
                if (MoveAmount.Text == "")
                {
                    MessageBox.Show(
                        string.Format("整数を入力してください"));
                    return;
                }
                else
                {
                    mamount = Int32.Parse(MoveAmount.Text);
                    if(mamount <= 0)
                    {
                        MessageBox.Show(
                            string.Format("0より大きい値を入力してください"));
                        return;
                    }
                }
            }
            catch (FormatException exception)
            {
                // 文字列の変換に失敗した
                MessageBox.Show(
                    string.Format("整数を文字型で入力してください{0}", exception.Message));
                return;
            }

            int ramount;
            try
            {
                if (ReverseAmount.Text == "")
                {
                    MessageBox.Show(
                        string.Format("整数を入力してください"));
                    return;
                }
                else
                {
                    ramount = Int32.Parse(ReverseAmount.Text);
                    if (ramount <= 0)
                    {
                        MessageBox.Show(
                            string.Format("0より大きい値を入力してください"));
                        return;
                    }
                }
            }
            catch (FormatException exception)
            {
                // 文字列の変換に失敗した
                MessageBox.Show(
                    string.Format("整数を文字型で入力してください{0}", exception.Message));
                return;
            }

            try
            {
                rightKey = (int)(Keys)converter.ConvertFromString(KeyR.Text);
                leftKey = (int)(Keys)converter.ConvertFromString(KeyL.Text);
            }
            catch (NotSupportedException exception)
            {
                MessageBox.Show(
                    string.Format("ローマ字を1字で登録して下さい{0}", exception.Message));
                return;
            }

            int rlimit;
            try
            {
                if (RotateLimit.Text == "")
                {
                    rlimit = 0;
                }
                else
                {
                    rlimit = Int32.Parse(RotateLimit.Text);
                    if (rlimit < 0)
                    {
                        MessageBox.Show(
                            string.Format("0より大きい値を入力してください"));
                        return;
                    }
                }
            }
            catch (FormatException exception)
            {
                // 文字列の変換に失敗した
                MessageBox.Show(
                    string.Format("整数を文字型で入力してください{0}", exception.Message));
                return;
            }

            inputHandler.RegisterConfig(
                anchorCoordXParse, anchorCoordYParse,
                handleCoordXParse, handleCoordYParse,
                mamount, ramount, rlimit);
        }

        private void UnRegisterButton_Click(object sender, EventArgs e)
        {
            if (inputHandler.Registered)
            {
                inputHandler.Registered = false;
            }
        }

        private void UpdateCoordUI(int x, int y)
        {
            CursorPos.Text = x.ToString() + ", " + y.ToString();
        }

        private void UpdateCursorVisible(string visible)
        {
            CursorVisible.Text = visible;
        }

        private void MainLoop()
        {
            short stateR;
            short stateL;
            bool escPressed = false;
            POINT pt = new POINT();
            updateCoordUI updateCoord = new updateCoordUI(UpdateCoordUI);
            updateCursorVisible updateCursor = new updateCursorVisible(UpdateCursorVisible);
            Invoke(updateCursor, "Show");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (keepAlive)
            {
                // カーソル位置を確認
                GetCursorPos(out pt);
                Invoke(updateCoord, pt.X, pt.Y);

                short escape = GetAsyncKeyState((int)Keys.Escape);
                if ((escape & 0x8000) != 0)
                {
                    if(escPressed == false)
                    {
                        escPressed = true;
                        cursorHandler.ShowCursor(false);
                        Invoke(updateCursor, cursorHandler.MouseVisible ? "Show" : "Hide");
                    }
                }
                else
                {
                    escPressed = false;
                }

                // キー入力を確認
                if (inputHandler.Registered)
                {
                    stateR = 0;
                    stateR = GetAsyncKeyState(rightKey);
                    if ((stateR & 0x8000) != 0)
                    {
                        if(pressed == false)
                        {
                            inputHandler.KeyDown();
                        }
                        else
                        {
                            inputHandler.KeyRotateRight();
                        }
                        pressed = true;
                    }
                    else
                    {
                        stateL = 0;
                        stateL = GetAsyncKeyState(leftKey);
                        if ((stateL & 0x8000) != 0)
                        {
                            if (pressed == false)
                            {
                                inputHandler.KeyDown();
                            }
                            else
                            {
                                inputHandler.KeyRotateLeft();
                            }
                            pressed = true;
                        }
                        else
                        {
                            if(pressed == true)
                            {
                                pressed = inputHandler.ReverseKeyRotate();
                            }
                        }
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "フォームを閉じますか?",
                "確認",
                MessageBoxButtons.YesNo);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                keepAlive = false;
                cursorHandler.ShowCursor(true);
                SaveConfig();
                MessageBox.Show("アプリケーションを終了します");
            }
        }

        private void SaveConfig()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            StreamWriter writer = new StreamWriter(
                @".\Resources\Configs.txt", false, Encoding.GetEncoding("Shift_JIS"));

            if(AnchorXCoord.Text == "" || AnchorYCoord.Text == ""
                || HandleXCoord.Text == "" || HandleYCoord.Text == ""
                || MoveAmount.Text == "" || ReverseAmount.Text == ""
                || KeyR.Text == "" || KeyL.Text == "")
            {
                writer.Close();
                return;
            }

            writer.WriteLine(
                AnchorXCoord.Text + "," +
                AnchorYCoord.Text + "," +
                HandleXCoord.Text + "," +
                HandleYCoord.Text + "," +
                MoveAmount.Text + "," +
                ReverseAmount.Text + "," +
                KeyR.Text + "," +
                KeyL.Text);
            writer.Close();
        }

        private void LoadConfig()
        {
            if (File.Exists(@".\Resources\Configs.txt"))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                FileStream stream = new FileStream(
                    @".\Resources\Configs.txt",
                    FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader reader = new StreamReader(stream);

                if (reader.Peek() != -1)
                {
                    string[] data = reader.ReadLine().Split(',');
                    AnchorXCoord.Text = data[0];
                    AnchorYCoord.Text = data[1];
                    HandleXCoord.Text = data[2];
                    HandleYCoord.Text = data[3];
                    MoveAmount.Text = data[4];
                    ReverseAmount.Text = data[5];
                    KeyR.Text = data[6];
                    KeyL.Text = data[7];
                }

                reader.Close();
                stream.Close();
            }
        }
    }
}
