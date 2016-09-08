using EGEJournal.Helpers;
using System;
using System.Collections.Generic;
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

namespace EGEJournal.View
{
    /// <summary>
    /// Логика взаимодействия для PPEExamView.xaml
    /// </summary>
    public partial class PPEExamView : UserControl
    {
        public PPEExamView()
        {
            InitializeComponent();
            GridPPEExam.KeyboardCommandProvider = new CustomKeyboardCommandProvider(GridPPEExam);
        }
    }
}
