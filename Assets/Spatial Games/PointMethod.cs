using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class PointMethod : Source {

	Tile[,] map;

	public int mapSize;
	public double b = 1.79;

	public float defectorsSpawnPercentage = 21;
	public string seed;
	public bool useRandomSeed = true;

	//Textures
	Texture2D mapTexture;

	//Regions
	List<Region> defectorsRegions;
	List<Region> cooperatorsRegions;

    //Output
    StreamWriter file;

    void Start () {
		SetUpMap ();	
	}

	void Update () {
		Tests ();

		if (Input.GetKeyDown (KeyCode.Return)) {
			SetUpMap ();
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			MakeStep ();
		}

		if (Input.GetKeyDown (KeyCode.RightAlt)) {
            file = new StreamWriter("C:/Users/Alez.M/Documents/University/SpatialGames/PointMethod/1.81.txt");
			FittingPoints ();
			Debug.Log ("Poits Set");
            file.Close();
		}


	}

	void OnGUI () {
		if (mapTexture != null)
			GUI.DrawTexture (new Rect (0, 0, Mathf.Min (Screen.width / 2, Screen.height), Mathf.Min (Screen.width / 2, Screen.height)), mapTexture);
	}

	void SetUpMap () {
		map = new Tile[mapSize, mapSize];

		if (useRandomSeed) {
			seed = UnityEngine.Random.value.ToString();
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

	
		foreach (Border border in borders) {
            file.Write("claster_" + border.points.Count.ToString() + " = { \n{");
            for (double R = 0.5; R < mapSize; R *= 2) {
                if (R*2 < mapSize)
                    file.Write(R.ToString() + ", ");
                else
                    file.Write(R.ToString());
            }
            file.Write("}, \n{");


            for (double R = 0.5; R < mapSize; R *= 2) {
				int listSize = border.points.Count;
                if (listSize < 4)
                    break;

				BorderPoint[] points = border.points.ToArray ();

				points [0].used = true; 	//В нулевую полюбому ставлю

				for (int i = 0, counter = 0; counter < listSize;) { //Это внешний цикл, тут я просто прохожусь по каждой точке и решаю, ставить в неё ТОЧКУ или нет
					//Прикол цикла в том, что он знает точку на которой мы стоим, и количество точек, в которых ТОЧКИ

					double prevDist = mapSize * mapSize;
					int nearestCellIndex = -1;

					//Ищу ближайшую точку в которую можно поставить ТОЧКУ
					for (int j = 0; j < listSize; j++) { 	//Тут ищу самую близкую к i-ой точке точку, но не ближе чем 2R

						if (points [j].used == true)	//Тут же цикл пропускает и точку в которой сечас находится.
							continue;  //Вдруг там уже есть точка

						double dist = Distanse (points [i], points [j]);  //Смотрим расстояние

						if (dist >= 2 * R && dist < prevDist) { //Проверяю j-ю точку
							//Блиииин, не учитываю, что могу случайно поставить в точку, радом с которой уже есть точки(((((((
							//Опять ломается((((((( Снова цикл, чтобы посмотреть,есть ли в окрестности точки?
							bool notEnoughSpace = false;
							for (int k = 0; k < listSize; k++) {
								if ( (Distanse (points [k], points [j]) < 2 * R) && (points [k].used) && (k != j) ) { 
									notEnoughSpace = true;
                                }
							}

							if (!notEnoughSpace) {
		/*///////*/			//	points [j].used = true;
								prevDist = dist;
								nearestCellIndex = j;		//Нашли точку в которую можно поставить ТОЧКУ
							}
                            else {
                                Debug.Log("notEnoughSpace");
                            }
                        }
                    }

					if (nearestCellIndex != -1) {
						i = nearestCellIndex;
                        points[nearestCellIndex].used = true;
                        counter++;					
					} else {
						//Почему-то не нашлось такой точки, похоже что больше не получится поставить, 
						//нужно выходить из цикла, и брать другую границу. Ещё нужно запомнить количество точек, еоторые я поставил
						//помоему это значение храниться в counter;
						Debug.Log("Full");

						counter = listSize;
						continue;
					}
				}

				int c = PointsCount (points);
				Debug.Log ("Size: " + listSize.ToString () + "; R: " + R.ToString () + "; Points: " + c.ToString ());
                file.Write(c.ToString() + ", ");
			}
            file.Write("} };");
        }
	}
		
	double Distanse (BorderPoint p1, BorderPoint p2) {
		double dx = Math.Abs(p2.x - p1.x);
        double dy = Math.Abs(p2.y - p1.y);
		double x = Math.Min ( Math.Abs (dx), Math.Abs ((double)mapSize - dx) );
		double y = Math.Min ( Math.Abs (dy), Math.Abs ((double)mapSize - dy) );

		return ( x*x + y*y );
	}

	int PointsCount (BorderPoint[] _points) {
		int k = 0;
		for (int i = 0; i < _points.Length; i++)
			if (_points[i].used == true)
                k++;
		return k;
	}

	void Tests () {
		if (Input.GetKeyDown (KeyCode.Keypad1)) {
			for (int i = 0; i < mapSize; i++) {
				for (int j = 0; j < mapSize; j++) {
					map [i, j] = (i == mapSize/2)? new Tile (PlayerType.C): new Tile (PlayerType.D);
				}
			}
			cooperatorsRegions = GetRegions (map, PlayerType.C);
			mapTexture = MapTexture (map);
		}

		if (Input.GetKeyDown (KeyCode.Keypad2)) {
			for (int i = 0; i < mapSize; i++) {
				for (int j = 0; j < mapSize; j++) {
					map [i, j] = (i == j)? new Tile (PlayerType.C): new Tile (PlayerType.D);
				}
			}
			cooperatorsRegions = GetRegions (map, PlayerType.C);
			mapTexture = MapTexture (map);
		}

		if (Input.GetKeyDown (KeyCode.Keypad3)) {
			for (int i = 0; i < mapSize; i++) {
				for (int j = 0; j < mapSize; j++) {
					map [i, j] = (i <= 0.75 * mapSize && i >= 0.25 * mapSize && j <= 0.75 * mapSize && j >= 0.25 * mapSize)? new Tile (PlayerType.C): new Tile (PlayerType.D);
				}
			}
			cooperatorsRegions = GetRegions (map, PlayerType.C);
			mapTexture = MapTexture (map);
		}
	}
}
