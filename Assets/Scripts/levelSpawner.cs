using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class levelSpawner : MonoBehaviour
{
    public GameObject cube;
    int roomnumber = 10;
    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            foreach (GameObject o in GameObject.FindObjectsOfType<GameObject>())
            {
                if(o.transform.GetComponent<BoxCollider>() != null)
                {
                    Destroy(o);

                }
            }

        }
        if (Input.GetKeyUp(KeyCode.P))
        {
            RoomHolder rum = new RoomHolder();
            rum.cube = cube;
            rum.createSpawnRoom(1);
        }


    }

    public string returnCoordinate(int[] arr)
    {
        return "[" + string.Join(",", arr) + "]";

    }


    public class Room
    {
        public int[] topLeft; //topleft is [x,y]
        public int[] dimensions;
        public int exitNum;
        public ArrayList exitLocations;
        public ArrayList adjacentRooms;
        public int distancefromSpawn;


        public void spawnRoomFloor(GameObject cube)
        {
            for (int i = 0; i < dimensions[0]; i++)
            {
                for (int a = 0; a < dimensions[1]; a++)
                {
                    GameObject.Instantiate(cube, new Vector3(topLeft[0] - i, 0, topLeft[1] - a), Quaternion.Euler(0, 0, 0));
                }
            }
        }

        public void setRoom(int x, int y, int hori, int verti)
        {
            topLeft = new int[] { x, y };
            dimensions = new int[] { hori, verti };
            exitLocations = new ArrayList();
        }

        public void findExit()
        {
            int[] exit = new int[3];
            int vertical = UnityEngine.Random.Range(0, 4);


            switch (vertical)
            {
                case 0: //Vertical left
                    exit = new int[] { topLeft[0], UnityEngine.Random.Range(topLeft[1], topLeft[1] - dimensions[1] + 1), 2 };
                    break;
                case 1: // Vertical Right
                    exit = new int[] { topLeft[0] + dimensions[0] - 1, UnityEngine.Random.Range(topLeft[1], topLeft[1] - dimensions[1] + 1), 3 };

                    break;
                case 2: // Horizontal Up
                    exit = new int[] { UnityEngine.Random.Range(topLeft[0], topLeft[0] + dimensions[0] - 1), topLeft[1], 0 };

                    break;
                case 3: // Horizontal Down
                    exit = new int[] { UnityEngine.Random.Range(topLeft[0], topLeft[0] + dimensions[0] - 1), topLeft[1] - dimensions[1] + 1, 1 };


                    break;
            }
            //exit[1] = exit[1] % 2 == 0 ? exit[1] : exit[1] - 1;
            bool checkForDupes = false;
            foreach (int[] i in exitLocations)
            {
                if (Enumerable.SequenceEqual(i, exit))
                {
                    checkForDupes = true;
                    break;
                }
            }
            if (checkForDupes)
            {

                Debug.Log("Not work");
                findExit();
            }
            else
            {
                exitLocations.Add(exit);
                exitNum++;
            }

        }

        public void removeExit()
        {
            exitLocations.Remove(exitNum - 1);
            exitNum -= 1;
            
        }



    }

    public class RoomHolder
    {
        public GameObject cube;
        int scale = 1;
        int roomCount = 0;
        Room startingRoom;
        GameObject level = new GameObject();
        public void createSpawnRoom(int roomCount)
        {

            startingRoom = new Room();
            startingRoom.setRoom(0, 0, 10, 10);
            startingRoom.spawnRoomFloor(cube);
            startingRoom.findExit();
            int[] newExit = (int[])startingRoom.exitLocations[0];
            GameObject.Instantiate(cube, new Vector3((float)((int[])startingRoom.exitLocations[0])[0], 1, (float)((int[])startingRoom.exitLocations[0])[1]), Quaternion.Euler(0, 0, 0));
            
            int[] pathEnd = newRoomStart(newExit, 10, 10, newExit[2]);

            int newRoomFromPath = checkIfRoomIsValid(pathEnd);
            while(newRoomFromPath == -1)
            {
                startingRoom.removeExit();
                pathEnd = newRoomStart(newExit, 10, 10, newExit[2]);
                newRoomFromPath = checkIfRoomIsValid(pathEnd);
            }
            Debug.Log("valid");

            GameObject temp = GameObject.Instantiate(cube, new Vector3(pathEnd[0], 0, pathEnd[1]), Quaternion.Euler(0, 0, 0));
            temp.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
            createExit(newExit, pathEnd);
            Room newRoom = new Room();

            switch (newRoomFromPath)
            {
                case 0:
                    newRoom.setRoom(pathEnd[0], pathEnd[1], 10, 10);
                    break;
                case 1:
                    newRoom.setRoom(pathEnd[0] + 10 - 1, pathEnd[1], 10, 10);
                    break;
                case 2:
                    newRoom.setRoom(pathEnd[0] + 10 - 1, pathEnd[1] + 10 - 1, 10, 10);
                    break;
                case 3:
                    newRoom.setRoom(pathEnd[0] , pathEnd[1] + 10 - 1, 10, 10);
                    break;
            }
            Debug.Log("NEW ROOM TOP LEFT: " + string.Join(",", newRoom.topLeft));
            Debug.Log("PATH END: " + string.Join(",", pathEnd));
            newRoom.spawnRoomFloor(cube);
            





        }

        public int checkIfRoomIsValid(int[] startingPoint)
        {
            int corner = UnityEngine.Random.Range(0, 4);
            //int[] dimensions = new int[] { UnityEngine.Random.Range(5, 20), UnityEngine.Random.Range(5, 20) };
            int[] dimensions = new int[] { 10, 10 };
            int[] count = new int[] { 0, 0, 0, 0 };
            System.Random random = new System.Random(); 
            int[] orientation = Enumerable.Range(0, 3).OrderBy(c => random.Next()).ToArray(); //0 Top Left, 1 Top Right, 2 Bottom Left, 3 Bottom Right
            for(int i = 0; i < orientation.Length; i++)
            {
                bool works = true;
                switch (orientation[i])
                {
                    case 0: // Topleft
                        works = Physics.CheckBox(new Vector3(startingPoint[0] - dimensions[0] / 2 + 1, 0, startingPoint[1] - dimensions[1] / 2 + 1), new Vector3(dimensions[0] / 2, 1, dimensions[1] / 2), Quaternion.Euler(0,0,0));
                        if (!works)
                        {
                            Debug.Log("0 [" + (startingPoint[0] - dimensions[0] / 2 + 1) + "," + (startingPoint[1] - dimensions[1] / 2 + 1) + "]");
                            GameObject temp = GameObject.Instantiate(cube, new Vector3(startingPoint[0] - dimensions[0] / 2 + 1, 0, startingPoint[1] - dimensions[1] / 2 + 1), Quaternion.Euler(0, 0, 0));
                            temp.GetComponent<Renderer>().material.color = new Color(255, 255, 0);
                            return 0;
                        }
                         break;
                    case 1: //Top Right
                        works = Physics.CheckBox(new Vector3(startingPoint[0] - dimensions[0] / 2 + 1, 0, startingPoint[1] + dimensions[1] / 2 - 1), new Vector3(dimensions[0] / 2, 1, dimensions[1] / 2), Quaternion.Euler(0, 0, 0));
                        if (!works)
                        {
                            Debug.Log("1 | " + (startingPoint[0] - dimensions[0] / 2 + 1) + "|" + (startingPoint[1] + dimensions[1] / 2 - 1));
                            GameObject temp = GameObject.Instantiate(cube, new Vector3(startingPoint[0] - dimensions[0] / 2 + 1, 0, startingPoint[1] + dimensions[1] / 2 - 1), Quaternion.Euler(0, 0, 0));
                            temp.GetComponent<Renderer>().material.color = new Color(255, 255, 0);
                            return 1;
                        }
                        break;
                    case 2: //Bottom Left
                        works = Physics.CheckBox(new Vector3(startingPoint[0] + dimensions[0] / 2 - 1, 0, startingPoint[1] + dimensions[1] / 2 - 1), new Vector3(dimensions[0] / 2, 1, dimensions[1] / 2), Quaternion.Euler(0, 0, 0));
                        if (!works)
                        {
                            Debug.Log("2 | " + (startingPoint[0] + dimensions[0] / 2 - 1) + "|" + (startingPoint[1] + dimensions[1] / 2 - 1));
                            GameObject temp = GameObject.Instantiate(cube, new Vector3(startingPoint[0] + dimensions[0] / 2 - 1, 0, startingPoint[1] + dimensions[1] / 2 - 1), Quaternion.Euler(0, 0, 0));
                            temp.GetComponent<Renderer>().material.color = new Color(255, 255, 0);
                            return 2;
                        }
                        break;
                    case 3:
                        works = Physics.CheckBox(new Vector3(startingPoint[0] + dimensions[0] / 2 - 1, 0, startingPoint[1] - dimensions[1] / 2 + 1), new Vector3(dimensions[0] / 2, 1, dimensions[1] / 2), Quaternion.Euler(0, 0, 0));
                        if (!works)
                        {                          
                            Debug.Log("3 | " + (startingPoint[0] + dimensions[0] / 2 - 1) + "|" + (startingPoint[1] - dimensions[1] / 2 + 1));
                            GameObject temp = GameObject.Instantiate(cube, new Vector3(startingPoint[0] + dimensions[0] / 2 - 1, 0, startingPoint[1] - dimensions[1] / 2 + 1), Quaternion.Euler(0, 0, 0));
                            temp.GetComponent<Renderer>().material.color = new Color(255, 255, 0);
                            return 3;
                        }
                        break;

                }
            }
            return -1;
            
        }

        public int[] newRoomStart(int[] newRoom, int width, int height, int position)
        {
            if (position < 2) //If the exit point and the top right share the same x value
            {
                int direction = position == 0 ? 1 : -1;
                return new int[] { UnityEngine.Random.Range(newRoom[0] - width / 2, newRoom[0] + width / 2 + 1), UnityEngine.Random.Range(newRoom[1] + 2 * direction, newRoom[1] + height * direction - 1 * direction) };
            }
            else
            {
                int direction = position == 2 ? -1 : 1;
                return new int[] { UnityEngine.Random.Range(newRoom[0] + 2 * direction, newRoom[0] + height * direction - 1 * direction), UnityEngine.Random.Range(newRoom[1] - width / 2, newRoom[1] + width / 2 + 1) };

            }
        }

        void createExit(int[] newExit,int[] pathEnd)
        {
            if (newExit[2] <= 1)
            {
                int direction = newExit[2] == 0 ? 1 : -1;
                int horiDirection = newExit[0] < pathEnd[0] ? 1 : -1;
                for (int i = 0; i < System.Math.Abs(newExit[1] - pathEnd[1]); i++)
                {
                    GameObject.Instantiate(cube, new Vector3(newExit[0], 0, newExit[1] + i * direction), Quaternion.Euler(0, 0, 0));
                }
                for (int i = 0; i < System.Math.Abs(newExit[0] - pathEnd[0]); i++)
                {
                    GameObject.Instantiate(cube, new Vector3(newExit[0] + i * horiDirection, 0, pathEnd[1]), Quaternion.Euler(0, 0, 0));
                }
            }
            else
            {
                int direction = newExit[2] == 2 ? -1 : 1;
                int horiDirection = newExit[1] < pathEnd[1] ? 1 : -1;
                for (int i = 0; i < System.Math.Abs(newExit[0] - pathEnd[0]); i++)
                {
                    GameObject.Instantiate(cube, new Vector3(newExit[0] + i * direction, 0, newExit[1]), Quaternion.Euler(0, 0, 0));
                }
                for (int i = 0; i < System.Math.Abs(newExit[1] - pathEnd[1]); i++)
                {
                    GameObject.Instantiate(cube, new Vector3(pathEnd[0], 0, newExit[1] + i * horiDirection), Quaternion.Euler(0, 0, 0));
                }
            }
        }




    }
}



