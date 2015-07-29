using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Enexure.Fire.Data
{
	public abstract class DataResultBase
	{
		

		
	}

	internal class CouldNotSetPropertyException : Exception
	{
		public CouldNotSetPropertyException(string key, Exception exception)
			: base(string.Format("Could not set property {0}", key), exception)
		{
			
		}
	}
}