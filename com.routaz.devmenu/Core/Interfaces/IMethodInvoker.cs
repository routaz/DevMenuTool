// IMethodInvoker.cs
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.Core.Interfaces
{
    public interface IMethodInvoker
    {
        void Invoke(IDevMethod method, object[] suppliedArgs = null);
    }
}