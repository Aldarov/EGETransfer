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
using System.Windows;
using System.Windows.Controls;

namespace EGEJournal.ViewModel
{
    public class SelectPPEViewModel : BindingViewModelBase<SelectPPEViewModel>
    {
        private readonly IDataService service;

        public SelectPPEViewModel(IDataService _service)
        {
            service = _service;
            Messenger.Default.Register<System.Windows.Controls.UserControl>(this,
            control =>
            {
                if (control.DataContext is SelectPPEViewModel)
                    UpdateListPPEExam();
            });
            ValidationRules();
        }

        private void UpdateListPPEExam()
        {
            RaisePropertyChanged(() => ListPPEExam);
            ListErrors.Clear();
            ClearValidationRoles();
        }

        public List<PPE> ListPPEExam
        {
            get
            {
                return service.ListPPE9;
            }
        }


        private int _SelectedValue;
        public int SelectedValue
        {
            get
            {
                return _SelectedValue;
            }
            set
            {
                if (value != _SelectedValue)
                {
                    _SelectedValue = value;
                    RaisePropertyChanged(() => SelectedValue);
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

        private RelayCommand<PPE> _AddPPECommand;
        public RelayCommand<PPE> AddPPECommand
        {
            get
            {
                return _AddPPECommand
                    ?? (_AddPPECommand = new RelayCommand<PPE>(
                    async (ppe) =>
                    {
                        if (ModelValidate())
                        {
                            IsBusy = true;
                            SelectPPEView select_ppe_view = ServiceLocator.Current.GetInstance<SelectPPEView>();
                            PPEExamView view = ServiceLocator.Current.GetInstance<PPEExamView>();
                            try
                            {
                                TaskResult res = await service.SetCurrentPPEExam(service.CurrentPPEJournal.exam_date_id, ppe.id);
                                if (res.fault != null)
                                    MessageBox.Show(res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                                else
                                {
                                    select_ppe_view.Close();
                                    Messenger.Default.Send<UserControl>(view);
                                }                                    
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

        private void ValidationRules()
        {
            AddValidationFor(() => SelectedValue).When(x => x.SelectedValue == 0).Show("Выберите ППЭ");
        }
    }
}