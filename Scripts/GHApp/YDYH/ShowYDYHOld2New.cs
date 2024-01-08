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

namespace CCTool.Scripts.GHApp.YDYH
{
    internal class ShowYDYHOld2New : Button
    {

        private YDYHOld2New _ydyhold2new = null;

        protected override void OnClick()
        {
            //already open?
            if (_ydyhold2new != null)
                return;
            _ydyhold2new = new YDYHOld2New();
            _ydyhold2new.Owner = FrameworkApplication.Current.MainWindow;
            _ydyhold2new.Closed += (o, e) => { _ydyhold2new = null; };
            _ydyhold2new.Show();
            //uncomment for modal
            //_ydyhold2new.ShowDialog();
        }

    }
}
