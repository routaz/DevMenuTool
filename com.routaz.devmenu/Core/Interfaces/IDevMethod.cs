namespace DevMenuTool.Core.Interfaces
{
    /// <summary>
    /// A dev menu method: its metadata, execution, parameter state, validation, and change events.
    /// Composed from focused role interfaces (ISP) rather than a flat member list.
    /// </summary>
    public interface IDevMethod :
        IDevMethodInfo,
        IDevMethodExecutor,
        IParameterManager,
        IParameterValidatorManager,
        IParameterChangeNotifier
    {
    }
}
