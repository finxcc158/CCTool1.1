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

namespace CCTool.Scripts.UI.ProWindow
{
    internal class ShowAreaStatistics : Button
{

    private AreaStatistics _areastatistics = null;

    protected override void OnClick()
    {
        //already open?
        if (_areastatistics != null)
            return;
        _areastatistics = new AreaStatistics();
        _areastatistics.Owner = FrameworkApplication.Current.MainWindow;
        _areastatistics.Closed += (o, e) => { _areastatistics = null; };
        _areastatistics.Show();
         //uncomment for modal
         //_areastatistics.ShowDialog();
}

}
}
