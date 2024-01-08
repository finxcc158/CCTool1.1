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

namespace CCTool.Scripts.UI.SD
{
    internal class ShowSDStatisticYD : Button
    {

        private SDStatisticYD _sdstatisticyd = null;

        protected override void OnClick()
        {
            //already open?
            if (_sdstatisticyd != null)
                return;
            _sdstatisticyd = new SDStatisticYD();
            _sdstatisticyd.Owner = FrameworkApplication.Current.MainWindow;
            _sdstatisticyd.Closed += (o, e) => { _sdstatisticyd = null; };
            _sdstatisticyd.Show();
            //uncomment for modal
            //_sdstatisticyd.ShowDialog();
        }

    }
}
