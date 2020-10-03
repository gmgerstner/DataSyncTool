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

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
