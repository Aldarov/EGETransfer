using EGEJournal.Infrustructure;
using EGEJournal.Model;
using EGEJournal.ServiceEGE;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using EGEJournal.View;
using Microsoft.Practices.ServiceLocation;
using System.Windows.Controls;
using System;
using Telerik.Windows.Controls;

namespace EGEJournal.ViewModel
{
    public class ListPPEExamViewModel : BindingViewModelBase<PPEExamViewModel>
    {
        private readonly IDataService service;

        public ListPPEExamViewModel(IDataService _service)
        {
            service = _service;
            Messenger.Default.Register<System.Windows.Controls.UserControl>(this,
                control =>
                {
                    if (control.DataContext is ListPPEExamViewModel)
                    {
                        RefreshWithoutLocated(service.CurrentPPEJournal.exam_date_id);
                    }
                });
        }

        private void UpdateListPPEExam()
        {
            RaisePropertyChanged(() => CurrentJournal);
            RaisePropertyChanged(() => ListPPEExam);
            RaisePropertyChanged(() => VisibilityAddPPE);
            ListErrors.Clear();
            ClearValidationRoles();
        }

        public ObservableCollection<PPEExam> ListPPEExam
        {
            get
            {
                return service.CurrentListPPEExam;
            }
        }

        public Journal CurrentJournal
        {
            get
            {
                return service.CurrentJournalForPPE;
            }
        }

        public Visibility VisibilityAddPPE
        {
            get
            {
                if ((service.isUserMOUO || service.isUserPPE) && service.CurrentPPEJournal != null && service.CurrentPPEJournal.class_number == 9)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
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

        private RelayCommand<PPEExam> _OpenPPEExam;
        public RelayCommand<PPEExam> OpenPPEExam
        {
            get
            {
                return _OpenPPEExam
                    ?? (_OpenPPEExam = new RelayCommand<PPEExam>(
                    async p =>
                    {
                        IsBusy = true;
                        PPEExamView view = ServiceLocator.Current.GetInstance<PPEExamView>();
                        try
                        {
                            TaskResult res = await service.SetCurrentPPEExam(p.exam_date_id, p.ppe_id);
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
                    }));
            }
        }

        private RelayCommand _ReturnToListExamCommand;
        public RelayCommand ReturnToListExamCommand
        {
            get
            {
                return _ReturnToListExamCommand
                    ?? (_ReturnToListExamCommand = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send<UserControl>(ServiceLocator.Current.GetInstance<PPEJournalsView>());
                    }));
            }
        }

        private RelayCommand _AddPPEExamCommand;
        public RelayCommand AddPPEExamCommand
        {
            get
            {
                return _AddPPEExamCommand
                    ?? (_AddPPEExamCommand = new RelayCommand(
                    () =>
                    {
                        SelectPPEView view = ServiceLocator.Current.GetInstance<SelectPPEView>();
                        view.ShowDialog();
                    }));
            }
        }

        private RelayCommand<RadGridView> _RefreshCommand;
        public RelayCommand<RadGridView> RefreshCommand
        {
            get
            {
                return _RefreshCommand
                    ?? (_RefreshCommand = new RelayCommand<RadGridView>(
                    grid =>
                    {
                        Refresh(grid);
                    }));
            }
        }

        private async void Refresh(RadGridView grid)
        {
            IsBusy = true;
            try
            {

                PPEExam grid_item = (PPEExam)grid.SelectedItem;
                string col_name = grid.CurrentCellInfo != null ? grid.CurrentCellInfo.Column.UniqueName : "";

                TaskResult res = await service.SetCurrentListPPEExam(grid_item.exam_date_id);
                if (res.fault != null)
                {
                    MessageBox.Show(res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    UpdateListPPEExam();
                    PPEExam prev_item = service.CurrentListPPEExam.Where(x => x.ppe_id == grid_item.ppe_id).SingleOrDefault();
                    grid.CurrentCellInfo = new GridViewCellInfo(prev_item, grid.Columns[col_name]);
                    grid.Focus();
                    if (prev_item != null)
                    {
                        grid.SelectedItem = prev_item;
                        grid.ScrollIntoView(prev_item);
                    }
                }
                IsBusy = false;
            }
            catch (System.Exception e)
            {
                IsBusy = false;
                MessageBox.Show(e.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshWithoutLocated(int exam_date_id)
        {
            IsBusy = true;
            try
            {
                TaskResult res = await service.SetCurrentListPPEExam(exam_date_id);
                if (res.fault != null)
                {
                    MessageBox.Show(res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    UpdateListPPEExam();
                }
                IsBusy = false;
            }
            catch (System.Exception e)
            {
                IsBusy = false;
                MessageBox.Show(e.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
    }
}