using GalaSoft.MvvmLight;
using EGEJournal.Model;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using EGEJournal.View;
using EGEJournal.Infrustructure;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;
using System.Linq;

namespace EGEJournal.ViewModel
{
    public class MainViewUserControl
    {
        public UserControl Control { get; set; }
        public string GroupType { get; set; }
        public bool IsLastShow { get; set; }
    }

    public class MainViewModel : BindingViewModelBase<MainViewModel>
    {
        private UserControl _CurrentView;
        public UserControl CurrentView
        {
            get
            {
                return _CurrentView;
            }
            set
            {
                if (value != _CurrentView)
                {
                    _CurrentView = value;
                    RaisePropertyChanged(() => CurrentView);
                }
            }
        }

        private bool _IsBusy;
        public bool IsBusy
        {
            get
            {
                return _IsBusy;
            }
            set
            {
                if (value != _IsBusy)
                {
                    _IsBusy = value;
                    RaisePropertyChanged(() => IsBusy);
                }
            }
        }

        private RelayCommand _LayoutLoaded;
        public RelayCommand LayoutLoaded
        {
            get
            {
                return _LayoutLoaded
                    ?? (_LayoutLoaded = new RelayCommand(
                    () =>
                    {
                        LoadClases();
                    }));
            }
        }

        List<MainViewUserControl> _ListShowUserControl;
        private List<MainViewUserControl> ListShowUserControl
        {
            get
            {
                return _ListShowUserControl ?? (_ListShowUserControl = new List<MainViewUserControl>());
            }
        }

        private readonly IDataService service;

        public MainViewModel(IDataService _service)
        {
            service = _service;
            Messenger.Default.Register<UserControl>(this, 
                control =>
                {
                    if (CurrentView != control)
                    {
                        CurrentView = control;
                        foreach (var item in ListShowUserControl.Where(x => x.GroupType == control.Tag.ToString()))
                        {
                            item.IsLastShow = false;
                        }
                        if (!ListShowUserControl.Select(x => x.Control).Contains(control))
                        {
                            ListShowUserControl.Add(new MainViewUserControl() { Control = control, GroupType = control.Tag.ToString(), IsLastShow = true });
                        }
                        else
                        {
                            MainViewUserControl c = ListShowUserControl.Where(x => x.Control == control).FirstOrDefault();
                            if (c != null)
                                c.IsLastShow = true;
                        }
                    }
                });
            Messenger.Default.Register<UserControl>(this, "ShowControlFromHeader",
                control =>
                {
                    if (CurrentView != control)
                    {
                        CurrentView = GetMainViewControl(control);
                    }
                });
            if (!ViewModelBase.IsInDesignModeStatic)
            {
                LoginView login = null;
                LoginViewModel loginVM = null;
                try
                {
                    login = new LoginView();
                    loginVM = new LoginViewModel(login);

                    //if (login.ShowDialog() == true)
                    //{
                    //    login.Close();
                    //    login = null;
                    //    loginVM.Cleanup();
                    //    loginVM = null;
                    //}
                    login.ShowDialog();
                }
                catch (System.Exception err)
                {
                    if (err != null && err.InnerException != null)
                    {
                        MessageBox.Show(err.InnerException.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show(err.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                finally
                {
                    if (login != null)
                    {
                        login.Close();
                        login = null;
                    }
                    if (loginVM != null)
                    {
                        loginVM.Cleanup();
                        loginVM = null;
                    }
                }
            }
        }

        private UserControl GetMainViewControl(UserControl control)
        {
            UserControl ctrl = null;

            if (!ListShowUserControl.Select(x => x.Control).Contains(control))
            {
                bool is_last_show = false;
                if (ListShowUserControl.Where(x => x.GroupType == control.Tag.ToString() && x.IsLastShow == true).Count() == 0)
                {
                    is_last_show = true;
                }
                ListShowUserControl.Add(new MainViewUserControl() { Control = control, GroupType = control.Tag.ToString(), IsLastShow = is_last_show });
            }

            ctrl = ListShowUserControl.Where(x => x.GroupType == control.Tag.ToString() && x.IsLastShow == true).Select(x => x.Control).FirstOrDefault();
            return ctrl;
        }

        private void LoadClases()
        {
            try
            {
                if (service.isUserRCOI || service.isUserScan)
                {
                    IsBusy = true;
                    service.LoadBaseClasesRCOI((err) =>
                    {
                        if (err != null)
                        {
                            MessageBox.Show(err.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                            App.Current.MainWindow.Close();
                        }
                        IsBusy = false;
                        if (service.isUserRCOI)
                            Messenger.Default.Send<UserControl>(ServiceLocator.Current.GetInstance<JournalsView>());
                        if (service.isUserScan)
                            Messenger.Default.Send<UserControl>(ServiceLocator.Current.GetInstance<PPEJournalsView>());
                    });
                }
                else if (service.isUserPPE)
                {
                    IsBusy = true;
                    service.LoadBaseClasesPPE((err) =>
                    {
                        if (err != null)
                        {
                            MessageBox.Show(err.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                            App.Current.MainWindow.Close();
                        }
                        IsBusy = false;
                        Messenger.Default.Send<UserControl>(ServiceLocator.Current.GetInstance<PPEJournalsView>());
                    });
                }
                else if (service.isUserMOUO)
                {
                    IsBusy = true;
                    service.LoadBaseClasesMOUO((err) =>
                    {
                        if (err != null)
                        {
                            MessageBox.Show(err.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                            App.Current.MainWindow.Close();
                        }
                        IsBusy = false;
                        Messenger.Default.Send<UserControl>(ServiceLocator.Current.GetInstance<PPEJournalsView>());
                    });
                }
            }
            catch (System.Exception)
            {
                IsBusy = false;
                throw;
            }
        }

        public override void Cleanup()
        {
            // Clean up if needed
            base.Cleanup();
        }
    }
}