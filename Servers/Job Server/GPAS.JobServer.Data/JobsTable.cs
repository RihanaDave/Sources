using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.JobServer.Data.Models
{
    [Table("JobsTable")]
    public class JobsTable
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string uniqeID { get; set; }
        [Required]
        public byte[] request { get; set; }
        [Required]
        public string type { get; set; }
        [Required]
        public string state { get; set; }
        [Required]
        public string priority { get; set; }
        [Required]
        public string registerDate { get; set; }
        [Required]
        public int timeOutDuration { get; set; }
        public string startDate { get; set; }
        public string finishDate { get; set; }
        public string processID { get; set; }
        public string processName { get; set; }
        public string message { get; set; }
        public string lastPublishedObjectIndex { get; set; }
        public string lastPublishedRelationIndex { get; set; }

    }
}
