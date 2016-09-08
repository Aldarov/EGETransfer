using System;
using EGEJournal.ServiceEGE;
using System.Windows;
using System.ServiceModel;
using System.ComponentModel;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using EGEJournal.Helpers;
using EGEJournal.Infrustructure;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;

namespace EGEJournal.Model
{
    public enum ModifyType
    {
        Insert,
        Update,
        Delete
    }

    public class TaskResult
    {
        public object result { get; set; }
        public Fault fault { get; set; }
    }

    public class DataService : IDataService
    {
        public UserInfo user_info { get; private set; }

        private WorkWithFTP ftp_service;

        public ObservableCollection<Journal> ListJournals { get; set; }
        public ObservableCollection<Area> ListAreas { get; set; }
        public List<PPE> ListPPE { get; set; }
        public List<PPE> ListPPE9 { get; set; }

        public ObservableCollection<BlankTypes> ListBlankTypes { get; set; }

        public bool isUserRCOI 
        {
            get 
            {
                if (user_info != null && user_info.role_name == "rcoi")
                    return true;
                else
                    return false;
            }
        }

        public bool isUserPPE
        {
            get
            {
                if (user_info != null && user_info.role_name == "ppe")
                    return true;
                else
                    return false;
            }
        }

        public bool isUserMOUO
        {
            get
            {
                if (user_info != null && user_info.role_name == "mouo")
                    return true;
                else
                    return false;
            }
        }

        public bool isUserScan
        {
            get
            {
                if (user_info != null && user_info.role_name == "scan")
                    return true;
                else
                    return false;
            }
        }

        public DataService()
        {
        }

        //private void ServiceRun<T>(EGESupportClient service, T result, Exception error, Action<T, Fault> FuncComplete, Action<T> Func)
        //{
        //    try
        //    {
        //        if (error != null)
        //        {
        //            if (error is FaultException<Fault>)
        //                FuncComplete(default(T), ((FaultException<Fault>)error).Detail);
        //            else
        //                FuncComplete(default(T), new Fault() { Token = "error", Message = error.Message });
        //        }
        //        else
        //        {
        //            Func(result);
        //            FuncComplete(result, null);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        FuncComplete(default(T), new Fault() { Token = "error", Message = e.Message });
        //    }
        //    finally
        //    {
        //        if (service.State == CommunicationState.Faulted)
        //            service.Abort();
        //        else
        //            service.Close();
        //        service = null;
        //    }
        //}

        private async void ServiceRunTask(Action<Fault> FuncComplete, Func<EGESupportClient, Task> Func)
        {
            EGESupportClient service = GetService(null, null);
            try
            {
                await Func(service);
            }
            catch (Exception error)
            {
                if ((error is FaultException<Fault>))
                    FuncComplete(((FaultException<Fault>)error).Detail);
                else if (error.InnerException != null && error.InnerException is FaultException<Fault>)
                    FuncComplete(((FaultException<Fault>)error.InnerException).Detail);
                else if (error.InnerException != null)
                    FuncComplete(new Fault() { Token = "error", Message = error.InnerException.Message });
                else
                    FuncComplete(new Fault() { Token = "error", Message = error.Message });
            }
            finally
            {
                if (service.State == CommunicationState.Faulted)
                    service.Abort();
                else
                    service.Close();
                service = null;
            }
        }

        private Task<TaskResult> ServiceRunTask(Func<EGESupportClient, object> Func, string login = null, string password = null)
        {
            return Task<TaskResult>.Factory.StartNew(() =>
                {
                    TaskResult res = new TaskResult();
                    EGESupportClient service = GetService(login, password);
                    try
                    {
                        res.result = Func(service);
                    }
                    catch (Exception error)
                    {
                        if ((error is FaultException<Fault>))
                            res.fault = ((FaultException<Fault>)error).Detail;
                        else if (error.InnerException != null && error.InnerException is FaultException<Fault>)
                            res.fault = ((FaultException<Fault>)error.InnerException).Detail;
                        else if (error.InnerException != null)
                            res.fault = new Fault() { Token = "error", Message = error.InnerException.Message };
                        else
                            res.fault = new Fault() { Token = "error", Message = error.Message };
                    }
                    finally
                    {
                        if (service.State == CommunicationState.Faulted)
                            service.Abort();
                        else
                            service.Close();
                        service = null;
                    }
                    return res;
                });
        }

        private EGESupportClient GetService(string login, string password)
        {
            EGESupportClient service = new EGESupportClient();
            if (login != null && password != null)
            {
                service.ClientCredentials.UserName.UserName = login;
                service.ClientCredentials.UserName.Password = password;
            }
            else
            {
                service.ClientCredentials.UserName.UserName = user_info.login;
                service.ClientCredentials.UserName.Password = user_info.password;
            }
            return service;
        }

        public Task<TaskResult> ValidateUser(string login, string password)
        {
            return ServiceRunTask(service =>
            {
                user_info = service.ValidateUser(login, password);
                if (user_info != null)
                {
                    user_info.password = password;
                    ftp_service = new WorkWithFTP(user_info.ftp_access);
                }                    
                return null;
            }, "auth", "authq2w13");
        }

        public void LoadBaseClasesRCOI(Action<Fault> complete)
        {
            ServiceRunTask(complete,
                async (service) =>
                {
                    Task[] taskArray = new Task[4];
                    taskArray[0] = Task<List<Journal>>.Factory.StartNew(() =>
                        {
                            return service.GetListJournals().ToList();
                        });
                    taskArray[1] = Task<List<Area>>.Factory.StartNew(() =>
                    {
                        return service.GetListAreas().ToList();
                    });
                    taskArray[2] = Task<List<PPE>>.Factory.StartNew(() =>
                    {
                        return service.GetListPPE().ToList();
                    });
                    taskArray[3] = Task<List<BlankTypes>>.Factory.StartNew(() =>
                    {
                        return service.GetListBlankTypes().ToList();
                    });

                    //await Task.WhenAll(taskArray);
                    await Task.Factory.StartNew(() => Task.WaitAll(taskArray));

                    ListJournals = new ObservableCollection<Journal>(((Task<List<Journal>>)taskArray[0]).Result);
                    ListAreas = new ObservableCollection<Area>(((Task<List<Area>>)taskArray[1]).Result);
                    ListPPE = ((Task<List<PPE>>)taskArray[2]).Result;
                    ListBlankTypes = new ObservableCollection<BlankTypes>(((Task<List<BlankTypes>>)taskArray[3]).Result.Where(x => x.blank_kind_id == 1));
                    complete(null);
                });
        }

        public void LoadBaseClasesPPE(Action<Fault> complete)
        {
            ServiceRunTask(complete,
                async (service) =>
                {
                    Task[] taskArray = new Task[1];
                    taskArray[0] = Task<List<Journal>>.Factory.StartNew(() =>
                    {
                        return service.GetListJournals().ToList();
                    });
                    await Task.Factory.StartNew(() => Task.WaitAll(taskArray));

                    ListJournals = new ObservableCollection<Journal>(((Task<List<Journal>>)taskArray[0]).Result.Where(x => x.class_number == user_info.ppe_class_number));

                    complete(null);
                });
        }

        public void LoadBaseClasesMOUO(Action<Fault> complete)
        {
            ServiceRunTask(complete,
                async (service) =>
                {
                    Task[] taskArray = new Task[2];
                    taskArray[0] = Task<List<Journal>>.Factory.StartNew(() =>
                    {
                        return service.GetListJournals().ToList();
                    });
                    taskArray[1] = Task<List<PPE>>.Factory.StartNew(() =>
                    {
                        return service.GetListPPE().ToList();
                    });
                    await Task.Factory.StartNew(() => Task.WaitAll(taskArray));

                    ListJournals = new ObservableCollection<Journal>(((Task<List<Journal>>)taskArray[0]).Result);
                    ListPPE9 = ((Task<List<PPE>>)taskArray[1]).Result.Where(x => x.class_num == 9 && x.area_id == user_info.area_id).ToList();
                    complete(null);
                });
        }

        #region JournalEditViewModel

        public Journal CurrentJournal { get; private set; }
        public ObservableCollection<PPE> ListPPEForJournal { get; private set; }
        public ObservableCollection<JournalContent> CurrentJournalContent { get; set; }

        public async void SetCurrentJournal(int exam_date_id, Action<Fault> complete)
        {
            CurrentJournal = ListJournals.Where(x => x.exam_date_id == exam_date_id).SingleOrDefault();
            ListPPEForJournal = new ObservableCollection<PPE>(ListPPE.Where(x => x.class_num == CurrentJournal.class_number));
            TaskResult res = await UpdateCurrentJournalContent(exam_date_id);
            complete(res.fault);
        }

        public Task<TaskResult> UpdateCurrentJournalContent(int exam_date_id)
        {
            return ServiceRunTask(service =>
                {
                    CurrentJournalContent = new ObservableCollection<JournalContent>(service.GetJournalContent(exam_date_id).ToList());
                    return null;
                });
        }

        public Task<TaskResult> SaveJournalContent(JournalContent content, ModifyType modify)
        {
            Task<TaskResult> res = null;
            if (modify == ModifyType.Update)
            {
                res = ServiceRunTask(service =>
                {
                    service.UpdateJournalContent(content);
                    return null;
                });
            }
            else if (modify == ModifyType.Insert)
            {
                res = ServiceRunTask(service =>
                {
                    content.exam_date_id = CurrentJournal.exam_date_id;
                    return service.InsertJournalContent(content);
                });
            }
            else if (modify == ModifyType.Delete)
            {
                res = ServiceRunTask(service =>
                {
                    return service.DeleteJournalContent(content.id);
                });
            }
            return res;
        }

        #endregion

        #region PPEExam
        public Journal CurrentJournalForPPE { get; private set; }
        public ObservableCollection<PPEExam> CurrentListPPEExam { get; private set; }
        public PPEExamContent CurrentPPEExamContent { get; private set; }
        public ObservableCollection<PPEExamBlank> CurrentPPEExamBlanks { get; set; }

        public Journal CurrentPPEJournal { get; private set; }

        public Task<TaskResult> SetCurrentListPPEExam(int exam_date_id)
        {
            Task<TaskResult> res = null;
            res = ServiceRunTask(service =>
            {
                int area_id = -1;
                if (isUserMOUO)
                    area_id = (int)user_info.area_id;
                CurrentListPPEExam = new ObservableCollection<PPEExam>(
                    from p in service.GetListPPEExam(exam_date_id, area_id)
                    orderby p.area_id, p.ppe_code
                    select p);
                CurrentJournalForPPE = ListJournals.Where(x => x.exam_date_id == exam_date_id).SingleOrDefault();
                CurrentPPEJournal = ListJournals.Where(x => x.exam_date_id == exam_date_id).SingleOrDefault();
                return CurrentListPPEExam;
            });
            return res;
        }

        public Task<TaskResult> SetCurrentPPEExam(int exam_date_id, int ppe_id)
        {
            Task<TaskResult> res = null;
            res = ServiceRunTask(service =>
            {
                CurrentPPEExamContent = service.GetPPEExamContent(exam_date_id, ppe_id);
                CurrentPPEExamBlanks = new ObservableCollection<PPEExamBlank>(CurrentPPEExamContent.ListPPEExamBlanks);
                return CurrentPPEExamContent;
            });
            return res;
        }

        public Task<TaskResult> FTPDownloadFile(FTPFile file, string targetPath, Action<int> ProgressChange)
        {
            Task<TaskResult> res = null;
            res = ServiceRunTask(service =>
            {
                return ftp_service.FTPDownloadFile(targetPath, file, ProgressChange);
            });
            return res;
        }

        public Task<TaskResult> FTPUploadPPEFile(PPEExamFile file, Action<string, int> ProgressChange)
        {
            Task<TaskResult> res = null;
            res = ServiceRunTask(service =>
            {
                string file_name = service.GetUploadFileName(file.ppe_exam_id, file.blank_folder_name) + ".zip";
                FastZipEvents zip_events = new FastZipEvents();
                int uptoFileCount = 0;
                zip_events.ProcessFile = (s, e) => 
                {
                    uptoFileCount++;
                    ProgressChange("Архивирование... ", uptoFileCount * 100 / file.upload_count_files);
                };
                FastZip zip = new FastZip(zip_events);
                zip.CreateZip(file_name, file.upload_path_files, true, null);
                return ftp_service.FTPUploadFile(file_name, file.server_file_folder, (p) => 
                {
                    ProgressChange("Загрузка... ", p);
                });
            });
            return res;
        }

        public Task<TaskResult> SavePPEExamBlank(PPEExamBlank content, ModifyType modify, int old_aud = -1)
        {
            Task<TaskResult> res = null;
            if (modify == ModifyType.Update)
            {
                res = ServiceRunTask(service =>
                {
                    service.UpdatePPEExamBlank(content, old_aud);
                    return null;
                });
            }
            else if (modify == ModifyType.Insert)
            {
                res = ServiceRunTask(service =>
                {
                    content.ppe_exam_id = CurrentPPEExamContent.ppe_exam_id;
                    return service.InsertPPEExamBlank(content);
                });
            }
            else if (modify == ModifyType.Delete)
            {
                res = ServiceRunTask(service =>
                {
                    return service.DeletePPEExamBlank(content);
                });
            }
            return res;
        }

        public Task<TaskResult> SetIsUploadPPEExam(int ppe_exam_id)
        {
            Task<TaskResult> res = null;
                res = ServiceRunTask(service =>
                {
                    service.SetIsUploadPPEExam(ppe_exam_id);
                    return null;
                });
            return res;
        }

        public Task<TaskResult> ChangeStatusPPEExam(int ppe_exam_id, int status_id)
        {
            Task<TaskResult> res = null;
                res = ServiceRunTask(service =>
                {
                    service.ChangeStatusPPEExam(ppe_exam_id, status_id);
                    return null;
                });
            return res;
        }

        public Task<TaskResult> SendPPEMessage(PPEExamMessage mes)
        {
            Task<TaskResult> res = null;
            res = ServiceRunTask(service =>
            {
                service.SendPPEMessage(mes);
                return null;
            });
            return res;
        }

        #endregion

    }
}