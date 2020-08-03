using UnityEngine;
using System.Collections;


public struct Successor
{
	public object state;
	public float cost;
	public Action action;


	public Successor(object state, float cost, Action a)
	{
		this.state = state;
		this.cost = cost;
		this.action = a;
	}
}

public delegate float Heuristic(object state);

public interface ISearchProblem
{
	object GetStartState ();
	bool IsGoal (object state);
	Successor[] GetSuccessors (object state);
	float GoalsMissing (object state);
	int GetVisited ();
	int GetExpanded ();
	float distCratePlayer (object state);
	float distCrateGoal (object state);
	float distEuclGoal (object state);
	float distEuclPlayer (object state);

}
