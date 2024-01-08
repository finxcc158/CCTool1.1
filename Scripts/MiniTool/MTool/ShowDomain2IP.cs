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

namespace CCTool.Scripts.MiniTool
{
    internal class ShowDomain2IP : Button
    {

        private Domain2IP _domain2ip = null;

        protected override void OnClick()
        {
            //already open?
            if (_domain2ip != null)
                return;
            _domain2ip = new Domain2IP();
            _domain2ip.Owner = FrameworkApplication.Current.MainWindow;
            _domain2ip.Closed += (o, e) => { _domain2ip = null; };
            _domain2ip.Show();
            //uncomment for modal
            //_domain2ip.ShowDialog();
        }

    }
}
