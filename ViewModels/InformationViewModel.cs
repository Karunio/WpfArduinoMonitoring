using MahApps.Metro.Controls.Dialogs;
using System.Reflection;
using System.Windows;
using WpfMonitoring.Core;

namespace WpfMonitoring.ViewModels
{
    class InformationViewModel : PropertyChangedBase
    {
        private Assembly assembly;
        private Assembly Assembly
        {
            get => assembly;
            set
            {
                assembly = value;
                NotifyPropertyChanged(() => ProductName);
                NotifyPropertyChanged(() => Version);
                NotifyPropertyChanged(() => CopyRight);
                NotifyPropertyChanged(() => Company);
            }
        }

        public string ProductName => Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;

        public string Version => $"Version : {Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version}";

        public string CopyRight => Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

        public string Company => $"Company : {Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company}";

        public string Description => Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

        public InformationViewModel()
        {
            assembly = Assembly.GetExecutingAssembly();
        }
    }
}
