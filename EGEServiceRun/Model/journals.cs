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
    
    public partial class journals
    {
        public int id { get; set; }
        public int ppe_exam_id { get; set; }
        public int blank_type_id { get; set; }
        public int auditorium { get; set; }
        public int count_blanks { get; set; }
        public int count_add_blanks { get; set; }
    
        public virtual blank_types blank_types { get; set; }
        public virtual ppe_exams ppe_exams { get; set; }
    }
}
