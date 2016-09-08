using EGEJournal.Infrustructure;
using EGEJournal.Model;
using EGEJournal.ServiceEGE;
using EGEJournal.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using System.Linq;
using System.Threading.Tasks;
using EGEJournal.Helpers;

namespace EGEJournal.ViewModel
{
    public class JournalEditViewModel : BindingViewModelBase<JournalEditViewModel>
    {
        private readonly IDataService service;

        public JournalEditViewModel(IDataService _service)
        {
            service = _service;
            Messenger.Default.Register<System.Windows.Controls.UserControl>(this,
            control =>
            {
                if (control.DataContext is JournalEditViewModel)
                    UpdateJournal();
            });
        }

        private void UpdateJournal()
        {
            RaisePropertyChanged(() => JournalHeader);
            RaisePropertyChanged(() => JournalContent);
            RaisePropertyChanged(() => ListPPE);
            RaisePropertyChanged(() => ListBlankTypes);
            ListErrors.Clear();
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

        public string JournalHeader
        {
            get
            {
                if (service.CurrentJournal != null)
                {
                    return service.CurrentJournal.journal_name;
                }
                else
                    return null;
            }
        }

        public ObservableCollection<JournalContent> JournalContent
        {
            get
            {
                if (service.CurrentJournalContent != null)
                {
                    return service.CurrentJournalContent;
                }
                else
                    return null;
            }
        }

        public ObservableCollection<PPE> ListPPE
        {
            get
            {
                if (service.ListAreas != null)
                {
                    return service.ListPPEForJournal;
                }
                else
                    return null;
            }
        }

        public ObservableCollection<BlankTypes> ListBlankTypes
        {
            get
            {
                if (service.ListBlankTypes != null)
                {
                    return service.ListBlankTypes;
                }
                else
                    return null;
            }
        }

        private RelayCommand _ReturnToListCommand;
        public RelayCommand ReturnToListCommand
        {
            get
            {
                return _ReturnToListCommand
                    ?? (_ReturnToListCommand = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send<UserControl>(ServiceLocator.Current.GetInstance<JournalsView>());
                    }));
            }
        }

        private RelayCommand<GridViewRowEditEndedEventArgs> _GridRowEditEndedCommand;
        public RelayCommand<GridViewRowEditEndedEventArgs> GridRowEditEndedCommand
        {
            get
            {
                return _GridRowEditEndedCommand
                    ?? (_GridRowEditEndedCommand = new RelayCommand<GridViewRowEditEndedEventArgs>(
                    (o) =>
                    {
                        if (o.EditAction == GridViewEditAction.Cancel)
                        {
                            o.UserDefinedErrors.Clear();
                            ListErrors.Clear();
                            return;
                        }
                    }));
            }
        }

        private async void RefreshJournal(RadGridView grid, int id = -1)
        {
            IsBusy = true;
            try
            {
                JournalContent grid_item = (JournalContent)grid.SelectedItem;
                string col_name = grid.CurrentCellInfo != null ? grid.CurrentCellInfo.Column.UniqueName : "";

                TaskResult res = await service.UpdateCurrentJournalContent(service.CurrentJournal.exam_date_id);
                if (res.fault != null)
                {
                    service.CurrentJournalContent = null;
                    AddError("error", res.fault.Message);
                }
                else
                {
                    UpdateJournal();
                    JournalContent prev_item = null;
                    if (id > 0)
                        prev_item = service.CurrentJournalContent.Where(x => x.id == id).SingleOrDefault();
                    else if (grid_item != null && col_name != "")
                        prev_item = service.CurrentJournalContent.Where(x => x.id == grid_item.id).SingleOrDefault();
                    else
                        prev_item = (JournalContent)grid.CurrentItem;
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
                AddError("error", e.Message);
            }
        }

        private RelayCommand<RadGridView> _RefreshJournalCommand;
        public RelayCommand<RadGridView> RefreshJournalCommand
        {
            get
            {
                return _RefreshJournalCommand
                    ?? (_RefreshJournalCommand = new RelayCommand<RadGridView>(
                    (grid) =>
                    {
                        RefreshJournal(grid);
                    }));
            }
        }

        private void AddValidating(GridViewRowValidatingEventArgs e, string error, string property_name)
        {
            GridViewCellValidationResult validationResult = new GridViewCellValidationResult();
            validationResult.PropertyName = property_name;
            validationResult.ErrorMessage = error;
            e.ValidationResults.Add(validationResult);
            e.IsValid = false;
            AddError(property_name, error);
        }

        private RelayCommand<GridViewRowValidatingEventArgs> _GridRowValidatingCommand;
        public RelayCommand<GridViewRowValidatingEventArgs> GridRowValidatingCommand
        {
            get
            {
                return _GridRowValidatingCommand
                    ?? (_GridRowValidatingCommand = new RelayCommand<GridViewRowValidatingEventArgs>(
                    e =>
                    {
                        JournalContent journal = e.Row.DataContext as JournalContent;
                        if (!(journal.ppe_id > 0))
                            AddValidating(e, "Выберите ППЭ", "ppe_id");
                        if (!(journal.blank_type_id > 0))
                            AddValidating(e, "Выберите тип бланка", "ppe_id");
                        if (!(journal.aud > 0))
                            AddValidating(e, "Введите номер аудитории", "aud");
                        if (!(journal.count_blanks > 0))
                            AddValidating(e, "Кол-во бланков должно быть больше нуля", "count_blanks");
                        if (journal.count_add_blanks > 0 && journal.blank_type_id != 3)
                            AddValidating(e, "Кол-во доп. бланков вносится только для бланков №2", "count_add_blanks");
                        if (JournalContent.Where(x => x.id != journal.id && x.exam_date_id == journal.exam_date_id && x.ppe_id == journal.ppe_id
                            && x.blank_type_id == journal.blank_type_id && x.aud == journal.aud).Count() > 0)
                            AddValidating(e, "В базу данных уже внесена запись с аналогичным ППЭ, ауд и типом бланка", "error");
                        if (journal.ppe_is_tom)
                            AddValidating(e, "Вносить изменения запрещено, т.к. данный ППЭ является ТОМом", "ppe_id");
                        if (e.EditOperationType == GridViewEditOperationType.Insert && ListPPE.Where(x => x.id == journal.ppe_id).Select(x => x.is_tom).SingleOrDefault())
                            AddValidating(e, "Вносить изменения запрещено, т.к. данный ППЭ является ТОМом", "ppe_id");

                        if (e.IsValid)
                        {
                            IsBusy = true;
                            int l_blank_type_id = journal.blank_type_id;
                            int l_ppe_id = journal.ppe_id;
                            Task<TaskResult> res = null;

                            if (e.EditOperationType == GridViewEditOperationType.Edit)
                                res = service.SaveJournalContent(journal, ModifyType.Update);
                            else if (e.EditOperationType == GridViewEditOperationType.Insert)
                                res = service.SaveJournalContent(journal, ModifyType.Insert);

                            while (!res.Wait(250))
                                AppDoEvents.DoEvents();

                            if (res.Result.fault != null)
                            {
                                AddValidating(e, res.Result.fault.Message, "save_item");
                                IsBusy = false;
                            }
                            else
                            {
                                if (e.EditOperationType == GridViewEditOperationType.Edit)
                                {
                                    RefreshJournal(e.Source as RadGridView);
                                }                                    
                                else if (e.EditOperationType == GridViewEditOperationType.Insert)
                                {
                                    last_blank_type_id = l_blank_type_id;
                                    last_ppe_id = l_ppe_id;
                                    RefreshJournal(e.Source as RadGridView, (int)res.Result.result);
                                }                                    
                            }
                        }
                    }));
            }
        }

        public int last_blank_type_id { get; set; }
        public int last_ppe_id { get; set; }

        private RelayCommand<GridViewBeginningEditRoutedEventArgs> _GridBeginningEditCommand;
        public RelayCommand<GridViewBeginningEditRoutedEventArgs> GridBeginningEditCommand
        {
            get
            {
                return _GridBeginningEditCommand
                    ?? (_GridBeginningEditCommand = new RelayCommand<GridViewBeginningEditRoutedEventArgs>(
                    e =>
                    {
                        //if (last_blank_type_id > 0 && last_ppe_id > 0)
                        //{
                        //    (e.Row.DataContext as JournalContent).blank_type_id = last_blank_type_id;
                        //    (e.Row.DataContext as JournalContent).ppe_id = last_ppe_id;
                        //    (e.Source as RadGridView).CurrentColumn = (e.Source as RadGridView).Columns["aud"];
                        //}
                    }));
            }
        }

        private RelayCommand<GridViewDeletingEventArgs > _GridRowDeletingCommand;
        public RelayCommand<GridViewDeletingEventArgs > GridRowDeletingCommand
        {
            get
            {
                return _GridRowDeletingCommand
                    ?? (_GridRowDeletingCommand = new RelayCommand<GridViewDeletingEventArgs >(
                    arg =>
                    {
                        JournalContent grid_item = (JournalContent)arg.Items.FirstOrDefault();
                        bool isValid = true;
                        if (grid_item.ppe_is_tom)
                        {
                            MessageBox.Show("Удалять данную запись запрещено, т.к. данный ППЭ является ТОМом", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                            isValid = false;
                        }
                        if (isValid)
                        {
                            RadWindow.Confirm(new DialogParameters()
                            {
                                Header = "Журнал",
                                DialogStartupLocation = WindowStartupLocation.CenterOwner,
                                Content = "Удалить текущую запись?",
                                Closed = (s, e) =>
                                {
                                    bool shouldDelete = e.DialogResult.HasValue ? e.DialogResult.Value : false;
                                    if (shouldDelete)
                                    {
                                        IsBusy = true;

                                        Task<TaskResult> res = null;
                                        res = service.SaveJournalContent(grid_item, ModifyType.Delete);
                                        while (!res.Wait(250))
                                            AppDoEvents.DoEvents();

                                        if (res.Result.fault != null)
                                        {
                                            arg.Cancel = true;
                                            AddError("save_item", res.Result.fault.Message);
                                            IsBusy = false;
                                        }
                                        else
                                            RefreshJournal(arg.Source as RadGridView);
                                    }
                                    else
                                    {
                                        arg.Cancel = true;
                                    }
                                }
                            });
                        }
                        else
                            arg.Cancel = true;
                    }));
            }
        }        
    }
}