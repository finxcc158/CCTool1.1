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
    internal class ShowStatisticsSD : Button
    {

        private StatisticsSD _statisticssd = null;

        protected override void OnClick()
        {
            //already open?
            if (_statisticssd != null)
                return;
            _statisticssd = new StatisticsSD();
            _statisticssd.Owner = FrameworkApplication.Current.MainWindow;
            _statisticssd.Closed += (o, e) => { _statisticssd = null; };
            _statisticssd.Show();
            //uncomment for modal
            //_statisticssd.ShowDialog();
        }

    }
}
