using System;
using System.Collections.Generic;

namespace Goap
{
	public abstract class GoapGoal
	{
		public abstract GoapCondition Goal { get; protected set; }

		public abstract bool Check();

		public  GoapAction[] GoapActions;

		public abstract event Action OnStateChange;

		public bool IgnorePlaner { get; protected set; } = false;
	}

	public interface ICondition
	{
		bool CheckCondition();
	}
}
