using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clickable : MonoBehaviour
{
    // Start is called before the first frame update
    private ClickInOrder parent_ = null;
    private int position_ = 0; 
    private int whenComputerClick = 0;


    float timer = 0;
    int tracker = 0;


    public bool clicked = false;
    public static int playerClick = 0;
    
    void Start()
    {
        
    }
    public void SetComputerClick(int click)
    {
        whenComputerClick = click;
    }
    public void SetPosition(int position)
    {
        position_ = position; 
    }

    public void SetParent(ClickInOrder parent)
    {
        parent_ = parent; 
    }

    public int GetComputerClick()
    {
        return whenComputerClick;
    }
    public int GetPlayerClick()
    {
        Debug.Log("click count" + playerClick);
        return playerClick;
    }
    public int GetPosition()
    {
        return position_;
    }

    // Update is called once per frame
    void Update()
    {
        if (parent_.IsPlayersTurn())
        {
            if (clicked)
            {
                timer += Time.deltaTime;

                if (timer > 0.5)
                {
                    tracker++;
                    timer = 0.0f; 
                }

                if (tracker > 2)
                {
                    GetComponent<Renderer>().material.color = Color.white;
                    tracker = 0;
                    //clicked = false; 
                }
            }
        }
    }

    private void OnMouseDown()
    {
        if (parent_.IsPlayersTurn())
        {
            Debug.Log("working");
        
            GetComponent<Renderer>().material.color = Color.black;
            clicked = true;
            Debug.Log("position: " + position_);
            playerClick++;
            parent_.CheckPlayerClick(this);

        }
    }

    public void Reset()
    {
        whenComputerClick = 0;
        playerClick = 0;
        clicked = false;
        position_ = 0;
        GetComponent<Renderer>().material.color = Color.white;
    }

}
