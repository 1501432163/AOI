﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace AOI
{
    public sealed class AoiZone
    {
        private readonly Dictionary<long, AoiEntity> _entityList = new Dictionary<long, AoiEntity>();

        private readonly AoiLinkedList _xLinks = new AoiLinkedList();
        private readonly AoiLinkedList _yLinks = new AoiLinkedList();

        public AoiEntity this[long key] => !_entityList.TryGetValue(key, out var entity) ? null : entity;

        /// <summary>
        /// Add a new AoiZone
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns></returns>
        public AoiEntity Enter(long key, float x, float y)
        {
            if (_entityList.TryGetValue(key, out var entity)) return entity;

            entity = AoiPool.Instance.Fetch<AoiEntity>().Init(key);

            entity.X = _xLinks.Add(x, entity);
            entity.Y = _yLinks.Add(y, entity);

            _entityList.Add(key, entity);

            return entity;
        }

        /// <summary>
        /// Update the AoiEntity
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="area">view</param>
        /// <returns></returns>
        public AoiEntity Update(long key, float x, float y, Vector2 area)
        {
            if (!_entityList.TryGetValue(key, out var entity)) return null;

            if (Math.Abs(entity.X.Value - x) > 0)
            {
                _xLinks.Move(ref entity.X, ref x);
            }

            if (Math.Abs(entity.Y.Value - y) > 0)
            {
                _yLinks.Move(ref entity.Y, ref y);
            }

            Find(ref entity, ref area);

            return entity;
        }

        private void Find(ref AoiEntity node, ref Vector2 area)
        {
            node.ViewEntity.Clear();
            
            #region xLinks

            for (var i = 0; i < 2; i++)
            {
                var cur = i == 0 ? node.X.Right : node.X.Left;

                while (cur != null)
                {
                    if (Math.Abs(Math.Abs(cur.Value) - Math.Abs(node.X.Value)) > area.X)
                    {
                        break;
                    }
                    else if (Math.Abs(Math.Abs(cur.Entity.Y.Value) - Math.Abs(node.Y.Value)) <= area.Y)
                    {
                        if (Distance(
                            new Vector2(node.X.Value, node.Y.Value),
                            new Vector2(cur.Entity.X.Value, cur.Entity.Y.Value)) <= area.X)
                        {
                            node.ViewEntity.Add(cur.Entity.Key);
                        }
                    }

                    cur = i == 0 ? cur.Right : cur.Left;
                }
            }

            #endregion

            #region yLinks

            for (var i = 0; i < 2; i++)
            {
                var cur = i == 0 ? node.Y.Right : node.Y.Left;

                while (cur != null)
                {
                    if (Math.Abs(Math.Abs(cur.Value) - Math.Abs(node.Y.Value)) > area.Y)
                    {
                        break;
                    }
                    else if (Math.Abs(Math.Abs(cur.Entity.X.Value) - Math.Abs(node.X.Value)) <= area.X)
                    {
                        if (Distance(
                            new Vector2(node.X.Value, node.Y.Value),
                            new Vector2(cur.Entity.X.Value, cur.Entity.Y.Value)) <= area.X)
                        {
                            node.ViewEntity.Add(cur.Entity.Key);
                        }
                    }

                    cur = i == 0 ? cur.Right : cur.Left;
                }
            }

            #endregion
        }

        private double Distance(Vector2 a, Vector2 b)
        {
            return Math.Pow((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y), 0.5);
        }
    }
}