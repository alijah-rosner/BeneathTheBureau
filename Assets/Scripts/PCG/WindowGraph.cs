using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
public class WindowGraph : MonoBehaviour
{
    // Start is called before the first frame update
    private RectTransform graphContainer_;
    [SerializeField] private Sprite circleSprite;

    private void Awake()
    {
        graphContainer_ = transform.Find("GraphContainer").GetComponent<RectTransform>();

        //CreateCircle(new Vector3(200, 200, -1));
        List<int> valueList = new List<int>() { 5, 98, 2, 54, 34, 72, 100, 94, 1 };
        ShowGraph(valueList);
    }

    private GameObject CreateCircle(Vector3 anchoredPos)
    {
        GameObject obj = new GameObject("circle", typeof(Image));
        obj.transform.SetParent(graphContainer_);
        obj.GetComponent<Image>().sprite = circleSprite;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;





        rect.sizeDelta = new Vector3(11, 11, -1);
        rect.anchorMin = new Vector3(0, 0, -1);
        rect.anchorMax = new Vector3(0, 0, -1);
        Vector3 local = rect.localPosition;
        local.z = 0.0f;
        rect.localPosition = local;

        return obj;
    }

    private void ShowGraph(List<int> valueList)
    {
        float yMaximum = 100f;
        float graphHeight = graphContainer_.sizeDelta.y;
        float xSize = 50f;

        GameObject prevCircle = null;

        for (int i = 0; i < valueList.Count; i++)
        {
            float xPos = i * xSize;
            float yPos = (valueList[i] / yMaximum) * graphHeight;
            GameObject circle = CreateCircle(new Vector2(xPos, yPos));
            if (prevCircle != null)
            {
                CreateConnection(prevCircle.GetComponent<RectTransform>().anchoredPosition, circle.GetComponent<RectTransform>().anchoredPosition);
            }
            prevCircle = circle;
        }
    }

    private void CreateConnection(Vector3 dotPosA, Vector3 dotPosB)
    {
        GameObject obj = new GameObject("dotConnection", typeof(Image));
        obj.transform.SetParent(graphContainer_, false);
        obj.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        RectTransform rect = obj.GetComponent<RectTransform>();

        Vector3 dir = (dotPosB - dotPosA).normalized;
        float distance = Vector3.Distance(dotPosA, dotPosB);

        rect.anchorMin = new Vector3(0, 0, 0);
        rect.anchorMax = new Vector3(0, 0, 0);
        rect.sizeDelta = new Vector3(distance, 3f, 0);
        rect.anchoredPosition = dotPosA + dir * distance * 0.5f;

        float angle = Vector3.Angle(dotPosA, dotPosB);
        rect.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVector(dir));
    }
}
