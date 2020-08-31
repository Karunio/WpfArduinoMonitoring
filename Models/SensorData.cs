using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMonitoring.Models
{
    class SensorData
    {
        public static readonly short MinValue = 0;

        public static readonly short MaxValue = 1023;

        public static int BaudRate = 9600;

        public DateTime Time { get; set; }
        private short value;
        public short Value
        {
            get => value;
            set
            {
                if (value < MinValue || MaxValue < value)
                {
                    throw new ArgumentOutOfRangeException();
                }
                this.value = value;
            }
        }

        public SensorData(DateTime time, short value)
        {
            Time = time;
            Value = value;
        }
    }
}
