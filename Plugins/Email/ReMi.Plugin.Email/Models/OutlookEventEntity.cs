using System;

namespace ReMi.Plugin.Email.Models
{
    public class OutlookEventEntity
    {
        public OutlookEmailMethod OutlookEmailMethod { get; set; }
        public DateTime StartTime { get; set; }
        public string AppointmentId { get; set; }
        public string Location { get; set; }
        public string Organizer { get; set; }

        public override string ToString()
        {
            return string.Format("[OutlookEmailMethod={0}, StartTime={1}, AppointmentId={2}, Location={3}, Organizer={4}]",
                OutlookEmailMethod, StartTime, AppointmentId, Location, Organizer);
        }
    }
}
