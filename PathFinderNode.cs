using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinderNode<T>: System.IComparable<PathFinderNode<T>>
{
    public PathFinderNode<T> Parent { get; set; }
    public Node<T> Location { get; private set; } //x y : 0,1

    //Costes del node
    public float FCost { get; private set; } //Coste Total
    public float GCost { get; private set; } //Coste desde el inicio hasta nodo actual
    public float HCost { get; private set; } //Coste Heur�stico desde el nodo hasta el destino

    public PathFinderNode(Node<T> location, PathFinderNode<T> parent, float gCost, float hCost)
    {
        Location = location;
        Parent = parent;
        HCost = hCost;
        GCost = gCost;
        SetFCost();
    }

    public void SetFCost()
    {
        FCost = GCost + FCost;
    }

    //Compara el FCost de 2 nodos
    public int CompareTo(PathFinderNode<T> other)
    {
        if (other == null) return 1;
        return FCost.CompareTo(other.FCost); //Retorna 0 si son iguales, de ser mayor, un positivo
    }
}
