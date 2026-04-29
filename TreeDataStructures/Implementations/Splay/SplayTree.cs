using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new BstNode<TKey, TValue>(key, value);
    }
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        Splay(parent ?? child);
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        BstNode<TKey, TValue>? node = FindNodeForAccess(key, out BstNode<TKey, TValue>? lastVisited);
        if (node != null)
        {
            value = node.Value;
            Splay(node);
            return true;
        }

        value = default;
        Splay(lastVisited);
        return false;
    }

    public override bool ContainsKey(TKey key)
    {
        BstNode<TKey, TValue>? node = FindNodeForAccess(key, out BstNode<TKey, TValue>? lastVisited);
        Splay(node ?? lastVisited);
        return node != null;
    }

    public override bool Remove(TKey key)
    {
        BstNode<TKey, TValue>? node = FindNodeForAccess(key, out BstNode<TKey, TValue>? lastVisited);
        if (node == null)
        {
            Splay(lastVisited);
            return false;
        }

        Splay(node);
        return base.Remove(key);
    }
    
    private void Splay(BstNode<TKey, TValue>? node)
    {
        while (node?.Parent != null)
        {
            if (node.Parent.Parent == null)
            {
                if (node.IsLeftChild)
                {
                    RotateRight(node.Parent);
                }
                else
                {
                    RotateLeft(node.Parent);
                }

                continue;
            }

            if (node.IsLeftChild && node.Parent.IsLeftChild)
            {
                RotateRight(node.Parent.Parent);
                RotateRight(node.Parent);
            }
            else if (node.IsRightChild && node.Parent.IsRightChild)
            {
                RotateLeft(node.Parent.Parent);
                RotateLeft(node.Parent);
            }
            else if (node.IsRightChild && node.Parent.IsLeftChild)
            {
                RotateBigRight(node.Parent.Parent);
            }
            else
            {
                RotateBigLeft(node.Parent.Parent);
            }
        }
    }

    private BstNode<TKey, TValue>? FindNodeForAccess(TKey key, out BstNode<TKey, TValue>? lastVisited)
    {
        BstNode<TKey, TValue>? current = Root;
        lastVisited = null;

        while (current != null)
        {
            lastVisited = current;

            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0)
            {
                return current;
            }

            current = cmp < 0 ? current.Left : current.Right;
        }

        return null;
    }
}
