using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EGEJournal.ServiceEGE;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace EGEJournal.Model
{
    public interface IDataService
    {
        UserInfo user_info { get; }
        ObservableCollection<Journal> ListJournals { get; set; }
        ObservableCollection<Area> ListAreas { get; set; }
        ObservableCollection<PPE> ListPPEForJournal { get; }
        List<PPE> ListPPE9 { get; }
        ObservableCollection<BlankTypes> ListBlankTypes { get; set; }

        bool isUserRCOI { get; }
        bool isUserPPE { get; }
        bool isUserMOUO { get; }
        bool isUserScan { get; }        

        //void ValidateUser(string login, string password, Action<UserInfo, Fault> complete);
        Task<TaskResult> ValidateUser(string login, string password);
        void LoadBaseClasesRCOI(Action<Fault> complete);
        void LoadBaseClasesPPE(Action<Fault> complete);
        void LoadBaseClasesMOUO(Action<Fault> complete);

        Journal CurrentJournal { get; }
        ObservableCollection<JournalContent> CurrentJournalContent { get; set; }
        void SetCurrentJournal(int exam_date_id, Action<Fault> complete);
        Task<TaskResult> UpdateCurrentJournalContent(int exam_date_id);
        Task<TaskResult> SaveJournalContent(JournalContent content, ModifyType modify);

        Journal CurrentJournalForPPE { get; }
        PPEExamContent CurrentPPEExamContent { get; }
        ObservableCollection<PPEExamBlank> CurrentPPEExamBlanks { get; set; }
        Journal CurrentPPEJournal { get; }
        Task<TaskResult> SetCurrentPPEExam(int exam_date_id, int ppe_id);
        Task<TaskResult> FTPDownloadFile(FTPFile file, string targetPath, Action<int> ProgressChange);
        Task<TaskResult> FTPUploadPPEFile(PPEExamFile file, Action<string, int> ProgressChange);
        Task<TaskResult> SavePPEExamBlank(PPEExamBlank content, ModifyType modify, int old_aud = -1);
        ObservableCollection<PPEExam> CurrentListPPEExam { get; }
        Task<TaskResult> SetCurrentListPPEExam(int exam_date_id);
        Task<TaskResult> SetIsUploadPPEExam(int ppe_exam_id);
        Task<TaskResult> ChangeStatusPPEExam(int ppe_exam_id, int status_id);
        Task<TaskResult> SendPPEMessage(PPEExamMessage mes);
    }
}
