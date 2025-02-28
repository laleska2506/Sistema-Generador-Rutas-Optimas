using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding
{
    //Estados del PathFinder
    public enum PathFinderStatus
    {
        NOT_INITIALISED,
        SUCCESS,
        FAILURE,
        RUNNING,
    }
    abstract public class PathFinder<T>
    {
        //No derivará de MonoBehaviour, es C# puro
        public delegate float CostFunction(T a, T b);
        public CostFunction HeuristicCost { get; set; }

        public CostFunction NodeTraversalCost { get; set; } //Costo de dirigirse de un nodo a otro

        //Por default, el status será No Inicializado
        public PathFinderStatus Status { get; private set; } = PathFinderStatus.NOT_INITIALISED;

        public Node<T> Start { get; private set; }
        public Node<T> Goal { get; private set; }

        //Acceder al nodo actual del PathFinder
        public PathFinderNode<T> CurrentNode { get; private set; }

        //Nodos descubiertos pero no explorados
        protected List<PathFinderNode<T>> openList = new List<PathFinderNode<T>>();

        //Nodos explorados
        protected List<PathFinderNode<T>> closedList = new List<PathFinderNode<T>>();

        //Toma una lista de Nodos y retorna el de menor FCost
        protected PathFinderNode<T> GetLeastCostNode(List<PathFinderNode<T>> nodeList)
        {
            int bestIndex = 0;
            float bestPriority = nodeList[0].FCost;
            for (int i = 1; i < nodeList.Count; i++)
            {
                if (bestPriority > nodeList[i].FCost) //Se queda con el menor y su indice
                {
                    bestPriority = nodeList[i].FCost;
                    bestIndex = i;
                }
            }
            PathFinderNode<T> n = nodeList[bestIndex];
            return n;
        }

        //Ver si un nodo está en la lista
        protected int IsInList(List<PathFinderNode<T>> nodeList, T cell)
        {
            for (int i = 0; i < nodeList.Count; ++i)
            {
                if (EqualityComparer<T>.Default.Equals(nodeList[i].Location.Value, cell)) return i;
            }
            return -1; //De no estar
        }

        #region Delegates
        //Usados para la representación gráfica de los cambios
        public delegate void DelegatePathFinderNode(PathFinderNode<T> node); //Estados del PathFinderNode
        public DelegatePathFinderNode onChangeCurrentNode;
        public DelegatePathFinderNode onAddToOpenList;
        public DelegatePathFinderNode onAddToClosedList;
        public DelegatePathFinderNode onDestinationFound;

        public delegate void DelegateNoArguments(); //Estados del PathFinder
        public DelegateNoArguments onStarted;
        public DelegateNoArguments onRunning;
        public DelegateNoArguments onFailure;
        public DelegateNoArguments onSuccess;

        #endregion

        //Resetea variables internas para nueva b�squeda
        protected void Reset()
        {
            if (Status == PathFinderStatus.RUNNING) return;

            CurrentNode = null;
            openList.Clear();
            closedList.Clear();

            Status = PathFinderStatus.NOT_INITIALISED;
        }

        //Pasos hasta �xito o Falla
        public PathFinderStatus Step()
        {
            closedList.Add(CurrentNode);
            onAddToClosedList?.Invoke(CurrentNode);

            //Como no hay nodos descubiertos y no explorados
            if (openList.Count == 0)
            {
                Status = PathFinderStatus.FAILURE;
                onFailure?.Invoke();
                return Status;
            }

            //Obtener menor coste de openList
            CurrentNode = GetLeastCostNode(openList); //Retorna el nodo con el menor costo F
            onChangeCurrentNode?.Invoke(CurrentNode);
            openList.Remove(CurrentNode);

            //Chekear si el nodo contiene el nodo destino
            if (EqualityComparer<T>.Default.Equals(CurrentNode.Location.Value, Goal.Value))
            {
                Status = PathFinderStatus.SUCCESS;
                onDestinationFound?.Invoke(CurrentNode);
                onSuccess?.Invoke();
                return Status;
            }

            //Encontrar vecinos y recorrerlos
            List<Node<T>> neighbours = CurrentNode.Location.GetNeighbours();

            //Recorrerlos con el algoritmo a implementar
            foreach (Node<T> cell in neighbours) AlgorithmImplementation(cell);

            Status = PathFinderStatus.RUNNING;
            onRunning?.Invoke();
            return Status;
        }

        abstract protected void AlgorithmImplementation(Node<T> cell);

        public bool Initialise(Node<T> start, Node<T> goal)
        {
            if (Status == PathFinderStatus.RUNNING) return false;
            Reset();
            Start = start;
            Goal = goal;

            float hCost = HeuristicCost(Start.Value, Goal.Value);
            PathFinderNode<T> root = new PathFinderNode<T>(Start, null, 0.0f, hCost);

            openList.Add(root);
            CurrentNode = root;

            onChangeCurrentNode?.Invoke(CurrentNode);
            onStarted?.Invoke();

            Status = PathFinderStatus.RUNNING;
            return true;
        }
    }
}
