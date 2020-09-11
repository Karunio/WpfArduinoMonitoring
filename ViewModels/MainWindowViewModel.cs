using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfMonitoring.Core;
using WpfMonitoring.Models;
using WpfMonitoring.Views;

namespace WpfMonitoring.ViewModels
{
    class MainWindowViewModel : PropertyChangedBase
    {
        #region Readonly Values
        public double ZoomInterval { get => 5; }
        private object SerialLock = new object();
        #endregion

        #region RelayCommands
        private RelayCommand openCommand;
        public RelayCommand OpenCommand
        {
            get
            {
                if (openCommand == null)
                {
                    openCommand = new RelayCommand(Open);
                }
                return openCommand;
            }
        }

        private RelayCommand saveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new RelayCommand(Save);
                }
                return saveCommand;
            }
        }

        private RelayCommand exitCommand;
        public RelayCommand ExitCommand
        {
            get
            {
                if (exitCommand == null)
                {
                    exitCommand = new RelayCommand((o) => Environment.Exit(0));
                }
                return exitCommand;
            }
        }

        private RelayCommand startSimulationCommand;
        public RelayCommand StartSimulationCommand
        {
            get
            {
                if (startSimulationCommand == null)
                {
                    startSimulationCommand = new RelayCommand(StartSimulation, CanStartSimulation);
                }
                return startSimulationCommand;
            }
        }

        private RelayCommand stopSimulationCommand;
        public RelayCommand StopSimulationCommand
        {
            get
            {
                if (stopSimulationCommand == null)
                {
                    stopSimulationCommand = new RelayCommand(StopSimulation, CanStopSimulation);
                }
                return stopSimulationCommand;
            }
        }

        private RelayCommand connectCommand;
        public RelayCommand ConnectCommand
        {
            get
            {
                if (connectCommand == null)
                {
                    connectCommand = new RelayCommand(async (o) => await Connect(o), CanConnect);
                }
                return connectCommand;
            }
        }

        private RelayCommand disconnectCommand;
        public RelayCommand DisconnectCommand
        {
            get
            {
                if (disconnectCommand == null)
                {
                    disconnectCommand = new RelayCommand(async (o) => await Disconnect(o), CanDisconnect);
                }
                return disconnectCommand;
            }
        }

        private RelayCommand viewAllCommand;
        public RelayCommand ViewAllCommand
        {
            get
            {
                if (viewAllCommand == null)
                {
                    viewAllCommand = new RelayCommand(ViewAll);
                }
                return viewAllCommand;
            }
        }

        private RelayCommand zoomCommand;
        public RelayCommand ZoomCommand
        {
            get
            {
                if (zoomCommand == null)
                {
                    zoomCommand = new RelayCommand(Zoom);
                }
                return zoomCommand;
            }
        }

        private RelayCommand openInformationCommand;
        public RelayCommand OpenInformationCommand
        {
            get
            {
                if (openInformationCommand == null)
                {
                    openInformationCommand = new RelayCommand(OepnInformation);
                }
                return openInformationCommand;
            }
        }
        #endregion

        #region Properties, Fields
        private ObservableCollection<string> portNames;
        public ObservableCollection<string> PortNames
        {
            get => portNames;
            set
            {
                portNames = value;
            }
        }

        private ObservableCollection<SensorData> sensorDatas;
        public ObservableCollection<SensorData> SensorDatas
        {
            get => sensorDatas;
            set
            {
                sensorDatas = value;
            }
        }

        private string port;
        public string Port
        {
            get => port;
            set
            {
                port = value;
                NotifyPropertyChanged();
            }
        }

        public SerialPort Serial { get; private set; }

        public ObservableLog Log { get; private set; }

        private DateTime connectTime;
        public DateTime ConnectTime
        {
            get => connectTime;
            set
            {
                connectTime = value;
                NotifyPropertyChanged();
            }
        }

        private short sensorValue;
        public short SensorValue
        {
            get => sensorValue;
            set
            {
                sensorValue = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsSimulating { get; private set; }

        public DispatcherTimer Timer { get; private set; }

        public ChartValues<short> ChartValuesList { get; private set; }

        private bool isZoom;
        private bool IsZoom
        {
            get => isZoom;
            set
            {
                isZoom = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(() => ChartStartPoint);
            }
        }

        public double ChartStartPoint
        {
            get
            {
                double result = ChartValuesList.Count - ZoomInterval;
                if (!IsZoom || result < 0)
                {
                    result = 0;
                }
                return result;
            }
        }
        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            Init();
        }

        #endregion
        private void Init()
        {
            Log = new ObservableLog();

            PortNames = new ObservableCollection<string>();
            AddPortNames(PortNames);

            SensorDatas = new ObservableCollection<SensorData>();

            Serial = new SerialPort();
            Serial.DataReceived += SerialDataReceived;

            IsSimulating = false;

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(500);
            Timer.Tick += Timer_Tick;

            ChartValuesList = new ChartValues<short>();
            ChartValuesList.CollectionChanged += ChartValuesList_CollectionChanged;

            IsZoom = false;
        }

        private void OepnInformation(object obj)
        {
            InformationView informationView = new InformationView();
            informationView.ShowDialog();
        }

        #region CollectionData Related Method
        private void DataListsClear()
        {
            ChartValuesList.Clear();
            Log.Clear();
            SensorDatas.Clear();
        }

        private void DataListsInsert(short value)
        {
            ChartValuesList.Add(value);
            Log.Append($"{value}\n");
            SensorDatas.Add(new SensorData(DateTime.Now, value));
        }
        #endregion

        #region File Open Save Related Method But Not Yet Implemented
        private void Open(object obj)
        {
            MessageBox.Show("Open");
        }

        private void Save(object obj)
        {
            MessageBox.Show("Close");
        }
        #endregion

        #region Simulation Related Method
        private void StartSimulation(object obj)
        {
            DataListsClear();
            IsSimulating = true;
            IsZoom = false;
            Timer.Start();
        }

        private bool CanStartSimulation(object obj)
        {
            if (Serial.IsOpen || IsSimulating)
            {
                return false;
            }
            return true;
        }

        private void StopSimulation(object obj)
        {
            Timer.Stop();
            IsSimulating = false;
        }

        private bool CanStopSimulation(object obj)
        {
            return IsSimulating;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            short value = (short)rand.Next(SensorData.MaxValue + 1);
            SensorValue = value;
            DataListsInsert(value);
        }
        #endregion

        #region Serial Related Method
        private async Task Connect(object obj)
        {
            DataListsClear();
            Serial.PortName = Port;
            Serial.BaudRate = SensorData.BaudRate;
            await Task.Run(() => Serial.Open());
            ConnectTime = DateTime.Now;
        }

        private bool CanConnect(object obj)
        {
            if (Serial.IsOpen ||
                string.IsNullOrEmpty(Port) ||
                IsSimulating)
            {
                return false;
            }

            return true;
        }

        private async Task Disconnect(object obj)
        {
            await Task.Run(() => Serial.Close());
        }

        private bool CanDisconnect(object obj)
        {
            return Serial.IsOpen;
        }

        private void AddPortNames(ObservableCollection<string> list)
        {
            foreach (var item in SerialPort.GetPortNames())
            {
                list.Add(item);
            }
        }

        private async void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = await Task.Run(() =>
            {
                string receivedData;
                lock (SerialLock)
                {
                    receivedData = Serial.ReadLine();
                }
                return receivedData;
            });

            try
            {
                short value = short.Parse(data);
                SensorData sensorData = new SensorData(DateTime.Now, value);
                SensorDatas.Add(sensorData);
                SensorValue = value;
                DataListsInsert(value);
            }
            catch (Exception ex)
            {
                Log.Append($"Error : {ex.Message}\n");
            }
        }
        #endregion

        #region Chart Related Method
        private void ViewAll(object obj)
        {
            IsZoom = false;
        }

        private void Zoom(object obj)
        {
            IsZoom = true;
        }

        private void ChartValuesList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(() => ChartStartPoint);
        }

        #endregion
    }
}
