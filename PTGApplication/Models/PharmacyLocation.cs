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
    
    public partial class PharmacyLocation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PharmacyLocation()
        {
            this.PharmacyBatchLocations = new HashSet<PharmacyBatchLocation>();
            this.PharmacyInventories = new HashSet<PharmacyInventory>();
            this.PharmacyInventories1 = new HashSet<PharmacyInventory>();
            this.PharmacyLocationTypes = new HashSet<PharmacyLocationType>();
            this.PharmacyLocationTypes1 = new HashSet<PharmacyLocationType>();
        }
    
        public int Id { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PharmacyBatchLocation> PharmacyBatchLocations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PharmacyInventory> PharmacyInventories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PharmacyInventory> PharmacyInventories1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PharmacyLocationType> PharmacyLocationTypes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PharmacyLocationType> PharmacyLocationTypes1 { get; set; }
    }
}
