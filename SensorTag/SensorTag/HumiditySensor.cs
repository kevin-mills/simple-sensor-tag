﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace SensorTag.SensorTag
{
    public class HumiditySensor : SensorBase
    {
        public HumiditySensor()
            : base(SensorNames.HumiditySensor, SensorTagUuid.UUID_HUM_SERV, SensorTagUuid.UUID_HUM_CONF, SensorTagUuid.UUID_HUM_DATA)
        {
            
        }

        /// <summary>
        /// Calculates the humidity in percent.
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        public static double CalculateHumidityInPercent(byte[] sensorData)
        {
            // more info http://www.sensirion.com/nc/en/products/humidity-temperature/download-center/?cid=880&did=102&sechash=c204b9cc
            int hum = BitConverter.ToUInt16(sensorData, 2);

            //cut first two status bits
            hum = hum - (hum % 4);

            // calculate in percent
            return (-6f) + 125f * (hum / 65535f);
        }
    }
}
