namespace DevMenuTool.Core.Interfaces
{
    /// <summary>
    /// Manages parameter change events and handlers, and raises them on behalf of a DevMethod.
    /// </summary>
    public interface IParameterEventManager : IParameterChangeNotifier
    {
        void OnParameterChanged(object sender, int parameterIndex, object oldValue, object newValue);
    }
}
