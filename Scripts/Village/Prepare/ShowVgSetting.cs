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
    internal class ShowVgSetting : Button
    {

        private VgSetting _vgsetting = null;

        protected override void OnClick()
        {
            //already open?
            if (_vgsetting != null)
                return;
            _vgsetting = new VgSetting();
            _vgsetting.Owner = FrameworkApplication.Current.MainWindow;
            _vgsetting.Closed += (o, e) => { _vgsetting = null; };
            _vgsetting.Show();
            //uncomment for modal
            //_vgsetting.ShowDialog();
        }

    }
}
