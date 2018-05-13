using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WiinUSoft.Windows
{
    /// <summary>
    /// Interaction logic for RemoveAllWiimotesWindow.xaml
    /// </summary>
    public partial class RemoveAllWiimotesWindow : Window
    {
        System.Threading.Thread workThread;

        public RemoveAllWiimotesWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            workThread = new System.Threading.Thread(() =>
            {
                //Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                //{
                SyncWindow.RemoveAllWiimotes();
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Close()));
                //}));
            });

            workThread.IsBackground = true;

            workThread.Start();

            //Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    SyncWindow.RemoveAllWiimotes();
            //    Close();
            //}));
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            //if (workThread != null && workThread.IsAlive)
            //{
            //    workThread.Abort();

            //}
        }
    }
}
