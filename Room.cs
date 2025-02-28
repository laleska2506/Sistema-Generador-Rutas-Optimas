using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Room : MonoBehaviour
{
    #region Atributos

    public enum Directions
    {
        TOP,
        RIGHT,
        BOTTOM,
        LEFT,
        NONE,
    }

   //Objetos para las paredes, las variables privadas se serializan, haci�ndolas accesibles en Unity
    [SerializeField] GameObject topWall;
    [SerializeField] GameObject bottomWall;
    [SerializeField] GameObject leftWall;
    [SerializeField] GameObject rightWall;
    [SerializeField] SpriteRenderer floor;
    [SerializeField] SpriteRenderer marker;
    [SerializeField] Color NORMAL_COLOR = new Color(100.0f / 255.0f, 100.0f / 255.0f, 150.0f/255.0f);

    //Mapa para relacionar las paredes con sus direcciones
    Dictionary<Directions, GameObject> wallsSet =
        new Dictionary<Directions, GameObject>();

    //Indica si nuestras direcciones est�n activas o no
    Dictionary<Directions, bool> directionFlag =
        new Dictionary<Directions, bool>();

    #endregion

    #region Metodos

    //Propiedad que guardar� el indic� de Room en la grilla del juego
    public Vector2Int Index { get; set; }

    //Indica si una pared ha sido visitado o no, por default, es falso
    public bool isVisited { get; set; } = false;

    //Cuando el programa empieza, se configuran las direcciones con sus respectivas paredes
    private void Start()
    {
        wallsSet[Directions.TOP] = topWall;
        wallsSet[Directions.BOTTOM] = bottomWall;
        wallsSet[Directions.LEFT] = leftWall;
        wallsSet[Directions.RIGHT] = rightWall;
    }

    //Activa o no, una pared con una direcci�n espec�fica
    private void SetWallFlag(Directions direction, bool flag)
    {
        wallsSet[direction].SetActive(flag);
    }

    //Activar o desactivar una direcci�n
    public void SetDirectionFlag(Directions direction, bool flag)
    {
        directionFlag[direction] = flag;
        SetWallFlag(direction, flag);
    }

    public bool GetDirectionFlag(Directions direction)
    {
        return directionFlag[direction];
    }

    public void SetFloorColor(Color color)
    {
        floor.color = color;
    }

    public void SetStartMarker()
    {
        marker.gameObject.SetActive(true);
        marker.color = Color.green;
    }

    public void SetGoalMarker()
    {
        marker.gameObject.SetActive(true);
        marker.color = Color.red;
    }


    public void ResetMarker()
    {
        marker.color = NORMAL_COLOR;
    }


    public void ResetFloor()
    {
        floor.color = NORMAL_COLOR;
    }
    #endregion

}
