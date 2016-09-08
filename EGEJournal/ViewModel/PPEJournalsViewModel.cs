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
    public class PPEJournalsViewModel : BindingViewModelBase<PPEJournalsViewModel>
    {
        private readonly IDataService service;

        public PPEJournalsViewModel(IDataService _service)
        {
            service = _service;
            Messenger.Default.Register<System.Windows.Controls.UserControl>(this,
            control =>
            {
                if (control.DataContext is PPEJournalsViewModel)
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
                    async id =>
                    {
                        if (service.isUserRCOI || service.isUserMOUO || service.isUserScan)
                        {
                            IsBusy = true;
                            ListPPEExamView view = ServiceLocator.Current.GetInstance<ListPPEExamView>();
                            try
                            {
                                TaskResult res = await service.SetCurrentListPPEExam(id);
                                if (res.fault != null)
                                    MessageBox.Show(res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                                else
                                    Messenger.Default.Send<UserControl>(view);
                            }
                            catch (Exception err)
                            {
                                MessageBox.Show(err.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            finally
                            {
                                IsBusy = false;
                            }
                        }
                        else if (service.isUserPPE)
                        {
                            IsBusy = true;
                            PPEExamView view = ServiceLocator.Current.GetInstance<PPEExamView>();
                            try
                            {
                                TaskResult res = await service.SetCurrentPPEExam(id, (int)service.user_info.ppe_id);
                                if (res.fault != null)
                                    MessageBox.Show(res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                                else
                                    Messenger.Default.Send<UserControl>(view);
                            }
                            catch (Exception err)
                            {
                                MessageBox.Show(err.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            finally
                            {
                                IsBusy = false;
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
