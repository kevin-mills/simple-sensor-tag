using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace SensorTag.SensorTag
{
    public enum SensorNames
    {
        Accelerometer,
        Gyroscope,
        HumiditySensor,
        TemperatureSensor,
        Magnetometer,
        PressureSensor
    }

    public class SensorBase : IDisposable
    {
        protected GattDeviceService gattDeviceService;
        private GattCharacteristic gattCharacteristic;

        public event EventHandler<SensorValueChangedEventArgs> SensorValueChanged;

        protected SensorNames sensorName { get; private set; }
        protected string sensorServiceUuid { get; private set; }
        protected string sensorConfigUuid { get; private set; }
        protected string sensorDataUuid { get; private set; }

        public SensorBase(SensorNames sensorName, string sensorServiceUuid, string sensorConfigUuid, string sensorDataUuid)
        {
            this.sensorName = sensorName;

            this.sensorServiceUuid = sensorServiceUuid;
            this.sensorConfigUuid = sensorConfigUuid;
            this.sensorDataUuid = sensorDataUuid;
        }
        
        public async Task<bool> Initialize()
        {
            this.gattDeviceService = await GetDevice();
            return (gattDeviceService != null);
        }

        public async Task<GattDeviceService> GetDevice()
        {
            var deviceSelector = GattDeviceService.GetDeviceSelectorFromUuid(new Guid(sensorServiceUuid));
            var devices = await DeviceInformation.FindAllAsync(deviceSelector);

            DeviceInformation first = devices.FirstOrDefault();
            return await GattDeviceService.FromIdAsync(first.Id);
        }

        public virtual async Task EnableSensor()
        {
            await EnableSensor(new byte[] { 1 });
        }

        protected async Task EnableSensor(byte[] sensorEnableData)
        {
            GattCharacteristic configCharacteristic = gattDeviceService.GetCharacteristics(new Guid(sensorConfigUuid))[0];

            GattCommunicationStatus status = await configCharacteristic.WriteValueAsync(sensorEnableData.AsBuffer());
            if (status == GattCommunicationStatus.Unreachable)
                Debug.WriteLine("Sensor unreachable!");
        }

        
        public virtual async Task EnableNotifications()
        {
            gattCharacteristic = gattDeviceService.GetCharacteristics(new Guid(sensorDataUuid))[0];

            gattCharacteristic.ValueChanged += DataCharacteristic_ValueChanged;

            GattCommunicationStatus status =
                    await gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

            if (status == GattCommunicationStatus.Unreachable)
            {
                Debug.WriteLine("Sensor unreachable!");
            }
        }
        
        public virtual async Task DisableNotifications()
        {
            gattCharacteristic = gattDeviceService.GetCharacteristics(new Guid(sensorDataUuid))[0];

            gattCharacteristic.ValueChanged -= DataCharacteristic_ValueChanged;

            GattCommunicationStatus status =
                await gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);

            if (status == GattCommunicationStatus.Unreachable)
            {
                Debug.WriteLine("Sensor unreachable!");
            }
        }

        private void DataCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var data = new byte[args.CharacteristicValue.Length];

            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);

            if (SensorValueChanged != null)
            {
                SensorValueChanged(this, new SensorValueChangedEventArgs(data, args.Timestamp, sensorName));
            }
        }

        public void Dispose()
        {
            if (gattDeviceService != null)
                gattDeviceService.Dispose();

            gattDeviceService = null;

            if (gattCharacteristic != null)
                gattCharacteristic.ValueChanged -= DataCharacteristic_ValueChanged;
        }
    }
}
