using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroAlerts
{
    [Table("Alerts")]
    public class AlertEntity
    {
        [Column("ID")]
        [Key]        
        [Required]
        public string ID { get; set; }

        [Column("When")]
        public DateTime When { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Description")]
        public string Decription { get; set; }
    }
}
