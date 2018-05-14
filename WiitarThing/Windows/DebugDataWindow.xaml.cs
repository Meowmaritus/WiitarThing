using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Shared.Windows;
using System.ComponentModel;
using System.Windows.Interop;
using NintrollerLib;

namespace WiinUSoft.Windows
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
            var wgt = new WiiGuitar();

            var sb = new StringBuilder();

            if (!Cancelled)
            {
                if (e.state is WiiGuitar)
                {
                    wgt = (WiiGuitar)e.state;

                    sb.Clear();

                    for (int i = 0; i < wgt.DebugLastData.Length; i++)
                    {
                        sb.Append(wgt.DebugLastData[i].ToString("X2"));
                        sb.Append(" ");
                    }

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
