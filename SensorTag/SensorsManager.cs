using SensorTag.SensorTag;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace SensorTag
{
    public class SensorManager : IDisposable
    {
        /// <summary>
        /// units are in 10ms intervals 
        /// 10 = 100ms 
        /// 33 = 330ms
        /// </summary>
        public const byte SAMPLES_PER_SECOND = 33;

        private Accelerometer acc;
        private Gyroscope gyro;
        private HumiditySensor hum;
        private Magnetometer mg;
        private PressureSensor ps;
        private IRTemperatureSensor tempSen;

        public IObservable<double> HumidityObs { get; private set; }
        public IObservable<double> PressureObs { get; private set; }
        public IObservable<double> TemperatureObs { get; private set; }
        public IObservable<double> AmbientTemperatureObs { get; private set; }
        public IObservable<Point3D> MagnetometerObs { get; private set; }
        public IObservable<Point3D> AccelerometerObs { get; private set; }
        public IObservable<Point3D> GyroscopeObs { get; private set; }

        public async Task<bool> SetupObservables()
        {
            bool returnValue = true;
            Exception ex = null;

            try
            {
                //########### ACCELEROMETER ############
                acc = new Accelerometer();
                AccelerometerObs = Observable.FromEventPattern<SensorValueChangedEventArgs>(acc, "SensorValueChanged")
                                             .ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
                                             .Select(x => Accelerometer.CalculateCoordinates(x.EventArgs.RawData, 1 / 64.0))
                                             .Select(x => new Point3D(x[0], x[1], x[2]));
                await acc.Initialize();
                await acc.SetReadPeriod(SAMPLES_PER_SECOND);
                await acc.EnableSensor();




                //########### GYROSCOPE ############
                gyro = new Gyroscope();
                GyroscopeObs = Observable.FromEventPattern<SensorValueChangedEventArgs>(gyro, "SensorValueChanged")
                                         .ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
                                         .Select(x => Gyroscope.CalculateAxisValue(x.EventArgs.RawData, GyroscopeAxis.XYZ))
                                         .Select(x => new Point3D(x[0], x[1], x[2]));
                await gyro.Initialize();
                await gyro.EnableSensor();



                //########### HUMIDITY ############
                hum = new HumiditySensor();
                HumidityObs = Observable.FromEventPattern<SensorValueChangedEventArgs>(hum, "SensorValueChanged")
                                        .ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
                                        .Select(x => HumiditySensor.CalculateHumidityInPercent(x.EventArgs.RawData));
                await hum.Initialize();
                await hum.EnableSensor();



                //########### MAGNETOMETER ############
                mg = new Magnetometer();
                MagnetometerObs = Observable.FromEventPattern<SensorValueChangedEventArgs>(mg, "SensorValueChanged")
                                            .ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
                                            .Select(x => Magnetometer.CalculateCoordinates(x.EventArgs.RawData))
                                            .Select(x => new Point3D(x[0], x[1], x[2]));
                await mg.Initialize();
                await mg.SetReadPeriod(SAMPLES_PER_SECOND);
                await mg.EnableSensor();


                //########### PRESSURE ############
                ps = new PressureSensor();
                PressureObs = Observable.FromEventPattern<SensorValueChangedEventArgs>(ps, "SensorValueChanged")
                                        .ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
                                        .Select(x => PressureSensor.CalculatePressure(x.EventArgs.RawData, ps.CalibrationData) / 100);
                await ps.Initialize();
                await ps.EnableSensor();


                //########### TEMPERATURE ############
                tempSen = new IRTemperatureSensor();
                AmbientTemperatureObs = Observable.FromEventPattern<SensorValueChangedEventArgs>(tempSen, "SensorValueChanged")
                                                  .ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
                                                  .Select(x => IRTemperatureSensor.CalculateAmbientTemperature(x.EventArgs.RawData, TemperatureScale.Celsius));

                TemperatureObs = Observable.FromEventPattern<SensorValueChangedEventArgs>(tempSen, "SensorValueChanged")
                                           .ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
                                           .CombineLatest(AmbientTemperatureObs, (target, ambient) => IRTemperatureSensor.CalculateTargetTemperature(target.EventArgs.RawData, ambient, TemperatureScale.Celsius)) //combine with ambient temp
                                           .Select(x => x);
                await tempSen.Initialize();
                tempSen.SetReadPeriod(SAMPLES_PER_SECOND);
                await tempSen.EnableSensor();



                // ENABLE NOTIFICATIONS
                await acc.EnableNotifications();
                await gyro.EnableNotifications();
                await hum.EnableNotifications();
                await mg.EnableNotifications();
                await ps.EnableNotifications();
                await tempSen.EnableNotifications();
            }
            catch (Exception exc)
            {
                ex = exc;
                returnValue = false;
            }

            if (ex != null) await new MessageDialog(ex.Message).ShowAsync();

            return returnValue;
        }


        public void Dispose()
        {
            if (acc != null) acc.Dispose();
            if (gyro != null) gyro.Dispose();
            if (hum != null) hum.Dispose();
            if (mg != null) mg.Dispose();
            if (ps != null) ps.Dispose();
            if (tempSen != null) tempSen.Dispose();
        }
    }

    public class Point3D
    {
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
