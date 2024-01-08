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

namespace CCTool.Scripts
{
    internal class ShowCheckYDYH : Button
    {

        private CheckYDYH _checkydyh = null;

        protected override void OnClick()
        {
            //already open?
            if (_checkydyh != null)
                return;
            _checkydyh = new CheckYDYH();
            _checkydyh.Owner = FrameworkApplication.Current.MainWindow;
            _checkydyh.Closed += (o, e) => { _checkydyh = null; };
            _checkydyh.Show();
            //uncomment for modal
            //_checkydyh.ShowDialog();
        }

    }
}
