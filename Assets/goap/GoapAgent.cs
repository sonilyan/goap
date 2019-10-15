using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Goap
{
	public class GoapAgent
	{
		private readonly List<GoapGoal> _goal = new List<GoapGoal>();
		private readonly Queue<GoapAction> _actions = new Queue<GoapAction>();
		public readonly GoapCondition Effects = new GoapCondition();

		private GoapFsm _fsm;

		private readonly Func<GoapFsm, GoapAgent, IEnumerator> _reset;
		private readonly Func<GoapFsm, GoapAgent, IEnumerator> _idle;
		private readonly Func<GoapFsm, GoapAgent, IEnumerator> _walk;
		private readonly Func<GoapFsm, GoapAgent, IEnumerator> _doAction;

		public event Action GoalActionChanged;

		public void AddGoal(GoapGoal g)
		{
			_goal.Add(g);
			g.OnStateChange += StateChange;
		}

		public void RemoveGoal(GoapGoal g)
		{
			_goal.Remove(g);
			g.OnStateChange -= StateChange;
		}

		private readonly IGoapMoveHandle _move;

		public GoapAgent(IGoapMoveHandle move)
		{
			_move = move;

			_fsm = new GoapFsm(this);

			_reset = Reset;
			_idle = Idle;
			_walk = Walk;
			_doAction = DoAction;

			_fsm.Push(_idle);
		}

		private IEnumerator Reset(GoapFsm fsm, GoapAgent agent)
		{
			_fsm.Pop();
			_fsm.Push(_idle);

			yield return 0;
		}

		private IEnumerator DoAction(GoapFsm fsm, GoapAgent agent)
		{
			if (_actions.Count > 0)
			{
				var action = _actions.Peek();

				if (action.IsDone())
				{
					agent.Effects.Add(action.EffectsAdd);
					agent.Effects.Remove(action.EffectsRemove);
					action.Exit();
					fsm.Pop();
					fsm.Push(_idle);
					_actions.Dequeue();
				}
				else
				{
					yield return action.Update(agent);
				}
			}
			else
			{
				fsm.Pop();
				fsm.Push(_idle);
			}
		}

		private IEnumerator Walk(GoapFsm fsm, GoapAgent agent)
		{
			fsm.Pop();
			var action = _actions.Peek();

			_move.SetTarget(action.Target);

			while (_move.pathPending() && !agent.InterruptState)
			{
				yield return 0;
			}

			while (!_move.IsDone() && !agent.InterruptState)
			{
				_move.MoveUpdate();
				yield return 0;
			}
			
			_move.Stop();
			
			action.Enter();
		}

		private IEnumerator Idle(GoapFsm fsm, GoapAgent agent)
		{
			if (_actions.Count > 0)
			{
				fsm.Pop();
				
				var action =  _actions.Peek();

				if (CurrentAction != action)
				{
					CurrentAction = action;
					GoalActionChanged?.Invoke();
				}
				
				yield return action.CheckInRange(agent);
				
				if (action.InRange)
				{
					action.Enter();
					fsm.Push(_doAction);
				}
				else
				{
					fsm.Push(_doAction);
					fsm.Push(_walk);
				}
			}
			else
			{
				if (!_manualMode)
				{
					yield return Planer(agent);
				}
				else
				{
					if (CurrentAction != null)
					{
						CurrentAction = null;
						GoalActionChanged?.Invoke();
					}

					yield return 0;	
				}
			}
		}

		~GoapAgent()
		{
			while (_goal.Count > 0)
			{
				RemoveGoal(_goal[0]);
			}
		}

		private bool _manualMode = false;

		public bool ManualMode
		{
			get => _manualMode;
			set
			{
				_manualMode = value;
				ResetFsm();
			}
		}


		private void StateChange()
		{
			if (!_manualMode)
			{
				ResetFsm();
			}
		}

		private void ResetFsm()
		{
			_actions.Clear();
			_fsm.Clear();
			_fsm.Push(_reset);
			_move.Stop();	
		}

		public void AddAction(GoapAction action, bool clear)
		{
			if (clear && CurrentAction != null)
			{
				ResetFsm();
			}

			_actions.Enqueue(action);
		}

		public GoapAction CurrentAction { get; private set; }
		public GoapGoal CurrentGoal { get; private set; }

		public IEnumerator Update()
		{
			while (true)
			{
				yield return _fsm.Update();
			}
		}

		public bool InterruptState
		{
			get { return _fsm.Peek() == _reset; }
		}
 
		private IEnumerator Planer(GoapAgent agent)
		{
			_aws = null;

			foreach (var g in agent._goal)
			{
				if (g.Check())
				{
					if (g.IgnorePlaner)
					{
						for (int i = 0; i < g.GoapActions.Length; i++)
						{
							_actions.Enqueue(g.GoapActions[i]);
						}
					}
					else
					{
						yield return GetAction(agent, agent.Effects, g.GoapActions, g.Goal, 0);
						
						if(agent.InterruptState)
							break;
						
						if (_aws == null)
							continue;

						while (_aws != null)
						{
							_actions.Enqueue(_aws.Action);
							_aws = _aws.Next;
						}
					}

					if (CurrentGoal != g)
					{
						CurrentGoal = g;
						GoalActionChanged?.Invoke();
					}

					break;
				}
			}
		}

		private ActionWapper _aws;

		IEnumerator GetAction(GoapAgent agent,GoapCondition effects, GoapAction[] actions, GoapCondition goal, int index)
		{
			ActionWapper retval = null;
			for (int i = index; i < actions?.Length; i++)
			{
				var action = actions[i];
				if (effects.Contains(action.Precondition))
				{
					yield return action.CheckActive(this);

					if(agent.InterruptState)
						break;
					
					if (!action.IsActive)
						continue;

					ActionWapper tmp = null;
					if (action.EffectsAdd.Contains(goal))
					{
						tmp = new ActionWapper(action);
						tmp.CostCount = action.Cost();
					}
					else
					{
						GoapCondition newEffects = new GoapCondition(action.EffectsAdd).Add(effects);

						if (i != index)
						{
							var tmp2 = actions[index];
							actions[index] = actions[i];
							actions[i] = tmp2;
						}

						yield return GetAction(agent, newEffects, actions, goal, index + 1);
						tmp = _aws;
						if (tmp != null)
						{
							tmp = new ActionWapper(action)
							{
								CostCount = action.Cost() + tmp.CostCount,
								Next = tmp,
							};
						}
					}

					if (tmp != null && (retval == null || retval.CostCount > tmp.CostCount))
					{
						retval = tmp;
					}
				}
			}

			_aws = retval;
		}

		private class ActionWapper
		{
			public readonly GoapAction Action;
			public int CostCount = 0;

			public ActionWapper(GoapAction action)
			{
				this.Action = action;
			}

			public ActionWapper Next = null;
		}
	}
}
