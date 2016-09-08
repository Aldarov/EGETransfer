using Crypto;
using EGEJournal.Infrustructure;
using EGEJournal.Model;
using EGEJournal.ServiceEGE;
using EGEJournal.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using System.Threading.Tasks;
using EGEJournal.Helpers;

namespace EGEJournal.ViewModel
{
    public class PPEExamViewModel : BindingViewModelBase<PPEExamViewModel>
    {
        private readonly IDataService service;

        public PPEExamViewModel(IDataService _service)
        {
            service = _service;
            Messenger.Default.Register<System.Windows.Controls.UserControl>(this,
                control =>
                {
                    if (control.DataContext is PPEExamViewModel)
                        UpdatePPEExamContent();
                });
        }

        private void UpdatePPEExamContent()
        {
            RaisePropertyChanged(() => CurrentPPEExamContent);
            RaisePropertyChanged(() => PPEExamBlanks);
            RaisePropertyChanged(() => VisibilityControlForMOUO);
            RaisePropertyChanged(() => VisibilityControlForRCOI);
            RaisePropertyChanged(() => isReadOnlyControl);
            ListErrors.Clear();
            ClearValidationRoles();
            ValidationRules();
        }

        public Visibility VisibilityControlForMOUO
        {
            get
            {
                if ((service.isUserPPE || service.isUserMOUO) && service.CurrentPPEExamContent != null && new int[]{ 1, 3 }.Contains(service.CurrentPPEExamContent.status_id))
                {
                    return Visibility.Visible;
                }
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility VisibilityControlForRCOI
        {
            get
            {
                if ((service.isUserRCOI || service.isUserScan) && service.CurrentPPEExamContent != null && new int[] { 2, 4 }.Contains(service.CurrentPPEExamContent.status_id))
                {
                    return Visibility.Visible;
                }
                else
                    return Visibility.Collapsed;
            }
        }

        public bool isReadOnlyControl
        {
            get
            {
                if ((service.isUserPPE || service.isUserMOUO) && service.CurrentPPEExamContent != null && new int[] { 1, 3 }.Contains(service.CurrentPPEExamContent.status_id))
                {
                    return false;
                }
                else
                    return true;
            }
        }

        public PPEExamContent CurrentPPEExamContent
        {
            get
            {
                if (service.CurrentPPEExamContent != null)
                {
                    return service.CurrentPPEExamContent;
                }
                else
                    return null;
            }
        }

        public ObservableCollection<PPEExamBlank> PPEExamBlanks
        {
            get
            {
                if (service.CurrentPPEExamBlanks != null)
                {
                    return service.CurrentPPEExamBlanks;
                }
                else
                    return null;
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

        private bool _IsBusyProgress;
        public bool IsBusyProgress
        {
            get
            {
                return _IsBusyProgress;
            }
            set
            {
                if (value != _IsBusyProgress)
                {
                    _IsBusyProgress = value;
                    RaisePropertyChanged(() => IsBusyProgress);
                }
            }
        }

        private int _ProgressValue;
        public int ProgressValue
        {
            get
            {
                return _ProgressValue;
            }
            set
            {
                if (value != _ProgressValue)
                {
                    _ProgressValue = value;
                    RaisePropertyChanged(() => ProgressValue);
                }
            }
        }

        private string _ProgressContentValue;   
        public string ProgressContentValue
        {
            get
            {
                return _ProgressContentValue;
            }
            set
            {
                if (value != _ProgressContentValue)
                {
                    _ProgressContentValue = value;
                    RaisePropertyChanged(() => ProgressContentValue);
                }
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
                        if (service.isUserPPE)
                            Messenger.Default.Send<System.Windows.Controls.UserControl>(ServiceLocator.Current.GetInstance<PPEJournalsView>());
                        else
                            Messenger.Default.Send<System.Windows.Controls.UserControl>(ServiceLocator.Current.GetInstance<ListPPEExamView>());
                    }));
            }
        }

        private RelayCommand<PPEExamFile> _ClearUploadPathCommand;
        public RelayCommand<PPEExamFile> ClearUploadPathCommand
        {
            get
            {
                return _ClearUploadPathCommand
                    ?? (_ClearUploadPathCommand = new RelayCommand<PPEExamFile>(
                    file =>
                    {
                        file.upload_path_files = "";
                        file.upload_count_files = 0;
                    }));
            }
        }

        private int FolderContentsCount(string path)
        {
            int result = Directory.GetFiles(path).Length;
            string[] subFolders = Directory.GetDirectories(path);
            foreach (string subFolder in subFolders)
            {
                result += FolderContentsCount(subFolder);
            }
            return result;
        }

        private RelayCommand<PPEExamFile> _ChangeUploadPathCommand;
        public RelayCommand<PPEExamFile> ChangeUploadPathCommand
        {
            get
            {
                return _ChangeUploadPathCommand
                    ?? (_ChangeUploadPathCommand = new RelayCommand<PPEExamFile>(
                    file =>
                    {
                        using (FolderBrowserDialog dlg = new FolderBrowserDialog())
                        {
                            dlg.Description = "Выберите папку с файлами, в папке должны быть только файлы выбранного типа и текущего экзамена, " +
                                "архивация файлов произойдет автоматически.";
                            dlg.ShowNewFolderButton = false;
                            DialogResult result = dlg.ShowDialog();
                            if (result == System.Windows.Forms.DialogResult.OK)
                            {
                                file.upload_path_files = dlg.SelectedPath;
                                try
                                {
                                    file.upload_count_files = FolderContentsCount(dlg.SelectedPath);
                                }
                                catch (System.Exception e)
                                {
                                    file.upload_path_files = "";
                                    file.upload_count_files = 0;
                                    System.Windows.MessageBox.Show(e.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                                }                                
                                UpdatePPEExamContent();
                            }
                        }
                    }));
            }
        }

        private RelayCommand<FTPFile> _DownloadFileCommand;
        public RelayCommand<FTPFile> DownloadFileCommand
        {
            get
            {
                return _DownloadFileCommand
                    ?? (_DownloadFileCommand = new RelayCommand<FTPFile>(
                    async file =>
                    {
                        using (FolderBrowserDialog dlg = new FolderBrowserDialog())
                        {
                            dlg.Description = "Выберите папку для сохранения файла";
                            dlg.ShowNewFolderButton = false;
                            DialogResult result = dlg.ShowDialog();
                            if (result == System.Windows.Forms.DialogResult.OK)
                            {
                                IsBusyProgress = true;
                                TaskResult res = await service.FTPDownloadFile(file, dlg.SelectedPath, (p) => 
                                {
                                    ProgressValue = p;
                                    ProgressContentValue = "Загрузка... " + p.ToString() + "%";
                                });
                                IsBusyProgress = false;
                                if (res.fault != null)
                                {
                                    System.Windows.MessageBox.Show(res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }));
            }
        }

        private string last_select_download_path { get; set; }
        private RelayCommand _DownloadAllFilesCommand;
        public RelayCommand DownloadAllFilesCommand
        {
            get
            {
                return _DownloadAllFilesCommand
                    ?? (_DownloadAllFilesCommand = new RelayCommand(
                    async () =>
                    {
                        using (FolderBrowserDialog dlg = new FolderBrowserDialog())
                        {
                            dlg.Description = "Выберите папку для сохранения файлов";
                            dlg.ShowNewFolderButton = false;
                                dlg.SelectedPath = last_select_download_path;
                            DialogResult result = dlg.ShowDialog();
                            if (result == System.Windows.Forms.DialogResult.OK)
                            {
                                last_select_download_path = dlg.SelectedPath;
                                IsBusyProgress = true;
                                try
                                {
                                    IEnumerable<PPEExamFile> upfiles = CurrentPPEExamContent.exam_files.Where(x => x.ftp_files.Count() > 0);
                                    int i = 0;
                                    foreach (PPEExamFile file in upfiles)
                                    {
                                        i++;
                                        string target_path = dlg.SelectedPath + @"\" + CurrentPPEExamContent.ppe_code.ToString() + @"\" + file.blank_folder_name;
                                        foreach (var ftpfile in file.ftp_files)
                                        {
                                            Directory.CreateDirectory(target_path);
                                            TaskResult res = await service.FTPDownloadFile(ftpfile, target_path, (p_value) =>
                                            {
                                                ProgressValue = p_value;
                                                ProgressContentValue = i + "(" + i.ToString() + " из " + upfiles.Count().ToString() + ") " + p_value.ToString() + "%";
                                            });
                                            if (res.fault != null)
                                            {
                                                System.Windows.MessageBox.Show(res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                                                break;
                                            }
                                        }
                                    }
                                }
                                catch (System.Exception err)
                                {
                                    System.Windows.MessageBox.Show(err.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                IsBusyProgress = false;
                            }
                        }
                    }));
            }
        }

        private RelayCommand _UploadFileCommand;
        public RelayCommand UploadFileCommand
        {
            get
            {
                return _UploadFileCommand
                    ?? (_UploadFileCommand = new RelayCommand(
                    async () =>
                    {
                        if (IsViewModelValid())
                        {
                            IsBusyProgress = true;
                            IEnumerable<PPEExamFile> upfiles = CurrentPPEExamContent.exam_files.
                                Where(x => !string.IsNullOrEmpty(x.upload_path_files) && x.upload_count_files > 0);
                            int i = 0;
                            foreach (PPEExamFile file in upfiles)
                            {
                                i++;
                                TaskResult res = await service.FTPUploadPPEFile(file, (step, p_value) =>
                                {
                                    ProgressValue = p_value;
                                    ProgressContentValue = step +"(" + i.ToString() + " из " + upfiles.Count().ToString() + ") "+ p_value.ToString() + "%";
                                });
                                if (res.fault != null)
                                {
                                    System.Windows.MessageBox.Show(res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                                }
                            }
                            TaskResult is_upload = await service.SetIsUploadPPEExam(CurrentPPEExamContent.ppe_exam_id);
                            if (is_upload.fault != null)
                                System.Windows.MessageBox.Show(is_upload.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                            ProgressContentValue = "Обновление...";
                            TaskResult upd_res = await service.SetCurrentPPEExam(CurrentPPEExamContent.exam_date_id, CurrentPPEExamContent.ppe_id);
                            if (upd_res.fault != null)
                                System.Windows.MessageBox.Show(upd_res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                            IsBusyProgress = false;
                            UpdatePPEExamContent();
                        }
                    }));
            }
        }

        private bool IsViewModelValid()
        {
            bool res = true;
            if (PPEExamBlanks.Count() == 0)
            {
                AddError("blanks", "Заполните таблицу данными о кол-ве бланков в каждой аудитории ППЭ");
                res = false;
            }
            foreach (var file in CurrentPPEExamContent.exam_files.Where(x => x.blank_type_id != 6))
            {
                if (file.ftp_files.Count() == 0 && (string.IsNullOrEmpty(file.upload_path_files) || file.upload_count_files == 0))
                {
                    AddError(file.blank_type_name, file.blank_type_name + ": Укажите папку с файлами данного типа");
                    res = false;
                }
            }
            return res;
        }

        private void ValidationRules()
        {
            //int i = 0;
            //foreach (var file in CurrentPPEExamContent.exam_files)
            //{
            //    AddValidationFor(() => CurrentPPEExamContent.exam_files[i]).
            //            When(x => x.CurrentPPEExamContent.exam_files.Where(a => a.Equals(file)).SingleOrDefault().ftp_files.Count() == 0 &&
            //                string.IsNullOrEmpty(x.CurrentPPEExamContent.exam_files.Where(a => a.Equals(file)).SingleOrDefault().upload_path_files)).
            //            Show(file.blank_type_name + ": Укажите папку с файлами данного типа");
            //    i++;
            //}
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

        private RelayCommand<RadGridView> _RefreshPPEExamCommand;
        public RelayCommand<RadGridView> RefreshPPEExamCommand
        {
            get
            {
                return _RefreshPPEExamCommand
                    ?? (_RefreshPPEExamCommand = new RelayCommand<RadGridView>(
                    (grid) =>
                    {
                        RefreshPPEExamContent(grid);
                    }));
            }
        }

        private async void RefreshPPEExamContent(RadGridView grid, int aud = -1)
        {
            IsBusy = true;
            try
            {
                PPEExamBlank grid_item = (PPEExamBlank)grid.SelectedItem;
                string col_name = grid.CurrentCellInfo != null && grid.CurrentCellInfo.Column != null ? grid.CurrentCellInfo.Column.UniqueName : "";

                TaskResult res = await service.SetCurrentPPEExam(CurrentPPEExamContent.exam_date_id,CurrentPPEExamContent.ppe_id);
                if (res.fault != null)
                {
                    service.CurrentJournalContent = null;
                    AddError("error", res.fault.Message);
                }
                else
                {
                    UpdatePPEExamContent();
                    PPEExamBlank prev_item = null;
                    if (aud > 0)
                        prev_item = service.CurrentPPEExamBlanks.Where(x => x.aud == aud).SingleOrDefault();
                    else if (grid_item != null && col_name != "")
                        prev_item = service.CurrentPPEExamBlanks.Where(x => x.aud == grid_item.aud).SingleOrDefault();
                    else
                        prev_item = (PPEExamBlank)grid.CurrentItem;
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
                        PPEExamBlank exam_blank = e.Row.DataContext as PPEExamBlank;

                        if (!(exam_blank.aud > 0))
                            AddValidating(e, "Введите номер аудитории", "aud");
                        if (PPEExamBlanks.Where(x => !x.Equals(exam_blank) && x.aud == exam_blank.aud).Count() > 0)
                            AddValidating(e, "Запись с аналогичной аудиторией уже существует", "error");
                        if (exam_blank.aud < 0)
                            AddValidating(e, "Неверно введен номер аудитории", "aud");
                        if (exam_blank.count_2 == 0 && exam_blank.count_add_2 > 0)
                            AddValidating(e, "Доп. бланки №2 не могут быть без бланков №2", "count_add_2");

                        for (int blank_type_id = 1; blank_type_id <= 3; blank_type_id++)
                        {
                            int count_blanks;
                            if (blank_type_id == 1)
                                count_blanks = exam_blank.count_r;
                            else if (blank_type_id == 2)
                                count_blanks = exam_blank.count_1;
                            else
                                count_blanks = exam_blank.count_2;
                            var exam_file = CurrentPPEExamContent.exam_files.Where(x => x.blank_type_id == blank_type_id).FirstOrDefault();
                            if (exam_file == null && count_blanks > 0)
                            {
                                if (blank_type_id == 1)
                                    AddValidating(e, "Регистрационные бланки для данного экзамена не используются, установите значение 0", "count_r");
                                else if (blank_type_id == 2)
                                    AddValidating(e, "Бланки №1 для данного экзамена не используются, установите значение 0", "count_1");
                                else
                                    AddValidating(e, "Бланки №2 для данного экзамена не используются, установите значение 0", "count_2");
                            }
                            else if (exam_file != null && count_blanks == 0)
                            {
                                if (blank_type_id == 1)
                                    AddValidating(e, "Введите кол-во регистрационных бланков", "count_r");
                                else if (blank_type_id == 2)
                                    AddValidating(e, "Введите кол-во бланков №1", "count_1");
                                else
                                    AddValidating(e, "Введите кол-во бланков №2", "count_2");
                            }                            
                        }

                        int count_last = 0;
                        bool isEquals = true;
                        foreach (int type_id in CurrentPPEExamContent.exam_files.Where(x => new int[] { 1, 2, 3 }.Contains(x.blank_type_id)).Select(x => x.blank_type_id).Distinct())
                        {
                            if (type_id == 1)
                            {
                                if (count_last > 0 && count_last != exam_blank.count_r)
                                {
                                    isEquals = false;
                                    break;
                                }
                                count_last = exam_blank.count_r;
                            }
                            else if (type_id == 2)
                            {
                                if (count_last > 0 && count_last != exam_blank.count_1)
                                {
                                    isEquals = false;
                                    break;
                                }
                                count_last = exam_blank.count_1;
                            }
                            else if (type_id == 3)
                            {
                                if (count_last > 0 && count_last != exam_blank.count_2)
                                {
                                    isEquals = false;
                                    break;
                                }
                                count_last = exam_blank.count_2;
                            }
                        }
                        if (!isEquals)
                        {
                            AddValidating(e, "Кол-ва бланков разных типов должны совпадать", "error");
                        }

                        if (e.IsValid)
                        {
                            IsBusy = true;

                            Task<TaskResult> res = null;
                            
                            if (e.EditOperationType == GridViewEditOperationType.Edit)
                                res = service.SavePPEExamBlank(exam_blank, ModifyType.Update, (int)e.OldValues["aud"]);
                            else if (e.EditOperationType == GridViewEditOperationType.Insert)
                                res = service.SavePPEExamBlank(exam_blank, ModifyType.Insert);

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
                                    RefreshPPEExamContent(e.Source as RadGridView);
                                else if (e.EditOperationType == GridViewEditOperationType.Insert)
                                    RefreshPPEExamContent(e.Source as RadGridView, (int)res.Result.result);
                            }
                        }
                    }));
            }
        }

        private RelayCommand<GridViewDeletingEventArgs> _GridRowDeletingCommand;
        public RelayCommand<GridViewDeletingEventArgs> GridRowDeletingCommand
        {
            get
            {
                return _GridRowDeletingCommand
                    ?? (_GridRowDeletingCommand = new RelayCommand<GridViewDeletingEventArgs>(
                    arg =>
                    {
                        RadWindow.Confirm(new DialogParameters()
                        {
                            Header = "Загрузка бланков",
                            DialogStartupLocation = WindowStartupLocation.CenterOwner,
                            Content = "Удалить текущую запись?",
                            Closed = (s, e) =>
                            {
                                PPEExamBlank exam_blank = (PPEExamBlank)arg.Items.FirstOrDefault();
                                bool shouldDelete = e.DialogResult.HasValue ? e.DialogResult.Value : false;
                                if (shouldDelete)
                                {
                                    IsBusy = true;

                                    Task<TaskResult> res = null;
                                    res = service.SavePPEExamBlank(exam_blank, ModifyType.Delete);
                                    while (!res.Wait(250))
                                        AppDoEvents.DoEvents();

                                    if (res.Result.fault != null)
                                    {
                                        arg.Cancel = true;
                                        AddError("save_item", res.Result.fault.Message);
                                        IsBusy = false;
                                    }
                                    else
                                        RefreshPPEExamContent(arg.Source as RadGridView);
                                }
                                else
                                {
                                    arg.Cancel = true;
                                }
                            }
                        });
                    }));
            }
        }

        private RelayCommand _ConfirmTransferCommand;
        public RelayCommand ConfirmTransferCommand
        {
            get
            {
                return _ConfirmTransferCommand
                    ?? (_ConfirmTransferCommand = new RelayCommand(
                    () =>
                    {
                        if (!service.CurrentPPEExamContent.is_uploaded)
                        {
                            System.Windows.MessageBox.Show(@"Перед подтверждением необходимо нажать 'Передать файлы в РЦОИ'", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (IsViewModelValid())
                        {
                            RadWindow.Confirm(new DialogParameters()
                                {
                                    Header = "Внимание",
                                    DialogStartupLocation = WindowStartupLocation.CenterOwner,
                                    Content = "После подтверждения вносить изменения будет запрещено.\nПодтвердить отправку данных в РЦОИ?",
                                    Closed = async (s, e) =>
                                    {
                                        bool shouldOk = e.DialogResult.HasValue ? e.DialogResult.Value : false;
                                        if (shouldOk)
                                        {
                                            IsBusy = true;
                                            TaskResult res = await service.ChangeStatusPPEExam(CurrentPPEExamContent.ppe_exam_id, 2);
                                            if (res.fault != null)
                                                System.Windows.MessageBox.Show(res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                                            TaskResult upd_res = await service.SetCurrentPPEExam(CurrentPPEExamContent.exam_date_id, CurrentPPEExamContent.ppe_id);
                                            if (upd_res.fault != null)
                                                System.Windows.MessageBox.Show(upd_res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                                            UpdatePPEExamContent();
                                            IsBusy = false;
                                        }
                                    }
                                });
                        }
                    }));
            }
        }

        private RelayCommand _CompleteCommand;
        public RelayCommand CompleteCommand
        {
            get
            {
                return _CompleteCommand 
                    ?? (_CompleteCommand = new RelayCommand(
                    () =>
                    {
                        RadWindow.Confirm(new DialogParameters()
                        {
                            Header = "Внимание",
                            DialogStartupLocation = WindowStartupLocation.CenterOwner,
                            Content = "Отметить обработанным?",
                            Closed = async (s, e) =>
                            {
                                bool shouldOk = e.DialogResult.HasValue ? e.DialogResult.Value : false;
                                if (shouldOk)
                                {
                                    IsBusy = true;
                                    TaskResult res = await service.ChangeStatusPPEExam(CurrentPPEExamContent.ppe_exam_id, 4);
                                    if (res.fault != null)
                                        System.Windows.MessageBox.Show(res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);

                                    TaskResult upd_res = await service.SetCurrentPPEExam(CurrentPPEExamContent.exam_date_id, CurrentPPEExamContent.ppe_id);
                                    if (upd_res.fault != null)
                                        System.Windows.MessageBox.Show(upd_res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);

                                    UpdatePPEExamContent();
                                    IsBusy = false;
                                }
                            }
                        });
                    }));
            }
        }

        private RelayCommand _SendErrorCommand;
        public RelayCommand SendErrorCommand
        {
            get
            {
                return _SendErrorCommand
                    ?? (_SendErrorCommand = new RelayCommand(
                    async () =>
                    {
                        PromptView prompt = ServiceLocator.Current.GetInstance<PromptView>();
                        PromptViewModel promptVM = (PromptViewModel)prompt.DataContext;
                        PPEExamMessage mes = new PPEExamMessage() 
                        { 
                            user_id = service.user_info.user_id,
                            ppe_exam_id = service.CurrentPPEExamContent.ppe_exam_id
                        };
                        Messenger.Default.Send<PPEExamMessage>(mes);
                        prompt.ShowDialog();
                        if (prompt.DialogResult == true)
                        {
                            IsBusy = true;
                            TaskResult res = await service.SendPPEMessage(promptVM.PPEMessage);
                            if (res.fault != null)
                                System.Windows.MessageBox.Show(res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);

                            TaskResult res_st = await service.ChangeStatusPPEExam(CurrentPPEExamContent.ppe_exam_id, 3);
                            if (res_st.fault != null)
                                System.Windows.MessageBox.Show(res_st.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);

                            TaskResult upd_res = await service.SetCurrentPPEExam(CurrentPPEExamContent.exam_date_id, CurrentPPEExamContent.ppe_id);
                            if (upd_res.fault != null)
                                System.Windows.MessageBox.Show(upd_res.fault.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);

                            UpdatePPEExamContent();
                            IsBusy = false;
                        }
                    }));
            }
        }
    }
}