using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace EGEJournal.Helpers
{
    class CustomKeyboardCommandProvider : DefaultKeyboardCommandProvider
    {
        private GridViewDataControl parentGrid;

        public CustomKeyboardCommandProvider(GridViewDataControl grid)
            : base(grid)
        {
            this.parentGrid = grid;
        }
        public override IEnumerable<ICommand> ProvideCommandsForKey(Key key)
        {
            List<ICommand> commandsToExecute = base.ProvideCommandsForKey(key).ToList();

            if (key == Key.Enter)
            {
                commandsToExecute.Clear();
                commandsToExecute.Add(RadGridViewCommands.CommitEdit);
                //commandsToExecute.Add(RadGridViewCommands.MoveNext);
                //commandsToExecute.Add(RadGridViewCommands.BeginEdit);
            }

            return commandsToExecute;

        }
    }
}
