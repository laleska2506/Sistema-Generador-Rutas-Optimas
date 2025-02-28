using PathFinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{

    #region Atributos
    [SerializeField]
    GameObject roomPrefab;

    //Array 2D de Rooms, representaro el grid del Maze
    Room[,] rooms = null;

    //Variables para la cantidad de Rooms en el eje XY
    [SerializeField]
    int numX = 10;
    [SerializeField]
    int numY = 10;

    //El ancho y alto del room
    float roomWidth;
    float roomHeight;

    //Stack para hacer backtracking
    Stack<Room> roomStack = new Stack<Room>();

    //Flag para progreso de generacion de maze, previene que otro maze se genere mientras uno esto en progreso
    bool isGenerating = false;

    #endregion

    #region Motodos

    private void GetRoomSize()
    {
        SpriteRenderer[] spriteRenderers =
            roomPrefab.GetComponentsInChildren<SpriteRenderer>(); //Obtiene componentes de Room

        //Valores min y max para el prefab del Room
        Vector3 minBounds = Vector3.positiveInfinity; //(Infinity, infinity, infinity)
        Vector3 maxBounds = Vector3.negativeInfinity;


        foreach (SpriteRenderer spRen in spriteRenderers)
        {
            //Devuelve valores monimos y moximos entre 2 vectores
            minBounds = Vector3.Min(
                minBounds, spRen.bounds.min);
            maxBounds = Vector3.Max(
                maxBounds, spRen.bounds.max);
        }

        //Determinar ancho y alto de Room
        roomWidth = maxBounds.x - minBounds.x;
        roomHeight = maxBounds.y - minBounds.y;
    }

    //Motodo para asegurar que la comara muestre todo el maze
    private void SetCamera()
    {
        Camera.main.transform.position = new Vector3(
            numX * (roomWidth - 1) / 2, //calcula eje X para la comara
            numY * (roomHeight - 1) / 2,
            -100.0f); //Ajustado para asegurar


        float min_value = Mathf.Min(numX * (roomWidth - 1), numY * (roomHeight - 1));
        Camera.main.orthographicSize = min_value * 0.75f;
    }

    //Cuando el programa empiece, se ejecutaro lo siguiente
    private void Start()
    {
        GetRoomSize();
        rooms = new Room[numX, numY];

        for (int i = 0; i < numX; ++i) //Coloca rooms en la grilla de 16x9
        {
            for (int j = 0; j < numY; ++j)
            {
                GameObject room = Instantiate(roomPrefab,
                    new Vector3(i * roomWidth, j * roomHeight, 0.0f),
                    Quaternion.identity); //Es decir, no tiene rotacion


                room.name = "Room_" + i.ToString() + "_" + j.ToString();
                rooms[i, j] = room.GetComponent<Room>();
                rooms[i, j].Index = new Vector2Int(i, j);
            }
        }

        SetCamera();
    }
    
    //Determinaro el tamaoo del room basondose en el tamaoo ya especificado

    //Remover una pared del maze para que pueda ser generado, el opuesto y el de la direccion
    private void RemoveRoomWall(int x, int y, Room.Directions direction)
    {
        if (direction != Room.Directions.NONE)
        {
            rooms[x, y].SetDirectionFlag(direction, false);
        }

        Room.Directions opposite = Room.Directions.NONE;
        switch (direction)
        {
            case Room.Directions.TOP:
                if (y < numY - 1) opposite = Room.Directions.BOTTOM; ++y;
                break;
            case Room.Directions.RIGHT:
                if (x < numX - 1) opposite = Room.Directions.LEFT; ++x;
                break;
            case Room.Directions.BOTTOM:
                if (y > 0) opposite = Room.Directions.TOP; --y;
                break;
            case Room.Directions.LEFT:
                if (x > 0) opposite = Room.Directions.RIGHT; --x;
                break;
        }
        if (opposite != Room.Directions.NONE)
        {
            rooms[x, y].SetDirectionFlag(opposite, false);
        }
    }

    //De una posicion, se consiguen los vecinos no visitados
    public List<Tuple<Room.Directions, Room>> GetNotVisitedNeightbours(int cx, int cy)
    {
        List<Tuple<Room.Directions, Room>> neighbours = new List<Tuple<Room.Directions, Room>>(); //Lista de vecinos no visitados

        foreach (Room.Directions dir in Enum.GetValues(typeof(Room.Directions))) //recorre todas las direcciones del enum
        {
            int x = cx;
            int y = cy;

            switch (dir)
            {
                case Room.Directions.TOP:
                    if (y < numY - 1)
                        ++y;
                    if (!rooms[x, y].isVisited) neighbours.Add(new Tuple<Room.Directions, Room>(Room.Directions.TOP, rooms[x, y]));
                    break;
                case Room.Directions.BOTTOM:
                    if (y > 0)
                        --y;
                    if (!rooms[x, y].isVisited) neighbours.Add(new Tuple<Room.Directions, Room>(Room.Directions.BOTTOM, rooms[x, y]));
                    break;
                case Room.Directions.LEFT:
                    if (x > 0)
                        --x;
                    if (!rooms[x, y].isVisited) neighbours.Add(new Tuple<Room.Directions, Room>(Room.Directions.LEFT, rooms[x, y]));
                    break;
                case Room.Directions.RIGHT:
                    if (x < numX - 1)
                        ++x;
                    if (!rooms[x, y].isVisited) neighbours.Add(new Tuple<Room.Directions, Room>(Room.Directions.RIGHT, rooms[x, y]));
                    break;
            }
        }
        return neighbours;
    }


    private bool GenerateStep()
    {
        if (roomStack.Count == 0) return true; //Se termino de generar el maze

        Room r = roomStack.Peek(); //Primer elemento de Stack
        var neighbours = GetNotVisitedNeightbours(r.Index.x, r.Index.y);

        //Si la lista esto con valores mayor a 1, generas un index random, accedes y
        if (neighbours.Count != 0)
        {
            var index = 0;
            if (neighbours.Count > 1) index = UnityEngine.Random.Range(0, neighbours.Count);

            var item = neighbours[index];
            Room neighbour = item.Item2; //es decir, el 2do elemento de la tupla, el room
            neighbour.isVisited = true;
            RemoveRoomWall(r.Index.x, r.Index.y, item.Item1);

            roomStack.Push(neighbour);
        }
        else
        {
            roomStack.Pop(); //Elimina el elemento al tope de la pila y lo devuelve
        }
        return false;
    }

    public void CreateMaze()
    {
        if (isGenerating) return; //si esto en progreso, no se hace nada
        Reset();
        RemoveRoomWall(0, 0, Room.Directions.BOTTOM); //Para asegurar que hay entrada al Maze en la esquina baja izquierda
        RemoveRoomWall(numX - 1, numY - 1, Room.Directions.RIGHT); //Lo mismo para la esquina superior derecha
        roomStack.Push(rooms[0, 0]); //Se pushea el room inferior izquierdo
        StartCoroutine(Coroutine_Generate());
    }

    IEnumerator Coroutine_Generate()
    {
        isGenerating = true;
        bool flag = false;

        while (!flag) //Se generan pasos mientras no se haya terminado el proceso
        {
            flag = GenerateStep();
            yield return new WaitForSeconds(0.05f);
        }
        isGenerating = false;
    }

    //Setea direcciones a true, indicando que estan intactas
    private void Reset()
    {
        for (int i = 0; i < numX; ++i)
        {
            for (int j = 0; j < numY; ++j)
            {
                rooms[i, j].SetDirectionFlag(Room.Directions.TOP, true);
                rooms[i, j].SetDirectionFlag(Room.Directions.BOTTOM, true);
                rooms[i, j].SetDirectionFlag(Room.Directions.RIGHT, true);
                rooms[i, j].SetDirectionFlag(Room.Directions.LEFT, true);
                rooms[i, j].isVisited = false;
            }
        }
    }

    //Actualiza cada vez que el usuario presione espacio en caso no exista una generacion en proceso
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isGenerating) CreateMaze();
        }
        if (Input.GetMouseButtonDown(0)) RaycastAndSetPosition();
    }

    #endregion

    #region PathFinding
    private Vector2Int startPosition = Vector2Int.zero;
    private Vector2Int goalPosition = Vector2Int.zero;

    //Ayuda a setear el goal y start en el maze dependiendo del input del mouse
    private void RaycastAndSetPosition()
    {
        Vector2 rayPosition = new Vector2(
            Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
            Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

        //Si se clickea un room, es retornado el objeto y accedido
        RaycastHit2D hit = Physics2D.Raycast(rayPosition, Vector2.zero, 0.0f);

        //Con cada click, el start se vuelve el goal, es decir, adquiere su posioion
        if (hit)
        {
            GameObject obj = hit.transform.gameObject;
            Room room = obj.GetComponent<Room>();
            rooms[startPosition.x, startPosition.y].ResetMarker();
            rooms[goalPosition.x, goalPosition.y].ResetMarker();

            startPosition.x = goalPosition.x;
            startPosition.y = goalPosition.y;

            goalPosition.x = room.Index.x;
            goalPosition.y = room.Index.y;

            rooms[startPosition.x, startPosition.y].SetStartMarker(); //Marker en Start
            rooms[goalPosition.x, goalPosition.y].SetGoalMarker(); //Marker en Goal

            FindPath();
        }
    }

    //Extrae los vecinos de un punto especofico
    public List<Node<Vector2Int>> GetNeighbours(int cx, int cy)
    {
        List<Node<Vector2Int>> neighbours = new List<Node<Vector2Int>>();
        foreach (Room.Directions direction in Enum.GetValues(typeof(Room.Directions)))
        {
            int x = cx;
            int y = cy;


            switch (direction)
            {
                case Room.Directions.TOP:
                    if (y < numY - 1)
                    {
                        if (!rooms[x, y].GetDirectionFlag(direction)) ++y; neighbours.Add(new RoomNode(new Vector2Int(x, y), this));
                    }
                    break;
                case Room.Directions.RIGHT:
                    if (x < numX - 1)
                    {
                        if (!rooms[x, y].GetDirectionFlag(direction)) ++x; neighbours.Add(new RoomNode(new Vector2Int(x, y), this));
                    }
                    break;
                case Room.Directions.BOTTOM:
                    if (y > 0)
                    {
                        if (!rooms[x, y].GetDirectionFlag(direction)) --y; neighbours.Add(new RoomNode(new Vector2Int(x, y), this));
                    }
                    break;
                case Room.Directions.LEFT:
                    if (x > 0)
                    {
                        if (!rooms[x, y].GetDirectionFlag(direction)) --x; neighbours.Add(new RoomNode(new Vector2Int(x, y), this));
                    }
                    break;
            }
        }
        return neighbours;
    }


    // Funciones para calcular costes
    public static float ManhattanCost(Vector2Int a, Vector2Int b) //0,2 y 9,3
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); //0-9  +  2-3
    }


    public static float CostBetweenTwoCells(Vector2Int a, Vector2Int b)
    {
        return Mathf.Sqrt(
            (a.x - b.x) * (a.x - b.x) +
            (a.y - b.y) * (a.y - b.y)
        );
    }


    public void OnChangeCurrentNode(PathFinderNode<Vector2Int> node)
    {
        int x = node.Location.Value.x;
        int y = node.Location.Value.y;
        rooms[x, y].SetFloorColor(Color.magenta);
    }


    public void OnAddToOpenList(PathFinderNode<Vector2Int> node)
    {
        int x = node.Location.Value.x;
        int y = node.Location.Value.y;
        rooms[x, y].SetFloorColor(Color.cyan);
    }


    public void OnAddToClosedList(PathFinderNode<Vector2Int> node)
    {
        int x = node.Location.Value.x;
        int y = node.Location.Value.y;
        rooms[x, y].SetFloorColor(Color.grey);
    }


    void ResetColor()
    {
        for (int i = 0; i < numX; i++)
        {
            for (int j = 0; j < numY; j++)
            {
                rooms[i, j].ResetFloor();
            }
        }
    }


    DijkstraAlgorithm<Vector2Int> pathFinder = new DijkstraAlgorithm<Vector2Int>();
    void FindPath()
    {
        if (pathFinder.Status == PathFinderStatus.RUNNING)
        {
            Debug.Log("PathFinder is running. Cannot start a new pathfinding now");
            return;
        }

        pathFinder.HeuristicCost = ManhattanCost;
        pathFinder.NodeTraversalCost = CostBetweenTwoCells;

        ResetColor();

        pathFinder.onAddToClosedList = OnAddToClosedList;
        pathFinder.onAddToOpenList = OnAddToOpenList;
        pathFinder.onChangeCurrentNode = OnChangeCurrentNode;

        pathFinder.Initialise(new RoomNode(startPosition, this),
            new RoomNode(goalPosition, this));
        StartCoroutine(Coroutine_FindPathStep());
    }

    IEnumerator Coroutine_FindPathStep()
    {
        while (pathFinder.Status == PathFinderStatus.RUNNING)
        {
            pathFinder.Step();
            yield return new WaitForSeconds(0.05f);
        }


        if (pathFinder.Status == PathFinderStatus.SUCCESS)
        {
            StartCoroutine(Coroutine_OnSuccessPathFinding());
        }


        if (pathFinder.Status == PathFinderStatus.FAILURE)
        {
            OnFailurePathFinding();
        }


        IEnumerator Coroutine_OnSuccessPathFinding()
        {
            PathFinderNode<Vector2Int> node = pathFinder.CurrentNode;
            List<Vector2Int> reverse_indices = new List<Vector2Int>();


            while (node != null)
            {
                reverse_indices.Add(node.Location.Value);
                node = node.Parent;
            }

            for (int i = reverse_indices.Count - 1; i >= 0; i--)
            {
                rooms[reverse_indices[i].x, reverse_indices[i].y].SetFloorColor(Color.black);
                yield return new WaitForSeconds(0.05f);
            }
        }

        void OnFailurePathFinding()
        {
            Debug.Log("Cannot find path as no valid path exists");
        }
    }


    #endregion
}
