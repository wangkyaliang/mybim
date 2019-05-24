using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace mybim
{
    
    class ClsNotifyobject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Notify(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
    /*
    class ClsIfcFileName : ClsNotifyobject// : INotifyPropertyChanged
    {
        //IFC路径
        private string _FileName;
        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                if (value != _FileName)
                {
                    _FileName = value;
                    Notify("FileName");
                }
            }
        }
    }
    */
    class ClsIfcProperty : ClsNotifyobject// : INotifyPropertyChanged
    {
        //IFC属性名
        private string _PropertyName;
        public string PropertyName
        {
            get
            {
                return _PropertyName;
            }
            set
            {
                if (value != _PropertyName)
                {
                    _PropertyName = value;
                    Notify("PropertyName");
                }
            }
        }

        //IFC属性值
        private string _PropertyValue;
        public string PropertyValue
        {
            get
            {
                return _PropertyValue;
            }
            set
            {
                if (value != _PropertyValue)
                {
                    _PropertyValue = value;
                    Notify("PropertyValue");
                }
            }
        }

        //IFC属性集
        public IIfcPropertySet PropertySet { get; set; }

        //IFC属性集名
        public string PropertySetName { get; set; }

    }
}
