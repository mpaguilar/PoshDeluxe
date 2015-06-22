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
            public Action<String> VerboseWriter { get {
                return Writer; } 
            }
            public Action<String> DebugWriter
            {
                get
                {
                    return Writer;
                }
            }
            public Action<String> WarningWriter
            {
                get
                {
                    return Writer;
                }
            }
            public Action<String> ErrorWriter
            {
                get
                {
                    return Writer;
                }
            }

            public List<String> VerboseMessages = new List<String>();

            private void Writer(String msg)
            {
                VerboseMessages.Add(msg);
            }
        }
        
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
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

                var writer = new PmGuiWriter();
                var netWait = netModule.Refresh(writer);
                Task.WaitAll(new[] { netWait });

                writer.VerboseMessages
                    .ForEach(m => listVerbose.Items.Add(m));                
            }
        }
    }
}
