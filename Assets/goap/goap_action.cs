using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goap {
	public abstract class goap_action : MonoBehaviour {
		public abstract string[] precondition { get; }
		public abstract string[] effects { get; }

		public abstract void enter();

		public abstract void exit();

		public abstract void update();

		public abstract bool isDone();

		public abstract bool inRange();

		public abstract bool isActive();

		public abstract int cost();
	}
}