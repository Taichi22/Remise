using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager: MonoBehaviour {
//Public Values:
//Global variables, used by other classes upon update.
//Hex object (placeholder)
	public GameObject Hex;
//Ground object
	public GameObject Ground;

//Currently selected tile
	public Tile selectedTile = null;
//Origin tile of pathing
	public TileBehaviour originTileTB = null;
//Destination tile of pathing
	public TileBehaviour destTileTB = null;

//Instance of GridManager
	public static GridManager instance = null;


//Public Lists:
//These values are global, but are held by the grid manager, as the manager class; these are all lists or collections of some sort
//The actual container for the board; a set of points correlated with their respective tile behaviors
	public Dictionary<Point, TileBehaviour> Board;

//The grid width (in hexes)
	public int gridWidthInHexes = 10;
//The grid height (in hexes)
	public int gridHeightInHexes = 10;

//A list of all terrain placements
	public List <Vector3> terrainPlacements;
//A list of terrain types (set would be more appropriate, however, it is much easier to use a 
						 //numerical representation of tile types rather than strings)
	public List <GameObject> terrainTypes;

//List variable for holding a series of tiles which will create a path, which can then be highlighted.
	public List<GameObject> path;




//Inherent Values: 
//Values which determine placement/size of the grid.
//Initial position of the grid in global coordinates; calculated by calcInitPos()
	private Vector2 initPos;
//Hex Renderer X value is the width.
	private float hexWidth;

//Hex Renderer Y value is the height.
	private float hexHeight;

//Ground Renderer X value is the width.
	private float groundWidth;

//Ground Renderer Y value is the height.
	private float groundHeight;


//Function: Awake
//Called from: initialization
//Set sizes of Hex renderer, and ground renderer for creation of grid
//Create hexagonal grid backdrop and tiles
	void Awake(){
		instance = this;
		initPos = calcInitPos();
		setSizes ();
		createGrid ();
	}

//Function: setSizes
//Called from: Awake()
//Set sizes of hex renderer for usage in createGrid, and size calculation functions
//Also, calculate sizes of backdrop for grid generation
	void setSizes(){
		hexWidth = Hex.GetComponent<Renderer>().bounds.size.x;
		hexHeight = Hex.GetComponent<Renderer> ().bounds.size.y;
		groundWidth = Ground.GetComponent<Renderer> ().bounds.size.x;
		groundHeight = Ground.GetComponent<Renderer> ().bounds.size.y;
	}


//Function: calcInitPos
//Called from: Awake()
//Determine inital position of grid in world; keeping in mind that we are working in a 2D world = no need for Z-axis
	Vector2 calcInitPos(){
		Vector2 initPos;
		//top left corner for board creation
		initPos = new Vector2 (hexWidth / 2 - groundWidth / 2, hexHeight / 2 + groundHeight / 2);
		return initPos;
	}


//Function: calcGridPos
//Called from:
//Calculates grid coordinates of a hex, given the coordinates; we are working on a 2D plane, so we use a Vector2
	public Vector2 calcGridPos(Vector2 coord){
		Vector2 gridPos = new Vector2();
		float offset = 0;
		gridPos.y = Mathf.RoundToInt ((initPos.y - coord.y) / (hexHeight * 0.75f));
		if (gridPos.y % 2 != 0) {
			offset = hexWidth / 2;
		}
		gridPos.x = Mathf.RoundToInt ((coord.x - initPos.x - offset) / hexWidth);
		return gridPos;
	}





//Function: calcWorldCoord
//Called from:
//Calculate the world coordinates of the center of a given tile.
//Useful when moving things around.
	public Vector2 calcWorldCoord(Vector2 gridPos){
		float offset = 0;
		//odd, even hexes
		if (gridPos.y % 2 != 0) {
			offset = hexWidth / 2;
		}
		//
		float x = gridPos.x * hexWidth + initPos.x + offset;
		float y = initPos.y - gridPos.y * hexHeight * 0.75f;
		return new Vector2 (x, y);
	}


//Function: calcGridSize()
//Called from: createGrid
//Calculate the size that the grid will take from any backdrop
	Vector2 calcGridSize(){
		float sideLength = hexHeight;
		int nrOfSides = (int)(groundHeight / sideLength + 0.00005f);
		int nrOfYHexes = (int)(nrOfSides * 2 / 3);
		//accounting for odd/even hexes
		if ((nrOfYHexes % 2 == 0) && (((nrOfSides + 0.5f) * sideLength) > groundHeight)) {
			nrOfYHexes--;
		}
			return new Vector2((int)(groundWidth/ hexWidth), nrOfYHexes);
	}




//Function: createGrid
//Called from: Awake(), 
//Create the basic hex tile grid which the battlefield will take place upon
	void createGrid(){
		Vector2 gridSize = calcGridSize ();
		GameObject hexGrid = new GameObject ("HexGrid");
		Board = new Dictionary<Point, TileBehaviour>();
		for (float y = 0; y < gridSize.y; y++) {
			float sizeX = gridSize.x;
			if (y % 2 != 0 && (gridSize.x + 0.5) * hexWidth > groundWidth)
				sizeX--;
			for (float x = 0; x < sizeX; x++) {
				GameObject hex = (GameObject)Instantiate (Hex);
				Vector2 gridPos = new Vector2(x, y);
				hex.transform.position = calcWorldCoord(gridPos);
				hex.transform.parent = hexGrid.transform;
				var tb = (TileBehaviour)hex.GetComponent("TileBehaviour");
				tb.tile = new Tile((int)x - (int)(y / 2), (int)y);
				Board.Add(tb.tile.Location, tb);
			}
		}
		bool equalLineLengths = (gridSize.x + 0.5) * hexWidth <= groundWidth;
		foreach(TileBehaviour tb in Board.Values)
			tb.tile.FindNeighbours(Board, gridSize, equalLineLengths);
	}



}



