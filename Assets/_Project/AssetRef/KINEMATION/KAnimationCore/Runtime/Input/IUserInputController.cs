﻿using System;

namespace KINEMATION.KAnimationCore.Runtime.Input
{
    [Obsolete("use `UserInputController` instead.")]
    public interface IUserInputController
    {
        public void Initialize();
        
        public int GetPropertyIndex(string propertyName);

        public void SetValue(string propertyName, object value);

        public T GetValue<T>(string propertyName);

        public void SetValue(int propertyIndex, object value);

        public T GetValue<T>(int propertyIndex);
    }
}