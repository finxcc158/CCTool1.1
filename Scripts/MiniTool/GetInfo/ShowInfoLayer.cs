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

namespace CCTool.Scripts.MiniTool.GetInfo
{
    internal class ShowInfoLayer : Button
    {

        private InfoLayer _infolayer = null;

        protected override void OnClick()
        {
            //already open?
            if (_infolayer != null)
                return;
            _infolayer = new InfoLayer();
            _infolayer.Owner = FrameworkApplication.Current.MainWindow;
            _infolayer.Closed += (o, e) => { _infolayer = null; };
            _infolayer.Show();
            //uncomment for modal
            //_infolayer.ShowDialog();
        }

    }
}
