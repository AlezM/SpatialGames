using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Source : MonoBehaviour {

	public enum PlayerType { D, C };

	public class Tile {
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

	public struct Coord {
		public int tileX;
		public int tileY;

		public Coord(int x, int y) {
			tileX = x;
			tileY = y;
		}
	}

	//doraction: horizontal - 0, vertical - 1;
	public struct BorderPoint {
		public double x;
		public double y;
		public bool used;

		public BorderPoint(double _x, double _y) {
			x = _x;
			y = _y;
			used = false;
		}
	}

	public struct Region {
		public List<Coord> coords;
		public int perimeter;
		public Region(List<Coord> _coords, int _perimeter) {
			coords = _coords;
			perimeter = _perimeter;
		}
		public Coord? Get (int x, int y) {
			foreach (Coord coord in coords) {
				if (coord.tileX == x && coord.tileY == y)
					return coord;
			}
			return null;
		}
	}

	public struct Border {
		int pointer;
		public List<BorderPoint> points;
		public Border(List<BorderPoint> _points) {
			pointer = 0;
			points = _points;
		}
		public void Sort () {
			
		}
	}


	//Main Funcs

	public void NextGeneration (Tile[,] map) {
		int size = map.GetLength (0);
		Tile[,] newMap = new Tile[size, size];

		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {

				//Search for the reachest member
				PlayerType newType = map [i, j].type;
				double maxMemberScore = 0;
				bool uncertainty = false;
				for (int x = -1; x <= 1; x++) { 
					for (int y = -1; y <= 1; y++) {
						double memberScore = map [(i + x + size) % size, (j + y + size) % size].score;
						PlayerType memberType = map [(i + x + size) % size, (j + y + size) % size].type;

						if (memberScore > maxMemberScore) {
							maxMemberScore = memberScore;
							newType = memberType;
							uncertainty = false;
						} else if (memberScore == maxMemberScore && newType != memberType) {
							uncertainty = true;
						}
					}
				}
				if (!uncertainty)
					newMap [i, j] = new Tile (newType);
				else 
					newMap [i, j] = (Random.value > 0.5)? new Tile (PlayerType.C): new Tile (PlayerType.D);
			}
		}

		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				map[i, j].SetType (newMap[i, j].type);
			}
		}
	}

	public void CalculateAllScores (Tile[,] map, double b) {
		int size = map.GetLength (0);
		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {

				for (int x = -1; x <= 1; x++) { //Count score for each
					for (int y = -1; y <= 1; y++) {
						map [i, j].InteractWith (map [(i + x + size) % size, (j + y + size) % size], b);
					}
				}

			}
		}
	}

	public void ResetScores (Tile[,] map) {
		int size = map.GetLength (0);
		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				map [i, j].score = 0;
			}
		}
	}


	//Regions

	public List<Region> GetRegions(Tile[,] map, PlayerType tileType) {
		int size = map.GetLength (0);
		List<Region> regions = new List<Region> ();
		int[,] mapFlags = new int[size, size];

		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				if (mapFlags[i, j] == 0 && map[i, j].type == tileType) {
					Region newRegion = GetRegionTiles(map, i, j);
					regions.Add(newRegion);

					foreach (Coord tile in newRegion.coords) {
						mapFlags[tile.tileX, tile.tileY] = 1;
					}
				}
			}
		}

		return regions;
	}

	public Region GetRegionTiles(Tile[,] map, int startX, int startY) {
		int size = map.GetLength (0);
		int perimeter = 0;
		List<Coord> tiles = new List<Coord> ();
		int[,] mapFlags = new int[size, size];
		PlayerType tileType = map [startX, startY].type;

		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (new Coord (startX, startY));
		mapFlags [startX, startY] = 1;

		while (queue.Count > 0) {
			Coord tile = queue.Dequeue();
			tiles.Add(tile);

			perimeter += GetMembersCount (map, tile, tileType);

			for (int x = -1; x <= 1; x++) {
				for (int y = -1; y <= 1; y++) {
					int memberX = (tile.tileX + x + size) % size;
					int memberY = (tile.tileY + y + size) % size;

					if (mapFlags[memberX, memberY] == 0 && map[memberX, memberY].type == tileType) {
						mapFlags[memberX, memberY] = 1;
						queue.Enqueue(new Coord(memberX, memberY));
					}
				}
			}
		}

		return new Region (tiles, perimeter);
	}
		
	public List<Border> GetBorders (Tile[,] map, List<Region> regions, PlayerType type) {
		int size = map.GetLength (0);
		List<Border> borders = new List<Border>();
		foreach (Region region in regions) {
			Border border = new Border();

			border.points = new List<BorderPoint>();
			foreach (Coord coord in region.coords) {

				bool up = false, right = false, down = false, left = false;
				for (int x = -1; x <= 1; x++) {
					for (int y = -1; y <= 1; y++) {
						int memberX = (coord.tileX + x + size) % size;
						int memberY = (coord.tileY + y + size) % size;
						if ((map [memberX, memberY].type != type) && (x == 0 || y == 0)) {
							if (x == -1 && y == 0)
								up = true;
							if (x == 1 && y == 0)
								down = true;
							if (x == 0 && y == -1)
								left = true;
							if (x == 0 && y == 1)
								right = true;
						}
					}
				}

				bool[,] sidePoints = new bool[3, 3];
				sidePoints.Initialize ();

				if (up) {
					sidePoints [0, 0] = true;
					sidePoints [1, 0] = true;
					sidePoints [2, 0] = true;
				}
				if (right) {
					sidePoints [2, 0] = true;
					sidePoints [2, 1] = true;
					sidePoints [2, 2] = true;
				}
				if (down) {
					sidePoints [0, 2] = true;
					sidePoints [1, 2] = true;
					sidePoints [2, 2] = true;
				}
				if (right) {
					sidePoints [0, 0] = true;
					sidePoints [0, 1] = true;
					sidePoints [0, 2] = true;
				}

				//Add border points to list
				for (int i = 0; i < 3; i++) {      					 //ПРОБЛЕМА!!!!
					for (int j = 0; j < 3; j++) {
						if (sidePoints [i, j] && (i == 1 || j == 1)) {
							double cache;
							double newI = ( (cache = (double)coord.tileX - (i - 1) * 0.5) >= 0)? cache: cache + size;
							double newJ = ( (cache = (double)coord.tileY - (j - 1) * 0.5)  >= 0)? cache: cache + size;

							border.points.Add (new BorderPoint (newI, newJ));
							//	(double)coord.tileX - (i - 1) * 0.5, (double)coord.tileY - (j - 1) * 0.5));
						}
					}
				}
			}

			//Sorting list of points

			//Then add it to the borders list
			borders.Add(border);
		}


		return borders;
	}

	int GetMembersCount (Tile[,] map, Coord tile, PlayerType type) {
		int size = map.GetLength (0);
		int count = 0;
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				int memberX = (tile.tileX + x + size) % size;
				int memberY = (tile.tileY + y + size) % size;
				if ((map [memberX, memberY].type != type) && (x == 0 || y == 0)) {
					count++;
				}
			}
		}
		return count;
	}

	public int GetCount (Tile[,] map, PlayerType type) {
		int size = map.GetLength (0);
		int count = 0;
		for (int x = 0; x < size; x++) {
			for (int y = 0; y < size; y++) {
				if (map [x, y].type == type)
					count++;
			}
		}
		return count;
	}

	void DeleteSamePoints (Border border) {
		foreach (BorderPoint point in border.points) {
			
		}
	}

	public int GetBorderOrientation (Tile[,] map, Coord tile, PlayerType type) {
		int size = map.GetLength (0);
		bool up = false, right = false, down = false, left = false;
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				int memberX = (tile.tileX + x + size) % size;
				int memberY = (tile.tileY + y + size) % size;
				if ((map [memberX, memberY].type != type) && (x == 0 || y == 0)) {
					if (x == -1 && y == 0)
						up = true;
					if (x == 1 && y == 0)
						down = true;
					if (x == 0 && y == -1)
						left = true;
					if (x == 0 && y == 1)
						right = true;
				}
			}
		}

		int orientation = 0;
		if (up && right && down && left) {
			orientation = 16;
		}
		if (up && right && down && !left) {
			orientation = 15;
		}
		if (up && right && !down && left) {
			orientation = 14;
		}
		if (up && right && !down && !left) {
			orientation = 13;
		}
		if (up && !right && down && left) {
			orientation = 12;
		}
		if (up && !right && down && !left) {
			orientation = 11;
		}
		if (up && !right && !down && !left) {
			orientation = 10;
		}
		if (up && !right && !down && left) {
			orientation = 9;
		}
		if (!up && right && down && left) {
			orientation = 8;
		}
		if (!up && right && down && !left) {
			orientation = 7;
		}
		if (!up && right && !down && left) {
			orientation = 6;
		}
		if (!up && right && !down && !left) {
			orientation = 5;
		}
		if (!up && !right && down && left) {
			orientation = 4;
		}
		if (!up && !right && down && !left) {
			orientation = 3;
		}
		if (!up && !right && !down && left) {
			orientation = 2;
		}
		if (!up && !right && !down && !left) {
			orientation = 1;
		}

		return orientation;
	}

	//Visualisation

	public Texture2D MapTexture (Tile[,] map) {
		int size = map.GetLength (0);
		Texture2D outTexure = new Texture2D (size, size);
		outTexure.filterMode = FilterMode.Point;
		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				outTexure.SetPixel (i, size - j - 1, map[i, j].color);
			}
		}
		outTexure.Apply ();

		return outTexure;
	}

	public List<Texture2D> RegionsTextures (Tile[,] map, List<Region> coopsRegions) {
		int size = map.GetLength (0);
	//	List<Region> cooperatorsRegions = GetRegions (PlayerType.D);
		List<Texture2D> regionsTextures = new List<Texture2D> ();

		foreach (Region cooperatorsRegion in coopsRegions) {
			Texture2D t = new Texture2D (size, size);
			t.filterMode = FilterMode.Point;
			for (int i = 0; i < size; i++) {
				for (int j = 0; j < size; j++) {
					t.SetPixel (i, size - j - 1, Color.black);
				}
			}
			foreach (Coord tile in cooperatorsRegion.coords) {
				t.SetPixel (tile.tileX, size - tile.tileY - 1, Color.white);
			}
			t.Apply ();
			regionsTextures.Add (t);
		}

		return regionsTextures;
	}
}
