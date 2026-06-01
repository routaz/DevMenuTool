// MethodInvoker.cs
using System;
using DevMenuTool.Core.Interfaces;
using UnityEngine;

namespace DevMenuTool.Runtime
{
    public class MethodInvoker : IMethodInvoker
    {
        public void Invoke(IDevMethod method, object[] suppliedArgs = null)
        {
            try
            {
                if (suppliedArgs != null)
                {
                    for (int i = 0; i < suppliedArgs.Length && i < method.Parameters.Length; i++)
                    {
                        method.SetParameter(i, suppliedArgs[i]);
                    }
                }
                
                method.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error invoking method {method.MethodInfo.Name}: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}