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
    internal class ShowUpdataYDYH : Button
    {

        private UpdataYDYH _updataydyh = null;

        protected override void OnClick()
        {
            //already open?
            if (_updataydyh != null)
                return;
            _updataydyh = new UpdataYDYH();
            _updataydyh.Owner = FrameworkApplication.Current.MainWindow;
            _updataydyh.Closed += (o, e) => { _updataydyh = null; };
            _updataydyh.Show();
            //uncomment for modal
            //_updataydyh.ShowDialog();
        }

    }
}
