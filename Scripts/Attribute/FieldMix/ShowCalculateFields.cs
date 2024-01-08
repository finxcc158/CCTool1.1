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
    internal class ShowCalculateFields : Button
    {

        private CalculateFields _calculatefields = null;

        protected override void OnClick()
        {
            //already open?
            if (_calculatefields != null)
                return;
            _calculatefields = new CalculateFields();
            _calculatefields.Owner = FrameworkApplication.Current.MainWindow;
            _calculatefields.Closed += (o, e) => { _calculatefields = null; };
            _calculatefields.Show();
            //uncomment for modal
            //_calculatefields.ShowDialog();
        }

    }
}
