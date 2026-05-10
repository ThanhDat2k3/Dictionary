using System.Configuration;
using System.Data;
using System.Windows;
using Dictionary.Services;
using System.Diagnostics;

namespace Dictionary.UI
{
    public partial class App : Application
    {
        public static DataPersistenceService PersistenceService { get; private set; }

        public App()
        {
            PersistenceService = new DataPersistenceService();
            Process.GetCurrentProcess().MaxWorkingSet = (IntPtr)(1 * 1024 * 1024);
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            SaveApplicationData();
            base.OnSessionEnding(e);
        }

        private void SaveApplicationData()
        {
            if (MainWindow is MainWindow mainWindow)
            {
                try
                {
                    mainWindow.SaveData();
                }
                catch
                {
                }
            }
        }
    }

}
