using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Windows.Interop;
using NintrollerLib;

namespace WiitarThing.Windows
{
    /// <summary>
    /// Interaction logic for DebugDataWindow.xaml
    /// </summary>
    public partial class DebugDataWindow : Window
    {
        public bool Cancelled { get; protected set; }
        //public int Count { get; protected set; }

        ////public List<ulong> ConnectedDeviceAddresses = new List<ulong>();

        //bool _notCompatable = false;

        public Nintroller nintroller;

        public DebugDataWindow()
        {
            InitializeComponent();
        }

        public void RegisterNintrollerUpdate()
        {
            nintroller.StateUpdate += Nintroller_StateUpdate;
        }

        private void Nintroller_StateUpdate(object sender, NintrollerStateEventArgs e)
        {
#if DEBUG
            var sb = new StringBuilder();

            if (!Cancelled)
            {
                if (e.state is Guitar)
                {
                    var gtr = new Guitar();
                    gtr = (Guitar)e.state;

                    sb.Clear();

                    for (int i = 0; i < gtr.DebugLastData.Length; i++)
                    {
                        sb.Append(gtr.DebugLastData[i].ToString("X2"));
                        sb.Append(" ");
                    }

                    Prompt(sb.ToString(), isBold: true, isItalic: false, isSmall: false, isDebug: false);

                    //System.Threading.Thread.Sleep(16);
                } 
                else if (e.state is Turntable)
                {
                    var ttb = new Turntable();
                    ttb = (Turntable)e.state;

                    sb.Clear();

                    for (int i = 0; i < ttb.DebugLastData.Length; i++)
                    {
                        sb.Append(ttb.DebugLastData[i].ToString("X2"));
                        sb.Append(" ");
                    }

                    byte[] data = ttb.DebugLastData;
                    byte JoyX, JoyY, Crossfade, Dial, LeftTable, RightTable;
                    int offset = 6;

                    sb.Append("\n");

                    JoyX = (byte)(data[offset + 0] & 0x3F);
                    JoyY = (byte)(data[offset + 1] & 0x3F);
                    sb.Append("JoyX: " + JoyX.ToString("X2"));
                    sb.Append("\n");
                    sb.Append("JoyY: " + JoyY.ToString("X2"));
                    sb.Append("\n");

                    // Crossfader
                    Crossfade = (byte)((data[offset + 2] & 0x1e) >> 1);
                    // Dial
                    Dial = (byte)(
                        ((data[offset + 2] & 0x60) >> 2) |
                        ((data[offset + 3] & 0xe0) >> 5)
                        );
                    sb.Append("Crossfade: " + Crossfade.ToString("X2"));
                    sb.Append("\n");
                    sb.Append("Dial: " + Dial.ToString("X2"));
                    sb.Append("\n");

                    // Right Turntable
                    RightTable = (byte)((
                        ((data[offset + 2] & 0x01) << 5) |
                        ((data[offset] & 0xc0) >> 3) |
                        ((data[offset + 1] & 0xc0) >> 5) |
                        ((data[offset + 2] & 0x80) >> 7)
                        ) % 64);
                    // Left Turntable
                    LeftTable = (byte)((
                        ((data[offset + 4] & 0x01) << 5) |
                        (data[offset + 3] & 0x1f)
                        ) % 64);
                    sb.Append("RightTable: " + Convert.ToString(RightTable, 2).PadLeft(8, '0'));
                    sb.Append("\n");
                    sb.Append("LeftTable: " + Convert.ToString(LeftTable, 2).PadLeft(8, '0'));
                    sb.Append("\n");

                    Prompt(sb.ToString(), isBold: true, isItalic: false, isSmall: false, isDebug: false);

                    //System.Threading.Thread.Sleep(16);
                }
            }
#endif
        }

        private void Prompt(string text, bool isBold = false, bool isItalic = false, bool isSmall = false, bool isDebug = false)
        {
            WiitarDebug.Log("SYNC WINDOW OUTPUT: \n\n" + text + "\n\n");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                var newInline = new System.Windows.Documents.Run(text);

                newInline.FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal;
                newInline.FontStyle = isItalic ? FontStyles.Italic : FontStyles.Normal;

                if (isSmall)
                {
                    newInline.FontSize *= 0.75;
                }

                if (isDebug)
                {
                    newInline.Foreground = System.Windows.Media.Brushes.Gray;
                }

                var newParagraph = new System.Windows.Documents.Paragraph(newInline);

                newParagraph.Padding = new Thickness(0);
                newParagraph.Margin = new Thickness(0);

                prompt.Blocks.Clear();
                prompt.Blocks.Add(newParagraph);

                promptBoxContainer.ScrollToEnd();

                //if (prompt.LineCount > 0)
                //    prompt.ScrollToLine(prompt.LineCount - 1);
                //prompt.ScrollToEnd();
            }));
        }

        //private void SetPrompt(string text)
        //{
        //    Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        prompt.Text = text;
        //        if (prompt.LineCount > 0)
        //            prompt.ScrollToLine(prompt.LineCount - 1);
        //        //prompt.ScrollToEnd();
        //    }));
        //}

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            //if (_notCompatable)
            //{
            //    Close();
            //}

            Prompt("Stopping...");
            Cancelled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Cancelled/* && Count == 0 && !_notCompatable*/)
            {
                Cancelled = true;
                Prompt("Stopping...");
                e.Cancel = true;
            }

            //if (Count > 0)
            //{
            //    MessageBox.Show("Device connected successfully. Give Windows up to a few minutes to install the drivers and it will show up in the list on the left.", "Device Found", MessageBoxButton.OK, MessageBoxImage.Information);
            //}

            if (nintroller != null)
                nintroller.StateUpdate -= Nintroller_StateUpdate;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Task t = new Task(() =>
            //{
            //    Application.Current.Dispatcher.BeginInvoke(new Action(() => 
            //    {
            //        prompt.Blocks.Clear();
            //    }), System.Windows.Threading.DispatcherPriority.Background);
            //});
            //t.Start();
        }
    }
}
