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
    
    public partial class au_roles
    {
        public au_roles()
        {
            this.au_users = new HashSet<au_users>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    
        public virtual ICollection<au_users> au_users { get; set; }
    }
}
