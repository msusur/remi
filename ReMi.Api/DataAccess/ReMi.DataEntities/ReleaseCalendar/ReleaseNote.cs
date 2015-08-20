using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.ReleaseCalendar
{
    [Table("ReleaseNotes", Schema = Constants.SchemaName)]
    public class ReleaseNote
    {
        [Key]
        public int ReleaseNoteId { get; set; }

        public String Issues { get; set; }

        public virtual ReleaseWindow ReleaseWindow { get; set; }

        public string ReleaseNotes { get; set; }

        public override string ToString()
        {
            return
                string.Format(
                    "[ReleaseNoteId={0}, ReleaseNotes={1}, Issues={2}]",
                    ReleaseNoteId, ReleaseNotes, Issues);
        }
    }
}
