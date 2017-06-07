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

	public double Step = 0.1;
	public int maxRecursion = 25;

	void Start () {
		Calculation (Step);
	}

	void Update () {
		
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


		for (double x = 0; x <= 1; x += step) {
			for (double y = 0; y <= 1; y += step) {
				float n;
				if ((n = f (z, new ComplexNumber (x - 0.5, y + 0.3), maxRecursion)) != -1) {
					float c = n / maxRecursion;
					texture.SetPixel (
						System.Convert.ToInt32 (texture.width * x), System.Convert.ToInt32 (texture.height * y), 
						new Color ( 1 - Mathf.Pow(c - 1, 2), 
									1 - 16 * Mathf.Pow(c - 0.5f, 2),
									1 - 16 * Mathf.Pow(c - 0.25f, 2) )
					);
				} 
				else {
					texture.SetPixel (
						System.Convert.ToInt32 (texture.width * x), System.Convert.ToInt32 (texture.height * y), 
						Color.white);
				}
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
}
