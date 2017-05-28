using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Engine : Source {

	Tile[,] map;

	public int mapSize;
	public double b = 1.79;

	public float defectorsSpawnPercentage = 21;
	public string seed;
	public bool useRandomSeed = false;
	public bool collectStatistic = false;
	public bool showStatistic = true;

	//Regions
	List<Region> defectorsRegions;
	List<Region> cooperatorsRegions;

	//Textures
	Texture2D mapTexture;
	List<Texture2D> regionsTextures;

	//Statistic
	double step;
	double defectorsRegionsPerimeter;
	double allDefectorsRegionsPerimeter;
	double averegeDefectorsRegionsPerimeter;
	double defectorsRegionsPerimeterDelta;
	double defectorsRegionsPerimeterDispersion;
	double defectorsRegionsPerimeterError;

	double defectorsPercentage;
	double allDefectorsPercentage;
	double averegeDefectorsPercentage;
	double defectorsPercentageDelta;
	double defectorsPercentageDispersion;
	double defectorsPercentageError;

	string fileName = "file.txt";
	StreamWriter file;

	void Start () {
		SetUpMap ();

	//	Experiment2 (200);
	//	Experiment3 ();

		return;
		b = 1.8;
		file = new StreamWriter ("C:/Users/Alez.M/Documents/University/SpatialGames/Tests2/1.8.txt");
		for (int k = 0; k < 20; k++) {
			file.WriteLine ("test" + k.ToString() + " = {{b, mapSize, seed, perimeter, avgPerimeter, perDispers, perError, defPerc, avgPerc, percDispers, percError}");
			Experiment1 ();
			file.WriteLine ("};\n");
		}
		file.Close ();
		file = null;
	}


	void Update () {
		if (Input.GetKey (KeyCode.Space)) {
		//	MakeStep ();
		}

		if (Input.GetKeyDown (KeyCode.RightAlt) || Input.touchCount == 1) {
			MakeStep ();
		}

		if (Input.GetKeyDown (KeyCode.Return) || Input.touchCount == 2) {
			SetUpMap ();
		}

		if (Input.GetKeyDown (KeyCode.C)) 
			collectStatistic = !collectStatistic;

		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit();
		}
	}

	void FixedUpdate () {
		if (Input.GetKeyDown (KeyCode.RightArrow) && mapSize < 500)
			mapSize++;
		if (Input.GetKeyDown (KeyCode.LeftArrow) && mapSize > 1)
			mapSize--;

		if (Input.GetKey (KeyCode.Space)) {
			MakeStep ();
		}
	}

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
		/*
		if ( (showStatistic = GUI.Toggle (new Rect (0, 80, 125, 20), showStatistic, "Show Statistic")) || Input.GetKey (KeyCode.I) ) {
			GUI.TextField (new Rect (Screen.width/2 - 150, Screen.height/2 - 70, 300, 140),
				"Step: " + step.ToString ("####") + "\n" +
				"Perimeter: " + defectorsRegionsPerimeter.ToString ("####") + "\n" +
				"Avr.perimeter: " + averegeDefectorsRegionsPerimeter.ToString ("##0.000") + "\n" +
				"Perim.dispersion: " + defectorsRegionsPerimeterDispersion.ToString ("##0.000") + "\n" +
				"Perim. error: "  + defectorsRegionsPerimeterError.ToString ("##0.000") + "\n" +
				"Defectors %: " + defectorsPercentage.ToString ("##0.000") + "\n" +
				"Avr.defectors %: " + averegeDefectorsPercentage.ToString ("##0.000") + "\n" +
				"% dispersion: " + defectorsPercentageDispersion.ToString ("##0.000")  + "\n" +
				"% error: " + defectorsPercentageError.ToString ("##0.000")
			);
		}
		/*
		//Interface
		Interface ();		

		//File
		if (file == null) {
			fileName = GUI.TextField (new Rect (0, 0, 60, 20), fileName);
			if (GUI.Button (new Rect (62, 0, 42, 20), "Open")) {
				file = new StreamWriter ("C:/Users/Alez.M/Documents/University/SpatialGames/Tests/1/" + fileName);
				file.WriteLine("{{b, mapSize, seed, perimeter, avgPerimeter, perDispers, perError, defPerc, avgPerc, percDispers, percError}");
			}
		} 
		else {
			GUI.TextField (new Rect (0, 0, 60, 20), fileName);
			if (GUI.Button (new Rect (62, 0, 42, 20), "Close")) {
				file.WriteLine("}");
				file.Close ();
				file = null;
			}
			if (GUI.Button (new Rect (106, 0, 42, 20), "Save")) {
				SaveStatiscticToFile ();
			}
		}*/
	}

	//// //// //// //// //// //// 

	void SetUpMap () {
		step = 0;
		defectorsRegionsPerimeter = 0;
		allDefectorsRegionsPerimeter = 0;
		averegeDefectorsRegionsPerimeter = 0;
		defectorsRegionsPerimeterDelta = 0;
		defectorsRegionsPerimeterDispersion = 0;
		defectorsRegionsPerimeterError = 0;

		defectorsPercentage = 0;
		allDefectorsPercentage = 0;
		averegeDefectorsPercentage = 0;
		defectorsPercentageDelta = 0;
		defectorsPercentageDispersion = 0;
		defectorsPercentageError = 0;

		collectStatistic = false;

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

		defectorsRegions = GetRegions (map, PlayerType.D);
		cooperatorsRegions = GetRegions (map, PlayerType.C);

		//Statistic
		if (collectStatistic)
			CollectStatistic ();

		Visualise ();
	}

	void MakeStep (bool visualise = true) {
		//Main Calucs
		CalculateAllScores (map, b);
		NextGeneration (map);
		ResetScores (map);
		defectorsRegions = GetRegions (map, PlayerType.D);
		cooperatorsRegions = GetRegions (map, PlayerType.C);

	//	step++;

		//Statistic
		if (collectStatistic)
			CollectStatistic ();

		//Graphics
		if (visualise)
			Visualise ();
	}

	void CollectStatistic () {
		step++;

		defectorsRegionsPerimeter = Perimeter ();
		allDefectorsRegionsPerimeter += defectorsRegionsPerimeter;
		averegeDefectorsRegionsPerimeter = (double)allDefectorsRegionsPerimeter / step;
		defectorsRegionsPerimeterDelta += (defectorsRegionsPerimeter - averegeDefectorsRegionsPerimeter) * (defectorsRegionsPerimeter - averegeDefectorsRegionsPerimeter);
		defectorsRegionsPerimeterDispersion = (double)defectorsRegionsPerimeterDelta / step;
		defectorsRegionsPerimeterError = (step > 1)? System.Math.Sqrt (defectorsRegionsPerimeterDispersion / (step - 1)) : 0;

		defectorsPercentage = 100 * (double)GetCount(map, PlayerType.D) / (map.GetLength(0)*map.GetLength(0));
		allDefectorsPercentage += defectorsPercentage;
		averegeDefectorsPercentage = allDefectorsPercentage / step;
		defectorsPercentageDelta += (defectorsPercentage - averegeDefectorsPercentage) * (defectorsPercentage - averegeDefectorsPercentage);
		defectorsPercentageDispersion = (double)defectorsPercentageDelta / step;
		defectorsPercentageError = (step > 1)? System.Math.Sqrt (defectorsPercentageDispersion / (step - 1)) : 0;
	}

	void SaveStatiscticToFile () {
		file.WriteLine (", {" + 
			b.ToString("0.00") + ", " + mapSize.ToString() + ", " + seed + ", " +
			defectorsRegionsPerimeter.ToString ("#####") + ", " +
			averegeDefectorsRegionsPerimeter.ToString ("##0.000") + ", " +
			defectorsRegionsPerimeterDispersion.ToString ("##0.000") + ", " +
			defectorsRegionsPerimeterError.ToString ("##0.000") + ", " +
			defectorsPercentage.ToString ("##0.000") + ", " +
			averegeDefectorsPercentage.ToString ("##0.000") + ", " +
			defectorsPercentageDispersion.ToString ("##0.000") + ", " +
			defectorsPercentageError.ToString ("##0.000") + "}"
		);
	}

	//
	void Visualise () {
		mapTexture = MapTexture (map);
		if (defectorsRegions.Count <= 25 && false)
			regionsTextures = RegionsTextures (map, cooperatorsRegions);
	}

	void Interface () {
		GUI.TextField (new Rect (0, 20, 60, 20), "Size:" + mapSize.ToString());
	//	mapSize = (int)GUI.HorizontalScrollbar (new Rect (62, 22, 70, 20), (float)mapSize, 0, 1, 500);
		if (GUI.Button (new Rect (134, 20, 65, 20), "Respawn"))
			SetUpMap ();
		GUI.TextField (new Rect (0, 40, 55, 20), "%:" + defectorsSpawnPercentage.ToString("##.00"));
		defectorsSpawnPercentage = GUI.HorizontalScrollbar (new Rect (57, 42, 70, 20), defectorsSpawnPercentage, 0, 0.01f, 100);
		collectStatistic = GUI.Toggle (new Rect (0, 60, 125, 20), collectStatistic, "Collect Statistic");
		/*
		if ( GUI.Toggle (new Rect (0, 100, 70, 20), (b > 1.8)? true: false, (b > 1.8)? "b=1.74":"b=1.64") )
			b = 1.74;
		else
			b = 1.64;*/
	}

	int Perimeter () {
		int count = 0;
		foreach (Region r in defectorsRegions) {
			count += r.perimeter;
		}
		return count;
	}

	void Experiment1 () {
		for (mapSize = 10; mapSize < 101; mapSize += 10) {
			SetUpMap ();
			for (int i = 0; i < 99; i++) {
				MakeStep (false);
			}
			collectStatistic = true;
			for (int i = 0; i < 899; i++) {
				MakeStep (false);
			}
			MakeStep (false);
			SaveStatiscticToFile ();
		}
	}

	void Experiment2 (int s) {
		file = new StreamWriter ("C:/Users/Alez.M/Documents/University/SpatialGames/Tests/size_distribution.txt");

		int[] sizeMap = new int[s * s];

		sizeMap.Initialize ();

		mapSize = s;
		b = 1.81;
		SetUpMap ();
		for (int i = 0; i < 500; i++) {
			MakeStep (false);
		}
	
		for (int i = 0; i < 10000; i++) {
			MakeStep (false);
			foreach (Region r in cooperatorsRegions) {
				sizeMap [r.coords.Count]++;
			}
		}

		file.Write ("{ ");
		for (int i = 0; i < s*s; i++) {
			file.Write (sizeMap[i].ToString());
			if (i == s * s - 1) {
				file.Write ("};\n");
			} else {
				file.Write (", ");
			}
		}
		file.Close ();
		file = null;
	}

	void Experiment3 () {
		file = new StreamWriter ("C:/Users/Alez.M/Documents/University/SpatialGames/Tests/size_distribution.txt");

		int[] sizeMap = new int[50 * 50];

		sizeMap.Initialize ();

		mapSize = 50;
		b = 1.79;
		for (int k = 0; k < 1000; k++) {
			SetUpMap ();
			for (int i = 0; i < 100; i++) {
				MakeStep (false);
			}

			for (int i = 0; i < 100; i++) {
				defectorsPercentage = 100 * (double)GetCount(map, PlayerType.D) / (map.GetLength(0)*map.GetLength(0));
				allDefectorsPercentage += defectorsPercentage;
		//		averegeDefectorsPercentage = allDefectorsPercentage / (i + 1);

				MakeStep (false);
				foreach (Region r in cooperatorsRegions) {
					sizeMap [r.coords.Count]++;
				}
			}
		}
		averegeDefectorsPercentage = allDefectorsPercentage / (1000 * 100);

		file.WriteLine (averegeDefectorsPercentage.ToString());
		for (int i = 0; i < 50 * 50 / 2; i++) {
			file.Write (sizeMap [i].ToString () + ", ");
			if (i == 50 * 50 / 2 - 1) {
				file.Write ("};\n");
			}
		}

	//	file.Write ("};\n");

		file.Close ();
		file = null;
	}
		
	void OnDestroy () {
		if (file != null) {
			file.WriteLine("}");
			file.Close ();
		}
	}
}
