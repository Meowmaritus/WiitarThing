using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Windows;

namespace WiitarThing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        internal const string PROFILE_FILTER = "WiitarThing Profile|*.wsp";

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // show the original instance
            if (this.MainWindow.WindowState == WindowState.Minimized)
            {
                ((MainWindow)this.MainWindow).ShowWindow();
            }

            this.MainWindow.Activate();

            MessageBox.Show("WiitarThing was already Running so the previous instance was brought into focus.",
                "WiitarThing Already Running", MessageBoxButton.OK, MessageBoxImage.Information);

            return true;
        }
    }
}
