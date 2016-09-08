using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace EGEServiceRun
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IEGESupport" в коде и файле конфигурации.
    [ServiceContract]
    public interface IEGESupport
    {
        [OperationContract]
        [FaultContract(typeof(Fault))]
        UserInfo ValidateUser(string login, string password);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<Journal> GetListJournals();

        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<Area> GetListAreas();

        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<PPE> GetListPPE();

        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<BlankTypes> GetListBlankTypes();

        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<JournalContent> GetJournalContent(int exam_date_id);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        void UpdateJournalContent(JournalContent content);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        int InsertJournalContent(JournalContent content);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool DeleteJournalContent(int id);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        PPEExamContent GetPPEExamContent(int exam_date_id, int ppe_id);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        string GetUploadFileName(int ppe_exam_id, string blank_folder_name);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        void UpdatePPEExamBlank(PPEExamBlank exam_blanks, int old_aud);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        int InsertPPEExamBlank(PPEExamBlank exam_blanks);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool DeletePPEExamBlank(PPEExamBlank exam_blanks);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<PPEExam> GetListPPEExam(int exam_date_id, int area_id = -1);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        void SetIsUploadPPEExam(int ppe_exam_id);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        void ChangeStatusPPEExam(int ppe_exam_id, int status_id);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        void SendPPEMessage(PPEExamMessage mes);
    }

    [DataContract]
    public class UserInfo
    {
        [DataMember]
        public int user_id { get; set; }
        [DataMember]
        public string login { get; set; }
        [DataMember]
        public string password { get; set; }
        [DataMember]
        public string fio { get; set; }
        [DataMember]
        public string role_name { get; set; }
        [DataMember]
        public int? area_id { get; set; }
        [DataMember]
        public int? ppe_id { get; set; }
        [DataMember]
        public int? ppe_class_number { get; set; }
        [DataMember]
        public byte[] ftp_access { get; set; }
    }

    [DataContract]
    public class Fault
    {
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public string Message { get; set; }
    }

    [DataContract]
    public class Journal
    {
        [DataMember]
        public int exam_date_id { get; set; }
        [DataMember]
        public string journal_name { get; set; }
        [DataMember]
        public int subject_id { get; set; }
        [DataMember]
        public string subject_name { get; set; }
        [DataMember]
        public DateTime exam_date { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public int class_number { get; set; }
        [DataMember]
        public int status_id { get; set; }
        [DataMember]
        public string status_name { get; set; }
    }

    [DataContract]
    public class Area
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string folder_name { get; set; }
    }

    [DataContract]
    public class PPE
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public int ppe_code { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public int area_id { get; set; }
        [DataMember]
        public string area_name { get; set; }
        [DataMember]
        public bool is_tom { get; set; }
        [DataMember]
        public string ppe_full_name { get; set; }
        [DataMember]
        public int class_num { get; set; }
    }

    [DataContract]
    public class BlankTypes
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public int blank_kind_id { get; set; }
        [DataMember]
        public string short_name { get; set; }
    }

    [DataContract]
    public class JournalContent
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public int exam_date_id { get; set; }
        [DataMember]
        public int ppe_id { get; set; }
        [DataMember]
        public bool ppe_is_tom { get; set; }
        [DataMember]
        public int area_id { get; set; }
        [DataMember]
        public string area_name { get; set; }
        [DataMember]
        public int blank_type_id { get; set; }
        [DataMember]
        public int aud { get; set; }
        [DataMember]
        public int count_blanks { get; set; }
        [DataMember]
        public int count_add_blanks { get; set; }
    }

    [DataContract]
    public class PPEExam
    {
        [DataMember]
        public int exam_date_id { get; set; }
        [DataMember]
        public int ppe_id { get; set; }
        [DataMember]
        public int ppe_code { get; set; }
        [DataMember]
        public string ppe_name { get; set; }
        [DataMember]
        public int area_id { get; set; }
        [DataMember]
        public string area_name { get; set; }
        [DataMember]
        public int status_id { get; set; }
        [DataMember]
        public string status_name { get; set; }
        [DataMember]
        public int class_num { get; set; }
    }

    [DataContract]
    public class PPEExamContent
    {
        [DataMember]
        public int exam_date_id { get; set; }
        [DataMember]
        public string exam_date { get; set; }
        [DataMember]
        public string ppe_name { get; set; }
        [DataMember]
        public int ppe_id { get; set; }
        [DataMember]
        public int ppe_code { get; set; }
        [DataMember]
        public int ppe_exam_id { get; set; }
        [DataMember]
        public string ppe_files_path { get; set; }
        [DataMember]
        public int status_id { get; set; }
        [DataMember]
        public string status_name { get; set; }
        [DataMember]
        public bool is_uploaded { get; set; }
        [DataMember]
        public List<PPEExamBlank> ListPPEExamBlanks { get; set; }
        [DataMember]
        public List<PPEExamFile> exam_files { get; set; }
        [DataMember]
        public List<PPEExamMessage> messages { get; set; }
    }

    [DataContract]
    public class PPEExamMessage
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public int ppe_exam_id { get; set; }
        [DataMember]
        public int user_id { get; set; }
        [DataMember]
        public string user_role { get; set; }
        [DataMember]
        public string fio { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public DateTime mes_date { get; set; }
    }

    [DataContract]
    public class PPEExamBlank
    {
        [DataMember]
        public int ppe_exam_id { get; set; }
        [DataMember]
        public int aud { get; set; }
        [DataMember]
        public int count_r { get; set; }
        [DataMember]
        public int count_1 { get; set; }
        [DataMember]
        public int count_2 { get; set; }
        [DataMember]
        public int count_add_2 { get; set; }
    }

    [DataContract]
    public class FTPFile
    {
        [DataMember]
        public string file_folder { get; set; }
        [DataMember]
        public string file_name { get; set; }
        [DataMember]
        public int size { get; set; }
    }

    [DataContract]
    public class PPEExamFile
    {
        [DataMember]
        public int ppe_exam_id { get; set; }
        [DataMember]
        public int blank_type_id { get; set; }
        [DataMember]
        public string blank_type_name { get; set; }
        [DataMember]
        public string blank_folder_name { get; set; }
        [DataMember]
        public string upload_path_files { get; set; }
        [DataMember]
        public int upload_count_files { get; set; }
        [DataMember]
        public string server_file_folder { get; set; }
        [DataMember]
        public List<FTPFile> ftp_files { get; set; }
    }
}
