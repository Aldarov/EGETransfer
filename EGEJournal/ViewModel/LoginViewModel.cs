using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using EGEJournal.Infrustructure;
using EGEJournal.Model;
using Microsoft.Practices.ServiceLocation;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace EGEJournal.ViewModel
{
    public class LoginViewModel : BindingViewModelBase<LoginViewModel>
    {
        private Window View;
        private readonly IDataService service;

        public LoginViewModel(Window view)
        {
            View = view;
            View.DataContext = this;
            service = ServiceLocator.Current.GetInstance<IDataService>();
            //SetCertificate();
            ValidationRules();
        }

        private void SetCertificate()
        {
            X509Store store = new X509Store(StoreName.Root);
            store.Open(OpenFlags.ReadWrite);
            string app_path = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            X509Certificate2 cert = new X509Certificate2(@".\Cert\localhost.cer");
            if (!store.Certificates.Contains(cert))
                store.Add(cert);
        }

        private string _Login;
        public string Login
        {
            get
            {
                return _Login;
            }
            set
            {
                if (value != _Login)
                {
                    _Login = value;
                    RaisePropertyChanged(() => Login);
                }
            }
        }

        private string _Password;
        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                if (value != _Password)
                {
                    _Password = value;
                    RaisePropertyChanged(() => Password);
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

        private RelayCommand _LoginCommand;
        public RelayCommand LoginCommand
        {
            get
            {
                return _LoginCommand
                    ?? (_LoginCommand = new RelayCommand(
                    async () =>
                    {
                        IsBusy = true;
                        if (ModelValidate())
                        {
                            TaskResult res = await service.ValidateUser(Login, Password);
                            if (res.fault != null)
                                AddError(res.fault.Token, res.fault.Message);
                            else
                                View.DialogResult = true;
                        }
                        IsBusy = false;
                    }));
            }
        }

        private void ValidationRules()
        {
            AddValidationFor(() => Login).When(x => string.IsNullOrEmpty(x.Login)).Show("Введите логин");
            AddValidationFor(() => Password).When(x => string.IsNullOrEmpty(x.Password)).Show("Введите пароль");
        }
    }
}