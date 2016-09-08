using EGEJournal.Model;
using EGEJournal.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using System.Windows;
using System.Windows.Controls;

namespace EGEJournal.ViewModel
{
    public class HeaderViewModel : ViewModelBase
    {
        private readonly IDataService service;

        public HeaderViewModel(IDataService _service)
        {
            service = _service;
            Messenger.Default.Register<System.Windows.Controls.UserControl>(this,
                control =>
                {
                    if (control is JournalsView)
                        JournalsChecked = true;
                    if (control is PPEJournalsView)
                        PPEJournalsChecked = true;
                });
        }

        public Visibility VisibleJournals
        {
            get
            {
                if (service.isUserRCOI || service.isUserScan)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        private bool _JournalsChecked;
        public bool JournalsChecked
        {
            get
            {
                return _JournalsChecked;
            }
            set
            {
                if (value != _JournalsChecked)
                {
                    _JournalsChecked = value;
                    RaisePropertyChanged(() => JournalsChecked);
                }
            }
        }

        private bool _PPEJournalsChecked;
        public bool PPEJournalsChecked
        {
            get
            {
                return _PPEJournalsChecked;
            }
            set
            {
                if (value != _PPEJournalsChecked)
                {
                    _PPEJournalsChecked = value;
                    RaisePropertyChanged(() => PPEJournalsChecked);
                }
            }
        }

        private RelayCommand _ShowJournals;
        public RelayCommand ShowJournals
        {
            get
            {
                return _ShowJournals
                    ?? (_ShowJournals = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send<UserControl>(ServiceLocator.Current.GetInstance<JournalsView>(), "ShowControlFromHeader");
                    }));
            }
        }

        private RelayCommand _ShowPPEJournals;
        public RelayCommand ShowPPEJournals
        {
            get
            {
                return _ShowPPEJournals
                    ?? (_ShowPPEJournals = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send<UserControl>(ServiceLocator.Current.GetInstance<PPEJournalsView>(), "ShowControlFromHeader");
                    }));
            }
        }

        //private RelayCommand _ShowReports;
        //public RelayCommand ShowReports
        //{
        //    get
        //    {
        //        return _ShowReports
        //            ?? (_ShowReports = new RelayCommand(
        //            () =>
        //            {
        //                Messenger.Default.Send<UserControl>(ServiceLocator.Current.GetInstance<ReportsView>(), "ShowControlFromHeader");
        //            }));
        //    }
        //}
    }
}
