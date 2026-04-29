using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new AvlNode<TKey, TValue>(key, value);
    }

    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        RebalanceUpwards(newNode);
    }

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child)
    {
        RebalanceUpwards(parent ?? child);
    }

    protected override void RemoveNode(AvlNode<TKey, TValue> node)
    {
        if (node.Left == null)
        {
            AvlNode<TKey, TValue>? repl = node.Right;
            Transplant(node, repl);
            OnNodeRemoved(node.Parent, repl);
            return;
        }

        if (node.Right == null)
        {
            AvlNode<TKey, TValue>? repl = node.Left;
            Transplant(node, repl);
            OnNodeRemoved(node.Parent, repl);
            return;
        }

        AvlNode<TKey, TValue> repl = node.Right;
        while (repl.Left != null)
        {
            repl = repl.Left;
        }

        AvlNode<TKey, TValue>? reb = repl.Parent;
        if (repl.Parent != node)
        {
            Transplant(repl, repl.Right);
            repl.Right = node.Right;
            repl.Right.Parent = repl;
        }

        Transplant(node, repl);
        repl.Left = node.Left;
        repl.Left.Parent = repl;

        if (reb == node)
        {
            reb = repl;
        }

        OnNodeRemoved(reb, repl);
    }

    private int Height(AvlNode<TKey, TValue>? current)
    {
        return current?.Height ?? 0;
    }

    private void Update(AvlNode<TKey, TValue> current)
    {
        current.Height = (Height(current.Left) >= Height(current.Right)) ? Height(current.Left) + 1 : Height(current.Right) + 1;
    }

    private void RebalanceUpwards(AvlNode<TKey, TValue>? node)
    {
        while (node != null)
        {
            Update(node);

            int bf = Height(node.Left) - Height(node.Right);
            if (bf > 1)
            {
                AvlNode<TKey, TValue> left = node.Left!;
                if (Height(left.Left) < Height(left.Right))
                {
                    RotateLeft(left);
                    Update(left);
                    Update(left.Parent!);
                }

                RotateRight(node);

                AvlNode<TKey, TValue> newRoot = node.Parent!;
                Update(node);
                Update(newRoot);
                node = newRoot;
            }
            else if (bf < -1)
            {
                AvlNode<TKey, TValue> right = node.Right!;
                if (Height(right.Right) < Height(right.Left))
                {
                    RotateRight(right);
                    Update(right);
                    Update(right.Parent!);
                }

                RotateLeft(node);

                AvlNode<TKey, TValue> newRoot = node.Parent!;
                Update(node);
                Update(newRoot);
                node = newRoot;
            }

            node = node.Parent;
        }
    }
}
