namespace DevMenuTool.Core.Interfaces
{
    public interface IDevMethod :
        IDevMethodInfo,
        IDevMethodExecutor,
        IParameterManager,
        IParameterValidatorManager,
        IParameterChangeNotifier
    {
    }
}
