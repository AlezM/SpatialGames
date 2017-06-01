using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMethod : Source {

	Tile[,] map;

	public int mapSize;
	public double b = 1.79;

	public float defectorsSpawnPercentage = 21;
	public string seed;
	public bool useRandomSeed = false;

	//Textures
	Texture2D mapTexture;

	//Regions
	List<Region> defectorsRegions;
	List<Region> cooperatorsRegions;

	void Start () {
		SetUpMap ();	
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			MakeStep ();
		}
		if (Input.GetKeyDown (KeyCode.RightAlt)) {
			FittingPoints ();
			Debug.Log ("Poits Set");
		}


	}

	void OnGUI () {
		if (mapTexture != null)
			GUI.DrawTexture (new Rect (0, 0, Mathf.Min (Screen.width / 2, Screen.height), Mathf.Min (Screen.width / 2, Screen.height)), mapTexture);
	}

	void SetUpMap () {
		map = new Tile[mapSize, mapSize];

		if (useRandomSeed) {
			seed = Random.value.ToString();
		}

		System.Random pseudoRandom = new System.Random(seed.GetHashCode());

		for (int i = 0; i < mapSize; i++) {
			for (int j = 0; j < mapSize; j++) {
				map [i, j] = (pseudoRandom.Next(0, 100) < defectorsSpawnPercentage)? new Tile (PlayerType.D): new Tile (PlayerType.C);
			}
		}

	//	defectorsRegions = GetRegions (map, PlayerType.D);
		cooperatorsRegions = GetRegions (map, PlayerType.C);

		mapTexture = MapTexture (map);
	}

	void MakeStep () {
		//Main Calucs
		CalculateAllScores (map, b);
		NextGeneration (map);
		ResetScores (map);
	//	defectorsRegions = GetRegions (map, PlayerType.D);
		cooperatorsRegions = GetRegions (map, PlayerType.C);

		mapTexture = MapTexture (map);
	}
		
	void FittingPoints () {
		if (cooperatorsRegions == null)
			return;
		
		List<Border> borders = GetBorders (map, cooperatorsRegions, PlayerType.C);

	//	Debug.Log ("cooperatorsRegions.Count: " + cooperatorsRegions.Count);
	//	Debug.Log ("borders.Count: " + borders.Count);

		int R = 1;

		foreach (Border border in borders) {
			int listSize = border.points.Count;					
			BorderPoint[] points = border.points.ToArray();
			points [0].used = true; 	//В нулевую полюбому ставлю

			for (int i = 0, counter = 0; counter < listSize;) { //Это внешний цикл, тут я просто прохожусь по каждой точке и решаю, ставить в неё ТОЧКУ или нет
																//Прикол цикла в том, что он знает точку на которой мы стоим, и количество точек, в которых ТОЧКИ
				/*
				if (points [i].used == true)	{
					counter++;
					continue; //Вдруг тут уже есть точка
				}*/

				double prevDist = mapSize*mapSize;
				int nearestCellIndex = -1;

				//Ищу ближайшую точку в которую можно поставить ТОЧКУ
				for (int j = 0; j < listSize; j++) { 	//Тут ищу самую близкую к i-ой точке точку, но не ближе чем 2R

					if (points [j].used == true)	//Тут же цикл пропускает и точку в которой сечас находится.
						continue;  //Вдруг там уже есть точка
					
					double dist = Distanse (points [i], points [j]);  //Смотрим расстояние

					if ( dist >= 2*R && dist < prevDist) {
						//Блиииин, не учитываю, что могу случайно поставить в точку, радом с которой уже есть точки(((((((
						//Опять ломается((((((( Снова цикл, чтобы посмотреть,есть ли в окрестности точки?
						bool notEnoughSpace = false;
						for (int k = 0; k < listSize; k++) {
							if (Distanse (border.points [k], border.points [j]) < 2*R && border.points [k].used && k != j)
								notEnoughSpace = true;
						}

						if (!notEnoughSpace) {
							points [j].used = true;
							nearestCellIndex = j;			//Нашли точку в которую можно поставить ТОЧКУ
						}
					}
				}

				if (nearestCellIndex != -1) {
					i = nearestCellIndex;
					counter++;					
				} else {
					//Почему-то не нашлось такой точки, похоже что больше не получится поставить, 
					//нужно выходить из цикла, и брать другую границу. Ещё нужно запомнить количество точек, еоторые я поставил
					//помоему это значение храниться в counter;
				//	Debug.Log("Size: " + listSize.ToString() + "; R: "+ R.ToString() + "; Points: " + counter.ToString());
					counter = listSize;
					break;
				}
			}

			int c = PointsCount(points);
			Debug.Log( "Size: " + listSize.ToString() + "; R: "+ R.ToString() + "; Points: " + c.ToString() );
		}
	}

	//ТУТ ОШИБКА!!!!
	double Distanse (BorderPoint p1, BorderPoint p2) {
		return ((p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y));
	}

	int PointsCount (BorderPoint[] _points) {
		int k = 0;
		for (int i = 0; i < _points.Length; i++)
			if (_points[i].used) k++;
		return k;
	}
}
