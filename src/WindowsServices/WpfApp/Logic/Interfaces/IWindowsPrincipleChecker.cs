namespace WpfApp.Logic.Interfaces
{
    public interface IWindowsPrincipleChecker
    {
        bool IsRunUnderAdministrator { get; }
    }
}