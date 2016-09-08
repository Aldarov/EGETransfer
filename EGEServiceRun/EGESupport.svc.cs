using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EGEServiceRun.Model;
using System.Security.Permissions;
using Crypto;
using EGEServiceRun.Infrustructure;
using System.Configuration;
using System.Web;
using EGEServiceRun.App_Code.Security;
using System.Collections.ObjectModel;
using System.Security.Principal;
using System.Globalization;

namespace EGEServiceRun
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "EGESupport" в коде, SVC-файле и файле конфигурации.
    // ПРИМЕЧАНИЕ. Чтобы запустить клиент проверки WCF для тестирования службы, выберите элементы EGESupport.svc или EGESupport.svc.cs в обозревателе решений и начните отладку.
    //[AspNetCompatibilityRequirements(
    //    RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class EGESupport : IEGESupport
    {
        public EGESupport()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

        private T TryFunc<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (FaultException<Fault>)
            {
                throw;
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.InnerException != null)
                    throw new FaultException<Fault>(new Fault() { Token = "error", Message = e.InnerException.InnerException.Message }, "soap error");
                else if (e.InnerException != null)
                    throw new FaultException<Fault>(new Fault() { Token = "error", Message = e.InnerException.Message }, "soap error");
                else
                    throw new FaultException<Fault>(new Fault() { Token = "error", Message = e.Message }, "soap error");
            }
        }

        private byte[] GetFTPPPEAccess(string role_name)
        {
            string ftp_access;
            if (role_name == "rcoi")
                ftp_access = ConfigurationManager.AppSettings["ftp_external_address"] + "#" +
                    ConfigurationManager.AppSettings["ftp_login"] + "#" + ConfigurationManager.AppSettings["ftp_password"];
            else
                ftp_access = ConfigurationManager.AppSettings["ftp_address"] + "#" +
                    ConfigurationManager.AppSettings["ftp_login"] + "#" + ConfigurationManager.AppSettings["ftp_password"];

            byte[] result = AES.EncryptStringToBytes(ftp_access, CKeys.key1, CKeys.key2);
            return result;
        }  

        [PrincipalPermission(SecurityAction.Demand, Role = "auth")]
        public UserInfo ValidateUser(string login, string password)
        {
            return TryFunc<UserInfo>(() =>
            {
                UserInfo user = null;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    user = (from u in context.au_users
                            join r in context.au_roles on u.role_id equals r.id
                            join p in context.ppe on u.ppe_id equals p.id into pp
                            from isppe in pp.DefaultIfEmpty()
                            where u.login == login && u.password == password
                            select new UserInfo()
                            {
                                user_id = u.id,
                                login = u.login,
                                fio = u.fio,
                                role_name = r.name,
                                area_id = u.area_id,
                                ppe_id = u.ppe_id,
                                ppe_class_number = isppe.@class
                            }).FirstOrDefault();
                    if (user != null)
                    {
                        user.ftp_access = GetFTPPPEAccess(user.role_name);
                    }
                }
                if (user == null)
                {
                    throw new FaultException<Fault>(new Fault() { Token = "Password", Message = "Неверный логин или пароль" }, "soap error");
                }
                return user;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "rcoi")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ppe")]
        [PrincipalPermission(SecurityAction.Demand, Role = "scan")]
        [PrincipalPermission(SecurityAction.Demand, Role = "mouo")]
        public List<Journal> GetListJournals()
        {
            return TryFunc<List<Journal>>(() =>
            {
                List<Journal> jour = null;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    jour = (from j in context.get_list_journals()
                           orderby j.exam_date, j.subject_id
                           select new Journal() 
                           { 
                               exam_date_id = j.exam_date_id,
                               journal_name = j.journal_name,
                               exam_date = j.exam_date,
                               subject_id = j.subject_id,
                               subject_name = j.subject_name,
                               description = j.description,
                               class_number = j.@class
                           }).ToList();
                }
                return jour;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "rcoi")]
        [PrincipalPermission(SecurityAction.Demand, Role = "scan")]
        public List<Area> GetListAreas()
        {
            return TryFunc<List<Area>>(() =>
            {
                List<Area> area = null;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    area = (from a in context.areas
                            orderby a.id
                            select new Area() 
                            { 
                                id = a.id,
                                name = a.name,
                                folder_name = a.folder_name
                            }).ToList();
                }
                return area;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "rcoi")]
        [PrincipalPermission(SecurityAction.Demand, Role = "scan")]
        [PrincipalPermission(SecurityAction.Demand, Role = "mouo")]
        public List<PPE> GetListPPE()
        {
            return TryFunc<List<PPE>>(() =>
            {
                List<PPE> ppe = null;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    ppe = (from p in context.ppe
                           join a in context.areas on p.area_id equals a.id
                            select new PPE()
                            {
                                id = p.id,
                                ppe_code = p.ppe_code,
                                name = p.name,
                                area_id = p.area_id,
                                area_name = a.name,
                                is_tom = p.is_tom,
                                ppe_full_name = p.ppe_code.ToString() + " - " + p.name,
                                class_num = p.@class
                            }).ToList();
                }
                return ppe;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "rcoi")]
        [PrincipalPermission(SecurityAction.Demand, Role = "scan")]
        public List<BlankTypes> GetListBlankTypes()
        {
            return TryFunc<List<BlankTypes>>(() =>
            {
                List<BlankTypes> BlankTypes = null;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    BlankTypes = (from b in context.blank_types
                                  select new BlankTypes()
                                  {
                                      id= b.id,
                                      name = b.name,
                                      blank_kind_id = b.blank_kind_id,
                                      short_name = b.short_name
                                  }).ToList();
                }
                return BlankTypes;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "rcoi")]
        [PrincipalPermission(SecurityAction.Demand, Role = "scan")]
        public List<JournalContent> GetJournalContent(int exam_date_id)
        {
            return TryFunc<List<JournalContent>>(() =>
            {
                List<JournalContent> content = null;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    content = (from j in context.get_journal_content()
                               join p in context.ppe on j.ppe_id equals p.id
                               join a in context.areas on p.area_id equals a.id
                               where j.exam_date_id == exam_date_id
                               select new JournalContent()
                               {
                                   id = (int)j.id,
                                   exam_date_id = j.exam_date_id,
                                   ppe_id = j.ppe_id,
                                   ppe_is_tom = p.is_tom,
                                   area_id = a.id,
                                   area_name = a.name,
                                   blank_type_id = j.blank_type_id,
                                   aud = j.aud,
                                   count_blanks = (int)j.count_blanks,
                                   count_add_blanks = (int)j.count_add_blanks
                               }).ToList();
                }
                return content;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "rcoi")]
        public void UpdateJournalContent(JournalContent content)
        {
            TryFunc<bool>(() =>
                {
                    bool res = false;
                    using (EGESupportEntities context = new EGESupportEntities())
                    {
                        ppe_exams ppe_exam = (from p in context.ppe_exams
                                           where (p.exam_date_id == content.exam_date_id) && (p.ppe_id == content.ppe_id)
                                           select p).FirstOrDefault();
                        int ppe_exam_id;
                        if (ppe_exam == null)
                        {
                            ppe_exams p = new ppe_exams()
                            {
                                exam_date_id = content.exam_date_id,
                                ppe_id = content.ppe_id
                            };
                            context.ppe_exams.Add(p);
                            context.SaveChanges();
                            ppe_exam_id = p.id;
                        }
                        else
                        {
                            ppe_exam_id = ppe_exam.id;
                            int double_rec = (from jour in context.journals
                                              where jour.ppe_exam_id == ppe_exam_id && jour.blank_type_id == content.blank_type_id && jour.auditorium == content.aud && jour.id != content.id
                                              select jour.id).Count();
                            if (double_rec > 0)
                            {
                                throw new FaultException<Fault>(new Fault() { Token = "error", Message = "В базу данных уже внесена запись с аналогичным ППЭ, ауд и типом бланка" }, "soap error");
                            }
                        }
                            
                        journals j = context.journals.Where(x => x.id == content.id).SingleOrDefault();
                        j.ppe_exam_id = ppe_exam_id;
                        j.blank_type_id = content.blank_type_id;
                        j.auditorium = content.aud;
                        j.count_blanks = content.count_blanks;
                        j.count_add_blanks = content.blank_type_id == 3 ? content.count_add_blanks : 0;
                        context.SaveChanges();
                        res = true;
                    }
                    return res;
                });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "rcoi")]
        public int InsertJournalContent(JournalContent content)
        {
            return TryFunc<int>(() =>
            {
                int id = 0;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    ppe_exams ppe_exam = (from p in context.ppe_exams
                                          where (p.exam_date_id == content.exam_date_id) && (p.ppe_id == content.ppe_id)
                                          select p).FirstOrDefault();
                    int ppe_exam_id;
                    if (ppe_exam == null)
                    {
                        ppe_exams p = new ppe_exams()
                        {
                            exam_date_id = content.exam_date_id,
                            ppe_id = content.ppe_id
                        };
                        context.ppe_exams.Add(p);
                        context.SaveChanges();
                        ppe_exam_id = p.id;
                    }
                    else
                    {
                        ppe_exam_id = ppe_exam.id;
                        int double_rec = (from jour in context.journals
                                          where jour.ppe_exam_id == ppe_exam_id && jour.blank_type_id == content.blank_type_id && jour.auditorium == content.aud && jour.id != content.id
                                          select jour.id).Count();
                        if (double_rec > 0)
                        {
                            throw new FaultException<Fault>(new Fault() { Token = "error", Message = "В базу данных уже внесена запись с аналогичным ППЭ, ауд и типом бланка" }, "soap error");
                        }
                    }

                    journals j = new journals();
                    j.ppe_exam_id = ppe_exam_id;
                    j.blank_type_id = content.blank_type_id;
                    j.auditorium = content.aud;
                    j.count_blanks = content.count_blanks;
                    j.count_add_blanks = content.blank_type_id == 3 ? content.count_add_blanks : 0;
                    context.journals.Add(j);
                    context.SaveChanges();
                    id = j.id;
                }
                return id;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "rcoi")]
        public bool DeleteJournalContent(int id)
        {
            return TryFunc<bool>(() =>
            {
                bool res = false;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    journals j = new journals(){ id = id };
                    context.journals.Attach(j);
                    context.journals.Remove(j);
                    context.SaveChanges();
                    res = true;
                }
                return res;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "rcoi")]
        [PrincipalPermission(SecurityAction.Demand, Role = "mouo")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ppe")]
        [PrincipalPermission(SecurityAction.Demand, Role = "scan")]
        public PPEExamContent GetPPEExamContent(int exam_date_id, int ppe_id)
        {
            return TryFunc<PPEExamContent>(() =>
            {
                PPEExamContent res = new PPEExamContent();
                res.exam_date_id = exam_date_id;
                res.ppe_id = ppe_id;

                using (EGESupportEntities context = new EGESupportEntities())
                {
                    var exam = (from e in context.exam_dates
                                     join s in context.subjects on e.subject_id equals s.id
                                     where e.id == exam_date_id
                                     select e).SingleOrDefault();
                    res.exam_date = "Выгрузка за " + exam.exam_date.ToString("dd.MM.yyyy") + " ЕГЭ " + exam.@class.ToString() + " класс (" + exam.subjects.name + ")";
                    ppe cur_ppe = (from p in context.ppe
                                   join a in context.areas on p.area_id equals a.id
                                   where p.id == ppe_id
                                   select p).SingleOrDefault();
                    res.ppe_name = "Код ППЭ: " + cur_ppe.ppe_code + " - " + cur_ppe.name + " (" + cur_ppe.areas.name + ")";
                    res.ppe_code = cur_ppe.ppe_code;
                    ppe_exams ppe_exam = (from p in context.ppe_exams
                                      where p.exam_date_id == exam_date_id && p.ppe_id == ppe_id
                                      select p).SingleOrDefault();
                    if (ppe_exam == null)
                    {
                        ppe_exam = new ppe_exams()
                        {
                            exam_date_id = exam_date_id,
                            ppe_id = ppe_id,
                            status_id = 1,
                            is_upload_data = false
                        };
                        context.ppe_exams.Add(ppe_exam);
                    }
                    if (string.IsNullOrEmpty(ppe_exam.files_path))
                    {
                        ppe_exam.files_path = context.get_ppe_files_path(exam_date_id, ppe_id).SingleOrDefault();
                    }
                    context.SaveChanges();
                    res.is_uploaded = ppe_exam.is_upload_data;
                    res.ppe_exam_id = ppe_exam.id;
                    res.ppe_files_path = ppe_exam.files_path;
                    res.status_id = (int)ppe_exam.status_id;
                    res.status_name = context.ppe_exam_statuses.Where(x => x.id == res.status_id).Select(x => x.name).SingleOrDefault();
                    List<journals> blanks = (from j in context.journals
                                 where j.ppe_exam_id == ppe_exam.id
                                 select j).ToList();
                    List<PPEExamBlank> exam_blanks = new List<PPEExamBlank>();
                    foreach (int auditorium in blanks.Select(x => x.auditorium).Distinct())
                    {
                        int? count_r = blanks.Where(x => x.auditorium == auditorium && x.blank_type_id == 1).Select(x => x.count_blanks).SingleOrDefault();
                        int? count_1 = blanks.Where(x => x.auditorium == auditorium && x.blank_type_id == 2).Select(x => x.count_blanks).SingleOrDefault();
                        int? count_2 = blanks.Where(x => x.auditorium == auditorium && x.blank_type_id == 3).Select(x => x.count_blanks).SingleOrDefault();
                        int? count_add_2 = blanks.Where(x => x.auditorium == auditorium && x.blank_type_id == 3).Select(x => x.count_add_blanks).SingleOrDefault();
                        exam_blanks.Add(new PPEExamBlank() 
                        { 
                            ppe_exam_id = ppe_exam.id,
                            aud = auditorium,
                            count_r = count_r != null ? (int)count_r : 0,
                            count_1 = count_1 != null ? (int)count_1 : 0,
                            count_2 = count_2 != null ? (int)count_2 : 0,
                            count_add_2 = count_add_2 != null ? (int)count_add_2 : 0
                        });
                    }
                    res.ListPPEExamBlanks = exam_blanks;
                    res.messages = (from m in context.ppe_messages
                                    join u in context.au_users on m.user_id equals u.id
                                    join r in context.au_roles on u.role_id equals r.id
                                    where m.ppe_exam_id == ppe_exam.id
                                    select new PPEExamMessage()
                                    {
                                        id = m.id,
                                        ppe_exam_id = m.ppe_exam_id,
                                        fio = u.fio,
                                        user_id = u.id,
                                        user_role = r.description,
                                        message = m.message,
                                        mes_date = m.mes_date
                                    }).ToList();
                    List<PPEExamFile> exam_files = (from e in context.exam_dates
                                                    join s in context.subjects on e.subject_id equals s.id
                                                    join sb in context.subject_blanks on s.id equals sb.subject_id
                                                    join bt in context.blank_types on sb.blank_type_id equals bt.id
                                                    where e.id == exam_date_id
                                                    select new PPEExamFile()
                                                    {
                                                        ppe_exam_id = ppe_exam.id,
                                                        blank_type_id = bt.id,
                                                        blank_type_name = bt.name,
                                                        blank_folder_name = bt.folder_name,
                                                        server_file_folder = ppe_exam.files_path + "/" + bt.folder_name
                                                    }).ToList();
                    foreach (PPEExamFile file in exam_files)
                    {
                        string work_dir = ppe_exam.files_path + "/" + file.blank_folder_name;
                        WorkWithFTP.FTPMakeDir(work_dir);
                        file.ftp_files = WorkWithFTP.GetListFilesFromDir(work_dir);
                    }
                    res.exam_files = exam_files;
                }
                return res;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "mouo")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ppe")]
        public string GetUploadFileName(int ppe_exam_id, string blank_folder_name)
        {
            return TryFunc<string>(() =>
            {
                string res = "";
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm", new CultureInfo("ru-RU"));
                    res = (from pe in context.ppe_exams
                             join e in context.exam_dates on pe.exam_date_id equals e.id
                             join s in context.subjects on e.subject_id equals s.id
                             join p in context.ppe on pe.ppe_id equals p.id
                             join a in context.areas on p.area_id equals a.id
                             where pe.id == ppe_exam_id
                             select s.folder_name + "_" + blank_folder_name + "_" +
                                 a.folder_name + "_" + p.ppe_code + "_" + date
                            ).SingleOrDefault();
                }
                return res;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "mouo")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ppe")]
        public void UpdatePPEExamBlank(PPEExamBlank exam_blanks, int old_aud)
        {
            TryFunc<bool>(() =>
            {
                bool res = false;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    IEnumerable<journals> jours = (from j in context.journals
                                                   where j.ppe_exam_id == exam_blanks.ppe_exam_id && j.auditorium == old_aud
                                           select j);

                    journals jour = jours.Where(x => x.blank_type_id == 1).SingleOrDefault();
                    if (exam_blanks.count_r > 0)
                    {
                        if (jour != null)
                        {
                            jour.auditorium = exam_blanks.aud;
                            jour.count_blanks = exam_blanks.count_r;
                        }
                        else
                        {
                            context.journals.Add(new journals()
                            {
                                ppe_exam_id = exam_blanks.ppe_exam_id,
                                blank_type_id = 1,
                                auditorium = exam_blanks.aud,
                                count_blanks = exam_blanks.count_r,
                                count_add_blanks = 0
                            });
                        }
                    }
                    else
                    {
                        if (jour != null)
                        {
                            context.journals.Remove(jour);
                        }
                    }

                    jour = jours.Where(x => x.blank_type_id == 2).SingleOrDefault();
                    if (exam_blanks.count_1 > 0)
                    {
                        if (jour != null)
                        {
                            jour.auditorium = exam_blanks.aud;
                            jour.count_blanks = exam_blanks.count_1;
                        }
                        else
                        {
                            context.journals.Add(new journals()
                            {
                                ppe_exam_id = exam_blanks.ppe_exam_id,
                                blank_type_id = 2,
                                auditorium = exam_blanks.aud,
                                count_blanks = exam_blanks.count_1,
                                count_add_blanks = 0
                            });
                        }
                    }
                    else
                    {
                        if (jour != null)
                        {
                            context.journals.Remove(jour);
                        }
                    }

                    jour = jours.Where(x => x.blank_type_id == 3).SingleOrDefault();
                    if (exam_blanks.count_2 > 0)
                    {
                        if (jour != null)
                        {
                            jour.auditorium = exam_blanks.aud;
                            jour.count_blanks = exam_blanks.count_2;
                            jour.count_add_blanks = exam_blanks.count_add_2;
                        }
                        else
                        {
                            context.journals.Add(new journals()
                            {
                                ppe_exam_id = exam_blanks.ppe_exam_id,
                                blank_type_id = 3,
                                auditorium = exam_blanks.aud,
                                count_blanks = exam_blanks.count_2,
                                count_add_blanks = exam_blanks.count_add_2
                            });
                        }
                    }
                    else
                    {
                        if (jour != null)
                        {
                            context.journals.Remove(jour);
                        }
                    }
                    context.SaveChanges();
                    res = true;
                }
                return res;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "mouo")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ppe")]
        public int InsertPPEExamBlank(PPEExamBlank exam_blanks)
        {
            return TryFunc<int>(() =>
            {
                int aud = exam_blanks.aud;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    if (exam_blanks.count_r > 0)
                    {
                        context.journals.Add(new journals()
                        {
                            ppe_exam_id = exam_blanks.ppe_exam_id,
                            blank_type_id = 1,
                            auditorium = exam_blanks.aud,
                            count_blanks = exam_blanks.count_r,
                            count_add_blanks = 0
                        });
                    }

                    if (exam_blanks.count_1 > 0)
                    {
                        context.journals.Add(new journals()
                        {
                            ppe_exam_id = exam_blanks.ppe_exam_id,
                            blank_type_id = 2,
                            auditorium = exam_blanks.aud,
                            count_blanks = exam_blanks.count_1,
                            count_add_blanks = 0
                        });
                    }

                    if (exam_blanks.count_2 > 0)
                    {
                        context.journals.Add(new journals()
                        {
                            ppe_exam_id = exam_blanks.ppe_exam_id,
                            blank_type_id = 3,
                            auditorium = exam_blanks.aud,
                            count_blanks = exam_blanks.count_2,
                            count_add_blanks = exam_blanks.count_add_2
                        });
                    }
                    context.SaveChanges();
                }
                return aud;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "mouo")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ppe")]
        public bool DeletePPEExamBlank(PPEExamBlank exam_blanks)
        {
            return TryFunc<bool>(() =>
            {
                bool res = false;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    IEnumerable<journals> jours = context.journals.Where(x => x.ppe_exam_id == exam_blanks.ppe_exam_id && x.auditorium == exam_blanks.aud);
                    context.journals.RemoveRange(jours);
                    context.SaveChanges();
                    res = true;
                }
                return res;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "rcoi")]
        [PrincipalPermission(SecurityAction.Demand, Role = "mouo")]
        [PrincipalPermission(SecurityAction.Demand, Role = "scan")]
        public List<PPEExam> GetListPPEExam(int exam_date_id, int area_id = -1)
        {
            return TryFunc<List<PPEExam>>(() =>
            {
                List<PPEExam> ppe_exam = null;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    string role = (Thread.CurrentPrincipal as CustomPrincipal).Roles[0];
                    if (role == "rcoi" || role == "scan")
                    {
                        ppe_exam = (from pe in context.ppe_exams
                                    join s in context.ppe_exam_statuses on pe.status_id equals s.id
                                    join p in context.ppe on pe.ppe_id equals p.id
                                    join a in context.areas on p.area_id equals a.id
                                    where pe.exam_date_id == exam_date_id
                                    select new PPEExam()
                                    {
                                        area_id = a.id,
                                        area_name = a.name,
                                        exam_date_id = pe.exam_date_id,
                                        ppe_code = p.ppe_code,
                                        ppe_id = p.id,
                                        ppe_name = p.name,
                                        status_id = pe.status_id != null ? (int)pe.status_id : 1,
                                        status_name = s.name,
                                        class_num = p.@class
                                    }).ToList();
                    }
                    else if (role == "mouo" && area_id > 0)
                    {
                        ppe_exam = (from pe in context.ppe_exams
                                    join s in context.ppe_exam_statuses on pe.status_id equals s.id
                                    join p in context.ppe on pe.ppe_id equals p.id
                                    join a in context.areas on p.area_id equals a.id
                                    where pe.exam_date_id == exam_date_id && a.id == area_id
                                    select new PPEExam()
                                    {
                                        area_id = a.id,
                                        area_name = a.name,
                                        exam_date_id = pe.exam_date_id,
                                        ppe_code = p.ppe_code,
                                        ppe_id = p.id,
                                        ppe_name = p.name,
                                        status_id = pe.status_id != null ? (int)pe.status_id : 1,
                                        status_name = s.name,
                                        class_num = p.@class
                                    }).ToList();
                    }
                }
                return ppe_exam;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "mouo")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ppe")]
        public void SetIsUploadPPEExam(int ppe_exam_id)
        {
            TryFunc<bool>(() =>
            {
                bool res = false;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    ppe_exams p = new ppe_exams() { id = ppe_exam_id };
                    context.ppe_exams.Attach(p);
                    p.is_upload_data = true;
                    context.SaveChanges();
                    res = true;
                }
                return res;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "rcoi")]
        [PrincipalPermission(SecurityAction.Demand, Role = "mouo")]
        [PrincipalPermission(SecurityAction.Demand, Role = "scan")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ppe")]
        public void ChangeStatusPPEExam(int ppe_exam_id, int status_id)
        {
            TryFunc<bool>(() =>
            {
                bool res = false;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    ppe_exams p = new ppe_exams() { id = ppe_exam_id };
                    context.ppe_exams.Attach(p);
                    p.status_id = status_id;
                    context.SaveChanges();
                    res = true;
                }
                return res;
            });
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "rcoi")]
        [PrincipalPermission(SecurityAction.Demand, Role = "mouo")]
        [PrincipalPermission(SecurityAction.Demand, Role = "scan")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ppe")]
        public void SendPPEMessage(PPEExamMessage mes)
        {
            TryFunc<bool>(() =>
            {
                bool res = false;
                using (EGESupportEntities context = new EGESupportEntities())
                {
                    ppe_messages m = new ppe_messages() 
                    { 
                        user_id = mes.user_id,
                        ppe_exam_id = mes.ppe_exam_id,
                        mes_date = DateTime.Now,
                        message = mes.message
                    };
                    context.ppe_messages.Add(m);
                    context.SaveChanges();
                    res = true;
                }
                return res;
            });
        }

    }
}
