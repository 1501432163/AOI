﻿using System;

namespace AOI
{
    public sealed class AoiLinkedList
    {
        private AoiNode _header;
        private readonly Random _random = new Random();
        private readonly float _limit;
        private readonly int _maxLayer;

        public AoiLinkedList(int maxLayer = 5, float limit = 0)
        {
            _limit = limit;
            _maxLayer = maxLayer;
            Add(float.MinValue, null);
            Count -= maxLayer;
        }

        public int Count { get; private set; }

        /// <summary>
        /// Add node
        /// </summary>
        /// <param name="target">target MinValue = -3.402823E+38f</param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public AoiNode Add(float target, AoiEntity entity)
        {
            // toss a coin and Create a new _header
            var rLayer = 1;
            if (_header == null)
            {
                rLayer = _maxLayer;
                var tempHeader = _header = AoiPool.Instance.Fetch<AoiNode>().Init(rLayer, target, entity);

                for (var layer = _maxLayer - 1; layer >= 1; --layer)
                {
                    _header = _header.Down =
                        AoiPool.Instance.Fetch<AoiNode>().Init(layer, target, entity, top: _header);
                }

                _header = tempHeader;
                return null;
            }

            while (rLayer < _maxLayer && _random.Next(2) == 0) ++rLayer;

            if (rLayer > _maxLayer)
            {
                _header = AoiPool.Instance.Fetch<AoiNode>().Init(rLayer, target, entity);
            }

            // Define the required variables
            AoiNode cur = _header, lastLayerNode = null, insertNode = null;
            // Iterate through all layers to create an AoiNode
            for (var layer = _maxLayer; layer >= 1; --layer)
            {
                while (cur.Right != null && cur.Right.Value <= target)
                {
                    cur = cur.Right;
                }

                if (layer > rLayer)
                {
                    cur = cur.Down;
                }
                else
                {
                    if (Math.Abs(target - cur.Value) > _limit)
                    {
                        insertNode = AoiPool.Instance.Fetch<AoiNode>().Init(layer, target, entity, top: lastLayerNode);

                        if (target - cur.Value > 0)
                        {
                            insertNode.Left = cur;
                            insertNode.Right = cur.Right;
                            if (cur.Right != null)
                            {
                                cur.Right.Left = insertNode;
                            }

                            cur.Right = insertNode;
                        }
                        else
                        {
                            insertNode.Right = cur;
                            insertNode.Left = cur.Left;
                            if (cur.Left != null)
                            {
                                cur.Left.Right = insertNode;
                            }

                            cur.Left = insertNode;
                        }

                        if (lastLayerNode != null)
                        {
                            lastLayerNode.Down = insertNode;
                        }

                        insertNode.Count = 1;
                    }
                    else if (layer == 1)
                    {
                        cur.Count++;
                    }

                    lastLayerNode = insertNode;
                    cur = cur.Down;
                }
            }

            Count++;
            return insertNode;
        }

        /// <summary>
        /// TryGetValue
        /// </summary>
        /// <param name="target">target MinValue = -3.402823E+38f</param>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool TryGetValue(float target, out AoiNode node)
        {
            node = null;
            var cur = _header;
            while (cur != null)
            {
                while (cur.Right != null && cur.Right.Value < target) cur = cur.Right;

                if (cur.Right != null && Math.Abs(cur.Right.Value - target) < _limit)
                {
                    node = cur.Right;
                    while (node.Down != null) node = node.Down;
                    return true;
                }

                cur = cur.Down;
            }

            return false;
        }

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool Remove(float target)
        {
            if (!TryGetValue(target, out var cur))
            {
                return false;
            }

            if (cur.Count > 1)
            {
                cur.Count--;
            }
            else
            {
                while (cur != null)
                {
                    var temp = cur;
                    cur = cur.Top;
                    CircuitBreaker(temp);
                    temp.Recycle();
                }
            }

            Count--;
            return true;
        }

        /// <summary>
        /// Move
        /// </summary>
        /// <param name="node"></param>
        /// <param name="target"></param>
        public void Move(AoiNode node, ref float target)
        {
            var cur = node;

            #region Left

            if (target > cur.Value)
            {
                while (cur != null)
                {
                    if (cur.Right != null && target > cur.Right.Value)
                    {
                        var findNode = cur;
                        // Find the target node to be moved to.
                        while (findNode.Right != null && findNode.Right.Value < target) findNode = findNode.Right;
                        // Fuse the current node.
                        CircuitBreaker(cur);
                        // Move to the target node location
                        cur.Left = findNode;
                        cur.Right = findNode.Right;
                        if (findNode.Right != null) findNode.Right.Left = cur;
                        findNode.Right = cur;
                    }

                    cur.Value = target;
                    cur = cur.Top;
                }

                return;
            }

            #endregion

            #region Right

            while (cur != null)
            {
                if (cur.Left != null && target < cur.Left.Value)
                {
                    // Find the target node to be moved to.
                    var findNode = cur;
                    while (findNode.Left != null && findNode.Left.Value > target) findNode = findNode.Left;
                    // Fuse the current node.
                    CircuitBreaker(cur);
                    // Move to the target node location
                    cur.Right = findNode;
                    cur.Left = findNode.Left;
                    if (findNode.Left != null) findNode.Left.Right = cur;
                    findNode.Left = cur;
                }

                cur.Value = target;
                cur = cur.Top;
            }

            #endregion
        }

        /// <summary>
        /// Circuit Breaker
        /// </summary>
        /// <param name="cur"></param>
        private void CircuitBreaker(AoiNode cur)
        {
            if (cur.Left != null) cur.Left.Right = cur.Right;
            if (cur.Right == null) return;
            cur.Right.Left = cur.Left;
            cur.Left = null;
            cur.Right = null;
        }
    }
}