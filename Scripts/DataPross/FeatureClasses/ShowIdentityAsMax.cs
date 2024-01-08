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

namespace CCTool.Scripts.DataPross.FeatureClasses
{
    internal class ShowIdentityAsMax : Button
    {

        private IdentityAsMax _identityasmax = null;

        protected override void OnClick()
        {
            //already open?
            if (_identityasmax != null)
                return;
            _identityasmax = new IdentityAsMax();
            _identityasmax.Owner = FrameworkApplication.Current.MainWindow;
            _identityasmax.Closed += (o, e) => { _identityasmax = null; };
            _identityasmax.Show();
            //uncomment for modal
            //_identityasmax.ShowDialog();
        }

    }
}
