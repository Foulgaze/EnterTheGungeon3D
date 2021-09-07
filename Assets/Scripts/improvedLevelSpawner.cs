//Abandon hope all ye who enter here
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Random = UnityEngine.Random;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;
using MiscUtil.Xml.Linq.Extensions;
using TMPro.EditorUtilities;
//2.3 seconds to traverse a 24 block wide room
public class improvedLevelSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject cube;
    public GameObject wallCube;
    public GameObject pathCube;
    public GameObject player;
    public GameObject floorCube;
    public GameObject desinationCube;
    ArrayList createdRooms = new ArrayList();
    HashSet<Vector2Int> roomVecs = new HashSet<Vector2Int>();
    int roomProperlyCreated = 0;
    public int roomNumber = 1;
    int roomDirection = 0;
    HashSet<RectInt> walls = new HashSet<RectInt>();
    UnityEngine.Object[] textures;
    UnityEngine.Object[] floorBlocks;
    UnityEngine.Object[] wallBlocks;
    UnityEngine.Object[] floorObjects;
    public GameObject normalPillar;
    public GameObject libraryPillar;
    public Texture2D libraryFloor;

    


    void Start()
    {
        loadFloorBlocks();
        loadWallBlocks();
        loadFloorObjects();

    }

    void loadFloorBlocks()
    {
        floorBlocks = Resources.LoadAll("floor", typeof(GameObject));
    }
    void loadWallBlocks()
    {
        wallBlocks = Resources.LoadAll("walls", typeof(GameObject));
    }

    void loadFloorObjects()
    {
        floorObjects = Resources.LoadAll("floorObjects", typeof(GameObject));

    }


    // Update is called once per frame
    void Update()
    {
        
        
        if (roomProperlyCreated == 2)
        {
            roomProperlyCreated = 3;
        }
        if (roomProperlyCreated == 1)
        {
            if (!transform.name.Equals("LevelSpawner"))
            {
                roomProperlyCreated = -2;
                roomDirection = 0;
            }
            else
            {
                roomProperlyCreated = -1;
                Debug.Log("Room Successfully Created!");
                for(int i =0; i < createdRooms.Count; i++)
                {
                    ((Room) createdRooms[i]).makeWallsNotTrigger();
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
          
               roomProperlyCreated = 1;
               spawnRooms(roomNumber);
        }
        if (roomProperlyCreated == 3)
        {
            foreach (GameObject o in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if (o.name != transform.name && o.name != "Camera" && o.name != "Directional Light")
                {
                    Destroy(o);
                    createdRooms.Clear();
                    roomVecs.Clear();
                }
            }
            transform.name = "LevelSpawner";
            spawnRooms(roomNumber);
            roomProperlyCreated = 1;
        }
        if (Input.GetKeyUp(KeyCode.N))
        {
            for (int i = 0; i < createdRooms.Count; i++)
            {

                /* ((Room)createdRooms[i]).spawnFloor((GameObject)floorBlocks[0], (GameObject)floorBlocks[1], (GameObject)floorBlocks[2], (GameObject)floorBlocks[3], (GameObject)floorBlocks[4], (GameObject) floorBlocks[5]);
                 ((Room)createdRooms[i]).spawnAesteticWalls((GameObject)wallBlocks[0], (GameObject)wallBlocks[1]);
                 ((Room)createdRooms[i]).combineME();*/
                if (((Room)createdRooms[i]).roomNum == 1)
                {
                    Debug.Log("Library");
                }
                ((Room)createdRooms[i]).spawnFloor(floorBlocks,wallBlocks, normalPillar, libraryPillar, floorObjects) ;
                ((Room)createdRooms[i]).combineME();

            }
            
        }
        if (Input.GetKeyUp(KeyCode.P) || roomProperlyCreated == -1)
        {
            Room randomRoom = (Room)createdRooms[UnityEngine.Random.Range(0, createdRooms.Count - 1)];

            GameObject.Instantiate(player, new Vector3(randomRoom.topLeft.x + 1, 10, randomRoom.topLeft.y - 1 ), Quaternion.Euler(0, 0, 0));
            roomProperlyCreated = -2;
        }
        if (Input.GetKeyUp(KeyCode.M))
        {
            foreach (GameObject o in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if(o.name != "LevelSpawner" && o.name != "Camera" && o.name != "Directional Light")
                {
                    Destroy(o);
                    createdRooms.Clear();
                    roomVecs.Clear();
                }
            }
            roomDirection = 0;
            Debug.ClearDeveloperConsole();
        }

    }
    bool spawnNewRoom(int num, Room oldRoom, ArrayList arr, Vector2Int dimensions)
    {
        num++;
        int roomSide = UnityEngine.Random.Range(0, 4);
        Vector2Int blockLocation = (Vector2Int) arr[2];
        Vector2Int topLeft = new Vector2Int(-1, -1);
        int distanceLeft = 0;
        int distanceUp = 0;
         
        switch (roomSide)
        {
            case 0: //Block is on the top
                distanceLeft = UnityEngine.Random.Range(2, dimensions.x-2);
                topLeft = new Vector2Int(blockLocation.x - distanceLeft, blockLocation.y);
                break;
            case 1: // Block is on the Bottom
                distanceLeft = UnityEngine.Random.Range(2, dimensions.x-2);
                topLeft = new Vector2Int(blockLocation.x - distanceLeft, blockLocation.y + dimensions.y - 1);
                break;
            case 2: // Block is on the Left
                distanceUp = UnityEngine.Random.Range(2, dimensions.y-2);
                topLeft = new Vector2Int(blockLocation.x, blockLocation.y + distanceUp);
                break;
            case 3:
                distanceUp = UnityEngine.Random.Range(2, dimensions.y-2);
                topLeft = new Vector2Int(blockLocation.x - dimensions.x + 1, blockLocation.y + distanceUp);
                break;
        }
        Room r = new Room(num, topLeft, dimensions, transform.gameObject, cube,wallCube, pathCube,desinationCube,floorCube,createdRooms, roomVecs, textures, (GameObject) wallBlocks[0], floorBlocks, wallBlocks, libraryFloor);
        r.originBlock = (Vector2Int)arr[1];
        r.originConnectionBlock = (Vector2Int)arr[2];
        r.originEdge = (int) arr[0];
        r.originBlock2 = (Vector2Int)arr[3];
        r.roomNum = Random.value > 0.5 ? 0 : 1;
       
        Vector2Int randomInt = Vector2Int.zero;
        switch (roomSide)
        {
            case 0:
                randomInt = Random.value < 0.5 ? Vector2Int.left : Vector2Int.right;
                r.originConnectionBlock2 = r.originConnectionBlock + randomInt;
                break;
            case 1:
                randomInt = Random.value < 0.5 ? Vector2Int.left : Vector2Int.right;
                r.originConnectionBlock2 = r.originConnectionBlock + randomInt;
                break;
            case 2:
                randomInt = Random.value < 0.5 ? Vector2Int.up : Vector2Int.down;
                r.originConnectionBlock2 = r.originConnectionBlock + randomInt;
                break;
            case 3:
                randomInt = Random.value < 0.5 ? Vector2Int.up : Vector2Int.down;
                r.originConnectionBlock2 = r.originConnectionBlock + randomInt;
                break;
        }

        
        HashSet<Vector2Int> tempArr = r.checkPathToOrigin(roomVecs, createdRooms);
        if (!r.isValid || tempArr.Count == 0)
        {
            GameObject.Destroy(r.roomParent);
            oldRoom.deleteExits();
            return false;
        }
        roomVecs.UnionWith(tempArr);
        r.spawnContainedRoom();
        r.createPathToOrigin();
        createdRooms.Add(r);
        return true;
    }

    void spawnRooms(int roomNum)
    {
        Room r = new Room(0, new Vector2Int(0, 0), new Vector2Int(20, 20), transform.gameObject, cube, wallCube, pathCube, desinationCube, floorCube, createdRooms, roomVecs, textures, (GameObject) wallBlocks[0], floorBlocks, wallBlocks, libraryFloor);
        r.originConnectionBlock = new Vector2Int(r.topLeft.x + (r.dimensions.x / 2), r.topLeft.y - (r.dimensions.y / 2));
        r.originConnectionBlock2 = new Vector2Int(r.topLeft.x + (r.dimensions.x / 2), r.topLeft.y - (r.dimensions.y / 2));
        r.originBlock = new Vector2Int(r.topLeft.x + (r.dimensions.x / 2), r.topLeft.y - (r.dimensions.y / 2));
        r.originBlock2 = new Vector2Int(r.topLeft.x + (r.dimensions.x / 2), r.topLeft.y - (r.dimensions.y / 2));
        r.spawnContainedRoom();
        createdRooms.Add(r);
        bool newRoomCreated = false;
        
        for (int i = 0; i < roomNum; i++)
        {
            //Create New Rooms
            while (!newRoomCreated)
            {
                // cube.transform.GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV();
                Room randomRoom = (Room)createdRooms[UnityEngine.Random.Range(roomDirection, createdRooms.Count - 1)];
                newRoomCreated = spawnNewRoom(i, randomRoom, randomRoom.SpawnExit(new Vector2Int(UnityEngine.Random.Range(10, 15), UnityEngine.Random.Range(10, 15))), new Vector2Int(UnityEngine.Random.Range(15, 80), UnityEngine.Random.Range(8, 80)));
            }
            roomDirection = 1;

            newRoomCreated = false;
        }


        for (int i = 0; i < createdRooms.Count; i++)
        {
            ((Room)createdRooms[i]).addWalls();
        }
    }
  
}

public class Room {
    public Vector2Int topLeft; // x ,z
    public Vector2Int bottomRight;
    public Vector2Int dimensions; // x, z
    public Vector2Int originBlock;
    public Vector2Int originBlock2;
    public Vector2Int originConnectionBlock;
    public Vector2Int originConnectionBlock2;
    GameObject realFloorCube;
    public int originEdge = -1;
    public ArrayList exitLocations = new ArrayList();
    public ArrayList exitLocations2 = new ArrayList();
    public ArrayList exitVectors = new ArrayList();
    public GameObject roomParent = new GameObject();
    public HashSet<RectInt> pathWalls = new HashSet<RectInt>();
    HashSet<Vector2Int> noSpawnZone = new HashSet<Vector2Int>();
    GameObject floorParent = new GameObject();
    GameObject pathParent = new GameObject();
    GameObject wallParent = new GameObject();
    GameObject decorationParent = new GameObject();
    GameObject floorCube;
    GameObject pathCube;
    GameObject wallCube;
    GameObject visibleWallCube;
    GameObject desinationCube;
    UnityEngine.Object[] textures;
    ArrayList gameObjectParents = new ArrayList();
    HashSet<Vector2Int> placedWalls = new HashSet<Vector2Int>();
    public int roomNum = 0;
    int roomType;
    public bool isValid = false;
    int roomNumber;
    int[][] floorPlan;
    Object[] floorParentList;
    Object[] wallParentList;
    int[][] floorPlanRotation;
    Texture2D libraryFloorTexture;
    public Room(int roomNum, Vector2Int v, Vector2Int dim, GameObject obj, GameObject fc, GameObject wc, GameObject pc, GameObject dc, GameObject rc, ArrayList createdRooms, HashSet<Vector2Int> vecs, UnityEngine.Object[] tex, GameObject vwc, Object[] floorTiles, Object[] wallTiles, Texture2D libraryFloor)
    {

        topLeft = v;
        dimensions = dim;
        bottomRight = new Vector2Int(v.x + dim.x - 1, v.y - dim.y + 1);
        if (createdRooms.Count == 0)
        {
            isValid = true;
        }
        for (int i = 0; i < createdRooms.Count; i++)
        {
            if (!doOverlap(v, bottomRight, ((Room)createdRooms[i]).topLeft, ((Room)createdRooms[i]).bottomRight)) {
                isValid = true;
            }
            else
            {
                isValid = false;
                break;
            }
        }
        foreach (Vector2Int vec in vecs)
        {
            if (specialIsIn(vec))
            {
                isValid = false;
                break;
            }
        }
        wallCube = wc;
        pathCube = pc;
        realFloorCube = rc;
        desinationCube = dc;
        roomParent.transform.parent = obj.transform;
        floorCube = fc;
        roomParent.name = "Room #" + roomNum;
        floorParent.name = roomParent.name + "|Floor";
        wallParent.name = roomParent.name + "|Wall";
        pathParent.name = roomParent.name + "|Path";
        decorationParent.name = roomParent.name + "|Decoration";
        floorParent.transform.parent = roomParent.transform;
        pathParent.transform.parent = roomParent.transform;
        wallParent.transform.parent = roomParent.transform;
        decorationParent.transform.parent = roomParent.transform;
        textures = tex;
        visibleWallCube = vwc;
        roomType = Random.value < 0.7 ? 0 : 1;
        floorPlan = new int[dimensions.y + 2][];
        floorPlanRotation = new int[dimensions.y + 2][];
        floorParentList = new Object[floorTiles.Length];
        wallParentList = new Object[wallTiles.Length];
        libraryFloorTexture = libraryFloor;
        for (int i = 0; i < floorPlan.Length; i++)
        {
            floorPlan[i] = new int[dimensions.x + 2];
            floorPlanRotation[i] = new int[dimensions.x + 2];
            for (int a = 0; a < floorPlan[i].Length; a++)
            {
                floorPlan[i][a] = -1;
                floorPlanRotation[i][a] = 0;
            }
        }
        for (int i = 0; i < floorTiles.Length; i++)
        {
            GameObject newObj = new GameObject();
            newObj.name = "Floor: " + i;
            newObj.transform.parent = roomParent.transform;
            floorParentList[i] = newObj;
        }
        for (int i = 0; i < wallTiles.Length; i++)
        {
            GameObject newObj = new GameObject();
            newObj.name = "Wall: " + i;
            newObj.transform.parent = roomParent.transform;
            wallParentList[i] = newObj;

        }

    }
    public void spawnContainedRoom()
    {
        /*for (int i = 0; i < dimensions[0]; i++)
        {
            for (int a = 0; a < dimensions[1]; a++)
            {
                GameObject.Instantiate(floorCube, new Vector3(topLeft.x + i, 0, topLeft.y - a), Quaternion.Euler(0, 0, 0), floorParent.transform);
            }
        }*/
        GameObject tempCube = GameObject.Instantiate(realFloorCube, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), floorParent.transform);
        tempCube.transform.position = new Vector3((topLeft.x + bottomRight.x) / 2.0f, 0, (topLeft.y + bottomRight.y) / 2.0f);
        tempCube.transform.localScale = new Vector3((Math.Abs(topLeft.x - bottomRight.x) + 1), 1, (Math.Abs(topLeft.y - bottomRight.y) + 1));


        /* if (roomNum != 1)
         {*/
        tempCube.transform.GetComponent<MeshRenderer>().enabled = false;
        /*  }
          else
          {
              Material mat = new Material(Shader.Find("Ambient"));
              mat.SetTexture("text", libraryFloorTexture);
              tempCube.transform.GetComponent<MeshRenderer>().material = mat;

          }*/



    }
    bool doOverlap(Vector2Int l1, Vector2Int r1, Vector2Int l2, Vector2Int r2)
    {
        l1 = new Vector2Int(l1.x - 2, l1.y + 2);
        r1 = new Vector2Int(r1.x + 2, r1.y - 2);
        l2 = new Vector2Int(l2.x - 2, l2.y + 2);
        r2 = new Vector2Int(r2.x + 2, r2.y - 2);
        // If one rectangle is on left side of other
        if (r1.x < l2.x || l1.x > r2.x)
        {
            //Debug.Log("FALSE 1" + "TOPLEFT" + l1 + " | BOTTOM RIGHT " + r1 + " | TOP LEFT SPAWN " + l2 + " | BOTTOM RIGHT SPAWN " + r2);
            return false;
        }

        // If one rectangle is above other
        if (r1.y > l2.y || l1.y < r2.y)
        {
            //Debug.Log("FALSE 2" + "TOPLEFT" + l1 + " | BOTTOM RIGHT " + r1 + " | TOP LEFT SPAWN " + l2 + " | BOTTOM RIGHT SPAWN " + r2);
            return false;
        }
        //Debug.Log("True" + "TOPLEFT" + l1 + " | BOTTOM RIGHT " + r1 + " | TOP LEFT SPAWN " + l2 + " | BOTTOM RIGHT SPAWN " + r2);
        return true;
    }
    public ArrayList SpawnExit(Vector2Int newRoomDistanceDimensions)
    {

        Vector2Int exitLocation = new Vector2Int(-1, -1);
        Vector2Int exitBlock = new Vector2Int(-1, -1);
        Vector2Int exitLocation2 = new Vector2Int(-1, -1);
        Vector2Int randomInt;
        int randomEdge = -1;
        //int randomEdge = 1;
        ArrayList returnList = new ArrayList();
        while (exitLocation.Equals(new Vector2Int(-1, -1)))
        {
            randomEdge = UnityEngine.Random.Range(0, 4);
            //randomEdge = 0;
            //randomEdge = 0;
            switch (randomEdge)
            {
                case 0: //Top (X pos)
                    exitLocation = new Vector2Int(UnityEngine.Random.Range(topLeft.x + 3, topLeft.x + dimensions.x - 4), topLeft.y);
                    randomInt = Random.value < 0.5 ? Vector2Int.left : Vector2Int.right;
                    exitLocation2 = exitLocation + randomInt;
                    exitBlock = new Vector2Int(UnityEngine.Random.Range(exitLocation.x - newRoomDistanceDimensions.x + 1, exitLocation.x + newRoomDistanceDimensions.x - 1), UnityEngine.Random.Range(topLeft.y + 3, topLeft.y + newRoomDistanceDimensions.y - 1));
                    break;
                case 1: // Bottom
                    randomInt = Random.value < 0.5 ? Vector2Int.left : Vector2Int.right;
                    exitLocation = new Vector2Int(UnityEngine.Random.Range(topLeft.x + 3, topLeft.x + dimensions.x - 4), topLeft.y - dimensions.y + 1);
                    exitBlock = new Vector2Int(UnityEngine.Random.Range(exitLocation.x - newRoomDistanceDimensions.x + 1, exitLocation.x + newRoomDistanceDimensions.x - 1), UnityEngine.Random.Range(exitLocation.y - 3, exitLocation.y - newRoomDistanceDimensions.y + 1));
                    exitLocation2 = exitLocation + randomInt;
                    break;
                case 2: // Left (Z Pos)
                    randomInt = Random.value < 0.5 ? Vector2Int.up : Vector2Int.down;
                    exitLocation = new Vector2Int(topLeft.x, UnityEngine.Random.Range(topLeft.y - 3, topLeft.y - dimensions.y + 4));
                    exitBlock = new Vector2Int(UnityEngine.Random.Range(topLeft.x - 3, topLeft.x - newRoomDistanceDimensions.x + 1), UnityEngine.Random.Range(exitLocation.y + newRoomDistanceDimensions.y - 1, exitLocation.y - newRoomDistanceDimensions.y + 1));
                    exitLocation2 = exitLocation + randomInt;
                    break;
                case 3: // Right 
                    randomInt = Random.value < 0.5 ? Vector2Int.up : Vector2Int.down;
                    exitLocation = new Vector2Int(topLeft.x + dimensions.x - 1, UnityEngine.Random.Range(topLeft.y - 1, topLeft.y - dimensions.y + 4));
                    exitBlock = new Vector2Int(UnityEngine.Random.Range(exitLocation.x + 3, exitLocation.x + newRoomDistanceDimensions.x - 1), UnityEngine.Random.Range(exitLocation.y - newRoomDistanceDimensions.x + 1, exitLocation.y + newRoomDistanceDimensions.x - 1));
                    exitLocation2 = Random.value < 0.5 ? exitLocation + Vector2Int.up : exitLocation + Vector2Int.down;

                    break;
            }
            for (int i = 0; i < exitLocations.Count; i++)
            {
                Vector2Int tempVec = (Vector2Int)exitLocations[i];
                Vector2Int tempVec2 = (Vector2Int)exitLocations2[i];
                if (tempVec.Equals(exitLocation) || (tempVec + Vector2Int.up).Equals(exitLocation) || (tempVec + Vector2Int.down).Equals(exitLocation) || (tempVec + Vector2Int.left).Equals(exitLocation) || (tempVec + Vector2Int.right).Equals(exitLocation))
                {
                    exitLocation = new Vector2Int(-1, -1);
                    break;
                }
                if (tempVec.Equals(exitLocation2) || (tempVec + Vector2Int.up).Equals(exitLocation2) || (tempVec + Vector2Int.down).Equals(exitLocation2) || (tempVec + Vector2Int.left).Equals(exitLocation2) || (tempVec + Vector2Int.right).Equals(exitLocation2))
                {
                    exitLocation = new Vector2Int(-1, -1);
                    break;
                }

                tempVec = originConnectionBlock;

                if (tempVec.Equals(exitLocation) || (tempVec + Vector2Int.up).Equals(exitLocation) || (tempVec + Vector2Int.down).Equals(exitLocation) || (tempVec + Vector2Int.left).Equals(exitLocation) || (tempVec + Vector2Int.right).Equals(exitLocation))
                {
                    exitLocation = new Vector2Int(-1, -1);
                    break;
                }
                if (tempVec.Equals(exitLocation2) || (tempVec + Vector2Int.up).Equals(exitLocation2) || (tempVec + Vector2Int.down).Equals(exitLocation2) || (tempVec + Vector2Int.left).Equals(exitLocation2) || (tempVec + Vector2Int.right).Equals(exitLocation2))
                {
                    exitLocation = new Vector2Int(-1, -1);
                    break;
                }
                if (tempVec2.Equals(exitLocation2) || (tempVec2 + Vector2Int.up).Equals(exitLocation2) || (tempVec2 + Vector2Int.down).Equals(exitLocation2) || (tempVec2 + Vector2Int.left).Equals(exitLocation2) || (tempVec2 + Vector2Int.right).Equals(exitLocation2))
                {
                    exitLocation = new Vector2Int(-1, -1);
                    break;
                }
                if (tempVec2.Equals(exitLocation) || (tempVec2 + Vector2Int.up).Equals(exitLocation) || (tempVec2 + Vector2Int.down).Equals(exitLocation) || (tempVec2 + Vector2Int.left).Equals(exitLocation) || (tempVec2 + Vector2Int.right).Equals(exitLocation))
                {
                    exitLocation = new Vector2Int(-1, -1);
                    break;
                }

                tempVec2 = originConnectionBlock2;

                if (tempVec2.Equals(exitLocation2) || (tempVec2 + Vector2Int.up).Equals(exitLocation2) || (tempVec2 + Vector2Int.down).Equals(exitLocation2) || (tempVec2 + Vector2Int.left).Equals(exitLocation2) || (tempVec2 + Vector2Int.right).Equals(exitLocation2))
                {
                    exitLocation = new Vector2Int(-1, -1);
                    break;
                }
                if (tempVec2.Equals(exitLocation) || (tempVec2 + Vector2Int.up).Equals(exitLocation) || (tempVec2 + Vector2Int.down).Equals(exitLocation) || (tempVec2 + Vector2Int.left).Equals(exitLocation) || (tempVec2 + Vector2Int.right).Equals(exitLocation))
                {
                    exitLocation = new Vector2Int(-1, -1);
                    break;
                }



            }

            if ((exitLocation.x + 1 == exitBlock.x || exitLocation.x - 1 == exitBlock.x) || (exitLocation.y + 1 == exitBlock.y || exitLocation.y - 1 == exitBlock.y))
            {
                exitLocation = new Vector2Int(-1, -1);
            }
            /*            if ((exitLocation2.x + 1 == exitBlock.x || exitLocation2.x - 1 == exitBlock.x) || (exitLocation2.y + 1 == exitBlock.y || exitLocation2.y - 1 == exitBlock.y))
                        {
                            exitLocation = new Vector2Int(-1, -1);
                        }*/
            if ((exitLocation.x + 2 == exitBlock.x || exitLocation.x - 2 == exitBlock.x) || (exitLocation.y + 2 == exitBlock.y || exitLocation.y - 2 == exitBlock.y))
            {
                exitLocation = new Vector2Int(-1, -1);
            }


        }
        /*        exitLocations.Add(new Vector2Int(0, -5));
                returnList.Add(2);
                returnList.Add(new Vector2Int(0, -5));
                returnList.Add(new Vector2Int(-5, -5));*/


        exitLocations.Add(exitLocation);
        exitLocations2.Add(exitLocation2);
        returnList.Add(randomEdge);
        returnList.Add(exitLocation);
        returnList.Add(exitBlock);
        returnList.Add(exitLocation2);
        return returnList;

    }
    public HashSet<Vector2Int> checkPathToOrigin(HashSet<Vector2Int> pathBlocks, ArrayList rooms)
    {
        HashSet<Vector2Int> pathVectorCheck = new HashSet<Vector2Int>();
        HashSet<Vector2Int> secondPathVectorCheck = new HashSet<Vector2Int>();
        if (originEdge == 0)
        {
            Vector2Int highestVector = originConnectionBlock.y > originConnectionBlock2.y ? originConnectionBlock : originConnectionBlock2;
            pathVectorCheck = checkOriginHelper(pathBlocks, rooms, originBlock, originConnectionBlock, false, highestVector.y);
            secondPathVectorCheck = checkOriginHelper(pathBlocks, rooms, originBlock2, originConnectionBlock2, true, highestVector.y);
        }
        else if (originEdge == 1)
        {
            Vector2Int highestVector = originConnectionBlock.y > originConnectionBlock2.y ? originConnectionBlock2 : originConnectionBlock;
            pathVectorCheck = checkOriginHelper(pathBlocks, rooms, originBlock, originConnectionBlock, false, highestVector.y);
            secondPathVectorCheck = checkOriginHelper(pathBlocks, rooms, originBlock2, originConnectionBlock2, true, highestVector.y);
        }

        else if (originEdge == 2)
        {
            Vector2Int highestVector = originConnectionBlock.x > originConnectionBlock2.x ? originConnectionBlock2 : originConnectionBlock;
            pathVectorCheck = checkOriginHelper(pathBlocks, rooms, originBlock, originConnectionBlock, false, highestVector.x);
            secondPathVectorCheck = checkOriginHelper(pathBlocks, rooms, originBlock2, originConnectionBlock2, true, highestVector.x);
        }
        else if (originEdge == 3)
        {
            Vector2Int highestVector = originConnectionBlock.x > originConnectionBlock2.x ? originConnectionBlock : originConnectionBlock2;
            pathVectorCheck = checkOriginHelper(pathBlocks, rooms, originBlock, originConnectionBlock, false, highestVector.x);
            secondPathVectorCheck = checkOriginHelper(pathBlocks, rooms, originBlock2, originConnectionBlock2, true, highestVector.x);
        }


        if (pathVectorCheck.Count == 0 || secondPathVectorCheck.Count == 0)
        {
            return new HashSet<Vector2Int>();
        }
        pathVectorCheck.UnionWith(secondPathVectorCheck);
        return pathVectorCheck;


    }
    HashSet<Vector2Int> checkOriginHelper(HashSet<Vector2Int> pathBlocks, ArrayList rooms, Vector2Int passedOriginBlock, Vector2Int passedOriginConnectionBlock, bool second, int highestValue)
    {
        HashSet<Vector2Int> vectorHolder = new HashSet<Vector2Int>();
        if (originEdge == 0 || originEdge == 1)
        {
            int direction = originEdge == 0 ? 1 : -1;
            int accountForDifference = Math.Abs(passedOriginBlock.y - passedOriginConnectionBlock.y);
            /*if (second)
            {
                accountForDifference++;
            }*/
            if (originConnectionBlock.x == originBlock.x && originConnectionBlock2.x == originBlock2.x)
            {
                if (second)
                {
                    for (int i = 0; i < accountForDifference - 1; i++)
                    {
                        Vector2Int tempVec = new Vector2Int(passedOriginBlock.x, passedOriginBlock.y + i * direction);
                        if (pathBlocks.Contains(tempVec) || isIn(rooms, tempVec))
                        {
                            isValid = false;
                            return new HashSet<Vector2Int>();
                        }
                        else
                        {
                            vectorHolder.Add(tempVec);
                        }
                        tempVec = new Vector2Int(originBlock.x, originBlock.y + i * direction);
                        if (pathBlocks.Contains(tempVec) || isIn(rooms, tempVec))
                        {
                            isValid = false;
                            return new HashSet<Vector2Int>();
                        }
                        else
                        {
                            vectorHolder.Add(tempVec);
                        }
                    }
                }
            }
            else {
                for (int i = 1; i < accountForDifference; i++)
                {
                    Vector2Int tempVec = new Vector2Int(passedOriginBlock.x, passedOriginBlock.y + i * direction);
                    if (pathBlocks.Contains(tempVec) || isIn(rooms, tempVec))
                    {
                        isValid = false;
                        return new HashSet<Vector2Int>();
                    }
                    else
                    {
                        vectorHolder.Add(tempVec);
                    }
                }
                int leftRight = passedOriginBlock.x < passedOriginConnectionBlock.x ? 1 : -1;
                for (int i = 0; i < Math.Abs(passedOriginBlock.x - passedOriginConnectionBlock.x); i++)
                {
                    Vector2Int tempVec = new Vector2Int(passedOriginBlock.x + i * leftRight, passedOriginConnectionBlock.y);
                    if (pathBlocks.Contains(tempVec) || isIn(rooms, tempVec))
                    {
                        isValid = false;

                        return new HashSet<Vector2Int>();
                    }
                    else
                    {
                        vectorHolder.Add(tempVec);
                    }
                }
            }
            if (second)
            {
                Vector2Int cornerCheck = new Vector2Int(originBlock.x, highestValue);
                if (!(exitVectors.Contains(cornerCheck) || vectorHolder.Contains(cornerCheck)))
                {
                    vectorHolder.Add(cornerCheck);
                }

                cornerCheck = new Vector2Int(originBlock2.x, highestValue);

                if (!(exitVectors.Contains(cornerCheck) || vectorHolder.Contains(cornerCheck)))
                {
                    vectorHolder.Add(cornerCheck);
                }

                Vector2Int leftMost = passedOriginBlock.x < originBlock.x ? passedOriginBlock : originBlock;
                Vector2Int rightMost = passedOriginBlock.Equals(leftMost) ? originBlock : passedOriginBlock;

                Vector2Int topVector = passedOriginConnectionBlock.y > originConnectionBlock.y ? passedOriginConnectionBlock : originConnectionBlock;
                Vector2Int botVector = passedOriginConnectionBlock.Equals(topVector) ? originConnectionBlock : passedOriginConnectionBlock;

                if (passedOriginBlock.y < passedOriginConnectionBlock.y)
                {
                    if (passedOriginBlock.x > passedOriginConnectionBlock.x) // LEFT
                    {
                        pathWalls.Add(new RectInt(leftMost.x - 1, leftMost.y + 2, leftMost.x - 1, botVector.y - 1));
                        pathWalls.Add(new RectInt(rightMost.x + 1, leftMost.y + 2, rightMost.x + 1, topVector.y + 1));

                        if (Math.Abs(botVector.x - leftMost.x) < 4)
                        {
                            isValid = false;

                            return new HashSet<Vector2Int>();
                        }
                        pathWalls.Add(new RectInt(botVector.x + 2, botVector.y - 1, leftMost.x - 2, botVector.y - 1));

                        pathWalls.Add(new RectInt(topVector.x + 2, topVector.y + 1, rightMost.x, topVector.y + 1));


                    }
                    else if (passedOriginBlock.x < passedOriginConnectionBlock.x)
                    {
                        pathWalls.Add(new RectInt(leftMost.x - 1, leftMost.y + 2, leftMost.x - 1, topVector.y));
                        pathWalls.Add(new RectInt(rightMost.x + 1, leftMost.y + 2, rightMost.x + 1, botVector.y - 2));

                        if (Math.Abs(botVector.x - rightMost.x) < 4)
                        {
                            isValid = false;

                            return new HashSet<Vector2Int>();

                        }
                        pathWalls.Add(new RectInt(botVector.x - 2, botVector.y - 1, rightMost.x + 1, botVector.y - 1));

                        pathWalls.Add(new RectInt(topVector.x - 2, topVector.y + 1, leftMost.x - 1, topVector.y + 1));
                    }
                    else
                    {
                        pathWalls.Add(new RectInt(leftMost.x - 1, leftMost.y + 2, leftMost.x - 1, topVector.y - 2));
                        pathWalls.Add(new RectInt(rightMost.x + 1, leftMost.y + 2, rightMost.x + 1, topVector.y - 2));
                    }

                }
                else if (passedOriginBlock.y > passedOriginConnectionBlock.y)
                {
                    if (passedOriginBlock.x > passedOriginConnectionBlock.x) // DOWN LEFT
                    {

                        pathWalls.Add(new RectInt(leftMost.x - 1, leftMost.y - 2, leftMost.x - 1, topVector.y + 1));
                        pathWalls.Add(new RectInt(rightMost.x + 1, leftMost.y - 2, rightMost.x + 1, botVector.y - 1));

                        if (Math.Abs(botVector.x - leftMost.x) < 4)
                        {
                            isValid = false;

                            return new HashSet<Vector2Int>();
                        }
                        pathWalls.Add(new RectInt(topVector.x + 2, topVector.y + 1, leftMost.x - 2, topVector.y + 1));

                        pathWalls.Add(new RectInt(botVector.x + 2, botVector.y - 1, rightMost.x, botVector.y - 1));

                    }
                    else if (passedOriginBlock.x < passedOriginConnectionBlock.x)
                    {
                        pathWalls.Add(new RectInt(leftMost.x - 1, leftMost.y - 2, leftMost.x - 1, botVector.y - 1));
                        pathWalls.Add(new RectInt(rightMost.x + 1, leftMost.y - 2, rightMost.x + 1, topVector.y + 1));

                        if (Math.Abs(botVector.x - rightMost.x) < 4)
                        {
                            isValid = false;

                            return new HashSet<Vector2Int>();

                        }
                        pathWalls.Add(new RectInt(topVector.x - 2, topVector.y + 1, rightMost.x + 2, topVector.y + 1));

                        pathWalls.Add(new RectInt(botVector.x - 2, botVector.y - 1, leftMost.x, botVector.y - 1));
                    }
                    else
                    {
                        pathWalls.Add(new RectInt(leftMost.x - 1, leftMost.y - 2, leftMost.x - 1, topVector.y + 2));
                        pathWalls.Add(new RectInt(rightMost.x + 1, leftMost.y - 2, rightMost.x + 1, topVector.y + 2));
                    }

                }




            }
        }
        else if (originEdge == 2 || originEdge == 3)
        {
            int direction = originEdge == 2 ? -1 : 1;
            bool cCheck = true;
            if (originBlock.y == originConnectionBlock.y && originBlock2.y == originConnectionBlock2.y || originBlock.y == originConnectionBlock2.y && originConnectionBlock.y == originBlock2.y)
            {
                cCheck = false;
                for (int i = 1; i < Math.Abs(passedOriginBlock.x - passedOriginConnectionBlock.x); i++)
                {
                    Vector2Int tempVec = new Vector2Int(passedOriginBlock.x + i, passedOriginBlock.y);
                    if (pathBlocks.Contains(tempVec) || isIn(rooms, tempVec))
                    {
                        isValid = false;

                        return new HashSet<Vector2Int>();
                    }
                    else
                    {
                        vectorHolder.Add(tempVec);
                    }
                }
            }
            else
            {

                for (int i = 1; i < Math.Abs(passedOriginBlock.x - passedOriginConnectionBlock.x); i++)
                {
                    Vector2Int tempVec = new Vector2Int(passedOriginBlock.x + i * direction, passedOriginBlock.y);
                    if (pathBlocks.Contains(tempVec) || isIn(rooms, tempVec))
                    {
                        isValid = false;

                        return new HashSet<Vector2Int>();
                    }
                    else
                    {
                        vectorHolder.Add(tempVec);
                    }
                }
                int upDown = passedOriginBlock.y < passedOriginConnectionBlock.y ? 1 : -1;

                for (int i = 0; i < Math.Abs(passedOriginBlock.y - passedOriginConnectionBlock.y); i++)
                {
                    Vector2Int tempVec = new Vector2Int(passedOriginConnectionBlock.x, passedOriginBlock.y + i * upDown);
                    if (pathBlocks.Contains(tempVec) || isIn(rooms, tempVec))
                    {
                        isValid = false;

                        return new HashSet<Vector2Int>();
                    }
                    else
                    {
                        vectorHolder.Add(tempVec);
                    }
                }
            }


            if (second && cCheck)
            {
                Vector2Int cornerCheck = new Vector2Int(highestValue, originBlock.y);
                if (!(exitVectors.Contains(cornerCheck) || vectorHolder.Contains(cornerCheck)))
                {
                    vectorHolder.Add(cornerCheck);

                }

                cornerCheck = new Vector2Int(highestValue, originBlock2.y);

                if (!(exitVectors.Contains(cornerCheck) || vectorHolder.Contains(cornerCheck)))
                {

                    vectorHolder.Add(cornerCheck);
                }


            }
            if (second)
            {



                Vector2Int topVector = passedOriginBlock.y > originBlock.y ? passedOriginBlock : originBlock;
                Vector2Int botVector = passedOriginBlock.Equals(topVector) ? originBlock : passedOriginBlock;

                Vector2Int leftMost = passedOriginConnectionBlock.x < originConnectionBlock.x ? passedOriginConnectionBlock : originConnectionBlock;
                Vector2Int rightMost = passedOriginConnectionBlock.Equals(leftMost) ? originConnectionBlock : passedOriginConnectionBlock;
                if (direction == -1)
                {
                    if (passedOriginBlock.y > passedOriginConnectionBlock.y) // DOWN
                    {

                        RectInt checkRect = new RectInt(topVector.x - 2, topVector.y + 1, leftMost.x - 1, topVector.y + 1);

                        pathWalls.Add(checkRect);

                        checkRect = new RectInt(topVector.x - 2, botVector.y - 1, rightMost.x + 1, botVector.y - 1);

                        pathWalls.Add(checkRect);

                        checkRect = new RectInt(leftMost.x - 1, leftMost.y + 2, leftMost.x - 1, topVector.y);

                        pathWalls.Add(checkRect);

                        checkRect = new RectInt(rightMost.x + 1, rightMost.y + 2, rightMost.x + 1, botVector.y - 2);

                        pathWalls.Add(checkRect);
                    }
                    else if (passedOriginBlock.y < passedOriginConnectionBlock.y)// UP
                    {

                        pathWalls.Add(new RectInt(topVector.x - 2, topVector.y + 1, rightMost.x + 1, topVector.y + 1));
                        pathWalls.Add(new RectInt(botVector.x - 2, botVector.y - 1, leftMost.x - 1, botVector.y - 1));

                        pathWalls.Add(new RectInt(leftMost.x - 1, leftMost.y - 2, leftMost.x - 1, botVector.y));
                        pathWalls.Add(new RectInt(rightMost.x + 1, rightMost.y - 2, rightMost.x + 1, topVector.y + 2));
                    }
                    else
                    {
                        pathWalls.Add(new RectInt(topVector.x - 2, topVector.y + 1, rightMost.x + 2, topVector.y + 1));
                        pathWalls.Add(new RectInt(topVector.x - 2, botVector.y - 1, rightMost.x + 2, botVector.y - 1));
                    }
                }
                else
                {
                    if (passedOriginBlock.y > passedOriginConnectionBlock.y) // DOWN
                    {
                        pathWalls.Add(new RectInt(topVector.x + 2, topVector.y + 1, rightMost.x, topVector.y + 1));
                        pathWalls.Add(new RectInt(topVector.x + 2, botVector.y - 1, leftMost.x - 2, botVector.y - 1));

                        pathWalls.Add(new RectInt(leftMost.x - 1, leftMost.y + 2, leftMost.x - 1, botVector.y - 1));
                        pathWalls.Add(new RectInt(rightMost.x + 1, rightMost.y + 2, rightMost.x + 1, topVector.y + 1));

                    }
                    else if (passedOriginBlock.y < passedOriginConnectionBlock.y)// UP
                    {
                        pathWalls.Add(new RectInt(topVector.x + 2, topVector.y + 1, leftMost.x - 2, topVector.y + 1));
                        pathWalls.Add(new RectInt(botVector.x + 2, botVector.y - 1, rightMost.x, botVector.y - 1));

                        pathWalls.Add(new RectInt(leftMost.x - 1, leftMost.y - 2, leftMost.x - 1, topVector.y + 1));
                        pathWalls.Add(new RectInt(rightMost.x + 1, leftMost.y - 2, rightMost.x + 1, botVector.y - 1));
                    }
                    else
                    {
                        pathWalls.Add(new RectInt(topVector.x + 2, topVector.y + 1, rightMost.x - 2, topVector.y + 1));
                        pathWalls.Add(new RectInt(topVector.x + 2, botVector.y - 1, rightMost.x - 2, botVector.y - 1));
                    }
                }









            }
        }
        exitVectors.Add(vectorHolder);
        return vectorHolder;
    }
    public void deleteExits()
    {
        exitLocations.RemoveAt(exitLocations.Count - 1);
        exitLocations2.RemoveAt(exitLocations2.Count - 1);
        //exitVectors.RemoveAt(exitVectors.Count - 1);
    }
    public void createPathToOrigin()
    {
        HashSet<Vector2Int> tempVecs = (HashSet<Vector2Int>)exitVectors[exitVectors.Count - 1];
        tempVecs.UnionWith((HashSet<Vector2Int>)exitVectors[exitVectors.Count - 2]);
        foreach (Vector2Int vec in tempVecs)
        {
            GameObject.Instantiate(pathCube, new Vector3(vec.x, 0f, vec.y), Quaternion.Euler(0, 0, 0), pathParent.transform);
        }
        //GameObject.Instantiate(desinationCube, new Vector3(originConnectionBlock.x, 1, originConnectionBlock.y), Quaternion.Euler(0, 0, 0));
        /*        GameObject.Instantiate(desinationCube, new UnityEngine.Vector3(originConnectionBlock.x, 1, originConnectionBlock.y), UnityEngine.Quaternion.Euler(0, 0, 0));
                GameObject.Instantiate(desinationCube, new UnityEngine.Vector3(originConnectionBlock2.x, 2, originConnectionBlock2.y), UnityEngine.Quaternion.Euler(0, 0, 0));*/
    }
    public void addWalls()
    {
        Vector2Int topWallExitCheckLeft = new Vector2Int(topLeft.x - 1, topLeft.y + 1);
        Vector2Int topWallExitCheckRight = topWallExitCheckLeft;

        Vector2Int bottomWallExitCheckLeft = new Vector2Int(topLeft.x - 1, bottomRight.y - 1);
        Vector2Int bottomWallExitCheckRight = bottomWallExitCheckLeft;

        Vector2Int leftWallExitCheckTop = new Vector2Int(topLeft.x - 1, topLeft.y);
        Vector2Int leftWallExitCheckBot = leftWallExitCheckTop;

        Vector2Int rightWallExitCheckTop = new Vector2Int(bottomRight.x + 1, topLeft.y);
        Vector2Int rightWallExitCheckBot = rightWallExitCheckTop;


        for (int i = topLeft.x - 1, a = topLeft.y + 1; i <= bottomRight.x + 1; i++)
        {
            Vector2Int topVec = new Vector2Int(i, topLeft.y);
            Vector2Int botVec = new Vector2Int(i, bottomRight.y);

            if (!exitLocations.Contains(topVec) && !originConnectionBlock.Equals(topVec) && !originConnectionBlock2.Equals(topVec) && !exitLocations2.Contains(topVec))
            {
                topWallExitCheckRight.x = i;
            }
            else
            {
                spawnWall(topWallExitCheckLeft, topWallExitCheckRight, wallCube);
                topWallExitCheckLeft = topWallExitCheckRight + new Vector2Int(3, 0);
                i++;
            }
            if (!exitLocations.Contains(botVec) && !originConnectionBlock.Equals(botVec) && !originConnectionBlock2.Equals(botVec) && !exitLocations2.Contains(botVec))
            {
                bottomWallExitCheckRight.x = i;
            }
            else
            {
                spawnWall(bottomWallExitCheckLeft, bottomWallExitCheckRight, wallCube);
                bottomWallExitCheckLeft = bottomWallExitCheckRight + new Vector2Int(3, 0);
                i++;
            }
        }
        spawnWall(topWallExitCheckLeft, topWallExitCheckRight, wallCube);
        spawnWall(bottomWallExitCheckLeft, bottomWallExitCheckRight, wallCube);

        for (int i = topLeft.y, a = topLeft.x - 1; i >= bottomRight.y; i--)
        {
            Vector2Int leftVec = new Vector2Int(topLeft.x, i);
            Vector2Int rightVec = new Vector2Int(bottomRight.x, i);
            if (!exitLocations.Contains(leftVec) && !originConnectionBlock.Equals(leftVec) && !originConnectionBlock2.Equals(leftVec) && !exitLocations2.Contains(leftVec))
            {
                leftWallExitCheckBot.y = i;

            }
            else
            {
                spawnWall(leftWallExitCheckTop, leftWallExitCheckBot, wallCube);
                leftWallExitCheckTop = leftWallExitCheckBot - new Vector2Int(0, 3);
                i--;
            }

            if (!exitLocations.Contains(rightVec) && !originConnectionBlock.Equals(rightVec) && !originConnectionBlock2.Equals(rightVec) && !exitLocations2.Contains(rightVec))
            {
                rightWallExitCheckBot.y = i;
            }
            else
            {
                spawnWall(rightWallExitCheckTop, rightWallExitCheckBot, wallCube);
                rightWallExitCheckTop = rightWallExitCheckBot - new Vector2Int(0, 3);
                i--;
            }
        }
        spawnWall(leftWallExitCheckTop, leftWallExitCheckBot, wallCube);
        spawnWall(rightWallExitCheckTop, rightWallExitCheckBot, wallCube);

        foreach (RectInt r in pathWalls)
        {
            spawnWall(new Vector2Int(r.x, r.y), new Vector2Int(r.width, r.height), visibleWallCube, true);
        }
    }
    void spawnWall(Vector2Int tL, Vector2Int bR, GameObject wallType)
    {
        GameObject wall = GameObject.Instantiate(wallCube, new Vector3((tL.x + bR.x) / 2.0f, ((int)wallCube.transform.localScale.y / 2), (tL.y + bR.y) / 2.0f), Quaternion.Euler(0, 0, 0), wallParent.transform);
        wall.transform.localScale = new Vector3((Math.Abs(tL.x - bR.x) + 1), wall.transform.localScale.y, (Math.Abs(tL.y - bR.y) + 1));
        wall.GetComponent<MeshRenderer>().enabled = false;


    }
    void spawnWall(Vector2Int tL, Vector2Int bR, GameObject wallType, bool gol)
    {
        GameObject wall = GameObject.Instantiate(wallCube, new Vector3((tL.x + bR.x) / 2.0f, ((int)wallCube.transform.localScale.y / 2), (tL.y + bR.y) / 2.0f), Quaternion.Euler(0, 0, 0), wallParent.transform);
        wall.transform.localScale = new Vector3((Math.Abs(tL.x - bR.x) + 1), wall.transform.localScale.y, (Math.Abs(tL.y - bR.y) + 1));
        wall.GetComponent<MeshRenderer>().enabled = false;
        spawnWall2(tL, bR, wallType);


    }
    void spawnWall2(Vector2Int tL, Vector2Int bR, GameObject wallType)
    {
        Vector2Int smallerVec = tL.x < bR.x ? tL : bR;
        Vector2Int otherVec = smallerVec.Equals(tL) ? bR : tL;


        for (int i = smallerVec.x; i <= otherVec.x; i++)
        {
            Vector2Int tempVec = new Vector2Int(i, tL.y);
            if (!placedWalls.Contains(tempVec))
            {
                GameObject.Instantiate(wallType, new Vector3(i, wallType.transform.localScale.y / 2.0f, tL.y), Quaternion.Euler(0, 0, 0), ((GameObject)wallParentList[0]).transform);
                placedWalls.Add(tempVec);
            }
        }


        smallerVec = tL.y < bR.y ? tL : bR;
        otherVec = smallerVec.Equals(tL) ? bR : tL;
        for (int i = smallerVec.y; i <= otherVec.y; i++)
        {
            Vector2Int tempVec = new Vector2Int(tL.x, i);
            if (!placedWalls.Contains(tempVec))
            {
                GameObject.Instantiate(wallType, new Vector3(tL.x, wallType.transform.localScale.y / 2.0f, i), Quaternion.Euler(0, 0, 0), ((GameObject)wallParentList[0]).transform);
                placedWalls.Add(tempVec);
            }

        }


    }
    public void spawnAesteticWalls(GameObject wall, GameObject edgeWall)
    {
        Debug.Log("ORIGINBLOCK1:" + originBlock);
        Debug.Log("ORIGINBLOCK2:" + originBlock2);
        Debug.Log("CORIGINBLOCK1:" + originConnectionBlock);
        Debug.Log("CORIGINBLOCK2:" + originConnectionBlock2);
        for (int i = topLeft.x - 1; i <= bottomRight.x + 1; i++)
        {
            Vector2Int tempVec = new Vector2Int(i, topLeft.y);
            if (!originBlock.Equals(tempVec) && !originBlock2.Equals(tempVec) && !originConnectionBlock.Equals(tempVec) && !originConnectionBlock2.Equals(tempVec) && !exitLocations.Contains(tempVec) && !exitLocations2.Contains(tempVec))
            {
                if (i == topLeft.x - 1)
                {
                    GameObject.Instantiate(edgeWall, new Vector3(i, (wall.transform.localScale.y / 2.0f), topLeft.y + 1), Quaternion.Euler(0, 0, 0));
                }
                else
                {
                    GameObject.Instantiate(wall, new Vector3(i, (wall.transform.localScale.y / 2.0f), topLeft.y + 1), Quaternion.Euler(0, 0, 0));
                }
            }
            tempVec = new Vector2Int(i, bottomRight.y);

            if (!originBlock.Equals(tempVec) && !originBlock2.Equals(tempVec) && !originConnectionBlock.Equals(tempVec) && !originConnectionBlock2.Equals(tempVec) && !exitLocations.Contains(tempVec) && !exitLocations2.Contains(tempVec))
            {

                if (i == bottomRight.x + 1)
                {
                    GameObject.Instantiate(edgeWall, new Vector3(i, (wall.transform.localScale.y / 2.0f), bottomRight.y - 1), Quaternion.Euler(0, 0, 0));

                }
                else
                {
                    GameObject.Instantiate(wall, new Vector3(i, (wall.transform.localScale.y / 2.0f), bottomRight.y - 1), Quaternion.Euler(0, 0, 0));
                }
            }


        }

        for (int i = topLeft.y; i >= bottomRight.y; i--)
        {
            Vector2Int tempVec = new Vector2Int(topLeft.x, i);

            if (!originBlock.Equals(tempVec) && !originBlock2.Equals(tempVec) && !originConnectionBlock.Equals(tempVec) && !originConnectionBlock2.Equals(tempVec) && !exitLocations.Contains(tempVec) && !exitLocations2.Contains(tempVec))
            {
                GameObject.Instantiate(wall, new Vector3(topLeft.x - 1, (wall.transform.localScale.y / 2.0f), i), Quaternion.Euler(0, 0, 0));
            }
            tempVec = new Vector2Int(bottomRight.x, i);
            if (!originBlock.Equals(tempVec) && !originBlock2.Equals(tempVec) && !originConnectionBlock.Equals(tempVec) && !originConnectionBlock2.Equals(tempVec) && !exitLocations.Contains(tempVec) && !exitLocations2.Contains(tempVec))
            {
                GameObject.Instantiate(wall, new Vector3(bottomRight.x + 1, (wall.transform.localScale.y / 2.0f), i), Quaternion.Euler(0, 0, 0));
            }
        }
    }
    public bool isIn(Vector2Int point)
    {

        //return ((point.x >= topLeft.x-1 && point.x <= bottomRight.x + 1) && (point.y <= topLeft.y+1 && point.y >= bottomRight.y-1));
        return ((point.x >= topLeft.x && point.x <= bottomRight.x) && (point.y <= topLeft.y && point.y >= bottomRight.y));
    }
    public bool specialIsIn(Vector2Int point)
    {
        if (point.Equals(originBlock) || point.Equals(originBlock2) || point.Equals(originConnectionBlock) || point.Equals(originConnectionBlock2))
        {
            return ((point.x >= topLeft.x && point.x <= bottomRight.x) && (point.y <= topLeft.y && point.y >= bottomRight.y));
        }
        else
        {
            return ((point.x >= topLeft.x - 3 && point.x <= bottomRight.x + 3) && (point.y <= topLeft.y + 3 && point.y >= bottomRight.y - 3));

        }
        //return ((point.x >= topLeft.x && point.x <= bottomRight.x) && (point.y <= topLeft.y && point.y >= bottomRight.y));
    }
    public bool isIn(ArrayList rooms, Vector2Int point)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (((Room)rooms[i]).isIn(point))
            {
                return true;
            }
        }

        return isIn(point);
    }
    public bool specialIsIn(ArrayList rooms, Vector2Int point)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (((Room)rooms[i]).specialIsIn(point))
            {
                return true;
            }
        }

        return isIn(point);
    }
    public bool specialIsInCheck(ArrayList rooms, Vector2Int point)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (((Room)rooms[i]).specialIsIn(point))
            {
                return true;
            }
        }

        return isIn(point);
    }
    public void makeWallsNotTrigger()
    {
        for (int i = 0; i < wallParent.transform.childCount; i++)
        {
            if (wallParent.transform.GetChild(i).GetComponent<BoxCollider>() != null)
            {
                wallParent.transform.GetChild(i).GetComponent<BoxCollider>().isTrigger = false;

            }

        }
    }
    public void spawnFloor(Object[] floorBlocks, Object[] wallBlocks, GameObject normalPillar, GameObject libraryPillar, Object[] floorObjects)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject secondCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        secondCube.transform.localScale = new Vector3(1, 10, 1);
        GameObject thirdCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        thirdCube.transform.localScale = new Vector3(1, 5, 1);
        GameObject fourthCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fourthCube.transform.GetComponent<MeshRenderer>().material.color = UnityEngine.Color.red;
        cube.transform.GetComponent<MeshRenderer>().material.color = UnityEngine.Color.black;

        thirdCube.transform.localScale = new Vector3(1, 10, 1);
        fourthCube.transform.localScale = new Vector3(1, 10, 1);

        fillFloorPlanNormal();
        /*for (int i = 0; i < floorPlan.Length; i++)
        {
            String s = "[";
            for (int a = 0; a < floorPlan[0].Length; a++)   
            {
                s += floorPlan[i][a] + ",";
            }
            s += "]";
            Debug.Log(s);
        }*/

        for (int i = 0; i < floorPlan.Length; i++)
        {
            for (int a = 0; a < floorPlan[0].Length; a++)
            {
                GameObject tempCube;
                GameObject parent;
                int rotation = 0;
                int index = floorPlan[i][a];
                switch (index)
                {
                    case 0:
                        if (roomNum == 0)
                        {
                            tempCube = (GameObject)wallBlocks[0];
                            parent = (GameObject)wallParentList[0];
                        }
                        else
                        {
                            int tempRand;
                            if (a == 1 & i == 0 || (a == floorPlan[0].Length - 2 && i == floorPlan.Length - 1))
                            {
                                if (i == 0)
                                {
                                    rotation = 180;
                                }
                                tempRand = Random.Range(8, 10);

                                tempCube = (GameObject)wallBlocks[tempRand];

                            }
                            else if (a == 0 && i == 1 || a == floorPlan[0].Length - 1 && i == floorPlan.Length - 2)
                            {
                                rotation = i == 1 ? 90 : -90;
                                tempRand = Random.Range(3, 5);
                                tempCube = (GameObject)wallBlocks[tempRand];

                            }
                            else if (a == 0 && i == floorPlan.Length - 2 || a == floorPlan[0].Length - 1 && i == 1)
                            {
                                rotation = i == 1 ? -90 : 90;
                                tempRand = Random.Range(8, 10);

                                tempCube = (GameObject)wallBlocks[tempRand];

                            }
                            else if (a == floorPlan[0].Length - 2 && i == 0 || a == 1 && i == floorPlan.Length - 1)
                            {
                                rotation = i == 0 ? -180 : 0;
                                tempRand = Random.Range(3, 5);

                                tempCube = (GameObject)wallBlocks[tempRand];
                            }
                            else
                            {
                                if (i == 0)
                                {
                                    rotation = 180;

                                }
                                else if (a == 0)
                                {
                                    rotation = 90;

                                }
                                else if (a == floorPlan[0].Length - 1)
                                {
                                    rotation = -90;

                                }
                                tempRand = Random.Range(5, 8);
                                tempCube = (GameObject)wallBlocks[tempRand];

                            }
                            parent = (GameObject)wallParentList[tempRand];



                        }
                        break;
                    case 1:
                        tempCube = roomNum == 0 ? (GameObject)wallBlocks[1] : (GameObject)wallBlocks[2];
                        parent = roomNum == 0 ? (GameObject)wallParentList[1] : (GameObject)wallParentList[2];

                        break;
                    case -3:
                        tempCube = roomNum == 0 ? (GameObject)wallBlocks[1] : (GameObject)wallBlocks[2];
                        parent = roomNum == 0 ? (GameObject)wallParentList[1] : (GameObject)wallParentList[2];

                        break;
                    case -5:
                        tempCube = roomNum == 0 ? normalPillar : libraryPillar;
                        parent = wallParent;

                        break;
                    case -10:
                        tempCube = (GameObject)floorBlocks[6];
                        parent = (GameObject)floorParentList[6];
                        if (Random.value > 0.75)
                        {
                            GameObject go = GameObject.Instantiate(floorObjects[Random.Range(4,8)], new Vector3(topLeft.x - 1 + a, 0.5f, topLeft.y + 1 - i), Quaternion.Euler(0, rotation, 0), decorationParent.transform);
                        }
                        break;
                    /* tempCube = roomNum == 0 ? (GameObject)wallBlocks[10] : (GameObject)wallBlocks[11];
                        parent = wallParent;
                        break;*/
                    case 10:
                        tempCube = (GameObject)floorBlocks[10 - 10];
                        rotation = floorPlanRotation[i][a];
                        parent = (GameObject)floorParentList[10 - 10];
                        break;
                    case 11:
                        tempCube = (GameObject)floorBlocks[11 - 10];
                        rotation = floorPlanRotation[i][a];
                        parent = (GameObject)floorParentList[11 - 10];


                        break;
                    case 12:
                        tempCube = (GameObject)floorBlocks[12 - 10];
                        rotation = floorPlanRotation[i][a];
                        parent = (GameObject)floorParentList[12 - 10];

                        break;
                    case 13:
                        tempCube = (GameObject)floorBlocks[13 - 10];
                        rotation = floorPlanRotation[i][a];
                        parent = (GameObject)floorParentList[13 - 10];
                        if (Random.value > 0.75)
                        {
                            
                                GameObject.Instantiate(floorObjects[Random.Range(1,4)], new Vector3(topLeft.x - 1 + a, 0.6f, topLeft.y + 1 - i), Quaternion.Euler(0, rotation, 0), decorationParent.transform);

                        }
                        f
                        break;
                    case 15:
                        tempCube = (GameObject)floorBlocks[15 - 10];
                        rotation = floorPlanRotation[i][a];
                        parent = (GameObject)floorParentList[15 - 10];
                        if (Random.value > 0.75)
                        {
                            
                                GameObject.Instantiate(floorObjects[Random.Range(1,4)], new Vector3(topLeft.x - 1 + a, 0.6f, topLeft.y + 1 - i), Quaternion.Euler(0, rotation, 0), decorationParent.transform);

                            
                        }
                        break;
                    default:
                        if (roomNum == 0)
                        {
                            tempCube = (GameObject)floorBlocks[0];
                            parent = (GameObject)floorParentList[0];
                        }
                        else
                        {
                            tempCube = (GameObject)floorBlocks[6];
                            parent = (GameObject)floorParentList[6];
                           

                        }

                        break;
                }
                if (index != -2)
                {
                    
                    float height = roomNum == 1 ? 0f : 0.5f;

                    if (index == 12)
                    {

                        GameObject.Instantiate(tempCube, new Vector3(topLeft.x - 1 / 2.0f + a, height, topLeft.y + 1 / 2.0f - i), Quaternion.Euler(0, rotation, 0), parent.transform);

                    }
                    else
                    {
                        GameObject.Instantiate(tempCube, new Vector3(topLeft.x - 1 + a, height, topLeft.y + 1 - i), Quaternion.Euler(0, rotation, 0), parent.transform);

                    }

                }

                else
                {
                    //Debug.Log("false 2");
                }

            }
            GameObject.Destroy(cube);
            GameObject.Destroy(secondCube);
            GameObject.Destroy(thirdCube);
            GameObject.Destroy(fourthCube);


            }
    }        
    void fillFloorPlanNormal()
    {
     /*   Debug.Log("originConnection: " + originConnectionBlock);
        Debug.Log("originConnection2: " + originConnectionBlock2);
        Debug.Log("originBlock: " + originBlock);
        Debug.Log("originBlock2: " + originBlock2);*/
        
        for(int i =0; i < floorPlan.Length; i++)
        {
            for(int a = 0; a < floorPlan[0].Length; a++)
            {
                Vector2Int currentBlock = new Vector2Int(topLeft.x - 1 + a, topLeft.y + 1 - i);
                int currentBlockIndex = floorPlan[i][a];
                if (!checkIfExit(currentBlock))
                {
                    if (a == 0 || a == floorPlan[0].Length - 1)
                    {
                        if (i == 0 || i == floorPlan.Length - 1)
                        {
                            floorPlan[i][a] = 1;
                        }
                        else
                        {
                            if(currentBlockIndex != -2)
                            {
                                floorPlan[i][a] = 0;
                            }
                            
                            
                        }
                    }
                    else if (i == 0 || i == floorPlan.Length - 1)
                    {
                        if(currentBlockIndex != -2)
                        {
                            floorPlan[i][a] = 0;
                        }
                    }
                    else
                    {
                        if(currentBlockIndex != -2)
                        {
                            floorPlan[i][a] = -1;

                        }
                    }
                }
                else
                {
                    if(i == 1)
                    {
                        floorPlan[0][a] = -2;
                       // GameObject.Instantiate(desinationCube, new Vector3(topLeft.x - 1 + a, 0, topLeft.y + 1), Quaternion.Euler(0, 0, 0));
                    } else if (a == 1)
                    {
                        //GameObject.Instantiate(desinationCube, new Vector3(topLeft.x - 1, 0, topLeft.y + 1 - i), Quaternion.Euler(0, 0, 0));
                        floorPlan[i][a-1] = -2;

                    }
                    else if (i == floorPlan.Length - 2)
                    {
                        //GameObject.Instantiate(desinationCube, new Vector3(topLeft.x - 1 + a, 0, bottomRight.y - 1), Quaternion.Euler(0, 0, 0));
                        floorPlan[floorPlan.Length - 1][a] = -2;
                    }
                    else if (a == floorPlan[0].Length - 2)
                    {
                        //GameObject.Instantiate(desinationCube, new Vector3(bottomRight.x + 1, 0, topLeft.y + 1 - i), Quaternion.Euler(0, 0, 0));
                        floorPlan[i][a + 1] = -2;

                    }
                    else
                    {
                        
                    }
                }
                
            }
        }

        for(int i = 0; i < 2; i++)
        {
            Vector2Int dimensionHalfed = new Vector2Int((int)Math.Ceiling(dimensions.x / 2.0), (int)Math.Ceiling(dimensions.y / 2.0));
            int xStart = Random.Range(4, dimensions.x / 2);
            int yStart = Random.Range(4, dimensions.y / 2);

            int curveDirection = Random.value > 0.5 ? -1 : 1;
            int featuresAdded = 0;
            int roomFeatures = Random.Range(0, 15);
            //roomFeatures = 7;
            int pillar = Random.value > 0.9 ? -2 : -5;

            roomParent.name += (" | DN: " + roomFeatures);
            switch (roomFeatures)
            {
                case 0: // 1 Middle Pillar
                    addMiddlePillar(new Vector2Int(0, 0), dimensions, 1, pillar);
                    break;
                case 1: // 4 Edge Pillars
                    addMiddlePillar(new Vector2Int(0, 0), dimensionHalfed, 0, pillar);
                    addMiddlePillar(new Vector2Int(dimensions.x / 2, 0), dimensionHalfed, 0, pillar);
                    addMiddlePillar(new Vector2Int(0, dimensions.y / 2), dimensionHalfed, 0, pillar);
                    addMiddlePillar(new Vector2Int(dimensions.x / 2, dimensions.y / 2), dimensionHalfed, 0, pillar);

                    break;
                case 2: // 2 Pillars Left, Other Side Walls
                    int start = Random.Range(3, dimensions.y / 2);

                    addMiddlePillar(new Vector2Int(0, 0), dimensionHalfed, 0, pillar);
                    addMiddlePillar(new Vector2Int(0, dimensions.y / 2), dimensionHalfed, 0, pillar);
                    curvedWallY(yStart, dimensions.x - dimensions.x / 2 / 2 + 1, curveDirection * -1);
                    break;
                case 3: // 2 Pillars on Right, Other Side Walls
                    curvedWallY(yStart, dimensions.x / 2 / 2, curveDirection);
                    addMiddlePillar(new Vector2Int(dimensions.x / 2, 0), dimensionHalfed, 0, pillar);
                    //addMiddlePillar(new Vector2Int(0, dimensions.y / 2), dimensionHalfed, 0, -3);
                    addMiddlePillar(new Vector2Int(dimensions.x / 2, dimensions.y / 2), dimensionHalfed, 0, pillar);
                    break;
                case 4: // One Pillar on Left
                    addMiddlePillar(new Vector2Int(0, 0), new Vector2Int((int)Math.Ceiling(dimensions.x / 2.0), dimensions.y), 0, pillar); // Left Half of Y 
                    curvedWallY(yStart, dimensions.x - dimensions.x / 2 / 2 + 1, curveDirection * -1);
                    break;
                case 5: // One pillar on Right
                    addMiddlePillar(new Vector2Int(dimensions.x / 2, 0), new Vector2Int((int)Math.Ceiling(dimensions.x / 2.0), dimensions.y), 0, pillar); // Right Half of Y
                    curvedWallY(yStart, dimensions.x / 2 / 2, curveDirection);
                    break;
                case 6: // Pillar on Bottom, curve on top
                    addMiddlePillar(new Vector2Int(0, dimensions.y / 2), new Vector2Int(dimensions.x, (int)Math.Ceiling(dimensions.y / 2.0)), 0, pillar); // Bottom X
                    curvedWallX(xStart, dimensions.y / 2 / 2, curveDirection);
                    break;
                case 7: // Pillar on Top, Curve on Bottom

                    addMiddlePillar(new Vector2Int(0, 0), new Vector2Int(dimensions.x, (int)Math.Ceiling(dimensions.y / 2.0)), 0, pillar); // Top X
                    curvedWallX(xStart, dimensions.y - (dimensions.y / 2 / 2) + 1, curveDirection * -1);
                    break;
                case 8: // Two Pillars on Bottom Curve on bottom
                    addMiddlePillar(new Vector2Int(0, dimensions.y / 2), dimensionHalfed, 0, pillar);
                    addMiddlePillar(new Vector2Int(dimensions.x / 2, dimensions.y / 2), dimensionHalfed, 0, pillar);
                    curvedWallX(xStart, dimensions.y - (dimensions.y / 2 / 2) + 1, curveDirection * -1);
                    break;
                case 9:
                    divideRoomX();
                    break;
                case 10:
                    divideRoomY();
                    break;
                case 11:
                    roomDecorationY();
                    break;
                case 12:
                    roomDecorationX();
                    break;
                case 13:
                    floorHolesY();
                    break;
                case 14:
                    floorHolesX();
                    break;
                case 15:
                    divideRoomX();
                    addMiddlePillar(new Vector2Int(0, 0), dimensions, 1, pillar);
                    break;
                case 16:
                    roomDecorationY();
                    floorHolesX();
                    break;
                case 17:
                    roomDecorationX();
                    floorHolesY();
                    break;
                case 18: //Pillars on bottom, curve on top
                    addMiddlePillar(new Vector2Int(0, dimensions.y / 2), dimensionHalfed, 0, pillar);
                    addMiddlePillar(new Vector2Int(dimensions.x / 2, dimensions.y / 2), dimensionHalfed, 0, pillar);
                    curvedWallX(xStart, dimensions.y / 2 / 2, curveDirection);
                    break;
                case 19: // Curve on top
                    curvedWallX(xStart, dimensions.y / 2 / 2, curveDirection);

                    break;
                case 20:
                    addMiddlePillar(new Vector2Int(0, dimensions.y / 2), dimensionHalfed, 0, pillar);
                    addMiddlePillar(new Vector2Int(dimensions.x / 2, dimensions.y / 2), dimensionHalfed, 0, pillar);
                    break;
                case 21:

                    break;
                case 22:

                    break;
                case 23:

                    break;
                case 24:

                    break;
                case 25:

                    break;
                case 26:

                    break;
                case 27:

                    break;
                case 28:

                    break;

            }
        }
        
        
        if(roomNum == 0)
        {
            floorTilePlacer(true);
        }
        else
        {
            floorTilePlacer(false);
        }


   
}
    bool addMiddlePillar(Vector2Int newTopLeft, Vector2Int dimenVec, int scale, int pillarNum)
    {
        
        Vector2 middleBlock = new Vector2((dimenVec.x) / 2.0f, (dimenVec.y) / 2.0f);
        //middleBlock = newTopLeft + middleBlock + new Vector2Int(1,1);
        if (middleBlock.x != (int)middleBlock.x) // Middle not on X
        {
            if (middleBlock.y != (int)middleBlock.y)
            {
               // Debug.Log("X IS ODD, Y IS ODD");
                for (int i = (int)middleBlock.x - 2 - scale; i < middleBlock.x + 4 + scale; i++)
                {
                    for (int a = (int)middleBlock.y + 4 + scale; a > middleBlock.y - 3 - scale; a--)
                    {
                        if (floorPlan[newTopLeft.y + a][newTopLeft.x + i] != -1)
                        {
                            return false;
                        }
                    }
                }

                for (int i = (int)middleBlock.x - scale; i < middleBlock.x + 2 + scale; i++)
                {
                    for (int a = (int)middleBlock.y + 2 + scale; a > middleBlock.y - 1 - scale; a--)
                    {
                        floorPlan[newTopLeft.y + a][newTopLeft.x + i]  = pillarNum;
                    }
                }
                Debug.Log("COLORING");
                for (int i = (int)middleBlock.x - scale - 1; i < middleBlock.x + 3 + scale; i++)
                {
                    for (int a = (int)middleBlock.y + 3 + scale; a > middleBlock.y - 2 - scale; a--)
                    {
                        if (i == middleBlock.x - scale - 1) //Top 
                        {
                            if(a == middleBlock.y + 3 + scale)
                            {
                                floorPlan[i][a] = 15;
                                floorPlanRotation[i][a] = 90;
                            }
                            else if(a == middleBlock.y - 2 - scale - 1)
                            {
                                floorPlan[i][a] = 15;
                                floorPlanRotation[i][a] = 180; 
                            }
                        } else if (i == middleBlock.x + 3 + scale - 1) // Bot
                        {
                            if (a == middleBlock.y + 3 + scale)
                            {
                                floorPlan[i][a] = 15;
                            }
                            else if (a == middleBlock.y - 2 - scale - 1)
                            {
                                floorPlan[i][a] = 15;
                                floorPlanRotation[i][a] = -90;

                            }
                        } else if (a == middleBlock.y + 3 + scale) // Left
                        {
                            floorPlan[i][a] = 13;
                            floorPlanRotation[i][a] = 0;
                        } else if(a == middleBlock.y - 2 - scale - 1) // Right
                        {
                            floorPlan[i][a] = 13;
                            floorPlanRotation[i][a] = 180;
                        }
                    }
                }
                //floorPlan[(int) middleBlock.y][(int) middleBlock.x] = 1;


                return true;
            }
            //Debug.Log("X IS ODD, Y IS EVEN");

            for (int i = (int) Math.Ceiling(middleBlock.x) - 3 - scale; i < (int) Math.Ceiling(middleBlock.x) + 4 + scale; i++)
            {
                for (int a = (int) middleBlock.y + 3 + scale; a > (int)middleBlock.y - 3 - scale; a--)
                {
                    if (floorPlan[newTopLeft.y + a][newTopLeft.x + i] != -1)
                    {
                        return false;
                    }
                }
            }
            
            //floorPlan[(int)middleBlock.y][(int)middleBlock.x] = 1;

            return true;

        }
        else if (middleBlock.y != (int)middleBlock.y) // NO MIDDLE on x,  y
        {
           // Debug.Log("X IS EVEN, Y IS ODD"); 

            for (int i = (int)middleBlock.x - 3 - scale; i < middleBlock.x + 5 + scale; i++)
            {
                for (int a = (int) Math.Ceiling(middleBlock.y) + 3 + scale; a > (int) Math.Ceiling(middleBlock.y) - 4 - scale; a--)
                {
                    if (floorPlan[newTopLeft.y + a][newTopLeft.x + i] != -1)
                    {
                        return false;
                    }
                }
            }
            for (int i = (int)middleBlock.x - 1 - scale; i < middleBlock.x + 3 + scale; i++)
            {
                for (int a = (int)Math.Ceiling(middleBlock.y) + 1 + scale; a > (int)Math.Ceiling(middleBlock.y) - 2 - scale; a--)
                {
                    floorPlan[newTopLeft.y + a][newTopLeft.x + i] = pillarNum;
                }
            }
            //floorPlan[(int)middleBlock.y][(int)middleBlock.x] = 1;


            return true;
        }
        else //There is a middle
        {
           // Debug.Log("EVEN, EVEN");
            /* if (floorPlan[(int) middleBlock.y][(int) middleBlock.x] == -1 && floorPlan[(int)middleBlock.y - 1][(int)middleBlock.x] == -1 && floorPlan[(int)middleBlock.y - 1][(int)middleBlock.x + 1] == -1 && floorPlan[(int)middleBlock.y][(int)middleBlock.x + 1] == -1)
             {
             
                 floorPlan[(int)middleBlock.y][(int)middleBlock.x] = 1;
                 floorPlan[(int)middleBlock.y + 1][(int)middleBlock.x] = -3;
                 floorPlan[(int)middleBlock.y + 1][(int)middleBlock.x + 1] = -3;
                 floorPlan[(int)middleBlock.y][(int)middleBlock.x + 1] = -3;*/

            if (floorPlan[newTopLeft.y + (int)middleBlock.y][newTopLeft.x + (int)middleBlock.x] == -1 && floorPlan[newTopLeft.y + (int)middleBlock.y + 1][newTopLeft.x + (int)middleBlock.x] == -1 && floorPlan[newTopLeft.y + (int)middleBlock.y + 1][newTopLeft.x + (int)middleBlock.x + 1] == -1 && floorPlan[newTopLeft.y + (int)middleBlock.y][newTopLeft.x + (int)middleBlock.x + 1] == -1)
            {
                floorPlan[newTopLeft.y + (int)middleBlock.y][newTopLeft.x + (int)middleBlock.x] = pillarNum;
                floorPlan[newTopLeft.y + (int)middleBlock.y + 1][newTopLeft.x + (int)middleBlock.x] = pillarNum;
                floorPlan[newTopLeft.y + (int)middleBlock.y + 1][newTopLeft.x + (int)middleBlock.x + 1] = pillarNum;
                floorPlan[newTopLeft.y + (int)middleBlock.y][newTopLeft.x +  (int)middleBlock.x + 1] = pillarNum;
                return true;
            }
            else
            {
                return false;
            }
            /*if (floorPlan[(int)Math.Ceiling(middleBlock.y)][(int)Math.Ceiling(middleBlock.x)] == -1 && floorPlan[(int)Math.Floor(middleBlock.y)][(int)Math.Ceiling(middleBlock.x)] == -1 && floorPlan[(int)Math.Floor(middleBlock.y)][(int)Math.Floor(middleBlock.x)] == -1 && floorPlan[(int)Math.Ceiling(middleBlock.y)][(int)Math.Floor(middleBlock.x)] == -1)
            {
                floorPlan[(int)Math.Ceiling(middleBlock.y)][(int)Math.Ceiling(middleBlock.x)] = -3;
                floorPlan[(int)Math.Floor(middleBlock.y)][(int)Math.Ceiling(middleBlock.x)] = -3;
                floorPlan[(int)Math.Ceiling(middleBlock.y)][(int)Math.Floor(middleBlock.x)] = -3;
                floorPlan[(int)Math.Floor(middleBlock.y)][(int)Math.Floor(middleBlock.x)] = -3;
                Debug.Log("THIS HAPPENED");
                return true;
            }
            else
            {
                return false;
            }*/
        }
    }
    bool checkIfExit(Vector2Int point)
    {
        if (exitVectors.Contains(point))
        {
            return true;
        }
        for(int i =0;i < exitLocations.Count; i++)
        {
            if (point.Equals(exitLocations[i]))
            {
                return true;
            }
        }
        for (int i = 0; i < exitLocations2.Count; i++)
        {
            if (point.Equals(exitLocations2[i]))
            {
                return true;
            }
        }
        return point.Equals(originBlock) || point.Equals(originBlock2) || point.Equals(originConnectionBlock) || point.Equals(originConnectionBlock2);
    }
    bool divideRoomY()
    {
        int middlePoint = dimensions.x % 2 == 0 ? Random.value > 0.5 ? dimensions.x / 2 + 1 : dimensions.x / 2  : dimensions.x / 2;
        int scale = Random.Range(2, dimensions.y / 2 - 1);
        for(int i = 1; i < scale + 1; i++)
        {
            if(floorPlan[i][middlePoint] != -1)
            {
                return false;
            }
            if (floorPlan[floorPlan.Length - 1 - i][middlePoint] != -1)
            {
                return false;
            }
            
        }

        for (int i = 1; i < scale; i++)
        {
            floorPlan[i][middlePoint] = -5;
            floorPlan[floorPlan.Length - 1 - i][middlePoint] = -5;
        }
        return true;
        /*for (int i = dimensions.y - 1; i > dimensions.y / 2; i--)
        {
            if (floorPlan[i][middlePoint] != -1)
            {
                return false;
            }
        }*/
    }
    bool divideRoomX()
    {
        int middlePoint = dimensions.y % 2 == 0 ? Random.value > 0.5 ? dimensions.y / 2 + 1 : dimensions.y / 2  : dimensions.y / 2;
        int scale = Random.Range(2, dimensions.x / 2- 1);
        for(int i = 1; i < scale; i++)
        {
            if(floorPlan[middlePoint][i] != -1)
            {
                return false;
            }
            if (floorPlan[middlePoint][floorPlan[0].Length -1 - i] != -1)
            {
                return false;
            }
        }

        for (int i = 1; i < scale; i++)
        {
            floorPlan[middlePoint][i] = -5;
            floorPlan[middlePoint][floorPlan[0].Length - 1 - i] = -5;
        }
        return true;
        
    }
    bool roomDecorationY()
    {
        int start = Random.Range(3, dimensions.y / 2);
        if (curvedWallY(start,dimensions.x / 2 / 2, 1) && curvedWallY(start,dimensions.x - dimensions.x / 2/2 + 1, -1))
        {
            return true;
        }
        return false;
    }
    bool roomDecorationX()
    {
        int start = Random.Range(3, dimensions.x / 2);
        int curveDirection = 1;
        if (curvedWallX(start, dimensions.y / 2 / 2, curveDirection) && curvedWallX(start, dimensions.y - dimensions.y / 2 / 2 + 1, curveDirection*-1))
        {
            return true;
        }
        return false;
        
    }
    bool curvedWallY(int start,int xPosition, int curveDirection)
    {
        
        for(int i = start; i < dimensions.y - start; i++)
        {
            if(floorPlan[i][xPosition] != -1)
            {
                return false;
            }
            if (floorPlan[floorPlan.Length - 1 - i][xPosition] != -1)
            {
                return false;
            }
            
        }
        if (floorPlan[start][xPosition - curveDirection] != -1)
        {
            return false;
        }
        if (floorPlan[floorPlan.Length - 1 - start][xPosition - curveDirection] != -1)
        {
            return false;
        }
        for (int i = 0; i < 4; i++)
        {
            if (floorPlan[start][xPosition - curveDirection * i] != -1)
            {
                return false;
            }
            if (floorPlan[floorPlan.Length - 1 - start][xPosition - curveDirection * i] != -1)
            {
                return false;
            }
        }

        for (int i = start; i < dimensions.y - start; i++)
        {
            floorPlan[i][xPosition] = -5;
            floorPlan[floorPlan.Length - 1 - i][xPosition] = -5;
        }
        floorPlan[start][xPosition - curveDirection] = -5;
        floorPlan[floorPlan.Length - 1 - start][xPosition - curveDirection] = -5;
        return true;
           
    }
    bool curvedWallX(int start,int xPosition, int curveDirection)
    {
        
        for(int i = start; i < dimensions.x - start; i++)
        {
            if(floorPlan[xPosition][i] != -1)
            {
                return false;
            }
            if (floorPlan[xPosition][floorPlan[0].Length - 1 - i] != -1)
            {
                return false;
            }
            
        }
        for(int i = 0; i < 4; i++)
        {
            if(xPosition - curveDirection * i > floorPlan.Length - 1 || floorPlan[xPosition - curveDirection*i][start] != -1)
            {
                return false;
            }
            if (xPosition - curveDirection * i > floorPlan.Length - 1  || floorPlan[xPosition - curveDirection*i][floorPlan[0].Length - 1 - start] != -1)
            {
                return false;
            }
        }


        for (int i = start; i < dimensions.x - start; i++)
        {
            floorPlan[xPosition][i] = -5;
            floorPlan[xPosition][floorPlan[0].Length - 1 - i] = -5;
        }
        floorPlan[xPosition - curveDirection][start] = -5;
        floorPlan[xPosition - curveDirection][floorPlan[0].Length - 1 - start] = -5;
        return true;
           
    }
    bool floorHolesX()
    {
        int leftMiddle;
        int rightMiddle;

        if(dimensions.x % 2 == 0)
        {
            leftMiddle = dimensions.x % 2 == 0 ? dimensions.x / 2 - 1 : dimensions.x / 2 - 2;
            rightMiddle = dimensions.x % 2 == 0 ? dimensions.x / 2 + 2 : dimensions.x / 2 + 2;
        }
        else
        {
            leftMiddle = floorPlan[0].Length % 2 == 0 ? floorPlan[0].Length / 2 - 1 : floorPlan[0].Length / 2 - 2;
            rightMiddle = floorPlan[0].Length % 2 == 0 ? floorPlan[0].Length / 2 + 2 : floorPlan[0].Length / 2 + 2;
        }
        int start = Random.Range(3, dimensions.y / 2);
        int height = floorPlan.Length- start;
        for(int i = start; i < height; i++)
        {
            if(floorPlan[i][leftMiddle] != -1 || floorPlan[i][leftMiddle - 1] != -1)
            {
                return false;
            }
            if(floorPlan[i][rightMiddle] != -1 || floorPlan[i][rightMiddle + 1] != -1)
            {
                return false;
            }
        }
        for(int i = start; i < height; i++)
        {
            floorPlan[i][leftMiddle] = -2;
            floorPlan[i][leftMiddle - 1] = -2;

            floorPlan[i][rightMiddle] = -2;
            floorPlan[i][rightMiddle + 1] = -2;
        }
        return true;
    }
    bool floorHolesY()
    {
        int leftMiddle;
        int rightMiddle;

        if (dimensions.y % 2 == 0)
        {
            leftMiddle = dimensions.y % 2 == 0 ? dimensions.y / 2 - 1 : dimensions.y / 2 - 2;
            rightMiddle = dimensions.y % 2 == 0 ? dimensions.y / 2 + 2 : dimensions.y / 2 + 2;
        }
        else
        {
            leftMiddle = floorPlan.Length % 2 == 0 ? floorPlan.Length / 2 - 1 : floorPlan.Length / 2 - 2;
            rightMiddle = floorPlan.Length % 2 == 0 ? floorPlan.Length / 2 + 2 : floorPlan.Length / 2 + 2;
        }
        int start = Random.Range(3, dimensions.x / 2);
        int height = floorPlan[0].Length - start;
        for (int i = start; i < height; i++)
        {
            if (floorPlan[leftMiddle][i] != -1 || floorPlan[leftMiddle-1][i] != -1)
            {
                return false;
            }
            if (floorPlan[rightMiddle][i] != -1 || floorPlan[rightMiddle + 1][i] != -1)
            {
                return false;
            }
        }
        for (int i = start; i < height; i++)
        {
            floorPlan[leftMiddle][i] = -2;
            floorPlan[leftMiddle - 1][i] = -2;

            floorPlan[rightMiddle][i] = -2;
            floorPlan[rightMiddle + 1][i] = -2;
        }
        return true;
    }
    void floorTilePlacer(bool nonLibrary)// Tile Name + 10
    {
        for (int i = 0; i < floorPlan.Length; i++)
        {
            for (int a = 0; a < floorPlan[0].Length; a++)
            {
                if(floorPlan[i][a] == -1)
                {
                    if (i == 1) // Top Layer
                    {
                        if (a == 1) // Top Right 
                        {
                            floorPlan[i][a] = nonLibrary ? 15 : -10;
                            floorPlanRotation[i][a] = 90;
                        }
                        else if (a == floorPlan[0].Length - 2) // Top Left
                        {
                            floorPlan[i][a] = nonLibrary ? 15 : -10;
                            floorPlanRotation[i][a] = 180;
                        }
                        else
                        {
                            floorPlan[i][a] = nonLibrary ? 13 :  -10;
                            floorPlanRotation[i][a] = 90;
                        }

                    }
                    else if (i == floorPlan.Length - 2) // Bottom Layer
                    {
                        if (a == 1) // Bot Right
                        {
                            floorPlan[i][a] = nonLibrary ? 15 : -10;
                            floorPlanRotation[i][a] = 0;
                        }
                        else if (a == floorPlan[0].Length - 2) // Bot Left
                        {
                            floorPlan[i][a] = nonLibrary ? 15 : -10;
                            floorPlanRotation[i][a] = -90;
                        }
                        else
                        {
                            floorPlan[i][a] = nonLibrary ? 13 : -10;
                            floorPlanRotation[i][a] = -90;
                        }

                    }
                    else if (a == 1) // Left Side
                    {
                        floorPlan[i][a] = nonLibrary ? 13 : -10;
                        floorPlanRotation[i][a] = 0;
                    }
                    else if (a == floorPlan[0].Length - 2) // Right Side
                    {
                        floorPlan[i][a] = nonLibrary ? 13 : -10;
                        floorPlanRotation[i][a] = 180;
                    }
                    else // In Middle somewhere
                    {
                        if (nonLibrary)
                        {
                            float rand = Random.value;
                            while (rand < 1.00)
                            {

                                if (rand < 0.5)
                                {
                                    floorPlan[i][a] = 10;
                                    rand = 2;
                                }
                                else if (rand < 0.95)
                                {
                                    floorPlan[i][a] = 11;
                                    rand = 2;

                                }
                                else
                                {
                                    if (i + 1 < floorPlan.Length - 1 && i - 1 > -1 && a + 1 < floorPlan[0].Length - 2 && a - 1 > -1 && i < floorPlan.Length - 3 && a < floorPlan[0].Length - 3)
                                    {
                                        if (floorPlan[i + 1][a] == -1 && floorPlan[i + 1][a + 1] == -1 && floorPlan[i][a + 1] == -1 && floorPlan[i][a] == -1)
                                        {
                                            floorPlan[i][a] = 12;
                                            floorPlan[i + 1][a] = -2;
                                            floorPlan[i + 1][a + 1] = -2;
                                            floorPlan[i][a + 1] = -2;
                                            rand = 2;

                                        }
                                        else
                                        {
                                            rand = Random.value;
                                        }
                                    }
                                    else
                                    {
                                        rand = Random.value;

                                    }

                                }
                            }
                        }
                        

                    }
                }
                
            }
        }
    }
    public void combineME()
    {

        for(int b =0; b < roomParent.transform.childCount; b++)
        {
            GameObject currGO = roomParent.transform.GetChild(b).gameObject;
            if (!currGO.Equals(wallParent) && !currGO.Equals(floorParent) && !currGO.Equals(pathParent) && !currGO.Equals(decorationParent))   {
                if (currGO.transform.childCount > 0)
                {
                    Material tempMat = currGO.transform.GetChild(0).GetComponent<MeshRenderer>().material;
                    bool isEnabled = true;


                    Vector3 position = currGO.transform.position;
                    currGO.transform.position = Vector3.zero;

                    currGO.AddComponent<MeshFilter>();
                    currGO.AddComponent<MeshRenderer>();

                    MeshFilter[] meshFilters = currGO.GetComponentsInChildren<MeshFilter>();
                    CombineInstance[] combine = new CombineInstance[meshFilters.Length];

                    for (int a = 0; a < meshFilters.Length; a++)
                    {
                        combine[a].mesh = meshFilters[a].sharedMesh;
                        combine[a].transform = meshFilters[a].transform.localToWorldMatrix;
                        meshFilters[a].gameObject.SetActive(false);
                    }



                    Mesh m = new Mesh();
                    m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    currGO.transform.GetComponent<MeshFilter>().mesh = m;
                    CombineInstance[] secondCombine = new CombineInstance[meshFilters.Length];
                    currGO.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
                    currGO.transform.gameObject.SetActive(true);

                    currGO.transform.position = position;
                    currGO.transform.GetComponent<MeshRenderer>().material = tempMat;

                    if (currGO.Equals(pathParent))
                    {
                        currGO.AddComponent<BoxCollider>();
                        currGO.AddComponent<Rigidbody>();
                        currGO.transform.GetComponent<Rigidbody>().isKinematic = true;
                    }
                }
            }
            
            
            

        }

       
        
        
    }
}






