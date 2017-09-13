using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebUXv2.Models
{
    public class PAPolicy
    {
        public int Id { get; set; }
        public decimal Premium { get; set; }

        public virtual int AnnuitantId { get; set; }

        [ForeignKey("AnnuitantId")]
        public virtual Customer Annuitant { get; set; }

        public virtual int? DependantId { get; set; }
        [ForeignKey("DependantId")]
        public virtual Customer Dependant { get; set; }

        public virtual int? BeneficiaryId { get; set; }
        [ForeignKey("BeneficiaryId")]
        public virtual Customer Beneficiary { get; set; }

        [NotMapped]
        public string Cargo { get; set; }

        [NotMapped]
        public string Description
        {
            get
            {
                return $"{Id} - PA Policy {Premium.ToString("C")}";
            }
        }

    }

    public class PAPolicySearchCriteria
    {
        [Required]
        [Display(Name = "Policy Id")]
        public int? Id { get; set; }
    }

    public class PAPolicyAdvancedSearchCriteria
    {
        //public int Id { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Age { get; set; }

        [Display(Name = "NI Number")]
        public string NINO { get; set; }
        public string Town { get; set; }
        public string PostCode { get; set; }

    }


}