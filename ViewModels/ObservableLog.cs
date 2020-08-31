using System.Text;
using WpfMonitoring.Core;

namespace WpfMonitoring.ViewModels
{
    class ObservableLog : PropertyChangedBase
    {
        public string Text
        {
            get => logBuilder.ToString();
        }

        private StringBuilder logBuilder;

        public ObservableLog()
        {
            logBuilder = new StringBuilder();
        }

        public void Append(string log)
        {
            logBuilder.Append(log);
            NotifyPropertyChanged(() => Text);
        }

        public void Clear()
        {
            logBuilder.Clear();
            NotifyPropertyChanged(() => Text);
        }
    }
}
