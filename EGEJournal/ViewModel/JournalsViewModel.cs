using EGEJournal.Infrustructure;
using EGEJournal.Model;
using EGEJournal.ServiceEGE;
using EGEJournal.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EGEJournal.ViewModel
{
    public class JournalsViewModel : BindingViewModelBase<JournalsViewModel>
    {
        private readonly IDataService service;

        public JournalsViewModel(IDataService _service)
        {
            service = _service;
            Messenger.Default.Register<System.Windows.Controls.UserControl>(this,
            control =>
            {
                if (control.DataContext is JournalsViewModel)
                    RaisePropertyChanged(() => ListJournals);
            });
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

        private RelayCommand<int> _OpenJournal;
        public RelayCommand<int> OpenJournal
        {
            get
            {
                return _OpenJournal
                    ?? (_OpenJournal = new RelayCommand<int>(
                    id =>
                    {
                        if (service.isUserRCOI || service.isUserScan)
                        {
                            IsBusy = true;
                            JournalEditView view = ServiceLocator.Current.GetInstance<JournalEditView>();
                            try
                            {
                                service.SetCurrentJournal(id, (err) =>
                                {
                                    if (err != null)
                                        MessageBox.Show(err.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                                    else
                                        Messenger.Default.Send<UserControl>(view);
                                    IsBusy = false;
                                });
                            }
                            catch (Exception err)
                            {
                                IsBusy = false;
                                MessageBox.Show(err.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }));
            }
        }

        public ObservableCollection<Journal> ListJournals
        {
            get
            {
                return service.ListJournals;
            }
        }
        
    }
}
