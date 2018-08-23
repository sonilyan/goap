using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goap {
	public class goap_fsm {
		public goap_fsm(goap_agent agent) {
			this.agent = agent;
		}

		private goap_agent agent;

		Stack<Action<goap_fsm, goap_agent>> stackAction = new Stack<Action<goap_fsm, goap_agent>>();

		public void update() {
			if (stackAction.Peek() != null) {
				stackAction.Peek().Invoke(this, agent);
			}
		}

		public Action<goap_fsm, goap_agent> pop() {
			return stackAction.Pop();
		}

		public void push(Action<goap_fsm, goap_agent> action) {
			stackAction.Push(action);
		}
	}

}

