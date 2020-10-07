using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTestsCore.Data
{
	[Table("EventLog")]
    public partial class EventLog
    {
        [Key]
        [Column("EventID")]
        public Guid EventId { get; set; }
        [Required]
        [StringLength(10)]
        public string EventType { get; set; }
        [Required]
        [StringLength(50)]
        public string EventData { get; set; }
    }
}
