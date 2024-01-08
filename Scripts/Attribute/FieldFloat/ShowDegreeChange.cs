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
    internal class ShowDegreeChange : Button
    {

        private DegreeChange _degreechange = null;

        protected override void OnClick()
        {
            //already open?
            if (_degreechange != null)
                return;
            _degreechange = new DegreeChange();
            _degreechange.Owner = FrameworkApplication.Current.MainWindow;
            _degreechange.Closed += (o, e) => { _degreechange = null; };
            _degreechange.Show();
            //uncomment for modal
            //_degreechange.ShowDialog();
        }

    }
}
