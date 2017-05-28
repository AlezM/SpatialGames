using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SpatialGame : MonoBehaviour {

	enum PlayerType { D, C };

	class Tile {
		public PlayerType type;
		public double score;
		public Color color;
		public Tile () {
			type = PlayerType.C; 
			score = 0;
			color = Color.blue;
		}

		public Tile (PlayerType _type) {
			type = _type;
			score = 0;
			if (type == PlayerType.C) 
				color = Color.blue;
			else
				color = Color.red;
		}

		public void InteractWith (Tile member, double b) {
			if (member.type == PlayerType.C) {
				if (type == PlayerType.C)
					score += 1;
				else
					score += b;
			}
		}

		public void SetType (PlayerType _type) {
			if (type == PlayerType.C) {
				if (_type == PlayerType.C)
					color = Color.blue;
				else
					color = Color.yellow;
			}
			else {
				if (_type == PlayerType.C)
					color = Color.green;
				else
					color = Color.red;
			}

			type = _type;
		}
	};

	struct Coord {
		public int tileX;
		public int tileY;

		public Coord(int x, int y) {
			tileX = x;
			tileY = y;
		}
	}

	struct Region {
		public List<Coord> coords;
		public int perimeter;
		public Region(List<Coord> _coords, int _perimeter) {
			coords = _coords;
			perimeter = _perimeter;
		}
	}

	[Range(1, 1024)]
	public int mapSize;

	[Range(0.01f, 100)]
	public float defectorsPercentage = 5;
	public string seed;
	public bool useRandomSeed = false;

	public double parameter = 1.79;
	public bool regionsEnable = false;

	Tile[,] map;

	Texture2D mapTexture;
	List<Texture2D> regionsTextures;

	int cooperatorCount = 0;
	int defectorCount = 0;
	List<Region> cooperatorsRegions;

	//Statistic
	int steps = 0;
	StreamWriter file;
	double allDefPersentage = 0;

	void Awake () {
		file = new StreamWriter (@"C:\Users\Alez.M\Documents\University\SpatialGames\Outs\test.txt");
	}

	void Start () {
		steps = 0;

	//	file = new StreamWriter (@"C:\Users\Alez.M\Documents\University\SpatialGames\Outs\test.txt");

		map = new Tile[mapSize, mapSize];
		gui_mapSize = mapSize.ToString ();

		if (useRandomSeed) {
			seed = Random.value.ToString();
		}

		System.Random pseudoRandom = new System.Random(seed.GetHashCode());

		for (int i = 0; i < mapSize; i++) {
			for (int j = 0; j < mapSize; j++) {
				map [i, j] = (pseudoRandom.Next(0, 100) < defectorsPercentage)? new Tile (PlayerType.D): new Tile (PlayerType.C);
			}
		}

		cooperatorCount = GetCount (PlayerType.C);
		defectorCount = GetCount (PlayerType.D);

		MakeMapTexture ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.RightAlt)) {
			MakeStep ();
		}

		if (Input.GetKeyDown (KeyCode.RightControl)) {
			for (int i = 0; i < 100; i++)
				MakeStep (false);
			MakeStep ();
		}

		if (Input.GetKey(KeyCode.Space)) {
			MakeStep ();
		}
	}

	void FixedUpdate () {
		
	}

	void MakeStep (bool enableTexture = true) {
		CalculateAllScores ();
		NextGeneration ();
		ResetScore ();

		cooperatorCount = GetCount (PlayerType.C);
		defectorCount = GetCount (PlayerType.D);

		CollectStatistic ();

		steps++;
		if (!enableTexture)
			return;

		MakeMapTexture ();
		if (regionsEnable)
			MakeRegionTexture ();

	}

	void CalculateAllScores () {
		int size = mapSize;

		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {

				for (int x = -1; x <= 1; x++) { //Count score for each
					for (int y = -1; y <= 1; y++) {
						map [i, j].InteractWith (map [(i + x + size) % size, (j + y + size) % size], parameter);
					}
				}

			}
		}
	}

	void NextGeneration () {
		Tile[,] newGrid = new Tile[mapSize, mapSize];

		for (int i = 0; i < mapSize; i++) {
			for (int j = 0; j < mapSize; j++) {
				
				//Search for the reachest member
				PlayerType newType = map [i, j].type;
				double maxMemberScore = 0;

				//double cooperatorsScore = 0;
				//double defectorsScore = 0;
				for (int x = -1; x <= 1; x++) { 
					for (int y = -1; y <= 1; y++) {
					/*	if (map [(i + x + mapSize) % mapSize, (j + y + mapSize) % mapSize].type == PlayerType.C) {
							cooperatorsScore += map [(i + x + mapSize) % mapSize, (j + y + mapSize) % mapSize].score;
						}
						else {
							defectorsScore += map [(i + x + mapSize) % mapSize, (j + y + mapSize) % mapSize].score;
						}*/

						double memberScore = map [(i + x + mapSize) % mapSize, (j + y + mapSize) % mapSize].score;
						PlayerType memberType = map [(i + x + mapSize) % mapSize, (j + y + mapSize) % mapSize].type;

						if ( memberScore > maxMemberScore ) { // || (memberScore == maxMemberScore && memberType == PlayerType.D) ) {
							maxMemberScore = memberScore;
							newType = memberType;
						}
					}
				}
				newGrid [i, j] = new Tile (newType);

			}
		}

		for (int i = 0; i < mapSize; i++) {
			for (int j = 0; j < mapSize; j++) {
				map[i, j].SetType (newGrid[i, j].type);
			}
		}
	}

	void ResetScore () {
		string str = "";
		for (int i = 0; i < mapSize; i++) {
			for (int j = 0; j < mapSize; j++) {
				str += map [i, j].type.ToString () + map [i, j].score.ToString() + " ";
				map [i, j].score = 0;
			}
			str += "\n";
		}
	//	Debug.Log (str);
	}

	List<Region> GetRegions(PlayerType tileType) {
		List<Region> regions = new List<Region> ();
		int[,] mapFlags = new int[mapSize, mapSize];

		for (int i = 0; i < mapSize; i++) {
			for (int j = 0; j < mapSize; j++) {
				if (mapFlags[i, j] == 0 && map[i, j].type == tileType) {
					Region newRegion = GetRegionTiles(i, j);
					regions.Add(newRegion);

					foreach (Coord tile in newRegion.coords) {
						mapFlags[tile.tileX, tile.tileY] = 1;
					}
				}
			}
		}

		return regions;
	}

	Region GetRegionTiles(int startX, int startY) {
		int perimeter = 0;
		List<Coord> tiles = new List<Coord> ();
		int[,] mapFlags = new int[mapSize, mapSize];
		PlayerType tileType = map [startX, startY].type;

		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (new Coord (startX, startY));
		mapFlags [startX, startY] = 1;

		while (queue.Count > 0) {
			Coord tile = queue.Dequeue();
			tiles.Add(tile);

			int m = GetMembersCount (tile, tileType);
			if (m < 4) {
				perimeter += 4 - m;
			}

			for (int x = -1; x <= 1; x++) {
				for (int y = -1; y <= 1; y++) {
					int memberX = (tile.tileX + x + mapSize) % mapSize;
					int memberY = (tile.tileY + y + mapSize) % mapSize;

					if (mapFlags[memberX, memberY] == 0 && map[memberX, memberY].type == tileType) {
						mapFlags[memberX, memberY] = 1;
						queue.Enqueue(new Coord(memberX, memberY));
					}
				}
			}
		}

		return new Region (tiles, perimeter);
	}

	int GetMembersCount (Coord tile, PlayerType type) {
		int count = -1;
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				int memberX = (tile.tileX + x + mapSize) % mapSize;
				int memberY = (tile.tileY + y + mapSize) % mapSize;
				if (map [memberX, memberY].type == type && (x == 0 || y == 0)) {
					count++;
				}
			}
		}
		return count;
	}

	int GetCount (PlayerType type) {
		int count = 0;
		for (int x = 0; x < mapSize; x++) {
			for (int y = 0; y < mapSize; y++) {
				if (map [x, y].type == type)
					count++;
			}
		}
		return count;
	}

	void MakeMapTexture () {
		Texture2D  t = new Texture2D (mapSize, mapSize);
		t.filterMode = FilterMode.Point;
		for (int i = 0; i < mapSize; i++) {
			for (int j = 0; j < mapSize; j++) {
				t.SetPixel (i, mapSize - j - 1, map[i, j].color);
			}
		}
		t.Apply ();
		mapTexture = t;
	}

	void MakeRegionTexture () {
		cooperatorsRegions = GetRegions (PlayerType.D);
		regionsTextures = new List<Texture2D> ();

		foreach (Region cooperatorsRegion in cooperatorsRegions) {
			Texture2D t = new Texture2D (mapSize, mapSize);
			t.filterMode = FilterMode.Point;
			for (int i = 0; i < mapSize; i++) {
				for (int j = 0; j < mapSize; j++) {
					t.SetPixel (i, mapSize - j - 1, Color.black);
				}
			}
			foreach (Coord tile in cooperatorsRegion.coords) {
				t.SetPixel (tile.tileX, mapSize - tile.tileY - 1, Color.white);
			}
			t.Apply ();
			regionsTextures.Add (t);
		}
	}

	void CollectStatistic () {
		allDefPersentage += 100 * (double)defectorCount / (mapSize * mapSize);

		string stepStatistic = "";
		if (steps == 0) {
			stepStatistic = "TableForm[{{Step, Percentage, Despersion}, ";

		}
		stepStatistic += "{ " + steps.ToString() + ", "; 
		string defPercentage = (100 * (double)defectorCount / (mapSize * mapSize)).ToString ("##.00");
		stepStatistic += defPercentage + ", 0";

		if (steps == 100) {
			stepStatistic += "}}]";
			Debug.Log (allDefPersentage/(steps + 1));
		}
		else 
			stepStatistic += "}, ";
		
		file.WriteLine(stepStatistic);
	}

	void OnDestroy () {
		file.Close ();
	}


	string gui_mapSize = "";
	void OnGUI () {
		if (mapTexture != null)
			GUI.DrawTexture (new Rect (0, 0, Mathf.Min (Screen.width/2, Screen.height), Mathf.Min (Screen.width/2, Screen.height)), mapTexture);

		if (regionsTextures != null) {
			for (int i = 0; i < regionsTextures.Count; i++) {
				int textureSize = (int)(Mathf.Sqrt (regionsTextures.Count) + 1);
				GUI.DrawTexture (
					new Rect (
						Screen.width/2 + (i % textureSize) * Screen.width / (2*textureSize) + 1,
						(i / textureSize) * Screen.width / (2*textureSize) + 1, 
						Screen.width / (2*textureSize) - 1, Screen.width / (2*textureSize) - 1
					),
					regionsTextures [i]);
				GUI.TextArea (new Rect (
					Screen.width / 2 + (i % textureSize) * Screen.width / (2 * textureSize) + 1,
					(i / textureSize) * Screen.width / (2 * textureSize) + 1, 30, 20), cooperatorsRegions[i].perimeter.ToString());

			}
		}

		gui_mapSize = GUI.TextField (new Rect (62, 0, 50, 20), gui_mapSize);

		if (GUI.Button (new Rect (0, 0, 60, 20), "Restart") || Input.GetKeyDown(KeyCode.Return)) {
			if (gui_mapSize != "")
				mapSize = System.Convert.ToInt32 (gui_mapSize);
			else {
				mapSize = 1;
			}
			Start ();
		}

		GUI.TextField (new Rect (0, 21, 50, 20), "%:" + defectorsPercentage.ToString());
		defectorsPercentage = GUI.HorizontalScrollbar (new Rect (52, 22, 70, 20), defectorsPercentage, 0, 1f, 50);

		useRandomSeed = GUI.Toggle (new Rect (0, 40, 120, 20), useRandomSeed, "use random seed");
		string gui_seed = GUI.TextField (new Rect (0, 60, 60, 20), seed);

		/*
		bLessCritical = GUI.Toggle (new Rect (0, 80, 120, 20), bLessCritical, (bLessCritical)?" b < 1.8":" b > 1.8");
		if (bLessCritical)
			parameter = 1.75;
		else
			parameter = 1.85;
		*/
		GUI.TextArea (new Rect (0, 100, 60, 20), (100 * (double)defectorCount / (mapSize*mapSize)).ToString ("##.00") + "%");

	//	GUI.TextField (new Rect (0, 81, 40, 20), "b:" + parameter.ToString());
	//	parameter = (double)GUI.HorizontalScrollbar (new Rect (42, 82, 70, 20), (float)parameter, 0, 1.7f, 2);

		if (!useRandomSeed)
			seed = gui_seed;
	}
}