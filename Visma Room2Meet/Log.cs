//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Visma_Room2Meet
{
    using System;
    using System.Collections.Generic;
    
    public partial class Log
    {
        public int LogID { get; set; }
        public int LogTypeID { get; set; }
        public System.DateTime DateTime { get; set; }
        public string Location { get; set; }
        public string Message { get; set; }
        public string InnerException { get; set; }
        public string Params { get; set; }
    
        public virtual LogType LogType { get; set; }
    }
}
