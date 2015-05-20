using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorTag.SensorTag
{
    public class SensorValueChangedEventArgs : EventArgs
    {
        public SensorValueChangedEventArgs(byte[] rawData, DateTimeOffset timestamp, SensorNames origin)
        {
            RawData = rawData;
            Origin = origin;
            Timestamp = timestamp;
        }

        public SensorNames Origin { get; private set; }

        public byte[] RawData { get; private set; }

        public DateTimeOffset Timestamp { get; private set; }
    }
}
