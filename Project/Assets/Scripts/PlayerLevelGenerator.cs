// using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
    TODO:
    // 1. Add more variety to rooms (Pillars)
    // 2. Add more variety to enemies (Modifier?)
    3. Add more hazards
    4. Add more pickups
    // 5. Implement EXP/Levelling system
*/
public class PlayerLevelGenerator : MonoBehaviour
{
    public GameObject WallPrefab;
    public GameObject FloorPrefab;
    public GameObject PortalPrefab;
    public GameObject TrivialEnemyPrefab;
    public GameObject EnemyPrefab;
    public GameObject MediumEnemyPrefab;
    public GameObject HardEnemyPrefab;
    public GameObject BossEnemyPrefab;
    public GameObject GoldDoorPrefab;
    public GameObject GoldKeyPrefab;
    public GameObject SilverDoorPrefab;
    public GameObject SilverKeyPrefab;
    public GameObject HealthBoostPrefab;
    public GameObject HeartPickupPrefab;
    public GameObject PowerBoostPrefab;
    public GameObject SpeedBoostPrefab;
    public GameObject TrapPrefab;
    public GameObject FireBallPrefab;

    // private List<GameObject> SilverDoorsToUnlock;

    public int GridSize = 10;
    public int RoomCount = 5;
    public int TrivialEnemyCount = 3;
    public int EnemyCount = 3;
    public int MediumEnemyCount = 2;
    public int HardEnemyCount = 2;
    public int BossEnemyCount = 1;

    private int SilverDoorCount = 1;

    private int GoldKeyCount = 1;
    // private int SilverKeyCount = 2;
    private int HealthBoostCount = 2;
    private int PowerBoostCount = 1;
    private int SpeedBoostCount = 2;

    // private int TrapCount = 10;

    // private int LevelSize = 0;
    
    private bool introEnemySpawned = false;

    public List<RoomData> roomDatas = new List<RoomData>();
    private List<Vector3> floorPositions = new List<Vector3>();
    private List<Vector3> pickupPositions = new List<Vector3>();
    public List<SpecialTarget> specialTargets = new List<SpecialTarget>();
    public List<GameObject> trapPositions = new List<GameObject>();

    private SpecialSilverRoom specialSilverRoom;
    // private bool isSpecialRoomIllegal = false;

    public struct SpecialTarget {
        public GameObject Target;
        public bool IsShot;
        public bool IsComplete;
    }

    private struct SpecialSilverRoom {
        public List<GameObject> SilverDoorsToUnlock;
        public RoomData specialRoom;
    }

    public struct RoomData {
        public List<RoomConnector> Connectors;
        public RoomType RoomType;
        public GameObject RoomParent;
        public List<ExtraData> Extras;

    }

    public struct ExtraData {
        public GameObject extra;
        public bool isIllegal;
    }

    public struct RoomConnector {
        public Vector3 Position;
        // Where the connector is facing in the current room
        public RoomDirection FacingDirection; 
        // Where the connector will be connected to
        public RoomDirection ConnectedDirection;
        public bool Connected;
    }

    public enum RoomDirection {
        Up,
        Right,
        Down,
        Left
    };
    public enum RoomType {
        DeadEnd,
        Path,
        Fork,
        Hub
    };

    // private List<EnemyData> enemyDatas = new List<EnemyData>();

    private struct EnemyData {
        public EnemyType Type;
        public List<EnemyModifiers> Modifiers;
        public EnemyDifficulty Difficulty;
        public GameObject EnemyPrefab;
        public Vector3 Position;
    }

    private enum EnemyType {
        Default,
        Shotgun,
        Stalker
    }

    private enum EnemyModifiers {
        Normal,
        Fast,
        Strong,
        Tank,
        Sniper,
        Bombarder
    }

    private enum EnemyDifficulty {
        Trivial,
        Easy,
        Medium,
        Hard,
        Boss
    }
    

    void Start() {
        GenerateLevel();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Finish")) {
            RegenerateLevel();
        }
    } 

    public void RegenerateLevel() {
        foreach (RoomData room in roomDatas) {
            Destroy(room.RoomParent);
        }
        roomDatas.Clear();
        floorPositions.Clear();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload the current scene

        GenerateLevel();
    }
    
    void GenerateLevel() {
        specialSilverRoom = new SpecialSilverRoom {
            specialRoom = new RoomData(),
            SilverDoorsToUnlock = new List<GameObject>(),
        };

        // Initialise first room
        RoomData firstRoom = CreateRoom(RoomType.DeadEnd, new Vector3(GridSize/2, GridSize, 0), RoomDirection.Up, 0);
        roomDatas.Add(firstRoom);
        floorPositions.AddRange(firstRoom.Connectors.Select(c => c.Position));
        InstantiateRoom(firstRoom, floorPositions);
        
        Vector3 startingPosition = firstRoom.RoomParent.transform.position;

        transform.position = firstRoom.RoomParent.transform.position;
        transform.rotation = firstRoom.RoomParent.transform.rotation;

        for (int i = 1; i < RoomCount; i++) {
            RoomData lastRoom = roomDatas[i - 1];
            List<RoomConnector> lastConnectors = lastRoom.Connectors;
            RoomConnector lastConnector;

            RoomType roomType = ChooseRoomType(lastRoom, i);

            if (lastRoom.RoomType == RoomType.Hub) {
                lastConnector = ChooseLegalConnector(lastRoom, i);
            }
            else {
                lastConnector = lastConnectors[lastRoom.Connectors.Count-1];
            }

            RoomData newRoom = CreateRoom(roomType, lastConnector.Position, lastConnector.FacingDirection, i);
            roomDatas.Add(newRoom);

            floorPositions.AddRange(newRoom.Connectors.Select(c => c.Position));

            InstantiateRoom(newRoom, floorPositions);
            InstantiateEntities(newRoom, floorPositions, i);

        }

        Physics2D.SyncTransforms();

        foreach (RoomData room in roomDatas) {
            InstantiateWallsAround(room, floorPositions);
            // float distance = Vector3.Distance(room.RoomParent.transform.position, startingPosition);
            // if (LevelSize < distance) LevelSize = (int)distance;
        }

        DeleteIllegalExtras();
        Physics2D.SyncTransforms();

        // if (!isSpecialRoomIllegal)
        InstantiateSpecialRoom();

        RoomData finalRoom = roomDatas[RoomCount-1];
        PortalPrefab.transform.position = finalRoom.RoomParent.transform.position;
    }

    RoomData CreateRoom(RoomType roomType, Vector3 startingConnectorPosition, RoomDirection incomingDirection, int index) {
        GameObject roomParent = new GameObject($"Room-{roomType}-{index}");
        RoomData newRoom = new RoomData 
        {
            RoomType = roomType,
            RoomParent = roomParent,
            Connectors = new List<RoomConnector>(),
            Extras = new List<ExtraData>()
        };

        Vector3 basePosition = startingConnectorPosition - DirectionToOffset(incomingDirection) * GridSize/2;

        roomParent.transform.position = basePosition;

        switch (roomType) {
            case RoomType.DeadEnd:
                newRoom.Connectors.Add(new RoomConnector {
                    Position = startingConnectorPosition,
                    FacingDirection = OppositeDirection(incomingDirection),
                    ConnectedDirection = incomingDirection,
                    Connected = true
                });
                break;

            case RoomType.Path:
                newRoom.Connectors.Add(new RoomConnector {
                    Position = startingConnectorPosition,
                    FacingDirection = OppositeDirection(incomingDirection),
                    ConnectedDirection = incomingDirection,
                    Connected = true
                });

                newRoom.Connectors.Add(new RoomConnector {
                    Position = startingConnectorPosition - DirectionToOffset(incomingDirection) * GridSize,
                    FacingDirection = incomingDirection,
                    ConnectedDirection = OppositeDirection(incomingDirection),
                    Connected = true
                });
                break;

            case RoomType.Fork:
                newRoom.Connectors.Add(new RoomConnector {
                    Position = startingConnectorPosition,
                    FacingDirection = OppositeDirection(incomingDirection),
                    ConnectedDirection = incomingDirection,
                    Connected = true
                });
                RoomDirection forkDirection = (RoomDirection) (((int)incomingDirection + 1) % 4);
                newRoom.Connectors.Add(new RoomConnector {
                    Position = basePosition + DirectionToOffset(OppositeDirection(forkDirection)) * GridSize/2,
                    FacingDirection = forkDirection,
                    ConnectedDirection = OppositeDirection(forkDirection),
                    Connected = true
                });
                break;
            
            case RoomType.Hub:
                // Initial Connector
                newRoom.Connectors.Add(new RoomConnector {
                    Position = startingConnectorPosition,
                    FacingDirection = OppositeDirection(incomingDirection),
                    ConnectedDirection = incomingDirection,
                    Connected = true
                });

                int additionalConnectors = Random.Range(1,2);
                List<RoomDirection> availableDirections = System.Enum.GetValues(typeof(RoomDirection)).Cast<RoomDirection>().ToList();
                availableDirections.Remove(incomingDirection);
                availableDirections = availableDirections.OrderBy(x => Random.value).ToList(); 

                for (int j = 0; j < additionalConnectors; j++) {
                    RoomDirection direction = availableDirections[j];
                    newRoom.Connectors.Add(new RoomConnector {
                        Position = basePosition + DirectionToOffset(direction) * GridSize/2,
                        FacingDirection = OppositeDirection(direction),
                        ConnectedDirection = direction,
                        Connected = false
                    });
                }
                break;
        }
        return newRoom;
    }

    RoomType ChooseRoomType(RoomData lastRoom, int index) {
        if (index == RoomCount-1) return RoomType.DeadEnd;
        if (index == 1) return RoomType.Path;   // Soft prevention of overlap
        if (index == 2) return RoomType.Hub;    // Intro Enemy

        RoomType lastRoomType = lastRoom.RoomType;
        RoomType chosenRoomType = (RoomType)Random.Range(1, RoomType.GetValues(typeof(RoomType)).Length);
        RoomType secondLastRoomType = (index >= 2) ? roomDatas[index-2].RoomType : RoomType.DeadEnd;

        bool invalidTransition = false;
        if (secondLastRoomType == RoomType.Hub && lastRoomType == RoomType.Hub && chosenRoomType == RoomType.Hub) {
            invalidTransition = true;
        }
        else if ((lastRoomType == RoomType.Path || lastRoomType == RoomType.Fork) &&
            (secondLastRoomType == RoomType.Path || secondLastRoomType == RoomType.Fork) &&
            (chosenRoomType == RoomType.Path || chosenRoomType == RoomType.Fork)) {
                invalidTransition = true;
        }

        RoomData secondLastRoom = roomDatas[0];
        if (index > 2) {
            secondLastRoom = roomDatas[index-2];
            if (chosenRoomType == RoomType.Path &&
                lastRoom.Connectors[1].ConnectedDirection == secondLastRoom.Connectors[1].ConnectedDirection) {
                invalidTransition = true;
            }
        }

        while (invalidTransition) {
            chosenRoomType = (RoomType)Random.Range(1, RoomType.GetValues(typeof(RoomType)).Length);
            invalidTransition = false;
            if (secondLastRoomType == RoomType.Hub && lastRoomType == RoomType.Hub && chosenRoomType == RoomType.Hub) {
                invalidTransition = true;
            }
            else if ((lastRoomType == RoomType.Path || lastRoomType == RoomType.Fork) &&
                (secondLastRoomType == RoomType.Path || secondLastRoomType == RoomType.Fork) &&
                (chosenRoomType == RoomType.Path || chosenRoomType == RoomType.Fork)) {
                invalidTransition = true;
            }

            if (index > 2) {
                if (chosenRoomType == RoomType.Path &&
                lastRoom.Connectors[1].ConnectedDirection == secondLastRoom.Connectors[1].ConnectedDirection) {
                    // Debug.Log($"{lastRoom.RoomParent}:{lastRoom.Connectors[1].ConnectedDirection} Detected x2 {secondLastRoom.RoomParent}:{secondLastRoom.Connectors[1].ConnectedDirection}");
                    invalidTransition = true;
                }

            }
        }
        
        return chosenRoomType;
        
        // TODO: last room cannot intersect
    }

    RoomConnector ChooseLegalConnector(RoomData room, int index) {
        List<RoomConnector> validConnectors = new List<RoomConnector>();

        foreach (RoomConnector connector in room.Connectors) {
            bool overlapFound = false;

            foreach (RoomData otherRoom in roomDatas) {
                if (otherRoom.Equals(room)) continue;

                foreach (RoomConnector otherConnector in otherRoom.Connectors) {
                    if (Vector3.Distance(connector.Position, otherConnector.Position) < 1.5f) {
                        overlapFound = true;
                        break;
                    }
                }
                if (overlapFound) break;
            }

            if (!overlapFound) {

                // Prevent connector to connect to spawn room
                if (Vector3.Distance(connector.Position, roomDatas[0].RoomParent.transform.position) > 8f) {
                    validConnectors.Add(connector);
                }
            }
        }

        RoomConnector chosenConnector = chosenConnector = room.Connectors[Random.Range(1, room.Connectors.Count)];

        if (validConnectors.Count > 0)
            chosenConnector = validConnectors[Random.Range(0, validConnectors.Count)];
        
        // Debug.Log($"Room: {room.RoomParent} - Chosen Connector: {chosenConnector.ConnectedDirection}");

        int originalIndex = room.Connectors.FindIndex(c => c.Position == chosenConnector.Position);
        if (originalIndex != -1) {
            room.Connectors[originalIndex] = new RoomConnector {
                Position = chosenConnector.Position,
                FacingDirection = chosenConnector.FacingDirection,
                ConnectedDirection = chosenConnector.ConnectedDirection,
                Connected = true
            };
        }
        else {
            Debug.Log("Invalid Connector");
        }

        // List<RoomConnector> unconnectedConnectors = room.Connectors.Where(c => c.Connected == false).ToList();
        // foreach(RoomConnector con in room.Connectors.Where(c => c.Connected == false).ToList()) {
        //     Debug.Log($"Unconnected connector: {con.ConnectedDirection}");
        // }
        
        return chosenConnector;
    }

    Vector3 DirectionToOffset(RoomDirection dir) {
        switch (dir) {
            case RoomDirection.Up:      return new Vector3(0, 1, 0);
            case RoomDirection.Right:   return new Vector3(1, 0, 0);
            case RoomDirection.Down:    return new Vector3(0, -1, 0);
            case RoomDirection.Left:    return new Vector3(-1, 0, 0);
            default:                    return Vector3.zero;
        }
    }

    RoomDirection OppositeDirection(RoomDirection dir) {
        return (RoomDirection)(((int)dir + 2) % 4);
    }

    void InstantiateRoom(RoomData room, List<Vector3> floorPositions) {
        

        InstantiateConnectors(room, floorPositions);
        InstantiateFloors(room, floorPositions);
        InstantiateExtras(room, floorPositions);
    }

    void InstantiateEntities(RoomData room, List<Vector3> floorPositions, int index) {
        if (index > 1 && index < RoomCount-1) {
            InstantiateEnemies(room, index);
            InstantiateTraps(room, floorPositions, index);
        }
        
        if (index > 1) {
            InstantiatePickups(room, floorPositions, index);
        }
    }

    void InstantiateConnectors(RoomData room, List<Vector3> floorPositions) {
        GameObject connectorsParent = new GameObject("Connectors");
        connectorsParent.transform.parent = room.RoomParent.transform;
        connectorsParent.transform.position = room.RoomParent.transform.position;
        connectorsParent.transform.localPosition = Vector3.zero;

        foreach (RoomConnector connector in room.Connectors) {
            GameObject connectorObj = new GameObject($"Con--{connector.FacingDirection}-{connector.ConnectedDirection}");
            connectorObj.transform.parent = connectorsParent.transform;
            connectorObj.transform.position = connector.Position;
            connectorObj.layer = LayerMask.NameToLayer("Floor");

            BoxCollider2D collider = connectorObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            Instantiate(FloorPrefab, connector.Position, Quaternion.identity, connectorObj.transform);

            floorPositions.Add(connector.Position);

        }
    }

    void InstantiateFloors(RoomData room, List<Vector3> floorPositions) {
        GameObject floorsParent = new GameObject("Floors");
        floorsParent.transform.parent = room.RoomParent.transform;
        floorsParent.transform.position = room.RoomParent.transform.position;

        switch (room.RoomType) {
            case RoomType.DeadEnd:
                FillSquareRoom(floorsParent.transform, GridSize-1, floorPositions);
                break;
            case RoomType.Path:
                FillPathRoom(floorsParent.transform, room, floorPositions);
                break;
            case RoomType.Fork:
                FillForkRoom(floorsParent.transform, room, floorPositions);
                break;
            case RoomType.Hub:
                FillSquareRoom(floorsParent.transform, GridSize-1, floorPositions);
                break;
        }
    }

    void InstantiateExtras(RoomData room, List<Vector3> floorPositions) {
        if (room.RoomType != RoomType.Hub) return;
        int totalExtrasCount = 5;

        List<Vector3> currentRoomFloorPositions = floorPositions.Where(pos => Vector3.Distance(pos, room.RoomParent.transform.position) <= GridSize/2+1).ToList();

        GameObject extrasParent = new GameObject("Extras");
        extrasParent.transform.parent = room.RoomParent.transform;
        extrasParent.transform.position = room.RoomParent.transform.position;

        // Pillars
        for (int i = 0; i < Random.Range(2, 8); i++) {
            if (totalExtrasCount <= 0) break;
            Vector3 randomPosition = currentRoomFloorPositions[Random.Range(0, currentRoomFloorPositions.Count)];

            bool illegal = false;
            foreach (RoomConnector con in room.Connectors) {
                if (Vector3.Distance(randomPosition, con.Position) < 2f) illegal = true;
            }

            if (illegal) continue;

            GameObject pillar = new GameObject("Pillar");
            pillar.transform.parent = extrasParent.transform;
            pillar.transform.position = randomPosition;
            Instantiate(WallPrefab, pillar.transform.position, Quaternion.identity, pillar.transform);
            
            room.Extras.Add(new ExtraData {
                extra = pillar,
                isIllegal = false
            });
            totalExtrasCount--;
        }
        
        // // Walls (2 wallPrefabs together in random direction)
        // for (int i = 0; i < Random.Range(1, 3); i++) {
        //     if (totalExtrasCount <= 0) break;
        //     Vector3 randomPosition = currentRoomFloorPositions[Random.Range(0, currentRoomFloorPositions.Count)];

        //     foreach(RoomConnector con in room.Connectors) {
        //         if (Vector3.Distance(randomPosition, con.Position) < 3f) {
        //             continue;
        //         }
        //     }

        //     // GameObject wall = new GameObject("Wall");
        //     // wall.transform.parent = extrasParent.transform;
        //     // wall.transform.position = randomPosition;

        //     RoomDirection randomDirection = (RoomDirection)Random.Range(0, 4);

        //     GameObject wall1 = new GameObject("Wall1");
        //     wall1.transform.parent = wall.transform;
        //     wall1.transform.position = wall.transform.position;

        //     GameObject wall2 = new GameObject("Wall2");
        //     wall2.transform.parent = wall.transform;
        //     wall2.transform.position = wall.transform.position + DirectionToOffset(randomDirection);
            
        //     Instantiate(WallPrefab, wall1.transform.position, Quaternion.identity, wall1.transform);
        //     Instantiate(WallPrefab, wall2.transform.position, Quaternion.identity, wall2.transform);

        //     room.Extras.Add(new ExtraData {
        //         extra = wall1,
        //         isIllegal = false
        //     });
        //     room.Extras.Add(new ExtraData {
        //         extra = wall2,
        //         isIllegal = false
        //     });
        //     totalExtrasCount--;
        // }
    }

    void DeleteIllegalExtras() {
        foreach (RoomData room in roomDatas) {
            if (room.RoomType != RoomType.Hub) continue;

            bool illegal = false;

            for (int i = 0; i < room.Extras.Count(); i++) {
                if (Vector3.Distance(room.Extras[i].extra.transform.position, roomDatas[RoomCount-1].RoomParent.transform.position) < 5f) {
                    ExtraData tempExtra = new ExtraData {
                        extra = room.Extras[i].extra,
                        isIllegal = true
                    };
                    room.Extras[i] = tempExtra;
                    Destroy(room.Extras[i].extra);    

                    // Debug.Log($"2: {room.Extras[i].extra}");
                    continue; 
                }

                foreach (Transform extra in room.Extras[i].extra.transform) {
                    if (roomDatas.Any(r => r.Connectors.Any(c => Vector3.Distance(extra.position, c.Position) < 2f))) {
                        illegal = true;
                        continue;
                    }
                    if (pickupPositions.Any(p => Vector3.Distance(extra.position, p) < 1f)) {
                        illegal = true;
                        continue;
                    }
                    if (trapPositions.Any(t => Vector3.Distance(extra.position, t.transform.position) < 1f)) {
                        illegal = true;
                        continue;
                    }
                }
                
                if (illegal) {
                    ExtraData tempExtra2 = new ExtraData {
                        extra = room.Extras[i].extra,
                        isIllegal = true
                    };
                    room.Extras[i] = tempExtra2;
                    Destroy(room.Extras[i].extra);    

                    // Debug.Log($"2: {room.Extras[i].extra}");
                }
                
            }
        }
    
        // Delete special room if overlap with last room
        if (specialSilverRoom.specialRoom.RoomParent != null) {
            if (Vector3.Distance(specialSilverRoom.specialRoom.RoomParent.transform.position, roomDatas[RoomCount-1].RoomParent.transform.position) < 5f) {
                List<GameObject> moolash = specialSilverRoom.specialRoom.RoomParent.transform.Cast<Transform>().Where(t => t.gameObject.name.Contains("Moolah")).Select(t => t.gameObject).ToList();

                foreach (GameObject moolah in moolash) {
                    Destroy(moolah);
                }

                foreach (GameObject silverDoor in specialSilverRoom.SilverDoorsToUnlock) {
                    Destroy(silverDoor);
                }

                Destroy(specialSilverRoom.specialRoom.RoomParent);

                specialSilverRoom.specialRoom.RoomParent = null;
                // isSpecialRoomIllegal = true;
            }
        }
    }

    void InstantiateWallsAround(RoomData room, List<Vector3> floorPositions) {
        GameObject WallsParent = new GameObject("Walls");
        WallsParent.transform.parent = room.RoomParent.transform;
        WallsParent.transform.position = room.RoomParent.transform.position;

        List<Vector3> currentRoomFloorPositions = floorPositions.Where(pos => Vector3.Distance(pos, room.RoomParent.transform.position) <= GridSize/2+1).ToList();

        foreach (Vector3 position in currentRoomFloorPositions) {
            InstantiateWallsAroundPosition(WallsParent.transform, position, room);
            
        }

    }

    void InstantiateWallsAroundPosition(Transform WallsParent, Vector3 position, RoomData room)
    {
        int floorLayerMask = LayerMask.GetMask("Floor");

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector3 neighborPos = position + new Vector3(dx, dy, 0);

                bool occupied = false;
                
                Collider2D hit = Physics2D.OverlapBox(neighborPos, new Vector2(0.9f, 0.9f), 0f, floorLayerMask);
                if (hit != null) {
                    occupied = true;
                }
                else {
                    occupied |= room.Connectors.Any(c => c.Position == neighborPos);
                }

                if (!occupied) {
                    GameObject wall = new GameObject($"Wall-{dx}-{dy}");
                    wall.transform.parent = WallsParent;
                    wall.transform.position = neighborPos;

                    Instantiate(WallPrefab, wall.transform.position, Quaternion.identity, wall.transform);
                }

                
            }
        }
    }


    void FillSquareRoom(Transform parent, int size, List<Vector3> floorPositions) {
        int halfSize = size/2;
        for (int x = -halfSize; x <= halfSize; x++) {
            for (int y = -halfSize; y <= halfSize; y++) {
                GameObject floor = new GameObject($"Floor-{x}-{y}");
                floor.transform.parent = parent;
                floor.transform.position = parent.position + new Vector3(x, y, 0);
                floor.layer = LayerMask.NameToLayer("Floor");
                BoxCollider2D collider = floor.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;


                Instantiate(FloorPrefab, floor.transform.position, Quaternion.identity, floor.transform);

                floorPositions.Add(floor.transform.position);
            }
        }
    }

    void FillPathRoom(Transform parent, RoomData room, List<Vector3> floorPositions) {
        Vector3 direction = (room.Connectors[1].Position - room.Connectors[0].Position).normalized;

        int distance = Mathf.RoundToInt(Vector3.Distance(room.Connectors[0].Position, room.Connectors[1].Position));

        for (int i = 1; i < distance; i++) {
            Vector3 position = room.Connectors[0].Position + direction * i;

            GameObject floor = new GameObject($"Floor-{i}");
            floor.transform.parent = parent;
            floor.transform.position = position;

            floor.layer = LayerMask.NameToLayer("Floor");
            BoxCollider2D collider = floor.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            Instantiate(FloorPrefab, floor.transform.position, Quaternion.identity, floor.transform);

            floorPositions.Add(floor.transform.position);
        }
    }

    void FillForkRoom(Transform parent, RoomData room, List<Vector3> floorPositions) {
        // Center to First connector
        Vector3 start = room.Connectors[0].Position;
        Vector3 direction = (start - parent.position).normalized;
        int distance = Mathf.RoundToInt(Vector3.Distance(start, parent.position));

        for (int i = 0; i < distance; i++) {
            Vector3 position = parent.position + direction * i;

            GameObject floor = new GameObject($"Floor-1-{i}");
            floor.transform.parent = parent;
            floor.transform.position = position;
            floor.layer = LayerMask.NameToLayer("Floor");
            BoxCollider2D collider = floor.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            Instantiate(FloorPrefab, floor.transform.position, Quaternion.identity, floor.transform);

            floorPositions.Add(floor.transform.position);

        }

        // Center to Second connector
        start = room.Connectors[1].Position;
        direction = (start - parent.position).normalized;
        distance = Mathf.RoundToInt(Vector3.Distance(start, parent.position));

        for (int i = 1; i < distance; i++) {
            Vector3 position = parent.position + direction * i;

            GameObject floor = new GameObject($"Floor-2-{i}");
            floor.transform.parent = parent;
            floor.transform.position = position;
            floor.layer = LayerMask.NameToLayer("Floor");
            BoxCollider2D collider = floor.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            Instantiate(FloorPrefab, floor.transform.position, Quaternion.identity, floor.transform);

            floorPositions.Add(floor.transform.position);
        }

    }

    void InstantiateEnemies(RoomData room, int index) {
        if (room.RoomType != RoomType.Hub) return; 
        
        if (Vector3.Distance(room.RoomParent.transform.position, roomDatas[0].RoomParent.transform.position) < 10f) 
            return;
        
        float enemiesRemaining = TrivialEnemyCount + EnemyCount + MediumEnemyCount + HardEnemyCount + BossEnemyCount;
        float roomsRemaining = RoomCount-2 - index;

        float probability;
        float baseProbability = 0.25f;
        float steepness = 5f;

        // if (enemiesRemaining > roomsRemaining) probability = 1f;

        float progress = (float)(index - 2) / (RoomCount - 3); // Excluding first and last rooms

        probability = baseProbability + (1f - baseProbability) / (1f + Mathf.Exp(-steepness * (progress - 0.5f)));

        if (index == 2 && !introEnemySpawned && room.RoomType == RoomType.Hub) {
            probability = 1f;
            introEnemySpawned = true;
            // Debug.Log($"Intro Enemy @ {room.RoomParent}");
        }

        if (Random.value < probability && enemiesRemaining > 0) {
            EnemyData enemyData = new EnemyData {
                Position = room.RoomParent.transform.position,
                Type = (EnemyType)Random.Range(0, 3),
            };

            // Difficulty based on enemy count
            if (TrivialEnemyCount > 0) { 
                enemyData.Difficulty = EnemyDifficulty.Trivial; 
                enemyData.EnemyPrefab = TrivialEnemyPrefab;
                TrivialEnemyCount--;
            }
            else if (EnemyCount > 0) { 
                enemyData.Difficulty = EnemyDifficulty.Easy; 
                enemyData.EnemyPrefab = EnemyPrefab;
                EnemyCount--;
            }
            else if (MediumEnemyCount > 0) { 
                enemyData.Difficulty = EnemyDifficulty.Medium; 
                enemyData.EnemyPrefab = MediumEnemyPrefab;
                MediumEnemyCount--;
            }
            else if (HardEnemyCount > 0) { 
                enemyData.Difficulty = EnemyDifficulty.Hard; 
                enemyData.EnemyPrefab = HardEnemyPrefab;
                HardEnemyCount--;
            }
            else if (BossEnemyCount > 0) {
                // Boss Enemy
                enemyData.Type = EnemyType.Shotgun;
                enemyData.Modifiers = new List<EnemyModifiers> { 
                    EnemyModifiers.Fast,
                    EnemyModifiers.Strong,
                    EnemyModifiers.Bombarder,
                    EnemyModifiers.Tank 
                    };
                enemyData.Difficulty = EnemyDifficulty.Boss;
                enemyData.EnemyPrefab = BossEnemyPrefab;
                BossEnemyCount--;
                
            }

            List<EnemyModifiers> randomModifiers = new List<EnemyModifiers>();
            for (int i = 0; i < Random.Range(1, (int)enemyData.Difficulty); i++) {
                // Add random modifier if list does not already contain it
                EnemyModifiers randomModifier = (EnemyModifiers)Random.Range(1, 6);
                if (!randomModifiers.Contains(randomModifier)) randomModifiers.Add(randomModifier);
            }
            enemyData.Modifiers = randomModifiers;
            
            GameObject enemyParent = new GameObject($"Enemy-{enemyData.Type}-{enemyData.Difficulty}-{index}");
            enemyParent.transform.parent = room.RoomParent.transform;
            enemyParent.transform.position = enemyData.Position;
            if (enemyParent) {
                GameObject enemy = Instantiate(enemyData.EnemyPrefab, enemyParent.transform.position, Quaternion.identity, enemyParent.transform);

                EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
                EnemyChaseLogic enemyChaseLogic = enemy.GetComponent<EnemyChaseLogic>();
                EnemyShootLogic enemyShootLogic = enemy.GetComponent<EnemyShootLogic>();
                Kaching enemyKaching = enemy.GetComponent<Kaching>();

                enemyStats.Difficulty = (int)enemyData.Difficulty;
                enemyStats.AssignedDifficulty = enemyData.Difficulty.ToString();
                enemyStats.AssignedType = enemyData.Type.ToString();
                enemyStats.StartingHealth = 4 + 4*(int)enemyData.Difficulty;
                enemyChaseLogic.AggroRange = (enemyData.Type == EnemyType.Stalker) ? 20 : 8;
                enemyChaseLogic.DeaggroRange = (enemyData.Type == EnemyType.Stalker) ? 30 : 12;
                enemyChaseLogic.MoveSpeed = (enemyData.Type == EnemyType.Stalker) ? 2f : 3 + 0.5f*(int)enemyData.Difficulty;

                enemyShootLogic.Type = (enemyData.Type == EnemyType.Shotgun) ? EnemyShootLogic.ShotType.Shotgun : EnemyShootLogic.ShotType.Single;
                enemyShootLogic.Power = 1;
                enemyShootLogic.ShootCooldown = 1.5f - 0.25f*(int)enemyData.Difficulty;

                SpriteRenderer enemySpriteRenderer = enemy.GetComponent<SpriteRenderer>();
                Transform enemyTransform = enemy.GetComponent<Transform>();

                // Add Modifiers
                foreach (EnemyModifiers modifier in enemyData.Modifiers) {
                    enemyStats.Modifiers += (modifier + " ");
                    switch (modifier) {
                        case EnemyModifiers.Fast:
                            enemyChaseLogic.MoveSpeed *= 1.5f;
                            enemyShootLogic.Power /= 2;
                            enemySpriteRenderer.color = enemySpriteRenderer.color + new Color(1f, 1f, 0);    // +Yel
                            enemyTransform.localScale *= 0.75f;
                            break;
                        case EnemyModifiers.Strong:
                            enemyShootLogic.Power *= 2;
                            enemyChaseLogic.AggroRange /= 1.5f;
                            enemyChaseLogic.DeaggroRange /= 1.5f;
                            enemySpriteRenderer.color = enemySpriteRenderer.color + new Color(1f, 0, 0);    // +Red
                            enemyTransform.localScale *= 1.25f;
                            break;
                        case EnemyModifiers.Tank:
                            enemyStats.StartingHealth *= 2;
                            enemyChaseLogic.MoveSpeed /= 2f;
                            enemySpriteRenderer.color = enemySpriteRenderer.color + new Color(1f, 0, 1f);  // +Pur
                            enemyTransform.localScale *= 1.5f;
                            break;
                        case EnemyModifiers.Sniper:
                            enemyShootLogic.BulletSpeed *= 2;
                            // enemyChaseLogic.AggroRange *= 2;
                            enemySpriteRenderer.color = enemySpriteRenderer.color + new Color(0, 1f, 0);    // +Gre
                            break;
                        case EnemyModifiers.Bombarder:
                            enemyShootLogic.ShotgunBullets *= 3;
                            enemyShootLogic.ShotgunAngle *= 1.5f;
                            enemyShootLogic.ShootCooldown /= 1.5f;
                            enemyChaseLogic.MoveSpeed /= 2f;
                            enemySpriteRenderer.color = enemySpriteRenderer.color + new Color(0, 0, 1f);    // +Blu
                            enemyTransform.localScale *= 1.25f;
                            break;
                    }

                    // Bounds
                    if (enemyShootLogic.Power < 1) enemyShootLogic.Power = 1;   // Min Power
                    if (enemyShootLogic.Power > 3) enemyShootLogic.Power = 5;   // Max Power
                    if (enemyShootLogic.BulletSpeed < 1) enemyShootLogic.BulletSpeed = 1;   // Min BulletSpeed
                    if (enemyChaseLogic.MoveSpeed < 1) enemyChaseLogic.MoveSpeed = 1;   // Min MoveSpeed
                }


                // Debug.Log($"{enemyParent} spawned at {room.RoomParent}");
                // foreach (EnemyModifiers modifier in enemyData.Modifiers) {
                //     Debug.Log($"{enemyParent} Modifier: {modifier}");
                // }
            }
        }
        
    }

    void InstantiatePickups(RoomData room, List<Vector3> floorPositions, int index) {
        // Debug.Log($"Item Counts: {GoldKeyCount} {SilverKeyCount} {SilverDoorCount} {HealthBoostCount} {PowerBoostCount} {SpeedBoostCount}");
        if (index == RoomCount-1) {
            Instantiate(GoldDoorPrefab, room.Connectors[0].Position, Quaternion.identity, room.RoomParent.transform);
        }
        else {
            // if room overlaps with spawn room, skip
            if (Vector3.Distance(room.RoomParent.transform.position, roomDatas[0].RoomParent.transform.position) < 5f) 
                return;
            // Random position in room using floorPositions within room
            List<Vector3> roomFloorPositions = floorPositions.Where(pos => Vector3.Distance(pos, room.RoomParent.transform.position) <= GridSize/2-1).ToList();
            Vector3 randomPosition;

            if (GoldKeyCount > 0 && index > (RoomCount-index-2)/8) {
                if (Random.Range(1, (RoomCount-index-2)/8) == 1) {
                    randomPosition = roomFloorPositions[Random.Range(0, roomFloorPositions.Count)];
                    Instantiate(GoldKeyPrefab, randomPosition, Quaternion.identity, room.RoomParent.transform);
                    GoldKeyCount--;
                    pickupPositions.Add(randomPosition);
                    // Debug.Log($"Gold Key spawned @ {room.RoomParent}");
                    return; // Only one pickup per room
                }
            }

            if (SilverDoorCount > 0) {
                RoomData lastRoom = roomDatas[index-1];
                if (index > 5 &&
                    (room.RoomType == RoomType.Path || room.RoomType == RoomType.Fork) &&
                    lastRoom.RoomType == RoomType.Hub &&
                    roomDatas.Count(r => r.RoomParent.transform.position == room.RoomParent.transform.position) <= 1) {

                    if (Random.Range(1, 1) == 1) {
                        GameObject silverDoor1 = Instantiate(SilverDoorPrefab, room.Connectors[0].Position, Quaternion.identity, room.RoomParent.transform);
                        GameObject silverDoor2 = Instantiate(SilverDoorPrefab, room.Connectors[1].Position, Quaternion.identity, room.RoomParent.transform);

                        specialSilverRoom.SilverDoorsToUnlock.Add(silverDoor1);
                        specialSilverRoom.SilverDoorsToUnlock.Add(silverDoor2);
                        specialSilverRoom.specialRoom = lastRoom;

                        Kaching kaching = GetComponent<Kaching>();
                        for (int i = 0; i < Random.Range(3, 6); i++) {
                            randomPosition = roomFloorPositions[Random.Range(0, roomFloorPositions.Count)];
                            kaching.Drop(randomPosition, room.RoomParent);
                        }
                        SilverDoorCount--;
                        
                        // InstantiateSpecialRoom(roomDatas[index-1], floorPositions);
                    }
                }
                // 1x more pickup allowed in Silver room
            }

            if (HealthBoostCount > 0) {
                if (index == 2 || Random.Range(1, 10) == 1) {
                    randomPosition = roomFloorPositions[Random.Range(0, roomFloorPositions.Count)];
                    Instantiate(HealthBoostPrefab, randomPosition, Quaternion.identity, room.RoomParent.transform);      
                    HealthBoostCount--;   
                    pickupPositions.Add(randomPosition);    
                    return; // Only one pickup per room
       
                }
            }

            if (PowerBoostCount > 0) {
                if (Random.Range(1, 10) == 1) {
                    randomPosition = roomFloorPositions[Random.Range(0, roomFloorPositions.Count)];
                    Instantiate(PowerBoostPrefab, randomPosition, Quaternion.identity, room.RoomParent.transform);   
                    PowerBoostCount--;     
                    pickupPositions.Add(randomPosition);  
                    return; // Only one pickup per room
                }
            }

            if (SpeedBoostCount > 0) {
                if (Random.Range(1, 10) == 1) {
                    randomPosition = roomFloorPositions[Random.Range(0, roomFloorPositions.Count)];
                    Instantiate(SpeedBoostPrefab, randomPosition, Quaternion.identity, room.RoomParent.transform);   
                    SpeedBoostCount--;   
                    pickupPositions.Add(randomPosition);    
                    return; // Only one pickup per room
                }
            }
        }
    }

    void InstantiateSpecialRoom() {
        RoomData room = specialSilverRoom.specialRoom;
        if (room.RoomParent == null) return;

        // List<Vector3> roomFloorPositions = room.Connectors.Select(c => c.Position).ToList();
        // Vector3 randomPosition = roomFloorPositions[Random.Range(0, roomFloorPositions.Count)];
        
        // get list of Extras
        // GameObject extrasParent = room.RoomParent.transform.Find("Extras").gameObject;
        List<ExtraData> extras = room.Extras;
        List<GameObject> extraObjs = extras.Where(e => !e.isIllegal).Select(e => e.extra).ToList();

        // foreach(ExtraData extra in extras) {
        //     Debug.Log($"Extra: {extra.extra} Illegal: {extra.isIllegal}");
        // }
        int attempts = 0;

        Debug.Log($"Room: {room.RoomParent} Legal Extra Count: {extraObjs.Count}");
        while (extraObjs == null || extraObjs.Count == 0) {
            Debug.Log("Instantiating extras");
            List<Vector3> roomFloorPositions = floorPositions.Where(pos => Vector3.Distance(pos, room.RoomParent.transform.position) <= GridSize/2-1).ToList();
            InstantiateExtras(room, roomFloorPositions);

            extraObjs = extras.Where(e => !e.isIllegal).Select(e => e.extra).ToList();
            attempts++;
            if (attempts > 5) {
                Debug.Log("Failed to instantiate extras");
                break;
            }
        }

        int numOfExtras = extraObjs.Count;  
        if (numOfExtras == 0) return;

        // Debug.Log($"Number of Extras: {numOfExtras}");

        int numOfTargets = Random.Range(1, numOfExtras);
        // Debug.Log($"Number of Targets: {numOfTargets}");
        foreach(GameObject extra in extraObjs) {
            if (numOfTargets <= 0) break;

            GameObject extraObj = extra.transform.GetChild(0).gameObject;
            SpecialTargetLogic targetComp = extraObj.AddComponent<SpecialTargetLogic>();
            targetComp.playerLevelGenerator = this;
            extraObj.tag = "SpecialTarget";
            specialTargets.Add(new SpecialTarget {
                Target = extraObj,
                IsShot = false
            });
            numOfTargets--;
        }
    }

    void InstantiateTraps(RoomData room, List<Vector3> floorPositions, int index) {
        // Trap
        if (room.RoomType == RoomType.Hub && Random.Range(1, RoomCount/10) == 1) {
            // Random position in room using floorPositions within room
            List<Vector3> roomFloorPositions = floorPositions.Where(pos => Vector3.Distance(pos, room.RoomParent.transform.position) <= GridSize/2-1).ToList();
            Vector3 randomPosition = roomFloorPositions[Random.Range(0, roomFloorPositions.Count)];

            GameObject trap = Instantiate(TrapPrefab, randomPosition, Quaternion.identity, room.RoomParent.transform);
            // TrapCount--;

            trapPositions.Add(trap);
        }
    
        // Fireball
        if (Random.Range(1, RoomCount/5) == 1) {
            // Random position in room using floorPositions within room
            List<Vector3> roomFloorPositions = floorPositions.Where(pos => Vector3.Distance(pos, room.RoomParent.transform.position) <= GridSize/2-2).ToList();
            Vector3 randomPosition = roomFloorPositions[Random.Range(0, roomFloorPositions.Count)];

            GameObject trap = Instantiate(FireBallPrefab, randomPosition, Quaternion.identity, room.RoomParent.transform);
            // TrapCount--;

            trapPositions.Add(trap);
        } 
    }

    public bool CheckAllTargetsHit() {
        if (specialTargets.All(t => t.IsShot) || specialTargets.Count == 0) {
            // set all to complete
            specialTargets.ForEach(t => t.IsComplete = true);
            
            foreach(GameObject silverDoor in specialSilverRoom.SilverDoorsToUnlock) {
                Destroy(silverDoor);
            }
            // foreach (SpecialTarget target in specialTargets) {
            //     Destroy(target.Target);
            // }
            // specialTargets.Clear();
            return true;
        }
        return false;
    }

}