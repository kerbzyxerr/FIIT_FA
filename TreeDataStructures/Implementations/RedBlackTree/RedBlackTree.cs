using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
{
    private bool _removeFixupIsLeftChild;

    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new RbNode<TKey, TValue>(key, value);
    }
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        RestoreAfterInsert(newNode);

        if (Root != null)
        {
            Root.Color = RbColor.Black;
        }
    }
    
    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        RestoreAfterRemove(parent, child);

        if (Root != null)
        {
            Root.Color = RbColor.Black;
        }
    }

    protected override void RemoveNode(RbNode<TKey, TValue> node)
    {
        RbNode<TKey, TValue> removedNode = node;
        RbColor removedColor = removedNode.Color;
        RbNode<TKey, TValue>? replacement;
        RbNode<TKey, TValue>? fixParent;
        bool fixupIsLeftChild;

        if (node.Left == null)
        {
            replacement = node.Right;
            fixParent = node.Parent;
            fixupIsLeftChild = node.IsLeftChild;

            Transplant(node, replacement);
        }
        else if (node.Right == null)
        {
            replacement = node.Left;
            fixParent = node.Parent;
            fixupIsLeftChild = node.IsLeftChild;

            Transplant(node, replacement);
        }
        else
        {
            removedNode = GetMinimum(node.Right);
            removedColor = removedNode.Color;
            replacement = removedNode.Right;

            if (removedNode.Parent == node)
            {
                fixParent = removedNode;
                fixupIsLeftChild = false;
            }
            else
            {
                fixParent = removedNode.Parent;
                fixupIsLeftChild = true;

                Transplant(removedNode, removedNode.Right);
                removedNode.Right = node.Right;
                removedNode.Right.Parent = removedNode;
            }

            Transplant(node, removedNode);
            removedNode.Left = node.Left;
            removedNode.Left.Parent = removedNode;
            removedNode.Color = node.Color;
        }

        if (removedColor == RbColor.Black)
        {
            _removeFixupIsLeftChild = replacement?.IsLeftChild ?? fixupIsLeftChild;
            OnNodeRemoved(fixParent, replacement);
        }
        else if (Root != null)
        {
            Root.Color = RbColor.Black;
        }
    }

    private void RestoreAfterInsert(RbNode<TKey, TValue> node)
    {
        while (node != Root && GetColor(node.Parent) == RbColor.Red)
        {
            if (node.Parent!.IsLeftChild)
            {
                RbNode<TKey, TValue>? uncle = node.Parent.Parent!.Right;
                if (GetColor(uncle) == RbColor.Red)
                {
                    node.Parent.Color = RbColor.Black;
                    uncle!.Color = RbColor.Black;
                    node.Parent.Parent.Color = RbColor.Red;
                    node = node.Parent.Parent;
                    continue;
                }

                if (node.IsRightChild)
                {
                    node = node.Parent;
                    RotateLeft(node);
                }

                node.Parent!.Color = RbColor.Black;
                node.Parent.Parent!.Color = RbColor.Red;
                RotateRight(node.Parent.Parent);
            }
            else
            {
                RbNode<TKey, TValue>? uncle = node.Parent.Parent!.Left;
                if (GetColor(uncle) == RbColor.Red)
                {
                    node.Parent.Color = RbColor.Black;
                    uncle!.Color = RbColor.Black;
                    node.Parent.Parent.Color = RbColor.Red;
                    node = node.Parent.Parent;
                    continue;
                }

                if (node.IsLeftChild)
                {
                    node = node.Parent;
                    RotateRight(node);
                }

                node.Parent!.Color = RbColor.Black;
                node.Parent.Parent!.Color = RbColor.Red;
                RotateLeft(node.Parent.Parent);
            }
        }
    }

    private void RestoreAfterRemove(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        RbNode<TKey, TValue>? current = child;
        RbNode<TKey, TValue>? currentParent = parent;
        bool currentIsLeftChild = _removeFixupIsLeftChild;

        while (current != Root && GetColor(current) == RbColor.Black)
        {
            if (currentParent == null)
            {
                break;
            }

            bool isLeftChild = current != null ? current.IsLeftChild : currentIsLeftChild;
            if (isLeftChild)
            {
                RbNode<TKey, TValue>? sibling = currentParent.Right;
                if (GetColor(sibling) == RbColor.Red)
                {
                    sibling!.Color = RbColor.Black;
                    currentParent.Color = RbColor.Red;
                    RotateLeft(currentParent);
                    sibling = currentParent.Right;
                }

                if (GetColor(sibling?.Left) == RbColor.Black &&
                    GetColor(sibling?.Right) == RbColor.Black)
                {
                    if (sibling != null)
                    {
                        sibling.Color = RbColor.Red;
                    }

                    current = currentParent;
                    currentParent = current.Parent;
                    currentIsLeftChild = current.IsLeftChild;
                }
                else
                {
                    if (GetColor(sibling?.Right) == RbColor.Black)
                    {
                        if (sibling?.Left != null)
                        {
                            sibling.Left.Color = RbColor.Black;
                        }

                        if (sibling != null)
                        {
                            sibling.Color = RbColor.Red;
                            RotateRight(sibling);
                        }

                        sibling = currentParent.Right;
                    }

                    if (sibling != null)
                    {
                        sibling.Color = currentParent.Color;
                    }

                    currentParent.Color = RbColor.Black;
                    if (sibling?.Right != null)
                    {
                        sibling.Right.Color = RbColor.Black;
                    }

                    RotateLeft(currentParent);
                    current = Root;
                }
            }
            else
            {
                RbNode<TKey, TValue>? sibling = currentParent.Left;
                if (GetColor(sibling) == RbColor.Red)
                {
                    sibling!.Color = RbColor.Black;
                    currentParent.Color = RbColor.Red;
                    RotateRight(currentParent);
                    sibling = currentParent.Left;
                }

                if (GetColor(sibling?.Left) == RbColor.Black &&
                    GetColor(sibling?.Right) == RbColor.Black)
                {
                    if (sibling != null)
                    {
                        sibling.Color = RbColor.Red;
                    }

                    current = currentParent;
                    currentParent = current.Parent;
                    currentIsLeftChild = current.IsLeftChild;
                }
                else
                {
                    if (GetColor(sibling?.Left) == RbColor.Black)
                    {
                        if (sibling?.Right != null)
                        {
                            sibling.Right.Color = RbColor.Black;
                        }

                        if (sibling != null)
                        {
                            sibling.Color = RbColor.Red;
                            RotateLeft(sibling);
                        }

                        sibling = currentParent.Left;
                    }

                    if (sibling != null)
                    {
                        sibling.Color = currentParent.Color;
                    }

                    currentParent.Color = RbColor.Black;
                    if (sibling?.Left != null)
                    {
                        sibling.Left.Color = RbColor.Black;
                    }

                    RotateRight(currentParent);
                    current = Root;
                }
            }
        }

        if (current != null)
        {
            current.Color = RbColor.Black;
        }
    }

    private RbNode<TKey, TValue> GetMinimum(RbNode<TKey, TValue> node)
    {
        while (node.Left != null)
        {
            node = node.Left;
        }

        return node;
    }

    private RbColor GetColor(RbNode<TKey, TValue>? node)
    {
        return node?.Color ?? RbColor.Black;
    }
}
