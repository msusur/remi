using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.BusinessEntities.ReleaseCalendar
{
	public class ReleaseWindowList : List<ReleaseWindow>
	{
		public ReleaseWindowList()
		{
		}

		public ReleaseWindowList(IEnumerable <ReleaseWindow> collection) : base(collection)
		{
		}

		public override string ToString()
		{
			return this.FormatElements<ReleaseWindow>();
		}
	}
}
