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
    public class IRTemperatureSensor : SensorBase
    {
        public IRTemperatureSensor()
            : base(SensorNames.TemperatureSensor, SensorTagUuid.UUID_IRT_SERV, SensorTagUuid.UUID_IRT_CONF, SensorTagUuid.UUID_IRT_DATA)
        {

        }

        /// <summary>
        /// Calculates the ambient temperature.
        /// </summary>
        public static double CalculateAmbientTemperature(byte[] sensorData, TemperatureScale scale)
        {
            if (scale == TemperatureScale.Celsius)
                return BitConverter.ToUInt16(sensorData, 2) / 128.0;
            else
                return (BitConverter.ToUInt16(sensorData, 2) / 128.0) * 1.8 + 32;
        }

        /// <summary>
        /// Calculates the target temperature.
        /// </summary>
        public static double CalculateTargetTemperature(byte[] sensorData, double ambientTemperature, TemperatureScale scale)
        {
            if (scale == TemperatureScale.Celsius)
                return CalculateTargetTemperature(sensorData, ambientTemperature);
            else
                return CalculateTargetTemperature(sensorData, ambientTemperature) * 1.8 + 32;
        }

        /// <summary>
        /// Sets the period the sensor reads data. Default is 1s. Lower limit is 100ms.
        /// </summary>
        /// <param name="time">Period in 10 ms</param>
        public async Task SetReadPeriod(byte time)
        {
            if (time < 10)
                Debug.WriteLine("Period can't be lower than 100ms");

            GattCharacteristic dataCharacteristic = gattDeviceService.GetCharacteristics(new Guid(SensorTagUuid.UUID_IRT_PERI))[0];

            byte[] data = new byte[] { time };
            GattCommunicationStatus status = await dataCharacteristic.WriteValueAsync(data.AsBuffer());
            if (status == GattCommunicationStatus.Unreachable)
            {
                Debug.WriteLine("IRT Sensor unreachable");
            }
        }


        /// <summary>
        /// Calculates the target temperature of the sensor.
        /// More info about the calculation: http://www.ti.com/lit/ug/sbou107/sbou107.pdf
        /// </summary>
        private static double CalculateTargetTemperature(byte[] sensorData, double ambientTemperature)
        {
            double Vobj2 = BitConverter.ToInt16(sensorData, 0);
            Vobj2 *= 0.00000015625;

            double Tdie = ambientTemperature + 273.15;

            double S0 = 5.593E-14;
            double a1 = 1.75E-3;
            double a2 = -1.678E-5;
            double b0 = -2.94E-5;
            double b1 = -5.7E-7;
            double b2 = 4.63E-9;
            double c2 = 13.4;
            double Tref = 298.15;
            double S = S0 * (1 + a1 * (Tdie - Tref) + a2 * Math.Pow((Tdie - Tref), 2));
            double Vos = b0 + b1 * (Tdie - Tref) + b2 * Math.Pow((Tdie - Tref), 2);
            double fObj = (Vobj2 - Vos) + c2 * Math.Pow((Vobj2 - Vos), 2);
            double tObj = Math.Pow(Math.Pow(Tdie, 4) + (fObj / S), .25);

            return tObj - 273.15;
        }
    }

    public enum TemperatureScale
    {
        Celsius,
        Farenheit
    }
}
