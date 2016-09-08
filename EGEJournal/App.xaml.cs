using System.Windows;
using GalaSoft.MvvmLight.Threading;
using Telerik.Windows.Controls;

namespace EGEJournal
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            LocalizationManager.Manager = new CustomLocalizationManager();
            DispatcherHelper.Initialize();
        }
    }
}
