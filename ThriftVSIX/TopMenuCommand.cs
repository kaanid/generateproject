using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ThriftService;
using Task = System.Threading.Tasks.Task;

namespace ThriftVSIX
{
    public class DynamicMenuCommand : OleMenuCommand
    {
        private Predicate<int> _matches;
        private int _matchedCommandId;

        public const string guidDynamicMenuPackageCmdSet = "b0af3d0e-60af-4dbe-91cb-510d7871a8bd";

        public const int CommandId = 0x0100;
        public const int SubMenu = 0x1100;
        public const int SubCommandId1 = 0x0101;
        public const int SubCommandId2 = 0x0102;
        public const int SubCommandId3 = 0x0103;
        public const int SubCommandId4 = 0x0104;

        public DynamicMenuCommand(
            CommandID rootId,
            Predicate<int> matches,
            EventHandler invokeHandler,
            EventHandler beforeQueryStatusHandler)
            : base(invokeHandler, null /*changeHandler*/, beforeQueryStatusHandler, rootId)
        {
            _matches = matches ?? throw new ArgumentNullException("matches");
        }

        public override bool DynamicItemMatch(int cmdId)
        {
            if (_matches(cmdId))
            {
                _matchedCommandId = cmdId;
                return true;
            }
            _matchedCommandId = 0;
            return false;
        }
    }

    class DynamicMenu
    {
        private DTE2 _dte2;
        private Package _package;
        public DynamicMenu(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            _package = package;

            OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                // Add the DynamicItemMenuCommand for the expansion of the root item into N items at run time.
                CommandID dynamicItemRootId = new CommandID(new Guid(DynamicMenuCommand.guidDynamicMenuPackageCmdSet), DynamicMenuCommand.SubMenu);
                DynamicMenuCommand dynamicMenuCommand = new DynamicMenuCommand(dynamicItemRootId,
                    IsValidDynamicItem,
                    OnInvokedDynamicItem,
                    OnBeforeQueryStatusDynamicItem);
                commandService.AddCommand(dynamicMenuCommand);

                CommandID dynamicItemRootId1 = new CommandID(new Guid(DynamicMenuCommand.guidDynamicMenuPackageCmdSet), DynamicMenuCommand.SubCommandId1);
                DynamicMenuCommand dynamicMenuCommand1 = new DynamicMenuCommand(dynamicItemRootId1,
                    IsValidDynamicItem,
                    OnInvokedDynamicItem,
                    null);
                commandService.AddCommand(dynamicMenuCommand1);

                CommandID dynamicItemRootId2 = new CommandID(new Guid(DynamicMenuCommand.guidDynamicMenuPackageCmdSet), DynamicMenuCommand.SubCommandId2);
                DynamicMenuCommand dynamicMenuCommand2 = new DynamicMenuCommand(dynamicItemRootId2,
                    IsValidDynamicItem,
                    OnInvokedDynamicItem,
                    null);
                commandService.AddCommand(dynamicMenuCommand2);

                CommandID dynamicItemRootId3 = new CommandID(new Guid(DynamicMenuCommand.guidDynamicMenuPackageCmdSet), DynamicMenuCommand.SubCommandId3);
                DynamicMenuCommand dynamicMenuCommand3 = new DynamicMenuCommand(dynamicItemRootId3,
                    IsValidDynamicItem,
                    OnInvokedDynamicItem,
                    null);
                commandService.AddCommand(dynamicMenuCommand3);

                CommandID dynamicItemRootId4 = new CommandID(new Guid(DynamicMenuCommand.guidDynamicMenuPackageCmdSet), DynamicMenuCommand.SubCommandId4);
                DynamicMenuCommand dynamicMenuCommand4 = new DynamicMenuCommand(dynamicItemRootId4,
                    IsValidDynamicItem,
                    OnInvokedDynamicItem,
                    null);
                commandService.AddCommand(dynamicMenuCommand4);
            }

            _dte2 = (DTE2)ServiceProvider.GetService(typeof(DTE));
            Assumes.Present(_dte2);
        }

        private System.IServiceProvider ServiceProvider
        {
            get
            {
                return _package;
            }
        }

        private bool IsValidDynamicItem(int commandId)
        {
            // The match is valid if the command ID is >= the id of our root dynamic start item
            // and the command ID minus the ID of our root dynamic start item
            // is less than or equal to the number of projects in the solution.
            return commandId >= DynamicMenuCommand.SubCommandId1 && commandId <= DynamicMenuCommand.SubCommandId4;
        }
        private void OnInvokedDynamicItem(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            OleMenuCommand menuCommand = null;
            try
            {
                menuCommand = sender as OleMenuCommand;
                if (menuCommand != null)
                {
                    switch (menuCommand.CommandID.ID)
                    {
                        case DynamicMenuCommand.SubCommandId1:
                            var ser1 = new ThriftGenerateNet45(_dte2.ActiveDocument.FullName);
                            ser1.GenerateProject();
                            break;
                        case DynamicMenuCommand.SubCommandId2:
                            ThriftGenerate ser2 = new ThriftGenerateNet45(_dte2.ActiveDocument.FullName);
                            ser2.GenerateSource(true);
                            break;
                        case DynamicMenuCommand.SubCommandId3:
                            ThriftGenerate ser3 = new ThriftGenerateNetCore(_dte2.ActiveDocument.FullName);
                            ser3.GenerateProject();
                            break;
                        case DynamicMenuCommand.SubCommandId4:
                            ThriftGenerate ser4 = new ThriftGenerateNetCore(_dte2.ActiveDocument.FullName);
                            ser4.GenerateSource(true);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = $"{menuCommand?.CommandID.ID ?? null}:{ex.Message}";
                VsShellUtilities.ShowMessageBox(
                    _package,
                    message,
                    "GrpcClient Exception",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
        private void OnBeforeQueryStatusDynamicItem(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                string fileName = _dte2.ActiveDocument.FullName;
                if (fileName.EndsWith(".thrift"))
                    menuCommand.Visible = true;
                else
                    menuCommand.Visible = false;
            }
        }
    }
}
