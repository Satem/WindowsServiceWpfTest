namespace WpfApp.Logic
{
    using System.Security.Principal;
    using Interfaces;

    public class WindowsPrincipleChecker : IWindowsPrincipleChecker
    {
        public bool IsRunUnderAdministrator =>
            new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);
    }
}