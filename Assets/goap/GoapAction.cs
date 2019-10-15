using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goap
{
	public abstract class GoapAction
	{
		public GameObject Target;
		public abstract GoapCondition Precondition { get; protected set; }
		public abstract GoapCondition EffectsAdd { get; protected set; }
		public abstract GoapCondition EffectsRemove { get; protected set; }
		public abstract IEnumerator CheckActive(GoapAgent agent);
		public abstract bool IsActive { get; protected set; }
		public abstract IEnumerator CheckInRange(GoapAgent agent);
		public abstract bool InRange { get; protected set; }
		public abstract void Enter();
		public abstract bool IsDone();
		public abstract IEnumerator Update(GoapAgent agent);
		public abstract void Exit();
		public abstract int Cost();
	}

	public interface IGoapMoveHandle
	{
		bool SetTarget(Vector3 pos);
		bool SetTarget(GameObject Target);
		void MoveUpdate();
		bool IsDone();
		void Stop();
		bool pathPending();
	}

	public interface IActionTrigger
	{
		event Action<GameObject, bool> OnInvokeTrigger;
		bool InvokeTrigger(GameObject obj);
	}
}
