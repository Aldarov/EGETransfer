﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EGEServiceRun.Model
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class EGESupportEntities : DbContext
    {
        public EGESupportEntities()
            : base("name=EGESupportEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<areas> areas { get; set; }
        public virtual DbSet<au_roles> au_roles { get; set; }
        public virtual DbSet<au_users> au_users { get; set; }
        public virtual DbSet<blank_types> blank_types { get; set; }
        public virtual DbSet<exam_dates> exam_dates { get; set; }
        public virtual DbSet<ppe> ppe { get; set; }
        public virtual DbSet<subject_blanks> subject_blanks { get; set; }
        public virtual DbSet<subjects> subjects { get; set; }
        public virtual DbSet<ppe_exam_statuses> ppe_exam_statuses { get; set; }
        public virtual DbSet<ppe_exams> ppe_exams { get; set; }
        public virtual DbSet<journals> journals { get; set; }
        public virtual DbSet<ppe_messages> ppe_messages { get; set; }
    
        [DbFunction("EGESupportEntities", "get_list_journals")]
        public virtual IQueryable<get_list_journals_Result> get_list_journals()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<get_list_journals_Result>("[EGESupportEntities].[get_list_journals]()");
        }
    
        [DbFunction("EGESupportEntities", "get_journal_content")]
        public virtual IQueryable<get_journal_content_Result> get_journal_content()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<get_journal_content_Result>("[EGESupportEntities].[get_journal_content]()");
        }
    
        [DbFunction("EGESupportEntities", "get_ppe_files_path")]
        public virtual IQueryable<string> get_ppe_files_path(Nullable<int> exam_date_id, Nullable<int> ppe_id)
        {
            var exam_date_idParameter = exam_date_id.HasValue ?
                new ObjectParameter("exam_date_id", exam_date_id) :
                new ObjectParameter("exam_date_id", typeof(int));
    
            var ppe_idParameter = ppe_id.HasValue ?
                new ObjectParameter("ppe_id", ppe_id) :
                new ObjectParameter("ppe_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<string>("[EGESupportEntities].[get_ppe_files_path](@exam_date_id, @ppe_id)", exam_date_idParameter, ppe_idParameter);
        }
    }
}
