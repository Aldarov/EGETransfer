using EGEJournal.Infrustructure;
using EGEJournal.Model;
using EGEJournal.ServiceEGE;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using Telerik.Windows.Controls;

namespace EGEJournal.ViewModel
{
    public class PromptViewModel : BindingViewModelBase<PromptViewModel>
    {
        private readonly IDataService service;

        public PromptViewModel(IDataService _service)
        {
            service = _service;
            Messenger.Default.Register<PPEExamMessage>(this,
            mes =>
            {
                PPEMessage = mes;
                ListErrors.Clear();
            });
        }

        private PPEExamMessage _PPEMessage;
        public PPEExamMessage PPEMessage
        {
            get
            {
                return _PPEMessage;
            }
            set
            {
                if (value != _PPEMessage)
                {
                    _PPEMessage = value;
                    RaisePropertyChanged(() => PPEMessage);
                }
            }
        }

        private RelayCommand<RadWindow> _SendMessageCommand;
        public RelayCommand<RadWindow> SendMessageCommand
        {
            get
            {
                return _SendMessageCommand
                    ?? (_SendMessageCommand = new RelayCommand<RadWindow>(
                    (w) =>
                    {
                        if (IsValidModel())
                        {
                            w.DialogResult = true;
                            w.Close();
                        }
                    }));
            }
        }

        private RelayCommand<RadWindow> _CloseCommand;
        public RelayCommand<RadWindow> CloseCommand
        {
            get
            {
                return _CloseCommand
                    ?? (_CloseCommand = new RelayCommand<RadWindow>(
                    w =>
                    {
                        ListErrors.Clear();
                        w.DialogResult = false;
                        w.Close();
                    }));
            }
        }

        private bool IsValidModel()
        {
            bool res = true;
            if (string.IsNullOrEmpty(PPEMessage.message))
            {
                AddError("message", "Введите описание ошибки");
                res = false;
            }
            return res;
        }
    }
}