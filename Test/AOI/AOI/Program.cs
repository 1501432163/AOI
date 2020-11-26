﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Transactions;

namespace AOI
{
    class Program
    {
        static void Main(string[] args)
        {
            var zone = new AoiZone();
            var area = new Vector2(3, 3);

            for (var i = 1; i <= 100000; i++)
            {
                var ss = zone.Enter(i, i, i);
            }

            var entity = zone.Update(50, area);

            Console.WriteLine("---------------加入玩家范围的玩家列表--------------");

            foreach (var aoiKey in entity.Enter)
            {
                var findEntity = zone[aoiKey];
                Console.WriteLine($"X:{findEntity.X.Value} Y:{findEntity.Y.Value}");
            }

            // 更新key为50的坐标

            entity = zone.Update(50, 3, 3, new Vector2(3, 3));

            Console.WriteLine("---------------离开玩家范围的玩家列表--------------");

            foreach (var aoiKey in entity.Leave)
            {
                var findEntity = zone[aoiKey];
                Console.WriteLine($"X:{findEntity.X.Value} Y:{findEntity.Y.Value}");
            }
        }
    }
}