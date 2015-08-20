using System.Collections.Generic;
using ReMi.Plugin.ZenDesk.Models;

namespace ReMi.Plugin.ZenDesk
{
    public interface IZenDeskRequest
    {
        User Me();
        List<Ticket> Tickets();
        Ticket Ticket(int id);
        Ticket CreateTicket(Ticket ticket);
        Ticket UpdateTicket(Ticket ticket);
        void DeleteTicket(string ticketRef);
        void MarkTicketAsResolved(string ticketRef);

        Attachment AddAttachment(int ticketId, string fileName, byte[] data, string comment = null);
        byte[] GetAttachment(int attachmentId);

        List<Group> Groups();
        User User(int id);
        List<User> Users();
        List<Ticket> NotSolvedTickets();
        List<User> UsersInGroup(int groupId);
        List<User> Users(params int[] userIds);
    }
}
