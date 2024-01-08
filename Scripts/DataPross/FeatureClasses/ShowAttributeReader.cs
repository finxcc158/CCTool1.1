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
    internal class ShowAttributeReader : Button
    {

        private AttributeReader _attributereader = null;

        protected override void OnClick()
        {
            //already open?
            if (_attributereader != null)
                return;
            _attributereader = new AttributeReader();
            _attributereader.Owner = FrameworkApplication.Current.MainWindow;
            _attributereader.Closed += (o, e) => { _attributereader = null; };
            _attributereader.Show();
            //uncomment for modal
            //_attributereader.ShowDialog();
        }

    }
}
