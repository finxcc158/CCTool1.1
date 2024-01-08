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
    internal class ShowPHTxt2Shp : Button
{

    private PHTxt2Shp _phtxt2shp = null;

    protected override void OnClick()
    {
        //already open?
        if (_phtxt2shp != null)
            return;
        _phtxt2shp = new PHTxt2Shp();
        _phtxt2shp.Owner = FrameworkApplication.Current.MainWindow;
        _phtxt2shp.Closed += (o, e) => { _phtxt2shp = null; };
        _phtxt2shp.Show();
         //uncomment for modal
         //_phtxt2shp.ShowDialog();
}

}
}
