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
    
    public partial class subject_blanks
    {
        public int id { get; set; }
        public int subject_id { get; set; }
        public int blank_type_id { get; set; }
    
        public virtual blank_types blank_types { get; set; }
        public virtual subjects subjects { get; set; }
    }
}
