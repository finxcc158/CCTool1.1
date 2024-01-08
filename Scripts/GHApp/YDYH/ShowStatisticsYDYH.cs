using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace CCTool.Scripts
{
    internal class ShowStatisticsYDYH : Button
{

    private StatisticsYDYH _statisticsydyh = null;

    protected override void OnClick()
    {
        //already open?
        if (_statisticsydyh != null)
            return;
        _statisticsydyh = new StatisticsYDYH();
        _statisticsydyh.Owner = FrameworkApplication.Current.MainWindow;
        _statisticsydyh.Closed += (o, e) => { _statisticsydyh = null; };
        _statisticsydyh.Show();
         //uncomment for modal
         //_statisticsydyh.ShowDialog();
}

}
}
