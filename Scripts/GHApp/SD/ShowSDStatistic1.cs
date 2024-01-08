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
    internal class ShowSDStatistic1 : Button
    {

        private SDStatistic1 _sdstatistic1 = null;

        protected override void OnClick()
        {
            //already open?
            if (_sdstatistic1 != null)
                return;
            _sdstatistic1 = new SDStatistic1();
            _sdstatistic1.Owner = FrameworkApplication.Current.MainWindow;
            _sdstatistic1.Closed += (o, e) => { _sdstatistic1 = null; };
            _sdstatistic1.Show();
            //uncomment for modal
            //_sdstatistic1.ShowDialog();
        }

    }
}
