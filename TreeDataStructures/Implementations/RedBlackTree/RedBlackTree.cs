using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
{
    private bool _removedWasLeftChild;

    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new RbNode<TKey, TValue>(key, value);
    }
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        RestoreAfterInsert(newNode);

        if(Root != null) Root.Color = RbColor.Black;
    }
    
    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        RestoreAfterRemove(parent, child);

        if(Root != null) Root.Color = RbColor.Black;
    }

    protected override void RemoveNode(RbNode<TKey, TValue> node)
    {
        if(node.Left == null)
        {
            RbNode<TKey, TValue>? child = node.Right;
            bool wasLeftChild = node.IsLeftChild;
            Transplant(node, child);

            if(node.Color == RbColor.Black)
            {
                _removedWasLeftChild = child?.IsLeftChild ?? wasLeftChild;
                OnNodeRemoved(node.Parent, child);
            }
            else if(Root != null) Root.Color = RbColor.Black;
        }
        else if(node.Right == null)
        {
            RbNode<TKey, TValue>? child = node.Left;
            bool wasLeftChild = node.IsLeftChild;
            Transplant(node, child);

            if(node.Color == RbColor.Black)
            {
                _removedWasLeftChild = child?.IsLeftChild ?? wasLeftChild;
                OnNodeRemoved(node.Parent, child);
            }
            else if(Root != null) Root.Color = RbColor.Black;
        }
        else
        {
            RbNode<TKey, TValue> repl = node.Right;
            while(repl.Left != null)
            {
                repl = repl.Left;
            }

            RbNode<TKey, TValue>? child = repl.Right;
            RbColor removedColor = repl.Color;
            RbNode<TKey, TValue>? fixParent = repl.Parent;
            bool fixupIsLeftChild = true;

            if(repl.Parent != node)
            {
                Transplant(repl, repl.Right);
                repl.Right = node.Right;
                repl.Right.Parent = repl;
            }
            else
            {
                fixParent = repl;
                fixupIsLeftChild = false;
            }

            Transplant(node, repl);
            repl.Left = node.Left;
            repl.Left.Parent = repl;
            repl.Color = node.Color;

            if(removedColor == RbColor.Black)
            {
                _removedWasLeftChild = child?.IsLeftChild ?? fixupIsLeftChild;
                OnNodeRemoved(fixParent, child);
            }
            else if(Root != null) Root.Color = RbColor.Black;
        }
    }

    private void RestoreAfterInsert(RbNode<TKey, TValue> node)
    {
        while(node != Root && node.Parent?.Color == RbColor.Red)
        {
            bool parentIsLeftChild = node.Parent!.IsLeftChild;
            RbNode<TKey, TValue> grandParent = node.Parent.Parent!;
            RbNode<TKey, TValue>? uncle = parentIsLeftChild ? grandParent.Right : grandParent.Left;
            if(uncle?.Color == RbColor.Red)
            {
                node.Parent.Color = RbColor.Black;
                uncle!.Color = RbColor.Black;
                grandParent.Color = RbColor.Red;
                node = grandParent;
                continue;
            }

            if(parentIsLeftChild ? node.IsRightChild : node.IsLeftChild)
            {
                node = node.Parent;
                if(parentIsLeftChild) RotateLeft(node);
                else RotateRight(node);
            }

            node.Parent!.Color = RbColor.Black;
            grandParent.Color = RbColor.Red;
            if(parentIsLeftChild) RotateRight(grandParent);
            else RotateLeft(grandParent);
        }
    }

    private void RestoreAfterRemove(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        RbNode<TKey, TValue>? current = child;
        RbNode<TKey, TValue>? currentParent = parent;
        bool currentIsLeftChild = _removedWasLeftChild;

        while(current != Root && current?.Color != RbColor.Red)
        {
            if(currentParent == null) break;

            bool isLeftChild = current != null ? current.IsLeftChild : currentIsLeftChild;
            RbNode<TKey, TValue>? sibling = isLeftChild ? currentParent.Right : currentParent.Left;
            if(sibling?.Color == RbColor.Red)
            {
                sibling!.Color = RbColor.Black;
                currentParent.Color = RbColor.Red;
                if(isLeftChild) RotateLeft(currentParent);
                else RotateRight(currentParent);

                sibling = isLeftChild ? currentParent.Right : currentParent.Left;
            }

            RbNode<TKey, TValue>? nearChild = isLeftChild ? sibling?.Left : sibling?.Right;
            RbNode<TKey, TValue>? farChild = isLeftChild ? sibling?.Right : sibling?.Left;

            if(nearChild?.Color != RbColor.Red && farChild?.Color != RbColor.Red)
            {
                if(sibling != null) sibling.Color = RbColor.Red;

                current = currentParent;
                currentParent = current.Parent;
                currentIsLeftChild = current.IsLeftChild;
                continue;
            }

            if(farChild?.Color != RbColor.Red)
            {
                if(nearChild != null) nearChild.Color = RbColor.Black;

                if(sibling != null)
                {
                    sibling.Color = RbColor.Red;
                    if(isLeftChild) RotateRight(sibling);
                    else RotateLeft(sibling);
                }

                sibling = isLeftChild ? currentParent.Right : currentParent.Left;
                farChild = isLeftChild ? sibling?.Right : sibling?.Left;
            }

            if(sibling != null) sibling.Color = currentParent.Color;

            currentParent.Color = RbColor.Black;
            if(farChild != null) farChild.Color = RbColor.Black;

            if(isLeftChild) RotateLeft(currentParent);
            else RotateRight(currentParent);

            current = Root;
        }

        if(current != null) current.Color = RbColor.Black;
    }
}
