using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
Вычисление размерности Миньковского
1) Генерируем карту, делаем несколько итераций для установления какой-то границы
2) Формируем на её основе картуграницы, гораздо мельце чем сама карта
3) Запускаем алгоритм "Box-counting".
*/
public class MinkovskiMethod : Source {

    Tile[,] map;

    public int mapSize;
    public double b = 1.79;

    public float defectorsSpawnPercentage = 21;
    public string seed;
    public bool useRandomSeed = true;

    [Space]
    public int borderMapSize = 4;

    //Textures
    Texture2D mapTexture;
    Texture2D borderTexture;

    //Border
    bool[,] borderMap;

    void Start() {
        SetUpMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            SetUpMap();

        if (Input.GetKey(KeyCode.Space))
        {
            MakeStep();            
        }

        if (Input.GetKeyDown(KeyCode.RightAlt))
        {
            borderMap = BorderMap(map, borderMapSize);
            borderTexture = BorderTexture(borderMap);
        }

        if (Input.GetKeyDown(KeyCode.S) && (borderTexture != null))
        {
            byte[] bytes = borderTexture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);
            Debug.Log("Saved");
        }
    }

    void OnGUI() {
        if (mapTexture != null)
            GUI.DrawTexture(new Rect(0, 0, Mathf.Min(Screen.width / 2, Screen.height), Mathf.Min(Screen.width / 2, Screen.height)), mapTexture);
        if (borderTexture != null)
        {
            //GUI.DrawTexture(new Rect(0, 0, Mathf.Min(Screen.width / 2, Screen.height), Mathf.Min(Screen.width / 2, Screen.height)), borderTexture);
            GUI.DrawTexture(new Rect(Screen.width / 2, 0, Mathf.Min(Screen.width / 2, Screen.height), Mathf.Min(Screen.width / 2, Screen.height)), borderTexture);
        }
    }

    void SetUpMap()
    {
        map = new Tile[mapSize, mapSize];

        if (useRandomSeed)
        {
            seed = UnityEngine.Random.value.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                map[i, j] = (pseudoRandom.Next(0, 100) < defectorsSpawnPercentage) ? new Tile(PlayerType.D) : new Tile(PlayerType.C);
            }
        }

        mapTexture = MapTexture(map);
    }

    void MakeStep()
    {
        //Main Calucs
        CalculateAllScores(map, b);
        NextGeneration(map);
        ResetScores(map);

        mapTexture = MapTexture(map);
    }


}
