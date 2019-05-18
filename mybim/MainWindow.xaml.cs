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
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using System.ComponentModel;

namespace mybim1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window//, INotifyPropertyChanged
    {
        string ifcFileName;

        public MainWindow()
        {
            InitializeComponent();
        }

        /*用于本窗口绑定数据：打开文件路径
        #region 用于本窗口绑定数据：打开文件路径
        public event PropertyChangedEventHandler PropertyChanged;

        private string m_FileName;
        public string FileName //记录打开文件路径
        {
            get
            {
                return m_FileName;
            }
            set
            {
                if (value != m_FileName)
                {
                    m_FileName = value;
                    //Notify("FileName");
                    if (PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("FileName"));
                    }
                }
            }
        }

        #endregion
        */

        private void OpenCommand(object sender, RoutedEventArgs e)
        {

            #region 打开IFC文件

            //MessageBox.Show("Open命令被触发了，命令源是:" + e.Source.ToString());

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "打开IFC文件",
                Filter = "IFC文件(*.ifc)|*.ifc",
                FileName = string.Empty,
                FilterIndex = 1,
                ValidateNames = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,//仅允许选择一个文件打开
                RestoreDirectory = false,
                DefaultExt = "ifc"
            };
            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }
            else
            {
                //记录选择的文件路径
                ifcFileName = openFileDialog.FileName;

                //以备选择多个文件打开
                //string[] ifcFiles = openFileDialog.FileNames;
                //fileNamesTextBox.Text = "";
                //foreach (string ifcFile in ifcFiles)
                //{
                //    MessageBox.Show("已选择文件:" + ifcFile, "选择文件提示");
                //    fileNamesTextBox.Text += ifcFile+"\n";
                //}

                //直接调用TextBox属性显示打开文件路径
                //TextBox.Text = openFileDialog.ifcFileName;

                // 设置Window对象的DataContext属性，以供绑定
                OpenFile openFile = new OpenFile { FileName = ifcFileName };
                fileNamesTextBox.DataContext = openFile;

            }

            #endregion


            #region 显示IfcSite上的basic properties

            using (var model = IfcStore.Open(ifcFileName))
            {
                // get site in the model (using IFC4 interface of IfcSite)
                var allSites = model.Instances.OfType<IIfcSite>();

                if (allSites.Count() == 0)
                {
                    MessageBox.Show("No IfcSite!");
                    return;
                }

                IIfcSite theSite = allSites.First();

                //MessageBox.Show(theSite.ToString());
                //MessageBox.Show($"Site ID: {theSite.GlobalId}, Name: {theSite.Name}");

                // 获取site的所有属性
                var properties = theSite.IsDefinedBy
                    .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                    .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                    .OfType<IIfcPropertySingleValue>();

                //定义一个属性的列表,设置给listview的数据源
                List<SiteProperty> propertyList = new List<SiteProperty>();
                
                foreach (var property in properties)
                {
                    //MessageBox.Show($"Property: {property.Name}, Value: {property.NominalValue}");

                    //将获取的属性添加至列表
                    propertyList.Add(new SiteProperty { PropertyName = property.Name, PropertyValue = property.NominalValue.ToString() });
                    
                }

                propertyListView.ItemsSource = propertyList;



            }

            #endregion

        }
        
    }
    
    //IFC属性数据
    public class SiteProperty : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //IFC属性名
        private string m_PropertyName;
        public string PropertyName
        {
            get
            {
                return m_PropertyName;
            }
            set
            {
                if (value != m_PropertyName)
                {
                    m_PropertyName = value;
                    Notify("PropertyName");
                }
            }
        }

        //IFC属性值
        private string m_PropertyValue;
        public string PropertyValue
        {
            get
            {
                return m_PropertyValue;
            }
            set
            {
                if (value != m_PropertyValue)
                {
                    m_PropertyValue = value;
                    Notify("PropertyValue");
                }
            }
        }
    }

    //IFC属性数据
    public class OpenFile : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //IFC属性名
        private string m_FileName;
        public string FileName
        {
            get
            {
                return m_FileName;
            }
            set
            {
                if (value != m_FileName)
                {
                    m_FileName = value;
                    Notify("FileName");
                }
            }
        }
        
    }


}
