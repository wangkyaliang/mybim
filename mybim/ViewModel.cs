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

        //文件模型
        public IfcStore Model { get; set; }

        //Site
        public IIfcSite TheSite{ get; set; }
        
        //文件路径
        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                Notify("FilePath");
            }
        }
        
        //文件属性
        private List<ClsIfcProperty> _propertyList;
        public List<ClsIfcProperty> PropertyList
        {
            get { return _propertyList; }
            set
            {
                _propertyList = value;
                Notify("PropertyList");
            }
        }
        
        //“打开”命令
        private MyCommand _cmdOpenFile;
        public MyCommand CmdOpenFile
        {
            get
            {
                if (_cmdOpenFile == null)
                {
                    _cmdOpenFile = new MyCommand(new Action<object>
                    (
                         o =>
                         {

                             //窗口选择打开IFC文件，获取返回文件路径
                             //文件路径赋值给MainWindowViewModel的FilePath属性
                             FilePath = OpenIfcFile();
                             if (FilePath == null){ return; }
                             
                             //获取site的属性列表
                             //属性列表赋值给MainWindowViewModel的PropertyList属性
                             PropertyList = GetIfcSiteProperties(FilePath);
                             if (PropertyList == null)
                             {
                                 return;
                             }
                             else if (PropertyList.Count == 0)
                             {
                                 MessageBox.Show("No property!");
                             }

                         }
                    ));
                }
                return _cmdOpenFile;
            }
        }
        

        //“保存”命令
        private MyCommand _cmdSaveFile;
        public MyCommand CmdSaveFile
        {
            get
            {
                if (_cmdSaveFile == null)
                {
                    _cmdSaveFile = new MyCommand(new Action<object>(
                         o =>
                         {
                             //检查是否有模型和属性
                             if (Model == null)
                             {
                                 MessageBox.Show("No Model!");
                                 return;
                             }
                             else if (PropertyList == null)
                             {
                                 MessageBox.Show("No Property!");
                                 return;
                             }

                             //修改Model的IFC信息
                             using (Model)
                             {
                                 // open transaction for changes
                                 using (var txn = Model.BeginTransaction("Site modification"))
                                 {

                                     //遍历属性列表，根据属性集与属性名给ifc属性重新赋值
                                     foreach (var property in PropertyList)
                                     {
                                         TheSite.IsDefinedBy
                                         .Where(r => r.RelatingPropertyDefinition!=null && r.RelatingPropertyDefinition == property.PropertySet) //属性集符合
                                         .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                                         .OfType<IIfcPropertySingleValue>()
                                         .Where(r => r.Name != null && r.Name == property.PropertyName) //属性名符合
                                         .First()
                                         .NominalValue = (IfcText)property.PropertyValue; //修改属性值
                                     }
                                     
                                     // commit changes
                                     txn.Commit();
                                 }

                                 //打开保存窗口
                                 string savePath = SaveIfcFile();
                                 Model.SaveAs(savePath);
                                 MessageBox.Show("已保存");
                                 //更新MainWindowViewModel的FilePath属性
                                 FilePath = savePath;

                             }
                             
                         }
                         ));

                }
                return _cmdSaveFile;
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
                
            }

            return fileName;
        }


        //窗口保存IFC文件，返回新的文件路径
        public string SaveIfcFile()
        {
            string saveFileName = null;

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "保存IFC文件",
                Filter = "IFC文件(*.ifc)|*.ifc",
                FileName = string.Empty,
                FilterIndex = 1,
                ValidateNames = true,
                RestoreDirectory = false,
                DefaultExt = "ifc"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                //记录选择的文件路径
                saveFileName = saveFileDialog.FileName;
                
            }

            return saveFileName;
        }

        //获取site的属性列表
        public List<ClsIfcProperty> GetIfcSiteProperties(string ifcFileName)
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

            //定义一个属性的列表,设置给listview的数据源
            List<ClsIfcProperty> propertyList = new List<ClsIfcProperty>();

            //// 获取site的所有属性
            //var properties = TheSite.IsDefinedBy
            //    .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
            //    .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
            //    .OfType<IIfcPropertySingleValue>();

            // 获取site的所有属性集
            var propertySets = TheSite.IsDefinedBy
                .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                .Select(r => (IIfcPropertySet)r.RelatingPropertyDefinition)
                .OfType<IIfcPropertySet>();

            foreach (var propertySet in propertySets)
            {
                //var pSetName=propertySet.Name;
                var ps = propertySet.HasProperties.OfType<IIfcPropertySingleValue>();
                foreach (var p in ps)
                {
                    //将获取的属性添加至列表
                    propertyList.Add(new ClsIfcProperty
                    {
                        PropertyName = p.Name,
                        PropertyValue = p.NominalValue.ToString(),
                        PropertySet = propertySet,
                        PropertySetName= propertySet.Name
                    });
                }
                
            }

            if (propertyList == null)
            {
                MessageBox.Show("No Property!");
            }
            
            return propertyList;
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
