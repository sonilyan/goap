using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goap {
	public class goap_action_print : goap_action {
		private int count;

		public readonly string[] _e = new[] {"print"};

		public override string[] precondition {
			get { return null; }
		}

		public override string[] effects {
			get { return _e; }
		}

		public override void enter() {
			count = 0;
		}

		public override void exit() {
			Debug.Log("goap_action_print exit");
		}

		public override void update() {
			Debug.Log("goap_action_print update " + count);
			count++;
		}

		public override bool isDone() {
			return count > 5;
		}

		public override bool inRange() {
			return true;
		}

		public override bool isActive() {
			return true;
		}

		public override int cost() {
			return 1;
		}
	}
}

