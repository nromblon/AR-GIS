﻿using System;
using Framework.Events;

namespace Framework.Variables
{
    [Serializable]
    public class GameEventObjectReference
    {
        public bool UseConstant = true;
        public GameEventObject ConstantValue;
        public GameEventObjectVariable Variable;

        public GameEventObjectReference()
        { }

        public GameEventObjectReference(GameEventObject value)
        {
            UseConstant = true;
            ConstantValue = value;
        }

        public GameEventObject Value
        {
            get { return UseConstant ? ConstantValue : Variable.Value; }
        }
    }
}