//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class subjects
    {
        public subjects()
        {
            this.exam_dates = new HashSet<exam_dates>();
            this.subject_blanks = new HashSet<subject_blanks>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public int test_type_code { get; set; }
        public string folder_name { get; set; }
    
        public virtual ICollection<exam_dates> exam_dates { get; set; }
        public virtual ICollection<subject_blanks> subject_blanks { get; set; }
    }
}