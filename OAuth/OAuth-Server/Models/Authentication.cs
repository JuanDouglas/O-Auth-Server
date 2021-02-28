//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OAuth.Server.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Authentication
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Authentication()
        {
            this.Authorization = new HashSet<Authorization>();
        }
    
        public int ID { get; set; }
        public string User_Agent { get; set; }
        public string IPAdress { get; set; }
        public string Token { get; set; }
        public int LoginFirstStep { get; set; }
        public System.DateTime Date { get; set; }
    
        public virtual LoginFirstStep LoginFirstStep1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Authorization> Authorization { get; set; }
    }
}
