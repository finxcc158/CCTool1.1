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
    internal class ShowYDYHChanger : Button
    {

        private YDYHChanger _ydyhchanger = null;

        protected override void OnClick()
        {
            //already open?
            if (_ydyhchanger != null)
                return;
            _ydyhchanger = new YDYHChanger();
            _ydyhchanger.Owner = FrameworkApplication.Current.MainWindow;
            _ydyhchanger.Closed += (o, e) => { _ydyhchanger = null; };
            _ydyhchanger.Show();
            //uncomment for modal
            //_ydyhchanger.ShowDialog();
        }

    }
}
