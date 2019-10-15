using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace Goap
{
    public class GoapCondition
    {
        public static readonly int MAX = 4;

        private int[] _bitmap;

        public GoapCondition()
        {
            _bitmap = new int[MAX];
        }
        
        public GoapCondition(GoapCondition status)
        {
            _bitmap = new int[MAX];
            for (int i = 0; i < MAX; i++)
            {
                _bitmap[i] = status._bitmap[i];
            }
        }
        
        public GoapCondition(int status)
        {
            _bitmap = new int[MAX];
            _bitmap[status >> 28] = status & 0x0fffffff;
        }

        public GoapCondition(int[] status)
        {
            _bitmap = new int[MAX];
            foreach (var i in status)
            {
                _bitmap[i >> 28] = i & 0x0fffffff;
            }
        }

        public bool Contains(GoapCondition goal)
        {
            if (goal == null)
                return true;
            for (int i = 0; i < MAX; i++)
            {
                if ((_bitmap[i] & goal._bitmap[i]) != goal._bitmap[i])
                    return false;
            }

            return true;
        }

        public GoapCondition Add(GoapCondition effects)
        {
            if (effects == null)
                return this;
            for (int i = 0; i < MAX; i++)
            {
                _bitmap[i] |= effects._bitmap[i];
            }

            return this;
        }
        
        public GoapCondition Remove(GoapCondition effects)
        {
            if (effects == null)
                return this;
            for (int i = 0; i < MAX; i++)
            {
                _bitmap[i] &= ~effects._bitmap[i];
            }

            return this;
        }
    }
}
