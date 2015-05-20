using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Reactive.Linq;
using SensorTag.Helpers;

namespace SensorTag
{
    public sealed partial class MainPage : Page
    {
        private SensorManager sensorManager;

        public MainPage()
        {
            this.InitializeComponent();
        }

        async void button_Click(object sender, RoutedEventArgs e)
        {
            button.Content = "Finding...";
            button.IsEnabled = false;

            sensorManager = new SensorManager();

            if (await sensorManager.SetupObservables())
            {
                button.Content = "Found!";
                SubscribeObservablesForDisplaying();
            }
            else
            {
                button.IsEnabled = true;
                button.Content = "Could not find a SensorTag... Try again!";
            }
        }

        private void SubscribeObservablesForDisplaying()
        {
            sensorManager.AccelerometerObs.Sample(TimeSpan.FromMilliseconds(300))
                        .Where(x => x != null)
                        .DistinctUntilChanged()
                        .Subscribe(x => DispatcherHelper.InvokeOnUIThread(() =>
                        {
                            AccelerometerTextBlock.Text = String.Format("X:{0}, Y:{1}, Z:{2}", x.X, x.Y, x.Z);
                        }));


            sensorManager.GyroscopeObs.Sample(TimeSpan.FromMilliseconds(300))
                        .Where(x => x != null)
                        .DistinctUntilChanged()
                        .Subscribe(x => DispatcherHelper.InvokeOnUIThread(() =>
                        {
                            GyroscopeTextBlock.Text = String.Format("X:{0}, Y:{1}, Z:{2}", x.X, x.Y, x.Z);
                        }));



            sensorManager.MagnetometerObs.Sample(TimeSpan.FromMilliseconds(300))
                        .Where(x => x != null)
                        .DistinctUntilChanged()
                        .Subscribe(x => DispatcherHelper.InvokeOnUIThread(() =>
                        {
                            MagnetometerTextBlock.Text = String.Format("X:{0}, Y:{1}, Z:{2}", x.X, x.Y, x.Z);
                        }));



            sensorManager.HumidityObs.Sample(TimeSpan.FromMilliseconds(2000))
                        .DistinctUntilChanged()
                        .Subscribe(x => DispatcherHelper.InvokeOnUIThread(() =>
                        {
                            HumidityTextBlock.Text = x.ToString();
                        }));


            sensorManager.PressureObs.Sample(TimeSpan.FromMilliseconds(2000))
                        .DistinctUntilChanged()
                        .SubscribeOn(DispatcherHelper.UIDispatcher)
                        .Subscribe(x => DispatcherHelper.InvokeOnUIThread(() =>
                        {
                            PressureTextBlock.Text = x.ToString();
                        }));


            sensorManager.AmbientTemperatureObs.Sample(TimeSpan.FromMilliseconds(2000))
                        .DistinctUntilChanged()
                        .Subscribe(x => DispatcherHelper.InvokeOnUIThread(() =>
                        {
                            AmbientTemperatureTextBlock.Text = x.ToString();
                        }));


            sensorManager.TemperatureObs.Sample(TimeSpan.FromMilliseconds(500))
                        .DistinctUntilChanged()
                        .Subscribe(x => DispatcherHelper.InvokeOnUIThread(() =>
                        {
                            TargetTemperatureTextBlock.Text = x.ToString();
                        }));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            sensorManager.Dispose();
        }
    }
}
