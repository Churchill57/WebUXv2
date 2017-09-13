using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUXv2.Models
{
    public class SecondLineDefenceQuestion
    {
        public int Id { get; set; }

        [Display(Name="Asked")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateAsked { get; set; }

        [Display(Name= "Benefit Option")]
        public string BenefitOption { get; set; }

        [Display(Name = "Contact Method")]
        public string ContactMethod { get; set; }

        [Display(Name = "Pensions Guidance")]
        public bool PensionsGuidance { get; set; }

        [Display(Name = "Regulated Advice")]
        public bool RegulatedAdvice { get; set; }

        [Display(Name = "Risk Warning")]
        public bool RiskWarning { get; set; }

        [Display(Name = "Script Name")]
        public string ScriptName { get; set; }

        [NotMapped]
        public string Description
        {
            get
            {
                var result = $"{DateAsked.ToShortDateString()} -";
                result += " PensGuide(" + (PensionsGuidance ? "y" : "n") + ")";
                result += " RegAdvice(" + (RegulatedAdvice ? "y" : "n") + ")";
                result += " RiskWarn(" + (RiskWarning ? "y" : "n") + ")";
                return result;
            }
        }

        public virtual int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

    }

    public class Customer
    {
        public int Id { get; set; }
        public string Title { get; set; }

        //[Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        //[Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        //[Required]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DOB { get; set; }

        [Display(Name = "NI Number")]
        public string NINO { get; set; }

        [NotMapped]
        public string Cargo { get; set; }

        public virtual ICollection<PropertyAddress> Addresses { get; set; }
        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }
        public virtual ICollection<EmailAddress> EmailAddresses { get; set; }
        public virtual ICollection<InterPersonRelationship> InterPersonRelationships { get; set; }

        public virtual ICollection<SecondLineDefenceQuestion> SecondLineDefenceQuestions { get; set; }
        public virtual ICollection<PAPolicy> PAPolicies { get; set; }
    }

    public class CustomerSearchCriteria
    {
        //public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? DOB { get; set; }

    }
    public class CustomerAdvancedSearchCriteria
    {
        //public int Id { get; set; }
        [Display(Name="First Name")]
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Age { get; set; }

        [Display(Name="NI Number")]
        public string NINO { get; set; }
        public string Town { get; set; }
        public string PostCode { get; set; }

    }
    public class PropertyAddress
    {
        public int Id { get; set; }

        //[Required]
        [Display(Name = "Address line 1")]
        public string Line1 { get; set; }

        [Display(Name = "Address line 2")]
        public string Line2 { get; set; }

        [Display(Name = "Address line 3")]
        public string Line3 { get; set; }

        //[Required]
        [Display(Name = "Postcode")]
        public string PostCode { get; set; }

        //[Required]
        public virtual int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public string CommasSeparated => $"{Line1},{Line2},{Line3},{PostCode}";
    }

    public class PhoneNumber
    {
        public int Id { get; set; }
        public string Number { get; set; }
    }

    public class EmailAddress
    {
        public int Id { get; set; }
        public string Email { get; set; }
    }

    public class InterPersonRelationship
    {
        public int Id { get; set; }
        public int Id1 { get; set; }
        public int Id2 { get; set; }
        public string RelationshipName { get; set; }
    }

}