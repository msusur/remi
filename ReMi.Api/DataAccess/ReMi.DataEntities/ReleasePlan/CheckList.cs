using System;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataEntities.ReleasePlan
{
    public class CheckList
    {
        #region scalar props

        public int CheckListId { get; set; }

        public Guid ExternalId { get; set; }

        public string Comment { get; set; }

        public int CheckListQuestionId { get; set; }

        public int ReleaseWindowId { get; set; }

        public string LastChangedBy { get; set; }

        #endregion


        #region navigational props

        public virtual CheckListQuestion CheckListQuestion { get; set; }

        public virtual bool Checked { get; set; }

        public virtual ReleaseWindow ReleaseWindow { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[CheckListId = {0}, ExternalId = {1}, Comment = {2}, CheckListQuestionId = {3}, ReleaseWindowId = {4} ]",
                CheckListId, ExternalId, Comment, CheckListQuestionId, ReleaseWindowId);
        }
    }
}
