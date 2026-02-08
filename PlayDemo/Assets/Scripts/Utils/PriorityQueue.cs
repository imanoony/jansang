public class PriorityQueue<T>
{
    private struct Node
    {
        public T Item;
        public float Priority;
    }

    private Node[] heap;
    private int count;

    public int Count => count;

    public PriorityQueue(int capacity)
    {
        heap = new Node[capacity];
        count = 0;
    }

    public void Clear()
    {
        count = 0;
    }

    public void Enqueue(T item, float priority)
    {
        int i = count++;
        heap[i].Item = item;
        heap[i].Priority = priority;

        // Heapify Up
        while (i > 0)
        {
            int parent = (i - 1) >> 1;
            if (heap[parent].Priority <= heap[i].Priority)
                break;

            Swap(i, parent);
            i = parent;
        }
    }

    public T Dequeue()
    {
        T result = heap[0].Item;
        count--;

        heap[0] = heap[count];

        // Heapify Down
        int i = 0;
        while (true)
        {
            int left = (i << 1) + 1;
            int right = left + 1;

            if (left >= count)
                break;

            int smallest = left;
            if (right < count && heap[right].Priority < heap[left].Priority)
                smallest = right;

            if (heap[i].Priority <= heap[smallest].Priority)
                break;

            Swap(i, smallest);
            i = smallest;
        }

        return result;
    }

    private void Swap(int a, int b)
    {
        Node tmp = heap[a];
        heap[a] = heap[b];
        heap[b] = tmp;
    }
}