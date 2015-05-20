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
    public class PressureSensor : SensorBase
    {
        public int[] CalibrationData { get; private set; }

        public PressureSensor()
            : base(SensorNames.PressureSensor, SensorTagUuid.UUID_BAR_SERV, SensorTagUuid.UUID_BAR_CONF, SensorTagUuid.UUID_BAR_DATA)
        {
            CalibrationData = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        /// <summary>
        /// Calculates the pressure from the raw sensor data.
        /// </summary>
        /// <param name="sensorData"></param>
        /// <param name="calibrationData"></param>
        /// <param name="calibrationDataSigned"></param>
        /// <returns>Pressure in pascal</returns>
        public static double CalculatePressure(byte[] sensorData, int[] calibrationData)
        {
            // more info about the calculation:
            // http://www.epcos.com/web/generator/Web/Sections/ProductCatalog/Sensors/PressureSensors/T5400-ApplicationNote,property=Data__en.pdf;/T5400_ApplicationNote.pdf
            int t_r, p_r;	// Temperature raw value, Pressure raw value from sensor
            double t_a, S, O; 	// Temperature actual value in unit centi degrees celsius, interim values in calculation

            t_r = BitConverter.ToInt16(sensorData, 0);
            p_r = BitConverter.ToUInt16(sensorData, 2);

            t_a = (100 * (calibrationData[0] * t_r / Math.Pow(2, 8) + calibrationData[1] * Math.Pow(2, 6))) / Math.Pow(2, 16);
            S = calibrationData[2] + calibrationData[3] * t_r / Math.Pow(2, 17) + ((calibrationData[4] * t_r / Math.Pow(2, 15)) * t_r) / Math.Pow(2, 19);
            O = calibrationData[5] * Math.Pow(2, 14) + calibrationData[6] * t_r / Math.Pow(2, 3) + ((calibrationData[7] * t_r / Math.Pow(2, 15)) * t_r) / Math.Pow(2, 4);
            return (S * p_r + O) / Math.Pow(2, 14);
        }

        /// <summary>
        /// Reads the calibration values of the sensor and then enables the sensor for pressor reads
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        /// <exception cref="DeviceNotInitializedException">Thrown if sensor has not been initialized successfully.</exception>
        public override async Task EnableSensor()
        {
            await StoreAndReadCalibrationValues();
            await base.EnableSensor();
        }

        /// <summary>
        /// Makes the sensor store calibration data, reads and processes it afterwards.
        /// </summary>
        /// <returns></returns>
        private async Task StoreAndReadCalibrationValues()
        {
            GattCharacteristic tempConfig = gattDeviceService.GetCharacteristics(new Guid(base.sensorConfigUuid))[0];

            byte[] confdata = new byte[] { 2 };
            GattCommunicationStatus status = await tempConfig.WriteValueAsync(confdata.AsBuffer());

            if (status == GattCommunicationStatus.Unreachable) Debug.WriteLine("Temperature sensor unreachable!");

            GattCharacteristic calibrationCharacteristic = gattDeviceService.GetCharacteristics(new Guid(SensorTagUuid.UUID_BAR_CALI))[0];

            GattReadResult res = await calibrationCharacteristic.ReadValueAsync(Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached);

            if (res.Status == GattCommunicationStatus.Unreachable) Debug.WriteLine("Pressure sensor unreachable!");

            var sdata = new byte[res.Value.Length];

            DataReader.FromBuffer(res.Value).ReadBytes(sdata);

            CalibrationData[0] = BitConverter.ToUInt16(sdata, 0);
            CalibrationData[1] = BitConverter.ToUInt16(sdata, 2);
            CalibrationData[2] = BitConverter.ToUInt16(sdata, 4);
            CalibrationData[3] = BitConverter.ToUInt16(sdata, 6);
            CalibrationData[4] = BitConverter.ToInt16(sdata, 8);
            CalibrationData[5] = BitConverter.ToInt16(sdata, 10);
            CalibrationData[6] = BitConverter.ToInt16(sdata, 12);
            CalibrationData[7] = BitConverter.ToInt16(sdata, 14);
        }
    }
}
