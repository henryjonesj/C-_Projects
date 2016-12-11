using System;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Collections;

namespace BallyTech.QCom.Model
{
	[CompactFormatter.Attributes.Serializable(Custom = true)]
	public partial class LinkObserverCollection : ObserverCollection<ILinkObserver>
	{
    }
}
