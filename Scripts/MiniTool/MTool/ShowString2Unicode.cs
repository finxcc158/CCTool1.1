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

namespace CCTool.Scripts.MiniTool.MTool
{
    internal class ShowString2Unicode : Button
    {

        private String2Unicode _string2unicode = null;

        protected override void OnClick()
        {
            //already open?
            if (_string2unicode != null)
                return;
            _string2unicode = new String2Unicode();
            _string2unicode.Owner = FrameworkApplication.Current.MainWindow;
            _string2unicode.Closed += (o, e) => { _string2unicode = null; };
            _string2unicode.Show();
            //uncomment for modal
            //_string2unicode.ShowDialog();
        }

    }
}
