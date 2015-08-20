using System;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using ReMi.Common.Utils;

namespace ReMi.Plugin.Email.Models
{
    public class IcsConstructor
    {
        public void Build(OutlookEventEntity outlookEvent, MailMessage message, String emailBody, int releaseDuration, string organizer)
        {
            var builder = new StringBuilder();
            builder.AppendLine("BEGIN:VCALENDAR");
            builder.AppendLine("PRODID:~//Schedule a meeting");
            builder.AppendLine("VERSION:2.O");
            builder.AppendLine(String.Format("METHOD:{0}", outlookEvent.OutlookEmailMethod));
            builder.AppendLine("BEGIN:VEVENT");
            builder.AppendLine(String.Format("DTSTART:{0:yyyyMMddTHHmmssZ}", outlookEvent.StartTime.ToUniversalTime()));
            builder.AppendLine(String.Format("DTSTAMP:{0:yyyyMMddTHHmmssZ}", SystemTime.Now));
            builder.AppendLine(String.Format("DTEND:{0:yyyyMMddTHHmmssZ}", outlookEvent.StartTime.AddMinutes(releaseDuration).ToUniversalTime()));
            builder.AppendLine(String.Format("LOCATION:{0}", outlookEvent.Location));
            builder.AppendLine(String.Format("UID:{0}", outlookEvent.AppointmentId));
            builder.AppendLine(String.Format("DESCRIPTION:{0}", emailBody));
            builder.AppendLine(String.Format(
                "X-ALT-DESC;FMTTYPE=text/html:{0}", emailBody));
            builder.AppendLine(String.Format("SUMMARY:{0}", message.Subject));
            builder.AppendLine(String.Format("ORGANIZER:MAILTO:{0}", organizer));
            builder.AppendLine(String.Format("ATTENDEE;CN=\"{0}\";RSVP=TRUE:mailto:{1}", message.To[0].DisplayName,
                message.To[0].Address));
            builder.AppendLine("BEGIN:VALARM");
            builder.AppendLine("TRIGGER:-PT15M");
            builder.AppendLine("ACTION:DISPLAY");
            builder.AppendLine("DESCRIPTION:Reminder");
            builder.AppendLine("END:VALARM");
            builder.AppendLine("END:VEVENT");
            builder.AppendLine("END:VCALENDAR");

            var contentType = new ContentType("text/calendar");
            contentType.Parameters.Add("method", "REQUEST");
            contentType.Parameters.Add("name", "Meeting.ics");
            var alternateView = AlternateView.CreateAlternateViewFromString(builder.ToString(), contentType);

            message.AlternateViews.Add(alternateView);
        }
    }
}
