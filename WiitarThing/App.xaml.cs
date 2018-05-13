using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Windows;

namespace WiinUSoft
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        internal const string PROFILE_FILTER = "WiinUSoft Profile|*.wsp";
        private const string Unique = "wiinupro-or-wiinusoft-instance";

        [STAThread]
        public static void Main()
        {
            System.Threading.Thread.Sleep(250);

            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                var application = new App();
                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            MessageBox.Show("WiitarThing is about to crash. Press OK to show the crash message.", "Sorry", MessageBoxButton.OK, MessageBoxImage.Error);

            Exception e = (Exception)args.ExceptionObject;
            
            SingleInstance<App>.Cleanup();
            Current.Dispatcher.Invoke(new Action(() => 
            {
                var box = new ErrorWindow(e);
                box.ShowDialog();
            }));
        }

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
