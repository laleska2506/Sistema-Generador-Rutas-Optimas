if (IsInList(closedList, cell.Value) == -1)
{
    float G = 0.0f;
    float H = HeuristicCost(cell.Value, Goal.Value);

    int idOList = IsInList(openList, cell.Value);

    if (idOList == -1)
    {
        PathFinderNode<T> n = new PathFinderNode<T>(cell, CurrentNode, G, H);
        openList.Add(n);
        onAddToOpenList?.Invoke(n);
    }
}
