using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
{
    /// <summary>
    /// Разрезает дерево с корнем <paramref name="root"/> на два поддерева:
    /// Left: все ключи <= <paramref name="key"/>
    /// Right: все ключи > <paramref name="key"/>
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        
        if(root == null) return (null, null);

        int cmp = Comparer.Compare(root.Key, key);
        if(cmp <= 0)
        {
            var result = Split(root.Right, key);
            root.Right = result.Left;
            if(root.Right != null) root.Right.Parent = root;

            return (root, result.Right);
        } else
        {
            var result = Split(root.Left, key);
            root.Left = result.Right;
            if(root.Left != null) root.Left.Parent = root;

            return (result.Left, root);
        }

        /* throw new NotImplementedException("Implement Split operation"); */
    }

    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if(left == null) return right;
        if(right == null) return left;

        if(left.Priority >= right.Priority)
        {
            left.Right = Merge(left.Right, right);
            if(left.Right != null) left.Right.Parent = left;

            return left;
        } else
        {
            right.Left = Merge(left, right.Left);
            if(right.Left != null) right.Left.Parent = right;

            return right;
        }
        
        /* throw new NotImplementedException("Implement Merge operation"); */
    }

    public override void Add(TKey key, TValue value)
    {
        var exist = FindNode(key);
        if(exist != null)
        {
            exist.Value = value;
            return;
        }

        var result = Split(Root, key);
        var newNode = CreateNode(key, value);
        Root = Merge(Merge(result.Left, newNode), result.Right);
        if(Root != null) Root.Parent = null;
        
        Count++;
        OnNodeAdded(newNode);
        /* throw new NotImplementedException("Implement Add using Split and Merge"); */
    }

    public override bool Remove(TKey key)
    {
        var node = FindNode(key);
        if(node == null) return false;

        if(node.Left != null) node.Left.Parent = null;   

        var merged = Merge(node.Left, node.Right);

        if(node.Parent == null) Root = merged;
        else if(node.IsLeftChild) node.Parent.Left = merged;
        else node.Parent.Right = merged;

        if(merged != null) merged.Parent = node.Parent;

        Count--;
        OnNodeRemoved(node.Parent, merged);

        return true;
        
        /* throw new NotImplementedException("Implement Remove using Split and Merge"); */
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new TreapNode<TKey, TValue>(key, value);
    }
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode)
    {
        /* throw new NotImplementedException(); */
    }
    
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child)
    {
        /* throw new NotImplementedException(); */
    }
    
}