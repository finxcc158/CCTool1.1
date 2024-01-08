﻿using ArcGIS.Core.CIM;
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
    internal class ShowCheckYDChange : Button
    {

        private CheckYDChange _checkydchange = null;

        protected override void OnClick()
        {
            //already open?
            if (_checkydchange != null)
                return;
            _checkydchange = new CheckYDChange();
            _checkydchange.Owner = FrameworkApplication.Current.MainWindow;
            _checkydchange.Closed += (o, e) => { _checkydchange = null; };
            _checkydchange.Show();
            //uncomment for modal
            //_checkydchange.ShowDialog();
        }

    }
}
