/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:EGEJournal.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using EGEJournal.Model;
using EGEJournal.View;

namespace EGEJournal.ViewModel
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                //SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
            }
            else
            {
                SimpleIoc.Default.Register<IDataService, DataService>();
            }

            SimpleIoc.Default.Register<MainViewModel>();

            SimpleIoc.Default.Register<HeaderViewModel>();
            SimpleIoc.Default.Register<JournalsViewModel>();
            SimpleIoc.Default.Register<JournalEditViewModel>();
            //SimpleIoc.Default.Register<ReportsViewModel>();
            SimpleIoc.Default.Register<PPEJournalsViewModel>();
            SimpleIoc.Default.Register<PPEExamViewModel>();
            SimpleIoc.Default.Register<ListPPEExamViewModel>();
            SimpleIoc.Default.Register<SelectPPEViewModel>();
            SimpleIoc.Default.Register<PromptViewModel>();

            SimpleIoc.Default.Register<JournalsView>();
            SimpleIoc.Default.Register<JournalEditView>();
            //SimpleIoc.Default.Register<ReportsView>();
            SimpleIoc.Default.Register<PPEJournalsView>();
            SimpleIoc.Default.Register<PPEExamView>();
            SimpleIoc.Default.Register<ListPPEExamView>();
            SimpleIoc.Default.Register<SelectPPEView>();
            SimpleIoc.Default.Register<PromptView>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel MainVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public HeaderViewModel HeaderVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<HeaderViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public JournalsViewModel JournalsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<JournalsViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public JournalEditViewModel JournalEditVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<JournalEditViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ReportsViewModel ReportsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ReportsViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public PPEJournalsViewModel PPEJournalsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PPEJournalsViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public PPEExamViewModel PPEExamVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PPEExamViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ListPPEExamViewModel ListPPEExamVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ListPPEExamViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public SelectPPEViewModel SelectPPEVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SelectPPEViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public PromptViewModel PromptVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PromptViewModel>();
            }
        }

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
        }
    }
}