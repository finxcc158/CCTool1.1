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

namespace CCTool.Scripts.DataPross.Excel
{
    internal class ShowSetBoundaryPointsFromNW : Button
    {

        private SetBoundaryPointsFromNW _setboundarypointsfromnw = null;

        protected override void OnClick()
        {
            //already open?
            if (_setboundarypointsfromnw != null)
                return;
            _setboundarypointsfromnw = new SetBoundaryPointsFromNW();
            _setboundarypointsfromnw.Owner = FrameworkApplication.Current.MainWindow;
            _setboundarypointsfromnw.Closed += (o, e) => { _setboundarypointsfromnw = null; };
            _setboundarypointsfromnw.Show();
            //uncomment for modal
            //_setboundarypointsfromnw.ShowDialog();
        }

    }
}
