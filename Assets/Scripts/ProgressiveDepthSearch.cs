using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressiveDepthSearch : SearchAlgorithm {

	private Stack<SearchNode> openStack = new Stack<SearchNode> ();
	private HashSet<object> closedSet = new HashSet<object> ();
	private int limit = 1;

	protected override void Begin () 
	{
		
		SearchNode start = new SearchNode (problem.GetStartState (), 0);
		openStack.Push (start);
	}

	protected override void Step()
	{
		if (openStack.Count > 0)
		{
			SearchNode cur_node = openStack.Pop();
			closedSet.Add (cur_node.state);
			if (problem.IsGoal (cur_node.state)) {
				solution = cur_node;
				finished = true;
				running = false;
			} else {
				if (cur_node.depth < limit) {
					Successor[] sucessors = problem.GetSuccessors (cur_node.state);
					foreach (Successor suc in sucessors) {
						if (!closedSet.Contains (suc.state)) {
							SearchNode new_node = new SearchNode (suc.state, suc.cost + cur_node.g, suc.action, cur_node);

							openStack.Push (new_node);

						} 

					}
				} 
			}

		}
		else
		{
			limit++;
			Debug.Log ("limite :" + limit);
			closedSet.Clear ();
			openStack.Clear();
			Begin ();
			running = true;


		}
	}
}
