using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace SensorTag.Helpers
{
    public static class DispatcherHelper
    {
        /// <summary>
        /// Get an istance of the UI Dispatcher
        /// </summary>
        public static CoreDispatcher UIDispatcher { get; set; }

        /// <summary>
        /// Invoke the action on a Background Thread
        /// </summary>
        public static void InvokeOnBackgroundThread(this Action action)
        {
            Task.Run(action);
        }

        /// <summary>
        /// Invokes the action on a UI Thread, if the current thread is suitable. Otherwise invokes the Dispatcher.
        /// </summary>
        public static void InvokeOnUIThread(this Action action)
        {
            if (UIDispatcher.HasThreadAccess)
                action();
            else
                UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }

        static DispatcherHelper()
        {
            if (UIDispatcher != null)
            {
                return;
            }
            else
            {
                if (Window.Current != null)
                {
                    UIDispatcher = Window.Current.Dispatcher;
                }
                else
                {
                    UIDispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
                }
            }
        }
    }
}
