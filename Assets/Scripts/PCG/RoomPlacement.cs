using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

//public class RoomPlacement : MonoBehaviour
//{
//    // Start is called before the first frame update
//    public enum RoomType
//    {
//        Engineering,
//        Medical,
//        Central,
//        Engine,
//        Default
//    };

//    public class Room
//    {
//        public RoomType type_;
//        public Vector3 centerPos_;
//        public Room[] connections_ = new Room[1];
//    }

//    public class Hall
//    {
//        public Vector3 pointA;
//        public Vector3 pointB; 
//    }

//    List<Room> rooms_ = new List<Room>();
//    List<Hall> halls_ = new List<Hall>();
//    public int maxHalls = 5;
//    public int maxRooms = 6;


//    void Start()
//    {
//        for (int i = 0; i < 6; i++)
//        {
//            rooms_.Add(new Room());

//            rooms_[i].centerPos_ = new Vector3(Random.Range(0.0f, 10), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 10));
//        }
//        CheckCollisions();

//        ConnectLines();
//    }

//    private void ConnectLines()
//    {

//        for (int i = 0; i < maxHalls; i++)
//        {
//            Room currentRoom = rooms_[i];

//            //float shortestDistance = 100.0f;
//            //for (int j = 0; j < maxRooms; j++)
//            //{
//            //    float d = Vector3.Distance(currentRoom.centerPos_, rooms_[j].centerPos_);
//            //    if (shortestDistance > d)
//            //    {
//            //        shortestDistance = d;
//            //        currentRoom.connections_[0] = rooms_[j];
//            //    }
//            //}
//            halls_.Add(new Hall());
//            halls_[i].pointA = rooms_[i].centerPos_;
//            halls_[i].pointB = rooms_[i + 1].centerPos_;
//        }
//    }

//    private void CheckCollisions()
//    {
//        for (int i = 0; i < maxRooms; i++)
//        {
//            for (int j = 0; j < maxRooms; j++)
//            {
//                if (Vector3.Distance(rooms_[i].centerPos_, rooms_[j].centerPos_) < 0.001f)
//                {
//                    rooms_[i].centerPos_ = Vector3.Cross(rooms_[i].centerPos_, rooms_[j].centerPos_);
//                }
//            }
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }

//    void OnDrawGizmos()
//    {
//        Gizmos.color = Color.black;

//        foreach (Room room in rooms_)
//        {
//            Gizmos.DrawSphere(room.centerPos_, 0.25f);
//        }

//        foreach (Hall hall in halls_)
//        {
//            Gizmos.DrawLine(hall.pointA, hall.pointB);
//        }

//    }
//}
