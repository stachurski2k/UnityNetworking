using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] Cell cellPrefab;
    [SerializeField] Text textPrefab;
    Canvas canvas;
    HexMesh hexMesh;
    [ContextMenu("Generate Map")]
    public void GenerateHexagonMap(){
        Debug.Log("generating");
    }
    Cell[] cells;

	void Awake () {
        canvas=GetComponentInChildren<Canvas>();
        hexMesh=GetComponentInChildren<HexMesh>();
		cells = new Cell[height * width];

		for (int z = 0, i = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				CreateCell(x, z, i++);
			}
		}
	}
    private void Start()
    {
        hexMesh.Triangulate(cells);
    }
	
	void CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x = (x+z*.5f-z/2) * HexMetrics.innerRadius*2f;
		position.y = 0f;
		position.z = z * HexMetrics.innerRadius*1.5f;

		Cell cell = cells[i] = Instantiate<Cell>(cellPrefab);
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;

        Text label = Instantiate<Text>(textPrefab);
		label.rectTransform.SetParent(canvas.transform, false);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = x.ToString() + "\n" + z.ToString();
	}
}
