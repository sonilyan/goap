using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goap {
	public class goap_agent : MonoBehaviour {
		public string goal = "print";

		private goap_fsm fsm;

		private Action<goap_fsm, goap_agent> idle;
		private Action<goap_fsm, goap_agent> walk;
		private Action<goap_fsm, goap_agent> do_action;

		private Queue<goap_action> actions = new Queue<goap_action>();

		public HashSet<string> effects = new HashSet<string>();

		[HideInInspector] public goap_action[] all_actions;

		void Start() {
			all_actions = gameObject.GetComponents<goap_action>();

			fsm = new goap_fsm(this);

			idle = (fsm, agent) => {
				if (actions.Count != 0) {
					fsm.pop();

					var action = actions.Peek();
					if (action.inRange()) {
						action.enter();
						fsm.push(do_action);
					}
					else {
						fsm.push(do_action);
						fsm.push(walk);
					}
				}
				else {
					goap_planer.planer(agent, ref actions);
				}
			};

			walk = (fsm, agent) => {
				fsm.pop();
				actions.Peek().enter();
			};

			do_action = (fsm, agent) => {
				var action = actions.Peek();
				if (action.isDone()) {
					action.exit();
					fsm.pop();
					fsm.push(idle);
					actions.Dequeue();
				}
				else {
					action.update();
				}
			};

			fsm.push(idle);
		}

		void Update() {
			fsm.update();
		}
	}

}