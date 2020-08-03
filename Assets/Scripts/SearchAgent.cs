using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;


public enum Action {North, East, South, West, None=-1};

public class Actions
{
	private static readonly Action[] all_actions = { Action.North, Action.East, Action.South, Action.West };
	private static readonly Vector2 vector_north = new Vector2 (0, 1);
	private static readonly Vector2 vector_east = new Vector2 (1, 0);
	private static readonly Vector2 vector_south = new Vector2 (0, -1);
	private static readonly Vector2 vector_west = new Vector2 (-1, 0);
	private static readonly Vector2[] all_vectors = {vector_north, vector_east, vector_south, vector_west};

	public static Action[] GetAll()
	{
		return all_actions;
	}

	public static Vector2[] GetAllVectors()
	{
		return all_vectors;
	}

	public static Vector2 GetVector(Action a)
	{
		if (a == Action.North) {
			return vector_north;
		} else if (a == Action.East) {
			return vector_east;
		} else if (a == Action.South) {
			return vector_south;
		} else {
			return vector_west;
		}
	}

	public static Action Reverse(Action dir)
	{
		if (dir == Action.North) {
			return Action.South;
		} else if (dir == Action.East) {
			return Action.West;
		} else if (dir == Action.South) {
			return Action.North;
		} else {
			return Action.East;
		}
	}

	public static string ToString(Action a)
	{
		if (a == Action.North) {
			return "North";
		} else if (a == Action.East) {
			return "East";
		} else if (a == Action.South) {
			return "South";
		} else {
			return "West";
		}
	}
}


public class SearchAgent : MonoBehaviour {

	// Simulation utility variables
	public string loggerName = "log.txt";
	public string outputName = "output.json";

	public bool tryLoad = true;
	public string solution = "output.json";

	// To be assessible across all classes
	public static bool batchmode = false;
	public static SimpleLogger LOGGER;
	//

	public Text visitedText;
	public Text expandedText;
	public Text actionsText;

	private int cellSize;
	private SokobanProblem problem;
	private SearchAlgorithm search;
	private List<Action> path;
	private GameObject[] crates;
	private int total_actions = 0;



	void Awake(){
		// handle arguments.. if any
		handleArgs ();

		// start main logger
		LOGGER = new SimpleLogger(loggerName);
	}
		
	void Start () {

		Map map = GameObject.Find ("Map").GetComponent<Map> ();

		// Get the cell size from the map.
		cellSize = map.cellSize;

		//Create the problem
		problem = new SokobanProblem(map.GetPlayerStart(), map.GetCrates(), map.GetGoals(), map.GetWalls());

		// Get the search algorithm to use
		Component[] all_algorithms = GetComponents<SearchAlgorithm> ();
		foreach (SearchAlgorithm alg in all_algorithms)
		{
			if (alg.isActiveAndEnabled) {
				search = alg;
			}
		}

		//set the problem in the search algorithm
		search.problem = problem;

		// checks if not in batchmode, if we and to load a solution and if a solution file exists
		if (!batchmode && tryLoad && File.Exists(solution) ){

			loadPath ();

			LOGGER.Log ("Path Length: " + path.Count.ToString ());
			LOGGER.Log ("[{0}]", string.Join (",", path.ConvertAll<string> (Actions.ToString).ToArray ()) );
			// turn off search algorithm
			search.setRunning (false);
			search.setFinished (true);

		}


		//Get the crate objects
		crates = GameObject.FindGameObjectsWithTag ("Crate");

		if (!batchmode) {
			LOGGER.Log ("waiting on key !");
		} else {
			LOGGER.Log("running on batchmode to solve {0}\n{1}\n", map.map.name,map.map.text);
		}
		LOGGER.Flush ();

		if (batchmode) {
			search.StartRunning ();
		}

	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.Space)) {
			search.StartRunning ();
		}
			
		if (path == null && search.Finished ()) {
			Debug.Log ("Visited: " + search.problem.GetVisited ().ToString ());
			Debug.Log ("Expanded: " + search.problem.GetExpanded ().ToString ());
			LOGGER.Log ("Visited: {0}" , search.problem.GetVisited ().ToString ());
			LOGGER.Log ("Expanded: {0}" , search.problem.GetExpanded ().ToString ());

			LOGGER.Log ("Building solution path");
			path = search.GetActionPath ();

			total_actions = path.Count;

			LOGGER.Log ("Saving path!");
			savePath (path);

			LOGGER.Log ("done.. !");			
			LOGGER.Log ("[{0}]", string.Join (",", path.ConvertAll<string> (Actions.ToString).ToArray ()) );

			if (path != null) {
				Debug.Log ("Path Length (Number of Actions): " + path.Count.ToString ());
				Debug.Log ("[" + string.Join (",", path.ConvertAll<string> (Actions.ToString).ToArray ()) + "]");
			}

			// if on batchmode terminate the application
			if (batchmode) {
				LOGGER.Log ("Finished.");
				LOGGER.Flush ();
				Application.Quit ();
			}
		} 

		visitedText.text = "Visited: "+search.problem.GetVisited ().ToString();
		expandedText.text = "Expanded: "+search.problem.GetExpanded ().ToString ();
		LOGGER.Flush ();
	}

	void FixedUpdate() {
		if (path != null && path.Count > 0) {

			Time.timeScale = 0.1f;
			Action action = path [0];
			path.RemoveAt (0);

			Vector3 movement = Actions.GetVector (action);

			Move (movement);

			actionsText.text = "Actions: "+path.Count.ToString() + "/" + total_actions;

		}
	}

	void Move(Vector3 movement)
	{
		Vector3 new_pos = transform.position + movement * cellSize;

		// Check if there is a crate in the new position
		foreach (GameObject crate in crates) {
			if (crate.transform.position == new_pos)
			{
				// Move crate
				crate.transform.position += movement * cellSize;
			}
		}

		// Move player
		transform.position += movement * cellSize;
	}

	void handleArgs(){
		// get the list of arguments 
		string[] arguments = Environment.GetCommandLineArgs();


		for(int i = 0 ; i < arguments.Length ; i++) {

			if (arguments[i].Contains ("-batchmode")) {
				batchmode = true;
				// if we are in batchmode, the next arguments must be :
				// logger filename
				loggerName = arguments[++i];
				// output filename
				outputName = arguments[++i];
				// example:     ./sokoban -batchmode mylog.txt solution.json
			}

		}
	}


	void savePath(List<Action> path){
		StreamWriter sw = File.CreateText (outputName);

		// Serialization to Json 
		string path_json = JsonHelper.ToJson<Action> (path.ToArray());
		sw.WriteLine (path_json);
		sw.Close ();
	}

	void loadPath(){
		
		string[] lines = File.ReadAllLines(solution);
		// join all lines
		string json_code = string.Join ("", lines);
		path = new List<Action> (JsonHelper.FromJson<Action> (json_code));
		total_actions = path.Count;
		LOGGER.Log ("Read solution: [{0}]", string.Join (",", path.ConvertAll<string> (Actions.ToString).ToArray ()));
	}

}

