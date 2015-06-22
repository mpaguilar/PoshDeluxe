using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Text;

using PoshManager;

namespace PoshManagerGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public class PmGuiWriter : IPoshStream
        {
            public class PoshMessage
            {
                public Brush MessageColor {get; set;}
                public String Message {get;set;}
            }
            public enum MessageType
            {
                Verbose,
                Debug,
                Warning,
                Error
            }

            public Action<String> VerboseWriter { get {
                return Writer(MessageType.Verbose); } 
            }
            public Action<String> DebugWriter
            {
                get
                {
                    return Writer(MessageType.Debug);
                }
            }
            public Action<String> WarningWriter
            {
                get
                {
                    return Writer(MessageType.Warning);
                }
            }
            public Action<String> ErrorWriter
            {
                get
                {
                    return Writer(MessageType.Error);
                }
            }

            public List<String> VerboseMessages = new List<String>();
            public List<PoshMessage> Messages = new List<PoshMessage>();

            private Action<String> Writer(MessageType messageType )
            {
                var color = Brushes.Aqua;
                SolidColorBrush brush = null;
                switch (messageType)
                {
                    case MessageType.Verbose:
                        brush = Brushes.Gray;
                        break;

                    case MessageType.Debug:
                        brush = Brushes.Blue;
                        break;

                    case MessageType.Error:
                        brush = Brushes.Red;
                        break;

                    case MessageType.Warning:
                        brush = Brushes.YellowGreen;
                        break;

                    default:
                        brush = Brushes.Black;
                        break;
                }
                return (msg) =>
                {
                    Messages.Add(new PoshMessage { MessageColor = brush, Message = msg });
                };
            }
        }
        
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            
            var writer = new PmGuiWriter();
            listMessages.ItemsSource = writer.Messages;

            var computerName = txtComputerName.Text;
            using (ManagerShell mgr = new ManagerShell())
            {
                var netModule = new NetModule(
                    mgr.GetPowerShell(),
                    computerName
                    );

                var diskModule = new DiskModule(
                    mgr.GetPowerShell(),
                    computerName
                    );
                
                var netWait = netModule.Refresh(writer);
                var diskWait = diskModule.Refresh(writer);

                Task.WaitAll(new[] { netWait, diskWait });

                
                listNetworkAdapters.ItemsSource = netModule.NetworkAdapters;
                listDisks.ItemsSource = diskModule.DiskDrives;
            }

            
        }
    }
}
