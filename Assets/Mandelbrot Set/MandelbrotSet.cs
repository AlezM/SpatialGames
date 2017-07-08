using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MandelbrotSet : MonoBehaviour {

	class ComplexNumber {
        public double x;
        public double y;
		public double r { get{ return (x * x + y * y); } }

		public ComplexNumber () {
			this.x = 0;
			this.y = 0;
		}
		public ComplexNumber (double x, double y) {
			this.x = x;
			this.y = y;
		}
		public static ComplexNumber operator+(ComplexNumber a, ComplexNumber b) {
			return new ComplexNumber (a.x + b.x, a.y + b.y);
		}
		public static ComplexNumber operator-(ComplexNumber a, ComplexNumber b) {
			return new ComplexNumber (a.x - b.x, a.y - b.y);
		}
		public static ComplexNumber operator*(ComplexNumber a, ComplexNumber b) {
			return new ComplexNumber (a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
		}
		public static ComplexNumber operator/(ComplexNumber a, ComplexNumber b) {
			return new ComplexNumber ( (a.x * b.x - a.y * b.y)/b.r, (a.x * b.y + a.y * b.x)/b.r);
		}
	}
		
	Texture2D texture;
    bool[,] myBitMap;


    public double Step = 0.1;
    public double scale = 3;
    public Vector2 center = Vector2.zero;
    public int maxRecursion = 25;

	void Start () {
		Calculation (Step);

    //  ClearInside();

    //  BoxCountingDemention(myBitMap, 1, 100, 5);
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.Return))
            Calculation(Step);
    }

	void OnGUI () {
		if (texture != null)
			GUI.DrawTexture (new Rect (0, 0, Screen.height, Screen.height), texture);
	}

	//[-2;1]x[-1;1]
	void Calculation (double step = 0.1) {
		ComplexNumber z = new ComplexNumber();
		int unitSize = System.Convert.ToInt32(1 / step);

		texture = new Texture2D (unitSize, unitSize);
		texture.filterMode = FilterMode.Point;

        myBitMap = new bool[unitSize, unitSize];

		for (double x = 0; x <= 1; x += step) {
			for (double y = 0; y <= 1; y += step) {
				int n;
				if ((n = f (z, new ComplexNumber (scale * (x - 0.5) + (double)center.x, scale * (y - 0.5) + (double)center.y), maxRecursion)) != -1) {
                    float c = 1.0f * n / maxRecursion;
                    c = ColorSystem(c);
					texture.SetPixel (
						System.Convert.ToInt32 (texture.width * x), System.Convert.ToInt32 (texture.height * y),
                        //	new Color ( 1 - Mathf.Pow(c - 1, 2), 1 - 16 * Mathf.Pow(c - 0.5f, 2), 1 - 16 * Mathf.Pow(c - 0.25f, 2) )
                        new Color(c, c, c)
                    );
				} 
				else {
					texture.SetPixel (
						System.Convert.ToInt32 (texture.width * x), System.Convert.ToInt32 (texture.height * y), 
						Color.black);
				}

                myBitMap[System.Convert.ToInt32(unitSize * x), System.Convert.ToInt32(unitSize * y)] = (n == -1);
            }
		}
		texture.Apply ();
	}

	int f(ComplexNumber z, ComplexNumber c, int n = 10) {
		for (int i = 0; i < n; i++) {
			z = z * z + c;
			if (z.r > 2)
				return i;
		}
		return -1;
	}

	int f2(ComplexNumber z, ComplexNumber c, int n = 10) {
		for (int i = 0; i < n; i++) {
			z = z * z * z * z * z * z * z * z + c;
			if (z.r > 2)
				return i;
		}
		return -1;
	}

    void ClearInside() {
        int size = myBitMap.GetLength(0);
        bool[,] cache = new bool[size, size];

        for (int i = 1; i < size - 1; i++)
        {
            for (int j = 1; j < size - 1; j++)
            {
                cache[i, j] = myBitMap[i, j];             
            }
        }

        for (int i = 1; i < size - 1; i++)
        {
            for (int j = 1; j < size - 1; j++)
            {
                if (cache[i, j])
                {
                    int k = -1;
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            if (cache[i + x, j + y])                             
                                k++;
                        }
                    }

                    if (k > 7)
                        myBitMap[i, j] = false;
                }
            }

        }
    }

    public void BoxCountingDemention(bool[,] bitMap, int startSize, int finishSize, int step)
    {
        string size = "{";
        string count = "{";

        for (int b = startSize; b <= finishSize; b += step)
        {
            // Filling Boxes
            int hCount = bitMap.GetLength(1) / b; //Hight
            int wCount = bitMap.GetLength(0) / b; //Width
            bool[,] filledBoxes =
                new bool[wCount + (bitMap.GetLength(0) > wCount * b ? 1 : 0), hCount + (bitMap.GetLength(1) > hCount * b ? 1 : 0)];

            for (int x = 0; x < bitMap.GetLength(0); x++)
            {
                for (int y = 0; y < bitMap.GetLength(1); y++)
                {
                    if (bitMap[x, y])
                    {
                        int xBox = x / b;
                        int yBox = y / b;
                        filledBoxes[xBox, yBox] = true;
                    }
                }
            }

            // Counting Boxes
            int a = 0;
            for (int i = 0; i < filledBoxes.GetLength(0); i++)
            {
                for (int j = 0; j < filledBoxes.GetLength(1); j++)
                {
                    if (filledBoxes[i, j])
                    {
                        a++;
                    }
                }
            }

            count += a.ToString() + ", ";
            size += b.ToString() + ", ";
        }

        Debug.Log("{" + size + "}, " + count + "}}");
    }

    float ColorSystem(float a) {
        for (int i = 0; i < 5; i++) {
            if (a <= 1 - Mathf.Pow(0.5f, i + 1))
                return a * Mathf.Pow(2, i + 1);
        }

        return 0;
    }
}
