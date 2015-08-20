using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using ReMi.Plugin.Common;
using ReMi.Plugin.ZenDesk.DataAccess.Gateways;
using ReMi.Plugin.ZenDesk.Models;
using RestSharp.Extensions;

namespace ReMi.Plugin.ZenDesk.BusinessLogic
{
    /// <summary>
    /// More info: http://developer.zendesk.com/documentation/rest_api/introduction.html
    /// </summary>
    public class ZenDeskRequest : RestApiRequest, IZenDeskRequest
    {
        public Func<IGlobalConfigurationGateway> GlobalConfigurationGatewayFactory { get; set; }

        public override string BaseUrl
        {
            get { return Configuration.ZenDeskUrl; }
        }

        protected override string UserName { get { return Configuration.ZenDeskUser; } set { } }
        protected override string Password { get { return Configuration.ZenDeskPassword; } set { } }

        private PluginConfigurationEntity Configuration
        {
            get
            {
                using (var gateway = GlobalConfigurationGatewayFactory())
                {
                    return gateway.GetGlobalConfiguration();
                }
            }
        }

        public User Me()
        {
            return GetData<UserWrapper>("users/me.json").User;
        }

        public List<Ticket> Tickets()
        {
            var response = GetData<TicketsWrapper>("tickets.json");

            return response.Tickets;
        }

        /// <summary>
        /// More info: http://developer.zendesk.com/documentation/rest_api/search.html , https://support.zendesk.com/entries/20239737
        /// </summary>
        /// <param name="seacrhCondition"></param>
        /// <returns></returns>
        private List<Ticket> SearchTickets(string seacrhCondition)
        {
            var response = GetData<SearchResultsWrapper>("search.json", new Dictionary<string, string> { { "query", seacrhCondition } });

            return response.Results;
        }

        public List<Ticket> NotSolvedTickets()
        {
            return SearchTickets("status<solved ticket_type:ticket");
        }

        public Ticket Ticket(int ticketId)
        {
            if (ticketId <= 0)
                throw new ArgumentException("Invaid ticket number");

            var response = GetData<TicketWrapper>(string.Format("tickets/{0}.json", ticketId));

            return response != null ? response.Ticket : null;
        }

        public Ticket CreateTicket(Ticket ticket)
        {
            var response = Post<TicketWrapper>("tickets.json", new TicketWrapper { Ticket = ticket });

            if (response.StatusCode != HttpStatusCode.Created || response.Data == null ||
                response.Data.Ticket == null) return null;

            return response.Data.Ticket;
        }

        public Ticket UpdateTicket(Ticket ticket)
        {
            var response = Put<TicketWrapper>(string.Format("tickets/{0}.json", ticket.Id),
                new TicketWrapper { Ticket = ticket });

            if (response.StatusCode != HttpStatusCode.OK || response.Data == null || response.Data.Ticket == null)
                return null;

            return response.Data.Ticket;
        }

        public void DeleteTicket(string ticketRef)
        {
            var response = Delete<TicketWrapper>(string.Format("tickets/{0}.json", ticketRef));

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return;
                case HttpStatusCode.Forbidden:
                    throw new ZenDeskAccessDeniedException("DeleteTicket");
                default:
                    throw new ZenDeskTicketNotFoundException(ticketRef);
            }
        }

        public void MarkTicketAsResolved(string ticketRef)
        {
            int ticketId;
            if (!int.TryParse(ticketRef, out ticketId))
                throw new ArgumentException("TicketRef is not a number", "ticketRef");
            var ticket = new Ticket
            {
                Id = ticketId,
                Status = Statuses.solved,
                Comment = new TicketComment { Body = "TicketResolved", Public = true },
                Assignee_Id = 683585607
            };
            var updatedTicket = UpdateTicket(ticket);
            if (updatedTicket == null)
            {
                throw new ZenDeskTicketNotFoundException(ticketRef);
            }
            if (updatedTicket.Status != Statuses.solved)
            {
                throw new ZenDeskFailedToUpdateTicketStatusException(ticketId);
            }
        }

        public Attachment AddAttachment(int ticketId, string fileName, byte[] data, string comment = null)
        {
            var text = comment ?? string.Format("Attachment: {0}", fileName);
            var uploadResponse = UploadFile<UploadWrapper>(
                string.Format("uploads.json?filename={0}", fileName), null,
                new Dictionary<string, byte[]> { { fileName, data } });

            if (uploadResponse.StatusCode != HttpStatusCode.Created || uploadResponse.Data == null ||
                uploadResponse.Data.Upload == null || string.IsNullOrEmpty(uploadResponse.Data.Upload.Token) ||
                !uploadResponse.Data.Upload.Attachments.Any())
            {
                return null;
            }

            var uploadToken = uploadResponse.Data.Upload.Token;
            var ticket = new Ticket
            {
                Id = ticketId,
                Comment = new TicketComment
                {
                    Body = text,
                    Uploads = new[] { uploadToken }
                }
            };

            return UpdateTicket(ticket) != null ? uploadResponse.Data.Upload.Attachments.First() : null;
        }

        public byte[] GetAttachment(int attachmentId)
        {
            if (attachmentId <= 0)
                throw new ArgumentException("Invaid attachment id");

            var response = GetData<AttachmentWrapper>(string.Format("attachments/{0}.json", attachmentId));

            if (response == null || response.Attachment == null
                || string.IsNullOrEmpty(response.Attachment.Content_Url))
                return null;

            var attachmentUrl = response.Attachment.Content_Url;

            var request = WebRequest.Create(attachmentUrl);
            request.Credentials = new NetworkCredential(UserName, Password);
            using (var webResponse = (HttpWebResponse)request.GetResponse())
            {
                return webResponse.StatusCode == HttpStatusCode.OK ? webResponse.GetResponseStream().ReadAsBytes() : null;
            }
        }

        public List<Group> Groups()
        {
            var response = GetData<GroupsWrapper>("groups.json");
            return response != null ? response.Groups : null;
        }

        public User User(int id)
        {
            return Users(id).FirstOrDefault();
        }

        public List<User> Users(params int[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
                throw new ArgumentException("Invalid list of user ids");

            var ids = string.Join(",", userIds.Select(o => o.ToString(CultureInfo.InvariantCulture)));

            var response = GetData<UsersWrapper>("users/show_many.json?ids=" + ids);
            return response.Users;
        }

        public List<User> Users()
        {
            var response = GetData<UsersWrapper>("users.json");

            return response.Users;
        }

        public List<User> UsersInGroup(int groupId)
        {
            var response = GetData<UsersWrapper>(string.Format("groups/{0}/users.json", groupId));

            return response.Users;
        }
    }
}
