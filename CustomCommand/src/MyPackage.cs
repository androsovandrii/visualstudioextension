using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace CustomCommandSample
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("Custom Command Sample", "Shows how to hook up a command in VS", "1.0")]       
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid("fa24d542-0b4d-4f6b-ac03-24ff47c11b76")] // must match GUID in the .vsct file
    public sealed class MyPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            EnvDTE80.DTE2 dte2 = (EnvDTE80.DTE2)GetService(typeof(EnvDTE.DTE));
            await MyCommand.InitializeAsync(this, dte2);
        }
    }
}
