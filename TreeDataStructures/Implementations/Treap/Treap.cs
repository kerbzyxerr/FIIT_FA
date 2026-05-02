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
        if (root == null) return (null, null);

        int cmp = Comparer.Compare(root.Key, key);
        if (cmp <= 0)
        {
            var (leftOfRight, rightPart) = Split(root.Right, key);
            root.Right = leftOfRight;
            if (leftOfRight != null) leftOfRight.Parent = root;
            root.Parent = null;
            if (rightPart != null) rightPart.Parent = null;
            return (root, rightPart);
        }
        else
        {
            var (leftPart, rightOfLeft) = Split(root.Left, key);
            root.Left = rightOfLeft;
            if (rightOfLeft != null) rightOfLeft.Parent = root;
            root.Parent = null;
            if (leftPart != null) leftPart.Parent = null;
            return (leftPart, root);
        }
    }

    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null) return right;
        if (right == null) return left;

        if (left.Priority >= right.Priority)
        {
            var mergedRight = Merge(left.Right, right);
            left.Right = mergedRight;
            if (mergedRight != null) mergedRight.Parent = left;
            left.Parent = null;
            return left;
        }
        else
        {
            var mergedLeft = Merge(left, right.Left);
            right.Left = mergedLeft;
            if (mergedLeft != null) mergedLeft.Parent = right;
            right.Parent = null;
            return right;
        }
    }
    

    public override void Add(TKey key, TValue value)
    {
        var node = FindNode(key);
        if (node != null)
        {
            node.Value = value;
            return;
        }

        var newNode = CreateNode(key, value);
        var (leftPart, rightPart) = Split(Root, key);
        Root = Merge(Merge(leftPart, newNode), rightPart);
        if (Root != null) Root.Parent = null;
        Count++;
    }

    protected override void RemoveNode(TreapNode<TKey, TValue> node)
    {
        TreapNode<TKey, TValue>? merged = Merge(node.Left, node.Right);

        if (merged != null) merged.Parent =  node.Parent;

        if (node.Parent == null) Root = merged;
        else if (node.Parent.Left == node)  node.Parent.Left = merged;
        else node.Parent.Right = merged;

        if (Root != null) Root.Parent = null;
        
        OnNodeRemoved(node.Parent, merged);
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new TreapNode<TKey, TValue>(key, value);
    }
}
