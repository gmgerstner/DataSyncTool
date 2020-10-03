using GMG.DataSyncTool.Library;
using Microsoft.Win32;
using System.Windows;

namespace GMG.DataSyncTool.WpfUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string SourceConnectionString { get; set; }
        private string TargetConnectionString { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectSourceDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Wpf.SQLConnectionDialog();
            if (dlg.ShowDialog() ?? false)
            {
                var connectionstring = dlg.ConnectionString;
                SourceConnectionString = connectionstring;
                SourceDatabaseField.Text = connectionstring;
            }
        }

        private void SelectTargetDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Wpf.SQLConnectionDialog();
            if (dlg.ShowDialog() ?? false)
            {
                var connectionstring = dlg.ConnectionString;
                TargetConnectionString = connectionstring;
                TargetDatabaseField.Text = connectionstring;
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string sql = "";
            using (Synchronizer sync = new Synchronizer(SourceConnectionString, TargetConnectionString))
            {
                sql = sync.GenerateScript();
                var dlg = new SaveFileDialog();
                dlg.DefaultExt = ".sql";
                dlg.Title = "Save SQL File";
                dlg.Filter = "SQL Files (*.sql)|*.sql|All Files (*.*)|*.*";
                if (dlg.ShowDialog(this) ?? false)
                {
                    var filename = dlg.FileName;
                    System.IO.File.WriteAllText(filename, sql);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
