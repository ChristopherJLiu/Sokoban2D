using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AStarsearch : SearchAlgorithm {

	private HashSet<object> closedSet = new HashSet<object> ();
	private PriorityQueue openlist = new PriorityQueue();
	private float heu=0;
	public int heu_chooser;
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
					
					if (!closedSet.Contains (suc.state)) {
						
						heu = heu_val (suc);
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



//Funcao que deixa o user escolher qual a euristica a escolher
	public float heu_val(Successor suc){
		float ans;
		if (heu_chooser == 1) {
			ans = problem.distCratePlayer (suc.state);
			return ans;
		}
		else if (heu_chooser == 2) {
			ans = problem.distCrateGoal (suc.state);
			return ans;
		}
		else if (heu_chooser == 3) {
			ans = problem.distEuclGoal (suc.state);
			return ans;
		}
		else if (heu_chooser == 4) {
			ans = problem.distEuclPlayer (suc.state);
			return ans;
		}
		else {
			ans = problem.GoalsMissing (suc.state);
			return ans;
		}
	}


}
