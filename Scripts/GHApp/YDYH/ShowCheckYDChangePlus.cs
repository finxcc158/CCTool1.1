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
    internal class ShowCheckYDChangePlus : Button
    {

        private CheckYDChangePlus _checkydchangeplus = null;

        protected override void OnClick()
        {
            //already open?
            if (_checkydchangeplus != null)
                return;
            _checkydchangeplus = new CheckYDChangePlus();
            _checkydchangeplus.Owner = FrameworkApplication.Current.MainWindow;
            _checkydchangeplus.Closed += (o, e) => { _checkydchangeplus = null; };
            _checkydchangeplus.Show();
            //uncomment for modal
            //_checkydchangeplus.ShowDialog();
        }

    }
}
