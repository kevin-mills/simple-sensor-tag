using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace SensorTag.SensorTag
{
    public class Gyroscope : SensorBase
    {
        public GyroscopeAxis GyroscopeAxis { get; private set; }

        public Gyroscope()
            : base(SensorNames.Gyroscope, SensorTagUuid.UUID_GYR_SERV, SensorTagUuid.UUID_GYR_CONF, SensorTagUuid.UUID_GYR_DATA)
        {
            
        }

        /// <summary>
        /// Calculates the value of the different gyroscope axis and scales it.
        /// </summary>
        /// <param name="data">Complete array of data retrieved from the sensor</param>
        /// <param name="axis">Specifies the axis the gyroscope was configured to read</param>
        /// <returns>Array of float with values in order of the GyroscopeAxis enum</returns>
        public static float[] CalculateAxisValue(byte[] data, GyroscopeAxis axis)
        {
            switch(axis)
            {
                case GyroscopeAxis.X:
                case GyroscopeAxis.Y:
                case GyroscopeAxis.Z:
                    return new float[] { BitConverter.ToInt16(data, 0) * (500f / 65536f) };
                case GyroscopeAxis.XY:
                case GyroscopeAxis.XZ:
                case GyroscopeAxis.YZ:
                    return new float[] { BitConverter.ToInt16(data, 0) * (500f / 65536f), 
                        BitConverter.ToInt16(data, 2) * (500f / 65536f) };
                case GyroscopeAxis.XYZ:
                    return new float[] { BitConverter.ToInt16(data, 0) * (500f / 65536f), 
                        BitConverter.ToInt16(data, 2) * (500f / 65536f), 
                        BitConverter.ToInt16(data, 4) * (500f / 65536f) };
                default:
                    return new float[] { 0, 0, 0 };
            }
        }

        /// <summary>
        /// Enables the sensor with the specified axis
        /// </summary>
        /// <param name="gyroscopeAxis">axis you want to record</param>
        public async Task EnableSensor(GyroscopeAxis gyroscopeAxis = SensorTag.GyroscopeAxis.XYZ)
        {
            GyroscopeAxis = gyroscopeAxis;
            await base.EnableSensor(new byte[] { (byte)gyroscopeAxis });
        }

        /// <summary>
        /// Enables the sensor to read all axis
        /// </summary>
        /// <returns></returns>
        public override async Task EnableSensor()
        {
            await EnableSensor(GyroscopeAxis.XYZ);
        }
    }

    /// <summary>
    /// Different options for reading values from the sensor
    /// </summary>
    public enum GyroscopeAxis
    {
        X = 1,
        Y = 2,
        XY = 3,
        Z = 4,
        XZ = 5,
        YZ = 6,
        XYZ = 7
    }
}
