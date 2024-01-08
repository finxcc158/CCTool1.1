using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.UI.ProWindow
{
    internal class ShowStatisticsSDDL : Button
    {

        private StatisticsSDDL _statisticssddl = null;

        protected override void OnClick()
        {
            //already open?
            if (_statisticssddl != null)
                return;
            _statisticssddl = new StatisticsSDDL();
            _statisticssddl.Owner = FrameworkApplication.Current.MainWindow;
            _statisticssddl.Closed += (o, e) => { _statisticssddl = null; };
            _statisticssddl.Show();
            //uncomment for modal
            //_statisticssddl.ShowDialog();
        }

    }
}
