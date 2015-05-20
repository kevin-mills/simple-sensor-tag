using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SensorTag.SensorTag
{
    public class Accelerometer : SensorBase
    {
        public Accelerometer()
            : base(SensorNames.Accelerometer, SensorTagUuid.UUID_ACC_SERV, SensorTagUuid.UUID_ACC_CONF, SensorTagUuid.UUID_ACC_DATA)
        {
            
        }
        
        /// <summary>
        /// Extracts the values of the 3 axis from the raw data of the sensor
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <param name="scale">Allows you to scale the accelerometer values</param>
        /// <returns>Array of doubles with the size of 3</returns>
        public static double[] CalculateCoordinates(byte[] sensorData, double scale = 1.0)
        {
            if (scale == 0) Debug.WriteLine("Scale cannot be 0.");
            return new double[] { sensorData[0] * scale, sensorData[1] * scale, sensorData[2] * scale };
        }

        /// <summary>
        /// Sets the period the sensor reads data. Default is 1s. Lower limit is 100ms.
        /// </summary>
        /// <param name="time">Period in 10 ms.</param>
        public async Task SetReadPeriod(byte time)
        {
            if (time < 10) Debug.WriteLine("Period can't be lower than 100ms");

            GattCharacteristic dataCharacteristic = gattDeviceService.GetCharacteristics(new Guid(SensorTagUuid.UUID_ACC_PERI))[0];

            byte[] data = new byte[] { time };
            GattCommunicationStatus status = await dataCharacteristic.WriteValueAsync(data.AsBuffer());
            if(status == GattCommunicationStatus.Unreachable)
            {
                Debug.WriteLine("Accelerometer unreachable!");
            }
        }
    }
}
