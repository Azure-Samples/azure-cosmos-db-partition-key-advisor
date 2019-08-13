using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PartitionKeyAdvisor.Models
{
    public class FormModel
    {
        [Required(ErrorMessage = "Connection string is required")]

        public string ConnectionString { get; set; }

        [Required(ErrorMessage = "Read Only Key is required")]
        public string ReadOnlyKey { get; set; }

        [Required(ErrorMessage = "Database name is required")]
        public string Database { get; set; }

        [Required(ErrorMessage = "Database name is required")]
        public string Collection { get; set; }

        [Required(ErrorMessage = "Filter is required")]
        public string Filter1 { get; set; }

        [Required(ErrorMessage = "Filter is required")]
        public string Filter2 { get; set; }

       [Required(ErrorMessage = "Filter is required")]
        public string Filter3 { get; set; }


    }
}
