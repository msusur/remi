using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace ReMi.Common.Utils.Enums
{
    [DataContract]
    public abstract class EnumDescription
    {
        [Key, DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; set; }

        [Required, DataMember]
        [StringLength(50)]
        [Index(IsUnique = true)]
        public virtual string Name { get; set; }

        [Required, DataMember]
        [StringLength(256)]
        public virtual string Description { get; set; }

        [NotMapped]
        public virtual string Annotation { get; set; }

        public override string ToString()
        {
            return string.Format("[Id={0}, Name={1}, Description={2}]", Id, Name, Description);
        }
    }
}
