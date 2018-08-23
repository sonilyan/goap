using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Goap {
	public static class goap_planer {
		static bool InState(string[] precondition, HashSet<string> states) {
			if (precondition == null)
				return true;
			foreach (var s in precondition) {
				if (!states.Contains(s))
					return false;
			}

			return true;
		}

		public static void planer(goap_agent agent, ref Queue<goap_action> actions) {
			List<goap_action> availableActions = agent.all_actions.Where(x => x.isActive()).ToList();
			var aws = getAction(agent.effects, availableActions, agent.goal);
			while (aws != null) {
				actions.Enqueue(aws.action);
				aws = aws.next;
			}
		}

		static actionWapper getAction(HashSet<string> effects, List<goap_action> availableActions, string goal) {
			actionWapper retval = null;
			foreach (var action in availableActions) {
				if (InState(action.precondition, effects)) {
					actionWapper tmp = null;
					if (action.effects.Contains(goal)) {
						tmp = new actionWapper(action);
						tmp.cost_count = action.cost();
					}
					else {
						HashSet<string> new_effects = new HashSet<string>(action.effects);
						new_effects.UnionWith(effects);
						tmp = getAction(new_effects, availableActions, goal);
						if (tmp != null) {
							tmp = new actionWapper(action) {
								cost_count = action.cost() + tmp.cost_count,
								next = tmp,
							};
						}
					}

					if (tmp != null && (retval == null || retval.cost_count > tmp.cost_count)) {
						retval = tmp;
					}
				}
			}

			return retval;
		}
	}

	public class actionWapper {
		public goap_action action;
		public int cost_count = 0;

		public actionWapper(goap_action action) {
			this.action = action;
		}

		public actionWapper next = null;
	}
}
