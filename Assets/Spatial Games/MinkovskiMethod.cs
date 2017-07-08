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
    public Texture2D inputTex;

    //Border
    bool[,] borderMap;

    void Start() {
        SetUpMap();
    }

    void Update()
    {
        Tests();

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

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            borderMap = BorderMap(map, borderMapSize);
            borderTexture = BorderTexture(borderMap);
            BoxCountingDemention(borderMap, 1, 100, 1);
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

    void Tests()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    map[i, j] = (i == mapSize / 2) ? new Tile(PlayerType.C) : new Tile(PlayerType.D);
                }
            }           
            mapTexture = MapTexture(map);
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    map[i, j] = (i < mapSize / 2) ? new Tile(PlayerType.C) : new Tile(PlayerType.D);
                }
            }
            mapTexture = MapTexture(map);
        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    map[i, j] = (i <= 0.75 * mapSize && i >= 0.25 * mapSize && j <= 0.75 * mapSize && j >= 0.25 * mapSize) ? new Tile(PlayerType.C) : new Tile(PlayerType.D);
                }
            }
            mapTexture = MapTexture(map);
        }

        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    map[i, j] = (i == mapSize/2 && j == mapSize/2) ? new Tile(PlayerType.D) : new Tile(PlayerType.C);
                }
            }
            mapTexture = MapTexture(map);
        }

        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            borderMap = TextureToBitMap(inputTex);
            borderTexture = inputTex;
            BoxCountingDemention(borderMap, 20, 200, 1);
        }
    }

    bool [,] TextureToBitMap (Texture2D tex)
    {
        if (tex == null)
            return null;

        int size = tex.width;
        bool[,] bitMap = new bool[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                bitMap[i, j] = (tex.GetPixel(i, j) == Color.black);
            }
        }

        return bitMap;
    }
}
