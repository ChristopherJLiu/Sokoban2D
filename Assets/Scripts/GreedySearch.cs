using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AStarsearch : SearchAlgorithm {

	private HashSet<object> closedSet = new HashSet<object> ();
	private PriorityQueue openlist = new PriorityQueue();
	private float heu;
	//private int heuxiliar;


	protected override void Begin () 
	{
		SearchNode start = new SearchNode (problem.GetStartState (), 0);

		openlist.Add(start,0);
	}
		


	protected override void Step(){
		if (openlist.Count > 0)
		{
			

			SearchNode cur_node = openlist.PopFirst ();;
			closedSet.Add (cur_node.state);
			if (problem.IsGoal (cur_node.state)) {
				solution = cur_node;
				finished = true;
				running = false;
			} else {
				Successor[] sucessors = problem.GetSuccessors (cur_node.state);
				foreach (Successor suc in sucessors) {
					if (closedSet.Contains (suc.state)) {
						continue;
					
					}
					if (!closedSet.Contains (suc.state)) {
						
						heu = problem.GoalsMissing (suc.state);
						SearchNode new_node = new SearchNode (suc.state, suc.cost + cur_node.g,heu, suc.action, cur_node);
						openlist.Add (new_node,(int)new_node.h);

					}

				}
			}
		
		}
		else
		{
			finished = true;
			running = false;
		}
	}
}
