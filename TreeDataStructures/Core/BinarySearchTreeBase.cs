using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null) 
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }
    
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys
    {
        get
        {
            var keys = new List<TKey>(Count);
            foreach (var item in InOrder())
            {
                keys.Add(item.Key);    
            }

            return keys;
        }
    }
    public ICollection<TValue> Values
    {
        get
        {
            var values = new List<TValue>(Count);
            foreach (var item in InOrder())
            {
                values.Add(item.Value);    
            }

            return values;
        }
    }
    

    public virtual void Add(TKey key, TValue value)
    {
        if(Root == null)
        {
            Root = CreateNode(key, value);
            ++Count;
            OnNodeAdded(Root);
            return;
        }

        TNode? current = Root;
        TNode? parent = null;

        while(current != null)
        {
            parent = current;
            int cmp = Comparer.Compare(key, current.Key);
            if(cmp == 0)
            {
                current.Value = value;
                return;
            }
            if(cmp < 0) current = current.Left;
            else current = current.Right;
        }

        TNode newnode = CreateNode(key, value);
        newnode.Parent = parent;

        if(Comparer.Compare(key, parent!.Key) < 0) parent.Left = newnode;
        else parent.Right = newnode;
        ++Count;
        OnNodeAdded(newnode);
        /* throw new NotImplementedException(
            "Implement standard BST add logic using <CreateNode(key, value)> and OnNodeAdded(newNode)");
        */
    }

    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        this.Count--;
        return true;
    }
      
    protected virtual void RemoveNode(TNode node)
    {

        if(node.Left == null)
        {
            Transplant(node, node.Right);
            OnNodeRemoved(node.Parent, node.Right);
        } else if(node.Right == null)
        {
            Transplant(node, node.Left);
            OnNodeRemoved(node.Parent, node.Left);
        } else
        {
            TNode repl = node.Right;
            while(repl.Left != null) repl = repl.Left;

            if(repl.Parent != node)
            {
                Transplant(repl, repl.Right);
                repl.Right = node.Right;
                repl.Right.Parent = repl;
            }

            Transplant(node, repl);
            repl.Left = node.Left;
            repl.Left.Parent = repl;

            OnNodeRemoved(node.Parent, repl);
        }

       /*  throw new NotImplementedException("Implement standard BST delete logic using Transplant helper"); */
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;
    
    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        TNode? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    
    #region Hooks
    
    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode) { }
    
    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }
    
    #endregion
    
    
    #region Helpers
    protected abstract TNode CreateNode(TKey key, TValue value);
    
    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) { return current; }
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }

    protected void RotateLeft(TNode x)
    {
        if(x.Right == null) return;

        TNode y = x.Right;

        x.Right = y.Left;
        if(y.Left != null) y.Left.Parent = x;

        Transplant(x, y);
        x.Parent = y;
        y.Left = x;
    }

    protected void RotateRight(TNode y)
    {
        if(y.Left == null) return;

        TNode x = y.Left;

        y.Left = x.Right;
        if(y.Left != null) y.Left.Parent = y;

        Transplant(y, x);
        x.Right = y;
        y.Parent = x;
    }
    
    protected void RotateBigLeft(TNode x)
    {
        if(x.Right == null) return;
        RotateRight(x.Right);
        RotateLeft(x);
        /* throw new NotImplementedException(); */
    }
    
    protected void RotateBigRight(TNode y)
    {
        if(y.Left == null) return;
        RotateLeft(y.Left);
        RotateRight(y);
        /* throw new NotImplementedException(); */
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        RotateLeft(x);
        RotateLeft(x);
        /* throw new NotImplementedException(); */
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        RotateRight(y);
        RotateRight(y); 
        /* throw new NotImplementedException(); */
    }
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v?.Parent = u.Parent;
    }
    #endregion
    

    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrder() => new TreeIterator(Root, TraversalStrategy.PreOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrderReverse() => new TreeIterator(Root, TraversalStrategy.PreOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrder() => new TreeIterator(Root, TraversalStrategy.InOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrderReverse() => new TreeIterator(Root, TraversalStrategy.InOrderReverse);
    
    /* private IEnumerable<TreeEntry<TKey, TValue>>  InOrderTraversal(TNode? node)
    {
        if (node == null) {  yield break; }
        throw new NotImplementedException();
    } */
    
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrder() => new TreeIterator(Root, TraversalStrategy.PostOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrderReverse() => new TreeIterator(Root, TraversalStrategy.PostOrderReverse);
    
    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
   
   private readonly struct StackEntry(TNode node, int depth, bool isvisited = false)
    {
        public readonly TNode Node = node;
        public readonly int Depth = depth;
        public readonly bool IsVisited = isvisited;
    }
    private struct TreeIterator : 
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        private TNode? _root;
        private readonly TraversalStrategy _strategy; // or make it template parameter?
        private Stack<StackEntry> _stack;
        private TNode? _IOcursor;
        private int _IOcursor_depth;
    
        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;  
        public TreeEntry<TKey, TValue> Current {get; private set;}

        IEnumerator IEnumerable.GetEnumerator() => this;
        object IEnumerator.Current => Current;
        
        
        public TreeIterator(TNode? root, TraversalStrategy strategy)
        {
            _root = root;
            _strategy = strategy;
            _stack = new Stack<StackEntry>();
            _IOcursor = null;
            _IOcursor_depth = 0;
            Current = default;
            Reset();
        }

        public bool MoveNext()
        {
            if(_root == null) return false;

            return _strategy switch
            {
                TraversalStrategy.InOrder => MoveNextInOrder(),
                TraversalStrategy.PreOrder => MoveNextPreOrder(),
                TraversalStrategy.InOrderReverse => MoveNextInOrderReverse(),
                TraversalStrategy.PreOrderReverse => MoveNextPreOrderReverse(),
                TraversalStrategy.PostOrder => MoveNextPostOrder(),
                TraversalStrategy.PostOrderReverse => MoveNextPostOrderReverse(),
                _ => throw new NotImplementedException("Strategy not implemented")
            };
        }
        private bool MoveNextPreOrder()
        {

            if(_stack.Count == 0) return false;
            
            StackEntry entry = _stack.Pop();
            Current = new TreeEntry<TKey, TValue>(entry.Node.Key, entry.Node.Value, entry.Depth);

            if(entry.Node.Right != null) _stack.Push(new StackEntry(entry.Node.Right, entry.Depth + 1));
            if(entry.Node.Left != null) _stack.Push(new StackEntry(entry.Node.Left, entry.Depth + 1));

            return true;
        }
        private bool MoveNextPreOrderReverse()
        {
            if(_stack.Count == 0) return false;

            StackEntry entry = _stack.Pop();
            Current = new TreeEntry<TKey, TValue>(entry.Node.Key, entry.Node.Value, entry.Depth);

            if(entry.Node.Left != null) _stack.Push(new StackEntry(entry.Node.Left, entry.Depth + 1));
            if(entry.Node.Right != null) _stack.Push(new StackEntry(entry.Node.Right, entry.Depth + 1));

            return true;
        }
        private bool MoveNextInOrder()
        {
            while(_IOcursor != null)
            {
                _stack.Push(new StackEntry(_IOcursor, _IOcursor_depth++));
                _IOcursor = _IOcursor.Left;
            }

            if(_stack.Count == 0) return false;

            StackEntry entry = _stack.Pop();
            Current = new TreeEntry<TKey, TValue>(entry.Node.Key, entry.Node.Value, entry.Depth);

            _IOcursor = entry.Node.Right;
            _IOcursor_depth = entry.Depth + 1;

            return true;
        }
        private bool MoveNextInOrderReverse()
        {
            while(_IOcursor != null)
            {
                _stack.Push(new StackEntry(_IOcursor, _IOcursor_depth++));
                _IOcursor = _IOcursor.Right;
            }

            if(_stack.Count == 0) return false;

            StackEntry entry = _stack.Pop();
            Current = new TreeEntry<TKey, TValue>(entry.Node.Key, entry.Node.Value, entry.Depth);

            _IOcursor = entry.Node.Left;
            _IOcursor_depth = entry.Depth + 1;

            return true;
        }
        private bool MoveNextPostOrder()
        {
            while(_stack.Count > 0)
            {
                StackEntry entry = _stack.Pop();

                if(entry.IsVisited)
                {
                    Current = new TreeEntry<TKey, TValue>(entry.Node.Key, entry.Node.Value, entry.Depth);
                    return true;
                }

                _stack.Push(new StackEntry(entry.Node, entry.Depth, true));

                if(entry.Node.Right != null) _stack.Push(new StackEntry(entry.Node.Right, entry.Depth + 1, false));
                if(entry.Node.Left != null) _stack.Push(new StackEntry(entry.Node.Left, entry.Depth + 1, false));
            }

            return false;
        }
        private bool MoveNextPostOrderReverse()
        {
            while(_stack.Count > 0)
            {
                StackEntry entry = _stack.Pop();

                if(entry.IsVisited)
                {
                    Current = new TreeEntry<TKey, TValue>(entry.Node.Key, entry.Node.Value, entry.Depth);
                    return true;
                }

                _stack.Push(new StackEntry(entry.Node, entry.Depth, true));

                if(entry.Node.Left != null) _stack.Push(new StackEntry(entry.Node.Left, entry.Depth + 1, false));
                if(entry.Node.Right != null) _stack.Push(new StackEntry(entry.Node.Right, entry.Depth + 1, false));
            }

            return false;
        }
        public void Reset()
        {
            _stack.Clear();
            _IOcursor = null;
            _IOcursor_depth = 0;
            Current = default;

            if (_root == null)
                return;

            switch (_strategy)
            {
                case TraversalStrategy.PreOrder:
                case TraversalStrategy.PreOrderReverse:
                    _stack.Push(new StackEntry(_root, 0));
                    break;

                case TraversalStrategy.InOrder:
                case TraversalStrategy.InOrderReverse:
                    _IOcursor = _root;
                    _IOcursor_depth = 0;
                    break;

                case TraversalStrategy.PostOrder:
                case TraversalStrategy.PostOrderReverse:
                    _stack.Push(new StackEntry(_root, 0, false));
                    break;

                default:
                    throw new NotSupportedException($"Reset is not supported for '{_strategy}'.");
            }
        }        
        public void Dispose() {}
    }
    
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        var items = new List<KeyValuePair<TKey, TValue>>(Count);
        foreach (var item in InOrder())
        {
            items.Add(new KeyValuePair<TKey, TValue>(item.Key, item.Value));
        }

        return items.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        TNode? node = FindNode(item.Key);
        if(node == null) return false;

        return true;
    }
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if(array == null) throw new ArgumentNullException(nameof(array));
        if(arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if(array.Length - arrayIndex < Count) throw new ArgumentException("The destination array has insufficient space.");
        
        int i = arrayIndex;
        foreach (var item in InOrder())
        {
            array[i++] = new KeyValuePair<TKey, TValue>(item.Key, item.Value);
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        TNode? node = FindNode(item.Key);
        if(node == null) return false;

        if(Comparer<TValue>.Default.Compare(node.Value, item.Value) != 0) return false;

        RemoveNode(node);
        Count--;

        return true;
    }
}
