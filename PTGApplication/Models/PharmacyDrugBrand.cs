//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PTGApplication.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PharmacyDrugBrand
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PharmacyDrugBrand()
        {
            this.Items = new HashSet<Item>();
            this.PharmacyBatches = new HashSet<PharmacyBatch>();
        }
    
        public int Id { get; set; }
        public int Barcode { get; set; }
        public string Name { get; set; }
        public int License { get; set; }
        public string Dose { get; set; }
        public string PackSize { get; set; }
        public int GenericNameId { get; set; }
        public int ManufacturerId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Item> Items { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PharmacyBatch> PharmacyBatches { get; set; }
        public virtual PharmacyDrugGeneric PharmacyDrugGeneric { get; set; }
        public virtual PharmacyManufacturingCompany PharmacyManufacturingCompany { get; set; }
    }
}