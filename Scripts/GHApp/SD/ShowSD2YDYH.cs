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
    internal class ShowSD2YDYH : Button
    {

        private SD2YDYH _sd2ydyh = null;

        protected override void OnClick()
        {
            //already open?
            if (_sd2ydyh != null)
                return;
            _sd2ydyh = new SD2YDYH();
            _sd2ydyh.Owner = FrameworkApplication.Current.MainWindow;
            _sd2ydyh.Closed += (o, e) => { _sd2ydyh = null; };
            _sd2ydyh.Show();
            //uncomment for modal
            //_sd2ydyh.ShowDialog();
        }

    }
}
