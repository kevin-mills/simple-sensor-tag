using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;

namespace SensorTag.SensorTag
{
    public class Magnetometer : SensorBase
    {
        public Magnetometer()
            : base(SensorNames.Magnetometer, SensorTagUuid.UUID_MAG_SERV, SensorTagUuid.UUID_MAG_CONF, SensorTagUuid.UUID_MAG_DATA)
        {

        }

        /// <summary>
        /// Extracts the three axis from the raw sensor data and scales it.
        /// http://cache.freescale.com/files/sensors/doc/app_note/AN4248.pdf?fpsp=1
        /// </summary>
        public static float[] CalculateCoordinates(byte[] sensorData)
        {
            return new float[] { BitConverter.ToInt16(sensorData, 0) * (2000f / 65536f), 
                BitConverter.ToInt16(sensorData, 2) * (2000f / 65536f), 
                BitConverter.ToInt16(sensorData, 4) * (2000f / 65536f)};
        }

        /// <summary>
        /// Sets the period the sensor reads data. Default is 1s. Lower limit is 100ms.
        /// </summary>
        /// <param name="time">Period in 10 ms</param>
        public async Task SetReadPeriod(byte time)
        {
            if (time < 10) Debug.WriteLine("Period can't be lower than 100ms");

            GattCharacteristic dataCharacteristic = gattDeviceService.GetCharacteristics(new Guid(SensorTagUuid.UUID_MAG_PERI))[0];

            byte[] data = new byte[] { time };
            GattCommunicationStatus status = await dataCharacteristic.WriteValueAsync(data.AsBuffer());
            if (status == GattCommunicationStatus.Unreachable)
            {
                Debug.WriteLine("Magnetometer unreachable");
            }
        }
    }
}
