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

namespace CCTool.Scripts.Attribute.FieldFloat
{
    internal class ShowCalculateArea : Button
    {

        private CalculateArea _calculatearea = null;

        protected override void OnClick()
        {
            //already open?
            if (_calculatearea != null)
                return;
            _calculatearea = new CalculateArea();
            _calculatearea.Owner = FrameworkApplication.Current.MainWindow;
            _calculatearea.Closed += (o, e) => { _calculatearea = null; };
            _calculatearea.Show();
            //uncomment for modal
            //_calculatearea.ShowDialog();
        }

    }
}
