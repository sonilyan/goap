using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goap
{
	public class GoapFsm
	{
		public GoapFsm(GoapAgent agent)
		{
			this.agent = agent;
		}

		private readonly GoapAgent agent;

		Stack<Func<GoapFsm, GoapAgent, IEnumerator>> _stackAction = new Stack<Func<GoapFsm, GoapAgent, IEnumerator>>();

		public IEnumerator Update()
		{
			if (_stackAction.Count > 0)
			{
				yield return _stackAction.Peek().Invoke(this, agent);
			}
		}

		public Func<GoapFsm, GoapAgent, IEnumerator> Pop()
		{
			return _stackAction.Pop();
		}
		
		public Func<GoapFsm, GoapAgent, IEnumerator> Peek()
		{
			if (_stackAction.Count == 0)
				return null;
			return _stackAction.Peek();
		}

		public void Push(Func<GoapFsm, GoapAgent, IEnumerator> action)
		{
			_stackAction.Push(action);
		}

		public void Clear()
		{
			_stackAction.Clear();
		}
	}
}
