if (IsInList(closedList, cell.Value) == -1)
{
    float G = CurrentNode.GCost + NodeTraversalCost(
      CurrentNode.Location.Value, cell.Value);
    float H = HeuristicCost(cell.Value, Goal.Value);

    int idOList = IsInList(openList, cell.Value);

    if (idOList == -1)
    {
        PathFinderNode<T> n = new PathFinderNode<T>(cell, CurrentNode, G, H);
        openList.Add(n);
        onAddToOpenList?.Invoke(n);
    }
    else
    {
        float oldG = openList[idOList].GCost;
        if (G < oldG)
        {
            openList[idOList].Parent = CurrentNode;
            openList[idOList].SetFCost();
            onAddToOpenList?.Invoke(openList[idOList]);
        }
    }
}

