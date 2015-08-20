using System;

namespace ReMi.Contracts.Plugins.Data.Email
{
    public class CalendarEvent
    {
        public CalendarEventType CalendarEventType { get; set; }
        public DateTime StartTime { get; set; }
        public string AppointmentId { get; set; }
        public string Location { get; set; }
        public string Organizer { get; set; }

        public override string ToString()
        {
            return string.Format("[CalendarEventType={0}, StartTime={1}, AppointmentId={2}, Location={3}, Organizer={4}]",
                CalendarEventType, StartTime, AppointmentId, Location, Organizer);
        }
    }
}
