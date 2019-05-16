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

namespace mybim
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenCommand(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Open命令被触发了，命令源是:" + e.Source.ToString());

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = "选择IFC文件";
            openFileDialog.Filter = "IFC文件(*.ifc)|*.ifc";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.ValidateNames = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = false;
            openFileDialog.DefaultExt = "ifc";
            //openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }
            else
            {
                string[] ifcFiles = openFileDialog.FileNames;
                fileNamesTextBox.Text = "";
                foreach (string ifcFile in ifcFiles)
                {
                    //System.Windows.MessageBox.Show("已选择文件:" + ifcFile, "选择文件提示");
                    fileNamesTextBox.Text += ifcFile+"\n";
                }
            }
        }
    }
}
