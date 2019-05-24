# mybim

本项目是一个简单的WPF应用程序，使用C#编写，用于查看IFC文件。读取IFC文件利用[Xbim.Essentials](https://www.nuget.org/packages/Xbim.Essentials/)实现。
本项目仅供本人学习使用。

## 编译|Compilation

最好使用Visual Studio 2017与.NET Framework 4.7.2——[VS2017社区版（免费）](https://visualstudio.microsoft.com/downloads/)。

## 功能|Features List

- 有一个打开按钮，可以打开ifc文件，打开完成后在窗口程序里显示打开的IFC文件地址。
- 显示`IfcSite`上的属性，包括属性集、属性名、属性值。
- 属性值允许编辑，并可保存/另存为IFC文件。
