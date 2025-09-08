using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameNpc
{    
    public static class NpcUtil
    {
        public static int GetRandomIndex(int length)
        {
            System.Random randomInstance = new System.Random();
            return randomInstance.Next(0, length);
        }
    }
}
