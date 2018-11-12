using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ThriftVSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class TopMenuCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;
        public const int SubMenu = 0x1100;
        public const int SubCommandId1 = 0x0101;
        public const int SubCommandId2 = 0x0102;
        public const int SubCommandId3 = 0x0103;
        public const int SubCommandId4 = 0x0104;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("b0af3d0e-60af-4dbe-91cb-510d7871a8bd");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopMenuCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private TopMenuCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;
            commandService.AddCommand(menuItem);

            var subMenuId = new CommandID(CommandSet, SubMenu);
            var subMenuItem = new OleMenuCommand(this.Execute, subMenuId);
            subMenuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;
            commandService.AddCommand(subMenuItem);

            CommandID subCommandID1 = new CommandID(CommandSet, SubCommandId1);
            OleMenuCommand subItem1 = new OleMenuCommand(new EventHandler(SubItemCallback1), subCommandID1);
            //subItem1.BeforeQueryStatus+= menuItem_BeforeQueryStatus; 
            commandService.AddCommand(subItem1);

            CommandID subCommandID2 = new CommandID(CommandSet, SubCommandId2);
            OleMenuCommand subItem2 = new OleMenuCommand(new EventHandler(SubItemCallback2), subCommandID2);
            //subItem2.BeforeQueryStatus += menuItem_BeforeQueryStatus;
            commandService.AddCommand(subItem2);

            CommandID subCommandID3 = new CommandID(CommandSet, SubCommandId3);
            OleMenuCommand subItem3 = new OleMenuCommand(new EventHandler(SubItemCallback3), subCommandID3);
            //subItem3.BeforeQueryStatus += menuItem_BeforeQueryStatus;
            commandService.AddCommand(subItem3);

            CommandID subCommandID4 = new CommandID(CommandSet, SubCommandId4);
            OleMenuCommand subItem4 = new OleMenuCommand(new EventHandler(SubItemCallback4), subCommandID4);
            //subItem4.BeforeQueryStatus += menuItem_BeforeQueryStatus;
            commandService.AddCommand(subItem4);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static TopMenuCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in TopMenuCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new TopMenuCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "TopMenuCommand";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private void SubItemCallback1(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = GetDTE();
            if (dte == null)
                return;

            try
            {
                var activeFile = dte.ActiveDocument.FullName;
                ThriftGenerate ser = new ThriftGenerateNet45(activeFile);
                ser.GenerateProject();
            }
            catch(Exception ex)
            {
                string message = ex.Message;
                VsShellUtilities.ShowMessageBox(
                this.package,
                "Thrift Client Net45",
                message,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private void SubItemCallback2(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = GetDTE();
            if (dte == null)
                return;

            try
            {
                var activeFile = dte.ActiveDocument.FullName;
                ThriftGenerate ser = new ThriftGenerateNet45(activeFile);
                ser.GenerateSource(true);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                VsShellUtilities.ShowMessageBox(
                this.package,
                "Thrift Server Net45",
                message,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }

            
        }

        private void SubItemCallback3(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = GetDTE();
            if (dte == null)
                return;

            try
            {
                var activeFile = dte.ActiveDocument.FullName;
                ThriftGenerate ser = new ThriftGenerateNetCore(activeFile);
                ser.GenerateProject();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                VsShellUtilities.ShowMessageBox(
                this.package,
                "Thrift Client dotcore",
                message,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private void SubItemCallback4(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = GetDTE();
            if (dte == null)
                return;

            try
            {
                var activeFile = dte.ActiveDocument.FullName;
                ThriftGenerate ser = new ThriftGenerateNetCore(activeFile);
                ser.GenerateSource(true);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                VsShellUtilities.ShowMessageBox(
                this.package,
                "Thrift Server dotcore",
                message,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private void menuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                var dte = GetDTE();
                if (dte == null)
                    return;

                string fileName = dte.ActiveDocument.FullName;

                if (fileName != null && fileName.EndsWith(".thrift", StringComparison.OrdinalIgnoreCase))
                {
                    menuCommand.Visible = true;
                }
                else
                {
                    menuCommand.Visible = false;
                }
            }
        }

        private DTE GetDTE()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var objDTE = ServiceProvider.GetServiceAsync(typeof(DTE))?.Result;
            if (objDTE == null)
                return null;
            DTE dte = objDTE as DTE;
            if (dte == null)
                return null;

            return dte;
        }
    }
}
