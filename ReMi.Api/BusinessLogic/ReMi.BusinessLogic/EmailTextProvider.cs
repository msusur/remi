using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ReMi.BusinessEntities.Exceptions;

namespace ReMi.BusinessLogic
{
    public class EmailTextProvider : IEmailTextProvider
    {
        private static IDictionary<string, string> _emailTemplates;
        private static readonly object Locker = new object();

        public IDictionary<string, string> EmailTemplates
        {
            get
            {
                if (_emailTemplates == null)
                    lock (Locker)
                    {
                        if (_emailTemplates == null)
                        {
                            _emailTemplates = InitTemplates();
                        }
                    }

                return _emailTemplates;
            }
        }

        public string GetText(string key, IEnumerable<KeyValuePair<string, object>> placeholders)
        {
            var template = EmailTemplates[key];
            if (template == null)
                throw new EmailTemplateNotRegisteredException(key);

            return ReplacePlaceholders(template, placeholders);
        }

        private string ReplacePlaceholders(string text, IEnumerable<KeyValuePair<string, object>> pairs, bool isHtml = true)
        {
            var result = pairs
                .Aggregate(
                    text,
                    (current, pair) =>
                        Regex.Replace(
                            current,
                            "##" + pair.Key,
                            pair.Value.ToString(),
                            RegexOptions.IgnoreCase | RegexOptions.Multiline));

            if (isHtml)
            {
                result = Regex.Replace(result, @"
", "<br />",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }


            if (Regex.IsMatch(result, @"##[\w]+"))
            {
                var placeholders = Regex.Matches(result, @"##[\w]+").Cast<Match>().Select(o => o.Value.Replace("#", ""));
                throw new EmailTemplateHasUnregisteredPlaceholdersException(placeholders);
            }

            return result;
        }

        private IDictionary<string, string> InitTemplates()
        {
            //TODO: need to get email templates from resource file

            return new Dictionary<string, string>
            {
                {"TaskWasAssignedEmail", TaskWasAssignedEmail},
                {"TaskWasCreatedEmail", TaskWasCreatedEmail},
                {"TaskWasCreatedSupportMembersEmail", TaskWasCreatedSupportMembersEmail},
                {"TaskWasUpdatedEmail", TaskWasUpdatedEmail},
                {"TaskWasUpdatedSupportMembersEmail", TaskWasUpdatedSupportMembersEmail},
                {"ReleaseWindowWasUpdatedEmailForParticipant", ReleaseWindowWasUpdatedEmailForParticipant},
                {"ReleaseWindowWasUpdatedEmailForAuthor", ReleaseWindowWasUpdatedEmailForAuthor},
                {"ReleaseWindowFullyApprovedEmail", ReleaseWindowFullyApprovedEmail},
                {"ReleaseWindowBookedEmail", ReleaseWindowBookedEmail},
                {"ReleaseWindowCancelled", ReleaseWindowCancelledEmail},
                {"ApproverAddedToReleaseWindowEmail", ApproverAddedToReleaseWindowEmail},
                {"ReleaseWindowUpdatedEmail", ReleaseWindowUpdatedEmail},
                {"ApproverRemovedFromReleaseWindowEmail", ApproverRemovedFromReleaseWindowEmail},
                {"ReleaseClosedEmail", ReleaseClosedEmail},
                {"ReviewerUpdatedForReleaseTaskEmail", ReviewerUpdatedForReleaseTaskEmail},
                {"ImplementorUpdatedForReleaseTaskEmail", ImplementorUpdatedForReleaseTaskEmail},
                {"CheckListQuestionRemovedEmail", CheckListQuestionRemovedEmail},
                {"CheckListQuestionAddedEmail", CheckListQuestionAddedEmail},
                {"CheckListWasNotAnsweredEmail", CheckListWasNotAnsweredEmail},
                {"CheckListWasAnsweredEmail", CheckListWasAnsweredEmail},
                {"CheckListCommentWasUpdatedEmail", CheckListCommentWasUpdatedEmail},
                {"ParticipantAddedToReleaseWindowEmail", ParticipantAddedToReleaseWindowEmail},
                {"ParticipantRemovedFromReleaseWindowEmail", ParticipantRemovedFromReleaseWindowEmail},
                {"SignOffAddedToReleaseWindowEmail", SignOffAddedToReleaseWindowEmail},
                {"SignOffRemovedFromReleaseWindowEmail", SignOffRemovedFromReleaseWindowEmail},
                {"ReleaseWindowFullySignedOffEmail", ReleaseWindowFullySignedOffEmail},
                {"ProductRequestRegistrationUpdated", ProductRequestRegistrationUpdated},
                {"ProductRequestRegistrationCreated", ProductRequestRegistrationCreated},
                {"ApiUpdated", ApiUpdated},
                {"ReleaseWindowUpdatedParticipantEmail", ReleaseWindowUpdatedParticipantEmail},
                {"SiteDown", SiteDown},
                {"SiteUp", SiteUp}
            };
        }

        #region Email text definition

        private const string SiteDown = @"Hello ##Recipient,
Release ##Product ##Sprint has started, site is in maintenance mode.
<a href=""##ReleaseUrl"">Open release page</a>";

        private const string SiteUp = @"Hello ##Recipient,
Release ##Product ##Sprint site is now online again.
<a href=""##ReleaseUrl"">Open release page</a>";

        private const string TaskWasAssignedEmail = @"Hello ##Assignee,
You were assigned to a task in release ##Products ##Sprint which will take place at ##StartTime.
Type: ##Type
Risk: ##Risk
Ticket: ##HelpDeskUrl
Description:
##Description

<a href=""##ConfirmUrl"">Please confirm the receipt.</a>";

        private const string TaskWasCreatedEmail = @"Hello ##Creator,
New task was created in release ##Product ##Sprint which will take place at ##StartTime.
Type: ##Type
Risk: ##Risk
Assignee: ##Assignee
Ticket: ##HelpDeskUrl
Description:
##Description

<a href=""##ReleasePlanUrl"">Open release plan page</a>.";

        private const string TaskWasCreatedSupportMembersEmail = @"Hello ##Recipient,
New task was created in release ##Products ##Sprint which will take place at ##StartTime.
Type: ##Type
Risk: ##Risk
Assignee: ##Assignee
Ticket ##HelpDeskUrl
Description:
##Description

<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string TaskWasUpdatedEmail = @"Hello ##Assignee,
Task that assigned to you was updated in release ##Products ##Sprint which will take place at ##StartTime.
Type: ##Type
Risk: ##Risk
Ticket: ##HelpDeskUrl
Description:
##Description

<a href=""##ConfirmUrl"">Please confirm the receipt.</a>";

        private const string TaskWasUpdatedSupportMembersEmail = @"Hello ##Recipient,
Task was updated in release ##Products ##Sprint which will take place at ##StartTime.
Type: ##Type
Risk: ##Risk
Assignee: ##Assignee
Ticket: ##HelpDeskUrl
Description:
##Description

<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string ReleaseWindowWasUpdatedEmailForAuthor = @"Hello ##Creator,
Release ##Product ##Sprint was updated. It will take place at ##StartTime. 
Please, pay attention to it, you are the member of release support team.
<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string ReleaseWindowWasUpdatedEmailForParticipant = @"Hello ##Assignee,
Release ##Product ##Sprint was updated. It will take place at ##StartTime.
<a href=""##AcknowledgeUrl=##ReleaseWindowExternalId""/>Approve my participation</a>";

        private const string ReleaseWindowFullyApprovedEmail = @"Hello, 
Release ##Products ##Sprint has been fully approved for ##StartTime.

List of signatories:
##ListOfApprovers

<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string ApproverAddedToReleaseWindowEmail = @"Hello ##Assignee,
You have been added as approver to release ##Products ##Sprint which will take place at ##StartTime.
<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string ReleaseWindowCancelledEmail = @"Hello ##Assignee, 
##ReleaseType ##Products ##Sprint originally planned on ##StartTime was cancelled.
<a href=""##ReleaseCalendarUrl"">Open calendar page</a>";

        private const string ApproverRemovedFromReleaseWindowEmail = @"Hello ##Assignee,
You have been removed from approvals list of release ##Products ##Sprint which will take place at ##StartTime.
<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string ReleaseWindowBookedEmail = @"Hello ##Assignee,
##ReleaseType ##Products ##Sprint was booked. It will take place at ##StartTime.
<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string ReleaseWindowUpdatedEmail = @"Hello ##Assignee,
##ReleaseType ##Products ##Sprint was updated. It will take place at ##StartTime.
<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string ReleaseClosedEmail =
            @"Release ##Products ##Sprint was closed. <a href=""##ReleasePlanUrl"">Open release plan page.</a>
Release notes:<br/>##Notes<br/>Release issues:<br/>##Issues<br/><br/>##Tickets";

        private const string CheckListCommentWasUpdatedEmail = @"Comment for checklist question '##Question' for release ##Product ##Sprint was changed to ##Comment.
<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string CheckListWasAnsweredEmail = @"Checklist question '##Question' for release ##Product ##Sprint was answered.
<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string CheckListWasNotAnsweredEmail = @"Checklist question '##Question' for release ##Product ##Sprint was marked as not answered.
<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string CheckListQuestionAddedEmail = @"Checklist questions were added for release ##Product ##Sprint: 
<ul> ##Questions </ul>

<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string CheckListQuestionRemovedEmail = @"Checklist question '##Question' was removed for release ##Product ##Sprint.
<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string ReviewerUpdatedForReleaseTaskEmail = @"Hello ##Reviewer,
You have been assigned as reviewer for task in release ##Product ##Sprint which will take place at ##StartTime.
Description:
##Description

<a href=""##ConfirmUrl"">Please confirm the receipt.</a>";

        private const string ImplementorUpdatedForReleaseTaskEmail = @"Hello ##Implementor,
You have been assigned as implementor for task in release ##Product ##Sprint which will take place at ##StartTime.
Description:
##Description

<a href=""##ConfirmUrl"">Please confirm the receipt.</a>";

        private const string ParticipantAddedToReleaseWindowEmail = @"Hello, ##Participant. 
You were added to the support team for release ##Products ##Sprint which will take place at ##StartTime.
<a href=""##AcknowledgeUrl"">Approve my participation</a>.";

        private const string ParticipantRemovedFromReleaseWindowEmail = @"Hello, ##Participant, 
You were removed from the support team for release ##Products ##Sprint which will take place at ##StartTime.
<a href=""##ReleasePlanUrl"">Open release plan page</a>";


        private const string SignOffAddedToReleaseWindowEmail = @"Hello ##Assignee,
You have been added as sign off to release ##Products ##Sprint which will take place at ##StartTime.
<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string SignOffRemovedFromReleaseWindowEmail = @"Hello ##Assignee,
You have been removed from sign offs list of release ##Products ##Sprint which will take place at ##StartTime.
<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string ReleaseWindowFullySignedOffEmail = @"Hello, 
Release ##Products ##Sprint has been fully signed off for ##StartTime.

List of signatories:
##ListOfSignOffs

<a href=""##ReleasePlanUrl"">Open release plan page</a>";

        private const string ProductRequestRegistrationUpdated = @"Hello ##Assignee,
Product request was updated: ##Description

##Tasks

<a href=""##RegistrationUrl"">Open product registration page</a>";


        private const string ProductRequestRegistrationCreated = @"Hello ##Assignee,
New product request was created: ##Description

##Tasks

<a href=""##RegistrationUrl"">Open product registration page</a>";

        private const string ApiUpdated = @"Hello!
New version of ReMi API were released! It contains the following changes

##AddedMethods
##RemovedMethods

For more information please go to <a href=""##RemiApiPageUrl"">ReMI API page</a>";

        private const string ReleaseWindowUpdatedParticipantEmail = @"Hello, ##Assignee, 
Release ##Products ##Sprint was updated, it will take place at ##StartTime.<br />
<a href=""##ConfirmationUrl""/>Approve my participation</a>";

        #endregion
    }
}
