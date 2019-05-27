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
    

    class ClsIfcProperty : ClsNotifyobject// : INotifyPropertyChanged
    {
        //IFC属性名
        private string _propertyName;
        public string PropertyName
        {
            get
            {
                return _propertyName;
            }
            set
            {
                if (value != _propertyName)
                {
                    _propertyName = value;
                    Notify("PropertyName");
                }
            }
        }

        //IFC属性值
        private string _propertyValue;
        public string PropertyValue
        {
            get
            {
                return _propertyValue;
            }
            set
            {
                if (value != _propertyValue)
                {
                    _propertyValue = value;
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
