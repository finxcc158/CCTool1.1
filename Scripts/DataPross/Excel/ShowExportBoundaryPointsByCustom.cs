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
    internal class ShowExportBoundaryPointsByCustom : Button
    {

        private ExportBoundaryPointsByCustom _exportboundarypointsbycustom = null;

        protected override void OnClick()
        {
            //already open?
            if (_exportboundarypointsbycustom != null)
                return;
            _exportboundarypointsbycustom = new ExportBoundaryPointsByCustom();
            _exportboundarypointsbycustom.Owner = FrameworkApplication.Current.MainWindow;
            _exportboundarypointsbycustom.Closed += (o, e) => { _exportboundarypointsbycustom = null; };
            _exportboundarypointsbycustom.Show();
            //uncomment for modal
            //_exportboundarypointsbycustom.ShowDialog();
        }

    }
}
