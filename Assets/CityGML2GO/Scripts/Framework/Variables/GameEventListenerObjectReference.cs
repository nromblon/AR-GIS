﻿using System;
using Framework.Events;

namespace Framework.Variables
{
    [Serializable]
    public class GameEventListenerObjectReference
    {
        public bool UseConstant = true;
        public GameEventListenerObject ConstantValue;
        public GameEventListenerObjectVariable Variable;

        public GameEventListenerObjectReference()
        { }

        public GameEventListenerObjectReference(GameEventListenerObject value)
        {
            UseConstant = true;
            ConstantValue = value;
        }

        public GameEventListenerObject Value
        {
            get { return UseConstant ? ConstantValue : Variable.Value; }
        }
    }
}