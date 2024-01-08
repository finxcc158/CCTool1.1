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
    internal class ShowAttributeMapper : Button
    {

        private AttributeMapper _attributemapper = null;

        protected override void OnClick()
        {
            //already open?
            if (_attributemapper != null)
                return;
            _attributemapper = new AttributeMapper();
            _attributemapper.Owner = FrameworkApplication.Current.MainWindow;
            _attributemapper.Closed += (o, e) => { _attributemapper = null; };
            _attributemapper.Show();
            //uncomment for modal
            //_attributemapper.ShowDialog();
        }

    }
}
