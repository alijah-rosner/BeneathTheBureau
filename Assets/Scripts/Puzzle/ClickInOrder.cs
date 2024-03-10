using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ClickInOrder : MonoBehaviour
{
    // Start is called before the first frame update
    private int clickTracker_ = 0;
    private GameObject[] array_;
    private bool isPlayersTurn = false;
    private int firstRandom;
    private int correct_ = 0;

    // timer
    float dt = 0;
    int tracker = 0;

    public GameObject type_;

    void Start()
    {
        array_ = new GameObject[6];
        int i = 0;

        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                array_[i] =  Instantiate(type_, new Vector3(x, y, 1.0f), Quaternion.identity);
                Clickable click = array_[i].GetComponentInChildren<Clickable>();
                array_[i].GetComponentInChildren<Renderer>().material.color = Color.white;

                click.SetParent(this);
                click.SetPosition(i);
                i++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        dt += Time.deltaTime;

        if (!isPlayersTurn)
        {
            if (dt > 0.5)
            {
                tracker++;
                dt = 0;
            }
            if (tracker > 2 && !isPlayersTurn)
            {
                tracker = 0;
                array_[firstRandom].GetComponentInChildren<Renderer>().material.color = Color.white;
                GenerateClickPattern();
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            Reset();
        }
    }

    private void GenerateClickPattern()
    {
        firstRandom = Random.Range(0, 6);

        if (clickTracker_ < 6)
        {
            while (array_[firstRandom].GetComponentInChildren<Clickable>().GetComputerClick() != 0)
            {
                firstRandom = Random.Range(0, 6);
            }
        }


        switch (clickTracker_)
        {
            case 0:
                array_[firstRandom].GetComponentInChildren<Renderer>().material.color = Color.yellow;
                array_[firstRandom].GetComponentInChildren<Clickable>().SetComputerClick(++clickTracker_);
                break;
            case 1:
                array_[firstRandom].GetComponentInChildren<Renderer>().material.color = Color.green;
                array_[firstRandom].GetComponentInChildren<Clickable>().SetComputerClick(++clickTracker_);
                break;
            case 2:
                array_[firstRandom].GetComponentInChildren<Renderer>().material.color = Color.blue;
                array_[firstRandom].GetComponentInChildren<Clickable>().SetComputerClick(++clickTracker_);
                break;

            //case 3:
            //    clickTracker_ = 0;
            //    isPlayersTurn = true;
            //    isComputersTurn = false;
            //    break;
            case 3:
                array_[firstRandom].GetComponentInChildren<Renderer>().material.color = Color.cyan;
                array_[firstRandom].GetComponentInChildren<Clickable>().SetComputerClick(++clickTracker_);
                break;


            case 4:
                array_[firstRandom].GetComponentInChildren<Renderer>().material.color = Color.red;
                array_[firstRandom].GetComponentInChildren<Clickable>().SetComputerClick(++clickTracker_);
                break;
            //case 5:
            //    clickTracker_ = 0;
            //    isPlayersTurn = true;
            //    break;
            case 5:
                array_[firstRandom].GetComponentInChildren<Renderer>().material.color = Color.magenta;
                array_[firstRandom].GetComponentInChildren<Clickable>().SetComputerClick(++clickTracker_);
                break;
            case 6:
                clickTracker_ = 0;
                isPlayersTurn = true;
                break;
        }

    }

    public void CheckPlayerClick(Clickable obj)
    {
        int pos = obj.GetPosition();
        Debug.Log("computer click" + obj.GetComputerClick());

        if (obj.GetPlayerClick() == obj.GetComputerClick())
        {
            Debug.Log("Yay!");
            correct_++;
        }
        else
        {
            Debug.Log("You lose!");
            Reset();
        }

        if (correct_ == 6)
        {
            Debug.Log("You win!");
        }

    }

    public bool IsPlayersTurn()
    {
        return isPlayersTurn;
    }

    private void Reset()
    {
        for (int i = 0; i < array_.Length; i++)
        {
            Clickable clickObj = array_[i].GetComponentInChildren<Clickable>();
            clickObj.Reset();
        }
        clickTracker_ = 0;
        correct_ = 0;
        isPlayersTurn = false;
    }
}
