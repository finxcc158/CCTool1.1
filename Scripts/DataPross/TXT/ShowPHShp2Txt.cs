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
    internal class ShowPHShp2Txt : Button
{

    private PHShp2Txt _phshp2txt = null;

    protected override void OnClick()
    {
        //already open?
        if (_phshp2txt != null)
            return;
        _phshp2txt = new PHShp2Txt();
        _phshp2txt.Owner = FrameworkApplication.Current.MainWindow;
        _phshp2txt.Closed += (o, e) => { _phshp2txt = null; };
        _phshp2txt.Show();
         //uncomment for modal
         //_phshp2txt.ShowDialog();
}

}
}
