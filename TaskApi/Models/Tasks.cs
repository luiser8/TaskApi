//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TaskApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tasks
    {
        public int IdTask { get; set; }
        public int IdUser { get; set; }
        public string Name { get; set; }
        public string Priority { get; set; }
        public string Description { get; set; }
        public System.DateTime CreateTask { get; set; }
        public Nullable<byte> Status { get; set; }
    
        public virtual Users Users { get; set; }
    }
}
