using EGEJournal.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;

namespace EGEJournal.View
{
    /// <summary>
    /// Логика взаимодействия для JournalEditView.xaml
    /// </summary>
    public partial class JournalEditView : UserControl
    {
        public JournalEditView()
        {
            InitializeComponent();
            GridJournal.KeyboardCommandProvider = new CustomKeyboardCommandProvider(GridJournal);
        }

        private void RadGridView_DataLoaded(object sender, EventArgs e)
        {
            RadGridView grid = (sender as RadGridView);
            grid.ExpandAllGroups();
        }

        private void GridJournal_DataLoading(object sender, Telerik.Windows.Controls.GridView.GridViewDataLoadingEventArgs e)
        {
            RadGridView grid = (sender as RadGridView);
            if (grid.GroupDescriptors.Count == 0)
            {
                grid.GroupDescriptors.Add(new GroupDescriptor()
                {
                    Member = "ppe.area_name",
                    SortDirection = ListSortDirection.Ascending,
                    DisplayContent = "Район"
                });
                grid.GroupDescriptors.Add(new GroupDescriptor()
                {
                    Member = "blank_type_id",
                    SortDirection = ListSortDirection.Ascending,
                    DisplayContent = "Тип бланков"
                });
            }
        }

    }
}
