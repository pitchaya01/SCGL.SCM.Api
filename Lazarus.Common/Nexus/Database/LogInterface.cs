using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Lazarus.Common.Nexus.Database
{
 
    public class LogInterface 
    {
        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        [Column(Order = 2)]
        [StringLength(40)]
        public string InterfaceId { get; set; }

        [Column(Order = 3)]
        [StringLength(20)]
        public string TargetSystem { get; set; }

        [Column(Order = 4)]
        public DateTime? InboundTime { get; set; }

        [Column(Order = 5)]
        public DateTime? OutboundTime { get; set; }

        [Column(Order = 6, TypeName = "nvarchar(max)")]
        public string InboundMessage { get; set; }

        [Column(Order = 7, TypeName = "nvarchar(max)")]
        public string OutboundMessage { get; set; }

        [Column(Order = 8)]
        [StringLength(50)]
        public string InterfaceStatus { get; set; }

        [StringLength(40)]
        public string Key { get; set; }
    }
}
