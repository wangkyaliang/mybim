using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using System.ComponentModel;
using System.Windows.Input;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.MeasureResource;
using System.IO;

namespace mybim
{
    class MainWindowViewModel : ClsNotifyobject
    {

        //文件数据
        //private string _FileText;
        public string FileText{ get; set; }

        //文件模型
        public IfcStore Model { get; set; }

        //Site
        public IIfcSite TheSite{ get; set; }
        
        //文件路径
        private string _FilePath;
        public string FilePath
        {
            get { return _FilePath; }
            set
            {
                _FilePath = value;
                Notify("FilePath");
            }
        }
        
        //文件属性
        private List<ClsIfcProperty> _PropertyList;
        public List<ClsIfcProperty> PropertyList
        {
            get { return _PropertyList; }
            set
            {
                _PropertyList = value;
                Notify("PropertyList");
            }
        }
        
        //“打开”命令
        private MyCommand _CmdOpenFile;
        public MyCommand CmdOpenFile
        {
            get
            {
                if (_CmdOpenFile == null)
                {
                    _CmdOpenFile = new MyCommand(new Action<object>
                    (
                         o =>
                         {

                             //窗口选择打开IFC文件，获取返回文件路径
                             string fileName = OpenIfcFile();
                             if (fileName == null)
                             {
                                 return;
                             }

                             //文件路径赋值给MainWindowViewModel的FilePath属性
                             FilePath = fileName;

                             //获取IfcSite上的basic properties
                             IEnumerable<IIfcPropertySingleValue> properties = GetIfcSiteProperties(FilePath);

                             //定义一个属性的列表,设置给listview的数据源
                             List<ClsIfcProperty> propertyList = new List<ClsIfcProperty>();

                             if (properties != null)
                             {

                                 foreach (var property in properties)
                                 {
                                     //MessageBox.Show($"Property: {property.Name}, Value: {property.NominalValue}");

                                     //将获取的属性添加至列表
                                     propertyList.Add(new ClsIfcProperty { PropertyName = property.Name, PropertyValue = property.NominalValue.ToString() });
                                 }
                                 //propertyListView.ItemsSource = propertyList;
                             }

                             //属性列表赋值给MainWindowViewModel的PropertyList属性
                             PropertyList = propertyList;

                         }
                    ));
                }
                return _CmdOpenFile;
            }
        }
        

        //“保存”命令
        private MyCommand _CmdSaveFile;
        public MyCommand CmdSaveFile
        {
            get
            {
                if (_CmdSaveFile == null)
                {
                    _CmdSaveFile = new MyCommand(new Action<object>
                    (
                         o =>
                         {

                             if (PropertyList == null)
                             {
                                 return;
                             }
                             /*
                             //test：查看ifc属性是否更改
                             foreach (var property in PropertyList)
                             {
                                 MessageBox.Show($"Property: {property.PropertyName}, Value: {property.PropertyValue}");
                             }
                             */

                             using (Model)
                             {
                                 // open transaction for changes
                                 using (var txn = Model.BeginTransaction("Doors modification"))
                                 {

                                     //test
                                     foreach (var property in PropertyList)
                                     {
                                         TheSite.IsDefinedBy
                                         .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                         .OfType<IIfcPropertySingleValue>()
                                         .Where(r => r.Name == property.PropertyName)
                                         .First()
                                         .NominalValue = (IfcText)property.PropertyValue;
                                     }
                                     
                                     // commit changes
                                     txn.Commit();
                                 }
                                 Model.SaveAs(@"D:\城建工作\BIM数字化交付平台2019\test111");
                                

                             }
                         }

                    ));
                       
                }
                return _CmdSaveFile;
            }
        }


        //窗口选择打开IFC文件，返回文件路径
        public string OpenIfcFile()
        {
            string fileName = null;

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

            if (openFileDialog.ShowDialog() == true)
            {
                //记录选择的文件路径
                fileName = openFileDialog.FileName;

                //记录文件数据至MainWindowViewModel的FileText属性
                FileText = File.ReadAllText(fileName);


                //MessageBox.Show(fileText.Count().ToString());

                //以备选择多个文件打开
                //string[] ifcFiles = openFileDialog.FileNames;
                //fileNamesTextBox.Text = "";
                //foreach (string ifcFile in ifcFiles)
                //{
                //    MessageBox.Show("已选择文件:" + ifcFile, "选择文件提示");
                //    fileNamesTextBox.Text += ifcFile+"\n";
                //}

            }

            return fileName;
        }

        //获取site的属性列表
        public IEnumerable<IIfcPropertySingleValue> GetIfcSiteProperties(string ifcFileName)
        {
            var model = IfcStore.Open(ifcFileName);
            
            //MainWindowViewModel的Model属性
            Model = model;

            //获取site (using IFC4 interface of IfcSite)，赋值给MainWindowViewModel的TheSite属性
            var allSites = model.Instances.OfType<IIfcSite>();

            if (allSites.Count() == 0)
            {
                MessageBox.Show("No IfcSite!");
                return null;
            }

            TheSite = allSites.First();

            //MessageBox.Show(theSite.ToString());
            //MessageBox.Show($"Site ID: {theSite.GlobalId}, Name: {theSite.Name}");

            // 获取site的所有属性
            var properties = TheSite.IsDefinedBy
                .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                .OfType<IIfcPropertySingleValue>();

            if (properties.Count() == 0)
            {
                MessageBox.Show("IfcSite has no property!");
            }

            return properties;

        }
    }

    //自定义命令
    public class MyCommand : ICommand
    {
        #region ICommand内部方法的实现
        public event EventHandler CanExecuteChanged
        {
            //Command检测到可能会影响到命令的可执行状态时触发RequerySuggested事件；
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        //监听了该事件命令的对象调用 CanExecute(object parameter)方法检测命令是否可以执行，并将bool类型的返回值设置到绑定该控件的IsEnabled属性上
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (_execute != null && CanExecute(parameter))
            {
                _execute(parameter);
            }
        }
        #endregion
        #region ClsMyCommds用户自定义
        //构造函数中传入Action<object>和Func<object,bool>，让CanExecute执行Func<object,bool>，Execute执行Action<object>。
        private Func<object, bool> _canExecute;
        private Action<object> _execute;
        public MyCommand(Action<object> execute) : this(execute, null)
        {
        }

        public MyCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion
    }
}
